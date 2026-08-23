using GuessNumber.Domain.Constants;
using GuessNumber.Domain.Enums;

namespace GuessNumber.Application.Services;

public static class GameLogic
{
    public static GuessResult EvaluateGuess(int guess, int secretNumber)
    {
        if (guess < secretNumber)
        {
            return GuessResult.Higher;
        }

        if (guess > secretNumber)
        {
            return GuessResult.Lower;
        }

        return GuessResult.Correct;
    }

    public static void ValidateGuessRange(int guess)
    {
        if (guess < GameConstants.MinNumber || guess > GameConstants.MaxNumber)
        {
            throw new InvalidOperationException($"Guess must be between {GameConstants.MinNumber} and {GameConstants.MaxNumber}.");
        }
    }

    public static int? UpdateBestScore(int? currentBestScore, int attempts)
    {
        if (currentBestScore is null || attempts < currentBestScore)
        {
            return attempts;
        }

        return currentBestScore;
    }

    public static string GenerateHint(int secretNumber, int hintsUsed)
    {
        return hintsUsed switch
        {
            0 when secretNumber % 2 == 0 => "The number is even.",
            0 => "The number is odd.",
            1 => GetRangeHint(secretNumber),
            _ => GetCloserRangeHint(secretNumber)
        };
    }

    private static string GetRangeHint(int secretNumber)
    {
        var lowerBound = secretNumber <= 21 ? GameConstants.MinNumber : 22;
        var upperBound = secretNumber <= 21 ? 21 : GameConstants.MaxNumber;
        return $"The number is between {lowerBound} and {upperBound}.";
    }

    private static string GetCloserRangeHint(int secretNumber)
    {
        var lowerBound = Math.Max(GameConstants.MinNumber, secretNumber - 5);
        var upperBound = Math.Min(GameConstants.MaxNumber, secretNumber + 5);
        return $"The number is between {lowerBound} and {upperBound}.";
    }
}
