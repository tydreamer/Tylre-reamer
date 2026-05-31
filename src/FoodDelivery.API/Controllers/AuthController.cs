using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs;
using FoodDelivery.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AppDbContext db,
    IConfiguration config,
    IWebHostEnvironment env,
    ILogger<AuthController> logger) : ControllerBase
{
    private const string ForgotPasswordMessage =
        "If an account exists for this email, you will receive password reset instructions.";
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
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        if (user.IsBlocked)
            return Forbid();

        var token = GenerateToken(user);
        return Ok(new LoginResponse(token, user.Name, user.Email, user.Role.ToString()));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        string? resetUrl = null;

        if (user is not null && !user.IsBlocked)
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
            resetUrl = $"{webBaseUrl.TrimEnd('/')}/reset-password?token={token}";
            logger.LogInformation("Password reset link for {Email}: {ResetUrl}", user.Email, resetUrl);
        }

        if (env.IsDevelopment())
            return Ok(new ForgotPasswordResponse(ForgotPasswordMessage, resetUrl));

        return Ok(new ForgotPasswordResponse(ForgotPasswordMessage));
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

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(config.GetValue<int>("Jwt:ExpiresInMinutes"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("name", user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
