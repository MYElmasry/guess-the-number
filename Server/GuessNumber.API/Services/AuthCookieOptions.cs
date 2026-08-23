using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GuessNumber.API.Services;

public class AuthCookieOptions
{
    public const string CookieName = "auth_token";

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public AuthCookieOptions(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public CookieOptions Create()
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
        var crossSite = frontendUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = crossSite || !_environment.IsDevelopment(),
            SameSite = crossSite ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/"
        };
    }
}
