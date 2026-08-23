namespace GuessNumber.API.Models;

public record ApiErrorResponse(string Error, Dictionary<string, string[]>? Errors = null);
