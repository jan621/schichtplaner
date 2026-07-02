using System.Security.Claims;
using System.Text.Json;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;

namespace Datastoring.EfCore;

public partial class PlanerContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public PlanerContext()
    {
    }

    public PlanerContext(DbContextOptions<PlanerContext> options, IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public virtual DbSet<MailTemplate> MailTemplates { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }
    
    public virtual DbSet<Appointment> Appointments { get; set; }

    //public virtual DbSet<Team> Teams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>().Property(p => p.AccommodationIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<List<string>>(v,  JsonSerializerOptions.Default)!);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        var modifiedEntries = ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Added
                        || x.State == EntityState.Modified
                        || x.State == EntityState.Deleted);

        var userId = _httpContextAccessor?.HttpContext?.User?.Claims.SingleOrDefault(
            c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        //If nothing is modified
        // ReSharper disable once PossibleMultipleEnumeration
        if (!modifiedEntries.Any())
        {
            throw new Exception("No changes were made.");
        }

        foreach (var entry in modifiedEntries)
        {
            var entity = entry.Entity as AuditableEntity;
            if (entity == null)
                continue;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedBy = (userId == null ? "-1" : userId) ?? throw new InvalidOperationException();
                entity.CreatedDate = DateTime.UtcNow;
            }

            if (entity.CreatedBy != "-1")
            {
                if (entity.CreatedBy != userId)
                    throw new UnauthorizedAccessException();
            }

            entity.UpdatedBy = (userId == null ? "-1" : userId) ?? throw new InvalidOperationException();
            entity.UpdatedDate = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}