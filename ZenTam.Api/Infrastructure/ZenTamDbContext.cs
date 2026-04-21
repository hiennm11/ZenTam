using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.Infrastructure;

public class ZenTamDbContext : DbContext
{
    public ZenTamDbContext(DbContextOptions<ZenTamDbContext> options) : base(options) { }

    public DbSet<User>             Users             => Set<User>();
    public DbSet<ActionCatalog>    ActionCatalog     => Set<ActionCatalog>();
    public DbSet<ActionRuleMapping> ActionRuleMappings => Set<ActionRuleMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Gender).IsRequired();
            entity.Property(e => e.SolarDOB).IsRequired();
            entity.Property(e => e.LunarYOB).IsRequired();
        });

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
            entity.Property(e => e.GenderConstraint).IsRequired(false);
            entity.HasOne<ActionCatalog>()
                  .WithMany()
                  .HasForeignKey(e => e.ActionId);
        });
    }
}
