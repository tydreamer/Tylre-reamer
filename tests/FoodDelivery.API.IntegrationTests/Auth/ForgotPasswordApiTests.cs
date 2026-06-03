using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs.Auth;
using FoodDelivery.API.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FoodDelivery.API.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public class ForgotPasswordApiTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task ForgotPassword_ExistingCustomer_CreatesResetToken()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/auth/forgot-password",
            new ForgotPasswordRequest("customer@test.com"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var token = db.PasswordResetTokens.SingleOrDefault(t => t.UserId == IntegrationTestIds.CustomerId);
        token.Should().NotBeNull();
        token!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_ReturnsOkWithoutToken()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/auth/forgot-password",
            new ForgotPasswordRequest("nobody@test.com"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PasswordResetTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPassword_ValidToken_UpdatesPassword()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync(
            "api/auth/forgot-password",
            new ForgotPasswordRequest("customer@test.com"));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var resetToken = db.PasswordResetTokens.Single(t => t.UserId == IntegrationTestIds.CustomerId);

        var resetResponse = await client.PostAsJsonAsync(
            "api/auth/reset-password",
            new ResetPasswordRequest(resetToken.Token, "NewPassword123!"));

        resetResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        db.PasswordResetTokens.Should().BeEmpty();
    }
}
