using System.Security.Claims;
using GuessNumber.Application.DTOs;
using GuessNumber.Application.Interfaces;
using GuessNumber.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuessNumber.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string AuthCookieName = "auth_token";
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _authService = authService;
        _tokenService = tokenService;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(user);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _authService.LoginAsync(request, cancellationToken);
        var token = _tokenService.GenerateToken(new User
        {
            Id = user.Id,
            Email = user.Email
        });

        AppendAuthCookie(token);
        return Ok(user);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/"
        });

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        return Ok(user);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? throw new UnauthorizedAccessException();

        return Guid.Parse(userIdClaim);
    }

    private void AppendAuthCookie(string token)
    {
        Response.Cookies.Append(AuthCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/"
        });
    }
}
