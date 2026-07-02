using System.Security.Claims;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrganizationManagement.Contract;

namespace Datastoring.EfCore;

public class PlanerIdentityContext : IdentityDbContext<User>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public PlanerIdentityContext()
    {
    }

    public PlanerIdentityContext(DbContextOptions<PlanerIdentityContext> options, IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public virtual DbSet<Organization> Organization { get; set; }

    public virtual DbSet<Team> Teams { get; set; }
    
    public virtual DbSet<BookingAddition> BookingAdditions { get; set; }
    
    public virtual DbSet<TimeTrackingEntry> TimeTrackingEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}