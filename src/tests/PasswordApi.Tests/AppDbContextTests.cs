using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PasswordApi.Data;

namespace PasswordApi.Tests;

public sealed class AppDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContextTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.Migrate();
    }

    public void Dispose() => _connection.Dispose();

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    [Fact]
    public void Migrate_AppliesAllMigrations()
    {
        using var context = CreateContext();

        Assert.NotEmpty(context.Database.GetAppliedMigrations());
        Assert.Empty(context.Database.GetPendingMigrations());
    }

    [Fact]
    public async Task SaveChanges_PersistsEntry_AndReadsDateAsUtc()
    {
        var entry = new PasswordEntry
        {
            Id = Guid.NewGuid(),
            Value = "k#9Vx!2mQpL@7zRt",
            Date = DateTime.UtcNow
        };

        await using (var context = CreateContext())
        {
            context.Passwords.Add(entry);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var stored = await context.Passwords.SingleAsync(p => p.Id == entry.Id);

            Assert.Equal(entry.Value, stored.Value);
            Assert.Equal(entry.Date, stored.Date);
            Assert.Equal(DateTimeKind.Utc, stored.Date.Kind);
        }
    }

    [Fact]
    public async Task SaveChanges_WithoutValue_Throws()
    {
        await using var context = CreateContext();
        context.Passwords.Add(new PasswordEntry { Id = Guid.NewGuid(), Value = null!, Date = DateTime.UtcNow });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public void Model_MapsPasswordEntry_ToPasswordsTable()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(PasswordEntry))!;
        var value = entity.FindProperty(nameof(PasswordEntry.Value))!;

        Assert.Equal("Passwords", entity.GetTableName());
        Assert.Equal(nameof(PasswordEntry.Id), entity.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(AppDbContext.PasswordMaxLength, value.GetMaxLength());
        Assert.False(value.IsNullable);
    }
}
