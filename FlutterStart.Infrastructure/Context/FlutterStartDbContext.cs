using FlutterStart.Entities;
using FlutterStart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlutterStart.Infrastructure.Context;

public class FlutterStartDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Movie> Movies { get; set; } 

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

        modelBuilder.Entity<Book>(b =>
        {
            b.ToTable("Books");
            b.HasKey(x => x.Id);
            b.Property(x => x.ISBN);
            b.Property(x => x.Title).IsRequired();
            b.Property(x => x.Summary);
            b.Property(x => x.Genre);
            b.Property(x => x.Author).IsRequired();
            b.Property(x => x.PublicationYear).IsRequired();
            b.Property(x => x.PageCount).IsRequired();
            b.Property(x => x.Publisher);
            b.Property(x => x.Edition);
            b.Property(x => x.IsRented).HasDefaultValue(false);
            b.Property(x => x.ImageUrl);
            b.Property(x => x.Language);
            b.Property(x => x.Format);
            b.Property(x => x.Dimensions);
            b.Property(x => x.Location);
            b.HasMany(x => x.Loans)
                .WithOne(x => x.Book)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Loan>(b =>
        {
            b.ToTable("Loans");
            b.HasKey(x => x.Id);
            b.Property(x => x.LoanDate).IsRequired();
            b.Property(x => x.DueDate).IsRequired();
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.Observations);
            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Book)
                .WithMany(x => x.Loans)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Movie>(b =>
        {
            b.ToTable("Movies");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired();
            b.Property(x => x.Director).IsRequired();
            b.Property(x => x.Year).IsRequired();
            b.Property(x => x.Language).IsRequired();
            b.Property(x => x.DurationMinutes).IsRequired();
            b.Property(x => x.Genre).IsRequired();
            b.Property(x => x.Director).IsRequired();
            b.Property(x => x.Cast).IsRequired();
            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.PosterUrl).IsRequired();
            b.Property(x => x.TrailerUrl).IsRequired();
        });
    }
}