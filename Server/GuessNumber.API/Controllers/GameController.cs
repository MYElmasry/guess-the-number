using System.Security.Claims;
using GuessNumber.Application.DTOs;
using GuessNumber.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuessNumber.API.Controllers;

[ApiController]
[Route("api/game")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartGameResponse>> Start(CancellationToken cancellationToken)
    {
        var response = await _gameService.StartGameAsync(GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpPost("{gameId:guid}/guess")]
    public async Task<ActionResult<GuessResponse>> Guess(Guid gameId, [FromBody] GuessRequest request, CancellationToken cancellationToken)
    {
        var response = await _gameService.MakeGuessAsync(GetUserId(), gameId, request.Guess, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{gameId:guid}/hint")]
    public async Task<ActionResult<HintResponse>> Hint(Guid gameId, CancellationToken cancellationToken)
    {
        var response = await _gameService.GetHintAsync(GetUserId(), gameId, cancellationToken);
        return Ok(response);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? throw new UnauthorizedAccessException();

        return Guid.Parse(userIdClaim);
    }
}
