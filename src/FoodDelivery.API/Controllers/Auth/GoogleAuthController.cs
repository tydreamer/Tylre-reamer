using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.API.Controllers.Auth;

[ApiController]
[Route("api/auth/google")]
public class GoogleAuthController(
    IConfiguration config,
    GoogleAuthService googleAuth,
    JwtTokenService jwtTokens) : ControllerBase
{
    [HttpGet("signin")]
    [AllowAnonymous]
    public IActionResult SignIn()
    {
        if (!IsGoogleConfigured())
            return BadRequest("Google sign-in is not configured.");

        return ChallengeGoogle("signin", null);
    }

    [HttpGet("signup")]
    [AllowAnonymous]
    public IActionResult SignUp([FromQuery] string role = "Customer")
    {
        if (!IsGoogleConfigured())
            return BadRequest("Google sign-in is not configured.");

        if (!GoogleAuthService.TryParseSignupRole(role, out _))
            return BadRequest("Invalid role. Use 'Customer' or 'Owner'.");

        return ChallengeGoogle("signup", role);
    }

    /// <summary>
    /// Finishes Google sign-in after OAuth middleware handles <c>/api/auth/google/callback</c>.
    /// </summary>
    [HttpGet("complete")]
    [AllowAnonymous]
    public async Task<IActionResult> Complete()
    {
        var webBase = config["App:WebBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5196";

        var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Principal is null)
            return Redirect($"{webBase}/auth/google-callback?error=google_failed");

        var flow = authResult.Properties?.Items.TryGetValue("flow", out var f) == true ? f : "signin";
        var roleParam = authResult.Properties?.Items.TryGetValue("role", out var r) == true ? r : "Customer";

        if (!GoogleAuthService.TryParseSignupRole(roleParam, out var signupRole))
            signupRole = Models.UserRole.Customer;

        var subjectId = GoogleAuthService.GetClaim(authResult.Principal, ClaimTypes.NameIdentifier);
        var email = GoogleAuthService.GetClaim(authResult.Principal, ClaimTypes.Email);
        var name = GoogleAuthService.GetClaim(authResult.Principal, ClaimTypes.Name)
            ?? GoogleAuthService.GetClaim(authResult.Principal, "name");

        if (string.IsNullOrWhiteSpace(subjectId) || string.IsNullOrWhiteSpace(email))
            return Redirect($"{webBase}/auth/google-callback?error=google_failed");

        var (user, errorCode) = await googleAuth.ResolveUserAsync(
            subjectId, email, name ?? email, flow ?? "signin", signupRole);

        if (user is null)
            return Redirect($"{webBase}/auth/google-callback?error={errorCode ?? "google_failed"}");

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var token = jwtTokens.GenerateToken(user);
        var destination = LandingPath(user.Role.ToString());
        var registered = flow == "signup" ? "true" : "false";
        return Redirect($"{webBase}/auth/google-callback?token={Uri.EscapeDataString(token)}&role={Uri.EscapeDataString(user.Role.ToString())}&returnUrl={Uri.EscapeDataString(destination)}&registered={registered}");
    }

    private IActionResult ChallengeGoogle(string flow, string? role)
    {
        var redirectUrl = Url.Action(nameof(Complete), "GoogleAuth", null, Request.Scheme)!;
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        properties.Items["flow"] = flow;
        if (role is not null)
            properties.Items["role"] = role;

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    private bool IsGoogleConfigured()
    {
        var clientId = config["Google:ClientId"];
        var clientSecret = config["Google:ClientSecret"];
        return !string.IsNullOrWhiteSpace(clientId)
            && !string.IsNullOrWhiteSpace(clientSecret)
            && !clientId.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);
    }

    private static string LandingPath(string role) => role switch
    {
        "Admin" => "/admin/users",
        "Owner" => "/owner/restaurants",
        _ => "/restaurants"
    };
}
