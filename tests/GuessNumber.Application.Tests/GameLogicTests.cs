using GuessNumber.Application.Exceptions;
using GuessNumber.Application.Services;
using GuessNumber.Domain.Constants;
using GuessNumber.Domain.Enums;

namespace GuessNumber.Application.Tests;

public class GameLogicTests
{
    [Theory]
    [InlineData(10, 20, GuessResult.Higher)]
    [InlineData(30, 20, GuessResult.Lower)]
    [InlineData(20, 20, GuessResult.Correct)]
    public void EvaluateGuess_ReturnsExpectedResult(int guess, int secret, GuessResult expected)
    {
        var result = GameLogic.EvaluateGuess(guess, secret);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(44)]
    [InlineData(-1)]
    public void ValidateGuessRange_RejectsInvalidGuesses(int guess)
    {
        Assert.Throws<InvalidOperationException>(() => GameLogic.ValidateGuessRange(guess));
    }

    [Theory]
    [InlineData(null, 5, 5)]
    [InlineData(7, 5, 5)]
    [InlineData(4, 6, 4)]
    public void UpdateBestScore_UpdatesOnlyWhenLower(int? current, int attempts, int? expected)
    {
        var result = GameLogic.UpdateBestScore(current, attempts);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void UpdateBestScore_DoesNotUpdateWhenHigher()
    {
        var result = GameLogic.UpdateBestScore(4, 6);
        Assert.Equal(4, result);
    }
}
