using GuessNumber.Application.DTOs;

namespace GuessNumber.Application.Interfaces;

public interface IGameService
{
    Task<StartGameResponse> StartGameAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GuessResponse> MakeGuessAsync(Guid userId, Guid gameId, int guess, CancellationToken cancellationToken = default);
    Task<HintResponse> GetHintAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
}
