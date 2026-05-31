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
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 12;

        var query = db.Meals.AsQueryable();
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(m => m.Restaurant.Name)
            .ThenBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return Ok(new PagedResult<MealBrowseResponse>(items, totalCount, page, pageSize));
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
