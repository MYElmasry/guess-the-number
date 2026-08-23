namespace GuessNumber.Application.DTOs;

public record StartGameResponse(Guid GameId, int Min, int Max);

public record GuessRequest(int Guess);

public record GuessResponse(string Result, int Attempts, bool Completed, int? BestScore = null);

public record HintResponse(string Hint, int HintsUsed);
