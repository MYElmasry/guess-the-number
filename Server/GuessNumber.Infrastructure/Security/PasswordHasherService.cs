using GuessNumber.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GuessNumber.Infrastructure.Security;

public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string HashPassword(string password) =>
        _hasher.HashPassword(new object(), password);

    public bool VerifyPassword(string password, string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(new object(), passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
