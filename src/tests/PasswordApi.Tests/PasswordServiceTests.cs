using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PasswordApi.Contracts;
using PasswordApi.Data;
using PasswordApi.Services;

namespace PasswordApi.Tests;

public sealed class PasswordServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly FakePasswordGenerator _generator = new();

    public PasswordServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.Migrate();
    }

    public void Dispose() => _connection.Dispose();

    private AppDbContext CreateContext(params IInterceptor[] interceptors) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(interceptors)
            .Options);

    [Fact]
    public async Task CreateAsync_ReturnsGeneratedPasswordAndNonEmptyId()
    {
        await using var context = CreateContext();
        var service = new PasswordService(context, _generator);

        var result = await service.CreateAsync();

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(_generator.LastPassword, result.Password);
    }

    [Fact]
    public async Task CreateAsync_PersistsEntryWithUtcDate()
    {
        var before = DateTime.UtcNow;

        PasswordResponse result;
        await using (var context = CreateContext())
        {
            result = await new PasswordService(context, _generator).CreateAsync();
        }

        var after = DateTime.UtcNow;

        await using (var context = CreateContext())
        {
            var stored = await context.Passwords.SingleAsync(p => p.Id == result.Id);

            Assert.Equal(result.Password, stored.Value);
            Assert.Equal(DateTimeKind.Utc, stored.Date.Kind);
            Assert.InRange(stored.Date, before, after);
        }
    }

    [Fact]
    public async Task CreateAsync_GeneratesNewIdAndPasswordOnEveryCall()
    {
        await using var context = CreateContext();
        var service = new PasswordService(context, _generator);

        var results = new List<PasswordResponse>();
        for (var i = 0; i < 100; i++)
        {
            results.Add(await service.CreateAsync());
        }

        Assert.Equal(100, results.Select(r => r.Id).Distinct().Count());
        Assert.Equal(100, results.Select(r => r.Password).Distinct().Count());
        Assert.Equal(100, await context.Passwords.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_WhenSaveFails_ThrowsAndPersistsNothing()
    {
        await using (var context = CreateContext(new FailingSaveChangesInterceptor()))
        {
            var service = new PasswordService(context, _generator);

            await Assert.ThrowsAsync<DbUpdateException>(() => service.CreateAsync());
        }

        await using (var context = CreateContext())
        {
            Assert.Equal(0, await context.Passwords.CountAsync());
        }
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsStoredPassword()
    {
        PasswordResponse created;
        await using (var context = CreateContext())
        {
            created = await new PasswordService(context, _generator).CreateAsync();
        }

        await using (var context = CreateContext())
        {
            var result = await new PasswordService(context, _generator).GetByIdAsync(created.Id);

            Assert.Equal(created, result);
        }
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        await using var context = CreateContext();
        var service = new PasswordService(context, _generator);
        await service.CreateAsync();

        Assert.Null(await service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsNull()
    {
        await using var context = CreateContext();
        var service = new PasswordService(context, _generator);
        await service.CreateAsync();

        Assert.Null(await service.GetByIdAsync(Guid.Empty));
    }

    private sealed class FakePasswordGenerator : IPasswordGenerator
    {
        private int _counter;

        public string? LastPassword { get; private set; }

        public string Generate()
        {
            LastPassword = $"Password#{++_counter:D8}";
            return LastPassword;
        }
    }

    private sealed class FailingSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default) =>
            throw new DbUpdateException("Simulated save failure.");
    }
}
