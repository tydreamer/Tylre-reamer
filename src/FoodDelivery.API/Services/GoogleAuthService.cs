using System.Security.Claims;
using FoodDelivery.API.Data;
using FoodDelivery.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Services;

public class GoogleAuthService(AppDbContext db)
{
    public static string UnusablePasswordHash => BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"));

    public async Task<(User? User, string? ErrorCode)> ResolveUserAsync(
        string googleSubjectId,
        string email,
        string name,
        string flow,
        UserRole signupRole)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.GoogleSubjectId == googleSubjectId || u.Email.ToLower() == normalizedEmail);

        if (user is not null)
        {
            if (user.IsBlocked)
                return (null, "blocked");

            if (user.GoogleSubjectId is null)
            {
                if (flow == "signup")
                    return (null, "email_exists");

                user.GoogleSubjectId = googleSubjectId;
                if (string.IsNullOrWhiteSpace(user.Name))
                    user.Name = name;
                await db.SaveChangesAsync();
            }
            else if (!string.Equals(user.GoogleSubjectId, googleSubjectId, StringComparison.Ordinal))
                return (null, "google_failed");

            return (user, null);
        }

        if (flow == "signin")
            return (null, "no_account");

        user = new User
        {
            Name = string.IsNullOrWhiteSpace(name) ? email.Trim() : name.Trim(),
            Email = email.Trim(),
            PasswordHash = UnusablePasswordHash,
            GoogleSubjectId = googleSubjectId,
            Role = signupRole
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user, null);
    }

    public static bool TryParseSignupRole(string? role, out UserRole signupRole)
    {
        signupRole = UserRole.Customer;
        if (string.IsNullOrWhiteSpace(role))
            return true;

        if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out signupRole) || signupRole == UserRole.Admin)
            return false;

        return true;
    }

    public static string? GetClaim(ClaimsPrincipal principal, string type) =>
        principal.FindFirstValue(type);
}
