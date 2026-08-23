using System.Net.Mail;
using GuessNumber.Application.DTOs;
using GuessNumber.Application.Exceptions;
using GuessNumber.Application.Interfaces;
using GuessNumber.Domain.Constants;
using GuessNumber.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GuessNumber.Application.Services;

public class AuthService : IAuthService
{
    private readonly DbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(DbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRegistration(request);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var users = _dbContext.Set<User>();

        if (await users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapUser(user);
    }

    public async Task<UserResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new AppException("Email and password are required.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        return MapUser(user);
    }

    public async Task<UserResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new UnauthorizedAppException("User is not authenticated.");

        return MapUser(user);
    }

    private static void ValidateRegistration(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new AppException("Email is required.");
        }

        if (!IsValidEmail(request.Email))
        {
            throw new AppException("Email format is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new AppException("Password is required.");
        }

        if (request.Password.Length < GameConstants.MinimumPasswordLength)
        {
            throw new AppException($"Password must be at least {GameConstants.MinimumPasswordLength} characters.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new AppException("Password and confirmation do not match.");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return address.Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static UserResponse MapUser(User user) =>
        new(user.Id, user.Email, user.BestScore);
}
