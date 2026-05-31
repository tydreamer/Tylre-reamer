using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/meals")]
public class BrowseMealsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var meals = await db.Meals
            .OrderBy(m => m.Restaurant.Name)
            .ThenBy(m => m.Name)
            .Select(m => new MealBrowseResponse(
                m.Id,
                m.Name,
                m.Description,
                m.ImageUrl,
                m.Price,
                m.IsAvailable,
                m.RestaurantId,
                m.Restaurant.Name))
            .ToListAsync();

        return Ok(meals);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var meal = await db.Meals
            .Where(m => m.Id == id)
            .Select(m => new MealBrowseResponse(
                m.Id,
                m.Name,
                m.Description,
                m.ImageUrl,
                m.Price,
                m.IsAvailable,
                m.RestaurantId,
                m.Restaurant.Name))
            .FirstOrDefaultAsync();

        if (meal is null)
            return NotFound();

        return Ok(meal);
    }
}
