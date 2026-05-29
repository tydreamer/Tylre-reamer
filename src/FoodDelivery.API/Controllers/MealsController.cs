using System.Security.Claims;
using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs;
using FoodDelivery.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId}/meals")]
public class MealsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int restaurantId)
    {
        var meals = await db.Meals
            .Where(m => m.RestaurantId == restaurantId)
            .Select(m => new MealResponse(m.Id, m.Name, m.Description, m.ImageUrl, m.Price, m.IsAvailable, m.RestaurantId))
            .ToListAsync();
        return Ok(meals);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int restaurantId, int id)
    {
        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == id && m.RestaurantId == restaurantId);
        if (meal is null) return NotFound();
        return Ok(new MealResponse(meal.Id, meal.Name, meal.Description, meal.ImageUrl, meal.Price, meal.IsAvailable, meal.RestaurantId));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public async Task<IActionResult> Create(int restaurantId, CreateMealRequest req)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var restaurant = await db.Restaurants.FindAsync(restaurantId);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != ownerId) return Forbid();

        var meal = new Meal
        {
            Name = req.Name,
            Description = req.Description,
            ImageUrl = req.ImageUrl,
            Price = req.Price,
            RestaurantId = restaurantId
        };
        db.Meals.Add(meal);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { restaurantId, id = meal.Id },
            new MealResponse(meal.Id, meal.Name, meal.Description, meal.ImageUrl, meal.Price, meal.IsAvailable, meal.RestaurantId));
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int restaurantId, int id, UpdateMealRequest req)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var restaurant = await db.Restaurants.FindAsync(restaurantId);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != ownerId) return Forbid();

        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == id && m.RestaurantId == restaurantId);
        if (meal is null) return NotFound();

        meal.Name = req.Name;
        meal.Description = req.Description;
        meal.ImageUrl = req.ImageUrl;
        meal.Price = req.Price;
        meal.IsAvailable = req.IsAvailable;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int restaurantId, int id)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var restaurant = await db.Restaurants.FindAsync(restaurantId);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != ownerId) return Forbid();

        var meal = await db.Meals.FirstOrDefaultAsync(m => m.Id == id && m.RestaurantId == restaurantId);
        if (meal is null) return NotFound();

        db.Meals.Remove(meal);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
