namespace FoodDelivery.API.Services.Images;

public interface IImageStorageService
{
    Task<string> SaveRestaurantImageAsync(int restaurantId, IFormFile file, CancellationToken cancellationToken = default);

    Task<string> SaveMealImageAsync(int restaurantId, int mealId, IFormFile file, CancellationToken cancellationToken = default);

    void DeleteIfStored(string? imageUrl);
}
