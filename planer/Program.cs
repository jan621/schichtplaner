using AppointmentManagement;
using AppointmentManagement.Contract;
using AutoMapper;
using Blazored.LocalStorage;
using BookingAdditionManagement;
using BookingAdditionManagement.Contract;
using CrossCutting.DataObjects;
using CrossCutting.Entities;
using CrossCutting.Identity;
using CrossCutting.Mapper;
using DatabaseManagement;
using DatabaseManagement.Contract;
using Datastoring.EfCore;
using EmployeeManagement;
using EmployeeManagement.Contract;
using MailManagement;
using MailManagement.Contract;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using OrganizationManagement;
using OrganizationManagement.Contract;
using planer.Identity;
using GuestyManagement;
using GuestyManagement.Contract;
using TagManagement;
using TagManagement.Contract;
using TeamManagement;
using TeamManagement.Contract;
using TimeTrackingManagement;
using TimeTrackingManagement.Contract;
using UserManagement;
using UserManagement.Contract;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();

// Distinct migrations history tables so both contexts can share a single
// database (e.g. the one MySQL instance provided by Railway).
builder.Services.AddDbContext<PlanerContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("PlanerContext"), ServerVersion.Parse("8.0.34-mysql"),
        mySqlOptions => mySqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Planer")));

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PlanerIdentityContext>();

builder.Services.AddDbContext<PlanerIdentityContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("PlanerIdentityContext"),
        ServerVersion.Parse("8.0.34-mysql"),
        mySqlOptions => mySqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Identity")));

builder.Services.Configure<MailConfiguration>(builder.Configuration.GetSection("MailConfiguration"));

// Persist auth-cookie encryption keys across restarts/deployments when a
// directory is configured (e.g. a mounted volume on the hosting platform);
// otherwise every deployment invalidates all logins.
var dataProtectionKeysDir = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEYS_DIR");
if (!string.IsNullOrWhiteSpace(dataProtectionKeysDir))
{
    Directory.CreateDirectory(dataProtectionKeysDir);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysDir))
        .SetApplicationName("planer");
}

//AutoMapper
var mapperConfig = new MapperConfiguration(cfg => { cfg.AddProfile(new AutoMapperProfile()); });
var mapper = mapperConfig.CreateMapper();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddSingleton(mapper);
//---

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped(typeof(IDatabaseManager<,>), typeof(DatabaseManager<,>));
builder.Services
    .AddScoped<IDatabaseManager<Organization, PlanerIdentityContext>,
        DatabaseManager<PlanerIdentityContext, Organization>>();
builder.Services
    .AddScoped<IDatabaseManager<Team, PlanerIdentityContext>, DatabaseManager<PlanerIdentityContext, Team>>();
builder.Services
    .AddScoped<IDatabaseManager<User, PlanerIdentityContext>, DatabaseManager<PlanerIdentityContext, User>>();
builder.Services
    .AddScoped<IDatabaseManager<TimeTrackingEntry, PlanerIdentityContext>, DatabaseManager<PlanerIdentityContext, TimeTrackingEntry>>();
builder.Services
    .AddScoped<IDatabaseManager<BookingAddition, PlanerIdentityContext>, DatabaseManager<PlanerIdentityContext, BookingAddition>>();
builder.Services
    .AddScoped<IDatabaseManager<Tag, PlanerContext>, DatabaseManager<PlanerContext, Tag>>();
builder.Services
    .AddScoped<IDatabaseManager<Appointment, PlanerContext>, DatabaseManager<PlanerContext, Appointment>>();

builder.Services.AddScoped<HttpClient>(s =>
{
    var navigationManager = s.GetRequiredService<NavigationManager>();
    var baseAddress = navigationManager.BaseUri;
    var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(baseAddress);
    return httpClient;
});

builder.Services.AddScoped<ITagManager, TagManager>();
builder.Services.AddScoped<IBookingAdditionManager, BookingAdditionManager>();
builder.Services.AddTransient<IUserManager, UserManager>();
builder.Services.AddScoped<IMailManager, MailManager>();
builder.Services.AddTransient<IOrganizationManager, OrganizationManager>();
builder.Services.AddTransient<ITeamManager, TeamManager>();
builder.Services.AddTransient<IEmployeeManager, EmployeeManager>();
builder.Services.AddScoped<IGuestyManager, GuestyManager>();
builder.Services.AddScoped<IAppointmentManager, AppointmentManager>();
builder.Services.AddScoped<ITimeTrackingManager, TimeTrackingManager>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddLocalization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dataContext = scope.ServiceProvider.GetRequiredService<PlanerContext>();
        dataContext.Database.Migrate();

        var dataIdentityContext = scope.ServiceProvider.GetRequiredService<PlanerIdentityContext>();
        dataIdentityContext.Database.Migrate();

        var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        await userManager.CreateBasicRolesAsync();

        var mailManager = scope.ServiceProvider.GetRequiredService<IMailManager>();
        await mailManager.EnsureDefaultTemplatesAsync();
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "Database migration or role seeding failed on startup");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Honor X-Forwarded-* headers when running behind a hosting proxy (Railway,
// nginx, ...) so the app knows the original scheme is https.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<LoginMiddleware>();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();