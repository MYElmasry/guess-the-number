using GuessNumber.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GuessNumber.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.SecretNumber).IsRequired();
            entity.Property(g => g.CreatedAt).IsRequired();
            entity.HasOne(g => g.User)
                .WithMany(u => u.Games)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
