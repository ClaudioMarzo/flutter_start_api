using FlutterStart.Entities;
using FlutterStart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlutterStart.Infrastructure.Context;

public class FlutterStartDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public FlutterStartDbContext(DbContextOptions<FlutterStartDbContext> options) : base(options) { }

    public override int SaveChanges()
    {
        AddTimestamps();
        return base.SaveChanges();
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    public void BaseEntityTimeStamp()
    {
        AddTimestamps();
    }
    private void AddTimestamps()
    {
        var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                ((BaseEntity)entry.Entity).CreatedAt = DateTime.UtcNow;
            }
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users");
            b.HasKey(x => x.Id);

            b.Property(x => x.Nome)
            .IsRequired();

            b.Property(x => x.Email)
            .IsRequired();

            b.Property(x => x.Senha)
            .IsRequired();

            b.Property(x => x.Role)
            .HasConversion<string>()
            .HasDefaultValue("USER")
            .IsRequired();
        });
    }
}