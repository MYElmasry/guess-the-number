namespace GuessNumber.Application.DTOs;

public record RegisterRequest(string Email, string Password, string ConfirmPassword);

public record LoginRequest(string Email, string Password);

public record UserResponse(Guid Id, string Email, int? BestScore);
