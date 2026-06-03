using FoodDelivery.API.Data;
using FoodDelivery.API.Helpers.Users;
using FoodDelivery.API.Models;
using FoodDelivery.API.Services.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AppDbContext db,
    IConfiguration config,
    IWebHostEnvironment env,
    ILogger<AuthController> logger,
    JwtTokenService jwtTokens,
    IEmailSender emailSender) : ControllerBase
{
    private const string ForgotPasswordMessage =
        "If an account exists for this email, you will receive password reset instructions. Check your inbox and spam folder.";
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict("Email already in use.");

        if (!Enum.TryParse<UserRole>(req.Role, ignoreCase: true, out var role) || role == UserRole.Admin)
            return BadRequest("Invalid role. Use 'Customer' or 'Owner'.");

        var user = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = role
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new { user.Id, user.Name, user.Email, Role = user.Role.ToString() });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null)
            return Unauthorized("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            if (user.GoogleSubjectId is not null)
                return Unauthorized("Invalid credentials. Try signing in with Google.");

            return Unauthorized("Invalid credentials.");
        }

        if (user.IsBlocked)
            return Forbid();

        var token = jwtTokens.GenerateToken(user);
        return Ok(new LoginResponse(token, user.Name, user.Email, user.Role.ToString()));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        var normalizedEmail = UserEmailNormalizer.Normalize(req.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        string? deliveryNote = null;

        if (user is not null && !user.IsBlocked && user.GoogleSubjectId is null)
        {
            var existing = await db.PasswordResetTokens.Where(t => t.UserId == user.Id).ToListAsync();
            db.PasswordResetTokens.RemoveRange(existing);

            var token = Guid.NewGuid().ToString("N");
            db.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
            await db.SaveChangesAsync();

            var webBaseUrl = config["App:WebBaseUrl"] ?? "http://localhost:5196";
            var resetUrl = $"{webBaseUrl.TrimEnd('/')}/reset-password?token={token}";

            try
            {
                await emailSender.SendPasswordResetAsync(user.Email, user.Name, resetUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
                if (env.IsDevelopment())
                    deliveryNote =
                        "SendGrid failed to send the reset email. Verify ApiKey and a verified sender/domain in appsettings.";
            }
        }

        return Ok(new ForgotPasswordResponse(ForgotPasswordMessage, deliveryNote));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
    {
        var reset = await db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == req.Token);

        if (reset is null || reset.ExpiresAt < DateTime.UtcNow)
            return BadRequest("Invalid or expired reset link.");

        if (reset.User.IsBlocked)
            return BadRequest("This account cannot reset its password.");

        reset.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

        var allTokens = await db.PasswordResetTokens.Where(t => t.UserId == reset.UserId).ToListAsync();
        db.PasswordResetTokens.RemoveRange(allTokens);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
