using Microsoft.EntityFrameworkCore;

namespace PasswordApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public const int PasswordMaxLength = 128;

    public DbSet<PasswordEntry> Passwords => Set<PasswordEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordEntry>(entity =>
        {
            entity.ToTable("Passwords");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Value).IsRequired().HasMaxLength(PasswordMaxLength);
            entity.Property(e => e.Date)
                .IsRequired()
                .HasConversion(
                    value => value.ToUniversalTime(),
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        });
    }
}
