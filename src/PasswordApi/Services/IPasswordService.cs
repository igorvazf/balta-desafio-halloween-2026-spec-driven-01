using PasswordApi.Contracts;

namespace PasswordApi.Services;

public interface IPasswordService
{
    Task<PasswordResponse> CreateAsync(CancellationToken cancellationToken = default);
    Task<PasswordResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
