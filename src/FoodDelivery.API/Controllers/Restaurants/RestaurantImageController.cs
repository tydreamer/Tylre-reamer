using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs.Images;
using FoodDelivery.API.Helpers.Images;
using FoodDelivery.API.Options;
using FoodDelivery.API.Services.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FoodDelivery.API.Controllers.Restaurants;

[ApiController]
[Authorize(Roles = "Owner")]
[Route("api/restaurants/{restaurantId:int}/image")]
public class RestaurantImageController(
    AppDbContext db,
    IImageStorageService imageStorage,
    IOptions<ImageUploadOptions> uploadOptions) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(int restaurantId, IFormFile file)
    {
        var options = uploadOptions.Value;
        if (!ImageFileValidator.TryValidate(file, options, out var validationError))
            return BadRequest(validationError);

        var (restaurant, error) = await OwnerRestaurantAccess.RequireOwnedRestaurantAsync(db, User, restaurantId);
        if (error is not null)
            return error;

        imageStorage.DeleteIfStored(restaurant!.ImageUrl);
        restaurant.ImageUrl = await imageStorage.SaveRestaurantImageAsync(restaurantId, file);
        await db.SaveChangesAsync();

        return Ok(new ImageUploadResponse(restaurant.ImageUrl));
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(int restaurantId)
    {
        var (restaurant, error) = await OwnerRestaurantAccess.RequireOwnedRestaurantAsync(db, User, restaurantId);
        if (error is not null)
            return error;

        imageStorage.DeleteIfStored(restaurant!.ImageUrl);
        restaurant.ImageUrl = string.Empty;
        await db.SaveChangesAsync();

        return NoContent();
    }
}
