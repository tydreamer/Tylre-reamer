using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using FoodDelivery.API.Models;
using FoodDelivery.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FoodDelivery.API.UnitTests.Services;

public class JwtTokenServiceTests
{
    private const string Key = "test-secret-key-at-least-32-characters-long!!";
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";

    private static JwtTokenService CreateService(int expiresInMinutes = 60)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = Key,
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:ExpiresInMinutes"] = expiresInMinutes.ToString()
            })
            .Build();

        return new JwtTokenService(config);
    }

    private static User MakeUser() => new()
    {
        Id = 42,
        Name = "Jane Owner",
        Email = "jane@test.com",
        PasswordHash = "hash",
        Role = UserRole.Owner
    };

    private static ClaimsPrincipal Validate(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))
        }, out _);
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyToken()
    {
        var token = CreateService().GenerateToken(MakeUser());

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ProducesTokenThatPassesValidation()
    {
        var token = CreateService().GenerateToken(MakeUser());

        // Validate throws if signature/issuer/audience/lifetime are wrong.
        var act = () => Validate(token);

        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateToken_IncludesUserId()
    {
        var token = CreateService().GenerateToken(MakeUser());

        var principal = Validate(token);

        principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("42");
    }

    [Fact]
    public void GenerateToken_IncludesRoleClaim()
    {
        var token = CreateService().GenerateToken(MakeUser());

        var principal = Validate(token);

        principal.FindFirst(ClaimTypes.Role)!.Value.Should().Be("Owner");
    }

    [Fact]
    public void GenerateToken_IncludesEmailClaim()
    {
        var token = CreateService().GenerateToken(MakeUser());

        var principal = Validate(token);

        principal.FindFirst(ClaimTypes.Email)!.Value.Should().Be("jane@test.com");
    }

    [Fact]
    public void GenerateToken_DifferentUsers_ProduceDifferentTokens()
    {
        var service = CreateService();
        var token1 = service.GenerateToken(MakeUser());
        var token2 = service.GenerateToken(new User
        {
            Id = 99,
            Name = "Other",
            Email = "other@test.com",
            PasswordHash = "hash",
            Role = UserRole.Customer
        });

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateToken_SignedWithConfiguredKey_FailsValidationUnderWrongKey()
    {
        var token = CreateService().GenerateToken(MakeUser());
        var handler = new JwtSecurityTokenHandler();

        var act = () => handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("a-completely-different-key-that-is-32-chars!"))
        }, out _);

        act.Should().Throw<SecurityTokenException>();
    }

    [Fact]
    public void GenerateToken_ExpiryReflectsConfiguredMinutes()
    {
        var token = CreateService(expiresInMinutes: 120).GenerateToken(MakeUser());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Allow a couple minutes of slack around the ~120-minute window.
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow.AddMinutes(118));
        jwt.ValidTo.Should().BeBefore(DateTime.UtcNow.AddMinutes(122));
    }
}
