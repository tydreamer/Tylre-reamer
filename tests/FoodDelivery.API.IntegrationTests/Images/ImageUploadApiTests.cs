using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using FoodDelivery.API.IntegrationTests.Infrastructure;

namespace FoodDelivery.API.IntegrationTests.Images;

[Collection(IntegrationTestCollection.Name)]
public class ImageUploadApiTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task UploadRestaurantImage_AsOwner_ReturnsImageUrl()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.OwnerId);
        using var content = CreatePngUpload();

        var response = await client.PostAsync(
            $"api/restaurants/{IntegrationTestIds.RestaurantId}/image",
            content);

        var body = await response.ReadBodyAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, body);
        body.Should().Contain("/uploads/");
    }

    [Fact]
    public async Task UploadRestaurantImage_AsCustomer_ReturnsForbidden()
    {
        var client = factory.CreateClient().AsUser(factory, IntegrationTestIds.CustomerId);
        using var content = CreatePngUpload();

        var response = await client.PostAsync(
            $"api/restaurants/{IntegrationTestIds.RestaurantId}/image",
            content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static MultipartFormDataContent CreatePngUpload()
    {
        var pngHeader = Convert.FromHexString(
            "89504E470D0A1A0A0000000D49484452000000010000000108060000001F15C4890000000A49444154789C6300010000050001");
        var fileContent = new ByteArrayContent(pngHeader);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var content = new MultipartFormDataContent();
        content.Add(fileContent, "file", "test.png");
        return content;
    }
}
