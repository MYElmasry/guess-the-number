using GuessNumber.Application.DTOs;

namespace GuessNumber.Application.Interfaces;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
