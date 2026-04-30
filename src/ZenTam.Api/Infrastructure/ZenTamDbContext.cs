using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Domain;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.Infrastructure;

public class ZenTamDbContext : DbContext
{
    public ZenTamDbContext(DbContextOptions<ZenTamDbContext> options) : base(options) { }

    public DbSet<ActionCatalog>    ActionCatalog     => Set<ActionCatalog>();
    public DbSet<ActionRuleMapping> ActionRuleMappings => Set<ActionRuleMapping>();
    public DbSet<ClientProfile>    ClientProfiles    => Set<ClientProfile>();
    public DbSet<ClientRelatedPerson> ClientRelatedPersons => Set<ClientRelatedPerson>();
    public DbSet<ConsultationSession> ConsultationSessions => Set<ConsultationSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActionCatalog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<ActionRuleMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ActionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RuleCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsMandatory).IsRequired();
            entity.Property(e => e.GenderScope).IsRequired();
            entity.Property(e => e.Tier).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            entity.HasOne<ActionCatalog>()
                  .WithMany()
                  .HasForeignKey(e => e.ActionId);
        });

        modelBuilder.Entity<ClientProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.SolarDob).IsRequired();
            entity.Property(e => e.Gender).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);
        });

        modelBuilder.Entity<ClientRelatedPerson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Label).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SolarDob).IsRequired();
            entity.Property(e => e.Gender).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasOne(e => e.ClientProfile)
                  .WithMany(c => c.RelatedPersons)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConsultationSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RawMessage).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasOne(e => e.ClientProfile)
                  .WithMany()
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
