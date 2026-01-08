using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Budget.Api.Auth;

/// <summary>
/// Development-only authentication handler for local testing.
/// </summary>
public class DevAuthHandler : AuthenticationHandler<DevAuthOptions>
{
    public DevAuthHandler(
        IOptionsMonitor<DevAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Options.DevUserId),
            new Claim(ClaimTypes.Name, Options.DevUserName),
            new Claim(ClaimTypes.Email, Options.DevUserEmail),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class DevAuthOptions : AuthenticationSchemeOptions
{
    public string DevUserId { get; set; } = "dev-user";
    public string DevUserName { get; set; } = "Developer";
    public string DevUserEmail { get; set; } = "dev@localhost";
}

