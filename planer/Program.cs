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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using OrganizationManagement;
using OrganizationManagement.Contract;
using planer.Identity;
using SmoobuManagement;
using SmoobuManagement.Contract;
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

builder.Services.AddDbContext<PlanerContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("PlanerContext"), ServerVersion.Parse("8.0.34-mysql")));

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PlanerIdentityContext>();

builder.Services.AddDbContext<PlanerIdentityContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("PlanerIdentityContext"),
        ServerVersion.Parse("8.0.34-mysql")));

builder.Services.Configure<MailConfiguration>(builder.Configuration.GetSection("MailConfiguration"));

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
builder.Services.AddScoped<ISmoobuManager, SmoobuManager>();
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
    try
    {
        var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        await userManager.CreateBasicRolesAsync();

        var dataContext = scope.ServiceProvider.GetRequiredService<PlanerContext>();
        dataContext.Database.Migrate();

        var dataIdentityContext = scope.ServiceProvider.GetRequiredService<PlanerIdentityContext>();
        dataIdentityContext.Database.Migrate();
    }
    catch
    {
        //ignore
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<LoginMiddleware>();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();