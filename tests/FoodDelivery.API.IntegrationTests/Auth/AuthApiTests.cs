using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.API.IntegrationTests.Infrastructure;

namespace FoodDelivery.API.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public class AuthApiTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task Register_ValidRequest_ReturnsOkWithEmailAndRole()
    {
        var client = factory.CreateClient();
        var email = $"new_{Guid.NewGuid():N}@test.com";

        var response = await client.PostAsJsonAsync("api/auth/register", new
        {
            Name = "New User",
            Email = email,
            Password = "Secret1!",
            Role = "Customer"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        body.GetProperty("email").GetString().Should().Be(email);
        body.GetProperty("role").GetString().Should().Be("Customer");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("api/auth/register", new
        {
            Name = "Duplicate",
            Email = "customer@test.com", // already seeded
            Password = "Secret1!",
            Role = "Customer"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_AdminRole_ReturnsBadRequest()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("api/auth/register", new
        {
            Name = "Hacker",
            Email = $"admin_{Guid.NewGuid():N}@test.com",
            Password = "Secret1!",
            Role = "Admin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_AfterRegister_ReturnsTokenAndRole()
    {
        var client = factory.CreateClient();
        var email = $"login_{Guid.NewGuid():N}@test.com";
        const string password = "Secret1!";

        await client.PostAsJsonAsync("api/auth/register", new
        {
            Name = "Login Test",
            Email = email,
            Password = password,
            Role = "Owner"
        });

        var response = await client.PostAsJsonAsync("api/auth/login", new
        {
            Email = email,
            Password = password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.ReadJsonAsync<LoginResponse>();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.Role.Should().Be("Owner");
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var email = $"wrongpw_{Guid.NewGuid():N}@test.com";

        await client.PostAsJsonAsync("api/auth/register", new
        {
            Name = "Test",
            Email = email,
            Password = "Correct1!",
            Role = "Customer"
        });

        var response = await client.PostAsJsonAsync("api/auth/login", new
        {
            Email = email,
            Password = "WrongPassword1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("api/auth/login", new
        {
            Email = "nobody@nowhere.com",
            Password = "anything"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
