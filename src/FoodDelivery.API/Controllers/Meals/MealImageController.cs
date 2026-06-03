using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs.Images;
using FoodDelivery.API.Helpers.Images;
using FoodDelivery.API.Options;
using FoodDelivery.API.Services.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FoodDelivery.API.Controllers.Meals;

[ApiController]
[Authorize(Roles = "Owner")]
[Route("api/restaurants/{restaurantId:int}/meals/{mealId:int}/image")]
public class MealImageController(
    AppDbContext db,
    IImageStorageService imageStorage,
    IOptions<ImageUploadOptions> uploadOptions) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(int restaurantId, int mealId, IFormFile file)
    {
        var options = uploadOptions.Value;
        if (!ImageFileValidator.TryValidate(file, options, out var validationError))
            return BadRequest(validationError);

        var (meal, error) = await OwnerRestaurantAccess.RequireOwnedMealAsync(db, User, restaurantId, mealId);
        if (error is not null)
            return error;

        imageStorage.DeleteIfStored(meal!.ImageUrl);
        meal.ImageUrl = await imageStorage.SaveMealImageAsync(restaurantId, mealId, file);
        await db.SaveChangesAsync();

        return Ok(new ImageUploadResponse(meal.ImageUrl));
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(int restaurantId, int mealId)
    {
        var (meal, error) = await OwnerRestaurantAccess.RequireOwnedMealAsync(db, User, restaurantId, mealId);
        if (error is not null)
            return error;

        imageStorage.DeleteIfStored(meal!.ImageUrl);
        meal.ImageUrl = string.Empty;
        await db.SaveChangesAsync();

        return NoContent();
    }
}
