using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Users.Application.Auth;
using Users.Application.Auth.Dtos;

namespace Users.Api.Auth;

public sealed class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthService _authService;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAuthService authService)
        : base(options, logger, encoder)
    {
        _authService = authService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Expect: Authorization: Basic base64(username:password)
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var header = authHeaderValues.ToString();
        if (string.IsNullOrWhiteSpace(header) ||
            !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var encoded = header["Basic ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(encoded))
        {
            return AuthenticateResult.Fail("Missing Basic credentials.");
        }

        string decoded;
        try
        {
            var bytes = Convert.FromBase64String(encoded);
            decoded = Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Base64 in Basic credentials.");
        }

        var colonIndex = decoded.IndexOf(':');
        if (colonIndex <= 0)
        {
            return AuthenticateResult.Fail("Invalid Basic credential format. Expected username:password.");
        }

        var userName = decoded[..colonIndex];
        var password = decoded[(colonIndex + 1)..];

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return AuthenticateResult.Fail("Username/password missing.");
        }

        // Reuse your existing DB auth (UserRepository + SHA256 hash check)
        var loginResult = await _authService.LoginAsync(
            new LoginRequest { UserName = userName, Password = password },
            Context.RequestAborted);

        if (!loginResult.Success || loginResult.UserId is null)
        {
            return AuthenticateResult.Fail("Invalid username or password.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, loginResult.UserId.Value.ToString()),
            new(ClaimTypes.Name, loginResult.UserName ?? userName),
        };

        if (!string.IsNullOrWhiteSpace(loginResult.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, loginResult.Role!));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // Make clients aware it’s Basic
        Response.Headers["WWW-Authenticate"] = "Basic realm=\"RESTful API Training\"";
        return base.HandleChallengeAsync(properties);
    }
}
