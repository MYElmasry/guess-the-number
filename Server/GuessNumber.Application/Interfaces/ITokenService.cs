using GuessNumber.Domain.Entities;

namespace GuessNumber.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
