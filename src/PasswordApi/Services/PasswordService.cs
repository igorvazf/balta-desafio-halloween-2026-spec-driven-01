using Microsoft.EntityFrameworkCore;
using PasswordApi.Contracts;
using PasswordApi.Data;

namespace PasswordApi.Services;

public class PasswordService(AppDbContext dbContext, IPasswordGenerator passwordGenerator) : IPasswordService
{
    public async Task<PasswordResponse> CreateAsync(CancellationToken cancellationToken = default)
    {
        var entry = new PasswordEntry
        {
            Id = Guid.NewGuid(),
            Value = passwordGenerator.Generate(),
            Date = DateTime.UtcNow
        };

        dbContext.Passwords.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PasswordResponse(entry.Id, entry.Value);
    }

    public async Task<PasswordResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Passwords
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PasswordResponse(p.Id, p.Value))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
