using System.Security.Claims;
using FoodDelivery.API.Data;
using FoodDelivery.API.Constants;
using FoodDelivery.API.Models;
using FoodDelivery.API.Services.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers.Meals;

[ApiController]
[Route("api/restaurants/{restaurantId}/meals")]
public class MealsController(AppDbContext db, IImageStorageService imageStorage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int restaurantId)
    {
        var meals = await db.Meals
            .Where(m => m.RestaurantId == restaurantId)
            .Include(m => m.MealType)
            .OrderBy(m => m.MealType.Name)
            .ThenBy(m => m.Name)
            .ToListAsync();

        return Ok(meals.Select(MealMapper.ToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int restaurantId, int id)
    {
        var meal = await db.Meals
            .Include(m => m.MealType)
            .FirstOrDefaultAsync(m => m.Id == id && m.RestaurantId == restaurantId);

        if (meal is null) return NotFound();
        return Ok(MealMapper.ToResponse(meal));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public async Task<IActionResult> Create(int restaurantId, CreateMealRequest req)
    {
        var ownerId = User.GetUserId();
        var restaurant = await db.Restaurants.FindAsync(restaurantId);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != ownerId) return Forbid();

        if (!await db.MealTypes.AnyAsync(t => t.Id == req.MealTypeId))
            return BadRequest(ValidationMessages.MealTypeInvalid);

        var meal = new Meal
        {
            Name = req.Name,
            Description = req.Description ?? string.Empty,
            ImageUrl = req.ImageUrl ?? string.Empty,
            Price = req.Price,
            MealTypeId = req.MealTypeId,
            RestaurantId = restaurantId
        };
        db.Meals.Add(meal);
        await db.SaveChangesAsync();

        await db.Entry(meal).Reference(m => m.MealType).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { restaurantId, id = meal.Id }, MealMapper.ToResponse(meal));
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int restaurantId, int id, UpdateMealRequest req)
    {
        var ownerId = User.GetUserId();
        var restaurant = await db.Restaurants.FindAsync(restaurantId);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != ownerId) return Forbid();

        if (!await db.MealTypes.AnyAsync(t => t.Id == req.MealTypeId))
            return BadRequest(ValidationMessages.MealTypeInvalid);

        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == id && m.RestaurantId == restaurantId);
        if (meal is null) return NotFound();

        ApplyImageUrlChange(meal, req.ImageUrl);
        meal.Name = req.Name;
        meal.Description = req.Description ?? string.Empty;
        meal.Price = req.Price;
        meal.MealTypeId = req.MealTypeId;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int restaurantId, int id)
    {
        var ownerId = User.GetUserId();
        var restaurant = await db.Restaurants.FindAsync(restaurantId);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != ownerId) return Forbid();

        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == id && m.RestaurantId == restaurantId);
        if (meal is null) return NotFound();

        imageStorage.DeleteIfStored(meal.ImageUrl);
        db.Meals.Remove(meal);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private void ApplyImageUrlChange(Meal meal, string? newImageUrl)
    {
        var next = newImageUrl ?? string.Empty;
        if (!string.Equals(meal.ImageUrl, next, StringComparison.Ordinal))
            imageStorage.DeleteIfStored(meal.ImageUrl);

        meal.ImageUrl = next;
    }
}
