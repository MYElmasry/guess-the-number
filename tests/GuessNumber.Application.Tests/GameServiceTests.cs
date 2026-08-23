using GuessNumber.Application.Exceptions;
using GuessNumber.Application.Services;
using GuessNumber.Domain.Entities;
using GuessNumber.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GuessNumber.Application.Tests;

public class GameServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly GameService _gameService;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    public GameServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _gameService = new GameService(_dbContext);

        _dbContext.Users.AddRange(
            new User { Id = _userId, Email = "player@test.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow },
            new User { Id = _otherUserId, Email = "other@test.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow });
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task MakeGuess_LowerThanSecret_ReturnsHigher()
    {
        var game = CreateGame(secretNumber: 25);
        var response = await _gameService.MakeGuessAsync(_userId, game.Id, 10);

        Assert.Equal("higher", response.Result);
        Assert.False(response.Completed);
    }

    [Fact]
    public async Task MakeGuess_HigherThanSecret_ReturnsLower()
    {
        var game = CreateGame(secretNumber: 25);
        var response = await _gameService.MakeGuessAsync(_userId, game.Id, 40);

        Assert.Equal("lower", response.Result);
        Assert.False(response.Completed);
    }

    [Fact]
    public async Task MakeGuess_CorrectGuess_CompletesGame()
    {
        var game = CreateGame(secretNumber: 25);
        var response = await _gameService.MakeGuessAsync(_userId, game.Id, 25);

        Assert.Equal("correct", response.Result);
        Assert.True(response.Completed);
        Assert.Equal(1, response.Attempts);
    }

    [Fact]
    public async Task MakeGuess_IncrementsAttemptCount()
    {
        var game = CreateGame(secretNumber: 25);
        await _gameService.MakeGuessAsync(_userId, game.Id, 10);
        var response = await _gameService.MakeGuessAsync(_userId, game.Id, 30);

        Assert.Equal(2, response.Attempts);
    }

    [Fact]
    public async Task MakeGuess_CreatesBestScoreWhenNoneExists()
    {
        var game = CreateGame(secretNumber: 12);
        var response = await _gameService.MakeGuessAsync(_userId, game.Id, 12);

        Assert.Equal(1, response.BestScore);
    }

    [Fact]
    public async Task MakeGuess_UpdatesBestScoreWhenLower()
    {
        var user = await _dbContext.Users.FirstAsync(u => u.Id == _userId);
        user.BestScore = 8;
        await _dbContext.SaveChangesAsync();

        var game = CreateGame(secretNumber: 15);
        await _gameService.MakeGuessAsync(_userId, game.Id, 10);
        await _gameService.MakeGuessAsync(_userId, game.Id, 20);
        var response = await _gameService.MakeGuessAsync(_userId, game.Id, 15);

        Assert.Equal(3, response.BestScore);
    }

    [Fact]
    public async Task MakeGuess_DoesNotUpdateBestScoreWhenHigher()
    {
        var user = await _dbContext.Users.FirstAsync(u => u.Id == _userId);
        user.BestScore = 4;
        await _dbContext.SaveChangesAsync();

        var game = CreateGame(secretNumber: 18);
        for (var i = 0; i < 5; i++)
        {
            await _gameService.MakeGuessAsync(_userId, game.Id, 1);
        }

        var response = await _gameService.MakeGuessAsync(_userId, game.Id, 18);
        Assert.Equal(4, response.BestScore);
    }

    [Fact]
    public async Task MakeGuess_RejectsInvalidGuess()
    {
        var game = CreateGame(secretNumber: 10);
        await Assert.ThrowsAsync<AppException>(() => _gameService.MakeGuessAsync(_userId, game.Id, 100));
    }

    [Fact]
    public async Task MakeGuess_RejectsAccessToAnotherUsersGame()
    {
        var game = CreateGame(secretNumber: 10, userId: _otherUserId);
        await Assert.ThrowsAsync<ForbiddenException>(() => _gameService.MakeGuessAsync(_userId, game.Id, 5));
    }

    private Game CreateGame(int secretNumber, Guid? userId = null)
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? _userId,
            SecretNumber = secretNumber,
            AttemptCount = 0,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Games.Add(game);
        _dbContext.SaveChanges();
        return game;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
