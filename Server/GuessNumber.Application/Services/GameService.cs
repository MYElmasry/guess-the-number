using System.Security.Cryptography;
using GuessNumber.Application.DTOs;
using GuessNumber.Application.Exceptions;
using GuessNumber.Application.Interfaces;
using GuessNumber.Domain.Constants;
using GuessNumber.Domain.Entities;
using GuessNumber.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GuessNumber.Application.Services;

public class GameService : IGameService
{
    private readonly DbContext _dbContext;

    public GameService(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StartGameResponse> StartGameAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureUserExistsAsync(userId, cancellationToken);

        var game = new Game
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretNumber = RandomNumberGenerator.GetInt32(GameConstants.MinNumber, GameConstants.MaxNumber + 1),
            AttemptCount = 0,
            IsCompleted = false,
            HintsUsed = 0,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Set<Game>().Add(game);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new StartGameResponse(game.Id, GameConstants.MinNumber, GameConstants.MaxNumber);
    }

    public async Task<GuessResponse> MakeGuessAsync(Guid userId, Guid gameId, int guess, CancellationToken cancellationToken = default)
    {
        try
        {
            GameLogic.ValidateGuessRange(guess);
        }
        catch (InvalidOperationException ex)
        {
            throw new AppException(ex.Message);
        }

        var game = await GetOwnedGameAsync(userId, gameId, cancellationToken);

        if (game.IsCompleted)
        {
            throw new AppException("This game has already been completed.");
        }

        game.AttemptCount++;
        var result = GameLogic.EvaluateGuess(guess, game.SecretNumber);

        if (result == GuessResult.Correct)
        {
            game.IsCompleted = true;
            var user = await _dbContext.Set<User>()
                .FirstAsync(u => u.Id == userId, cancellationToken);

            user.BestScore = GameLogic.UpdateBestScore(user.BestScore, game.AttemptCount);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new GuessResponse("correct", game.AttemptCount, true, user.BestScore);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new GuessResponse(result == GuessResult.Higher ? "higher" : "lower", game.AttemptCount, false);
    }

    public async Task<HintResponse> GetHintAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        var game = await GetOwnedGameAsync(userId, gameId, cancellationToken);

        if (game.IsCompleted)
        {
            throw new AppException("Hints are not available for completed games.");
        }

        if (game.AttemptCount < GameConstants.HintThresholdAttempts)
        {
            throw new AppException($"Hints are available after {GameConstants.HintThresholdAttempts} failed attempts.");
        }

        var hint = GameLogic.GenerateHint(game.SecretNumber, game.HintsUsed);
        game.HintsUsed++;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new HintResponse(hint, game.HintsUsed);
    }

    private async Task EnsureUserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Set<User>().AnyAsync(u => u.Id == userId, cancellationToken);
        if (!exists)
        {
            throw new UnauthorizedAppException("User is not authenticated.");
        }
    }

    private async Task<Game> GetOwnedGameAsync(Guid userId, Guid gameId, CancellationToken cancellationToken)
    {
        var game = await _dbContext.Set<Game>()
            .FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken)
            ?? throw new NotFoundException("Game not found.");

        if (game.UserId != userId)
        {
            throw new ForbiddenException("You do not have access to this game.");
        }

        return game;
    }
}
