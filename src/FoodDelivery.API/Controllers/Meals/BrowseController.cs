using FoodDelivery.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers.Meals;

[ApiController]
[Route("api/meals")]
public class BrowseMealsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 12;

        var query = db.Meals
            .Include(m => m.Restaurant)
            .Include(m => m.MealType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(m =>
                EF.Functions.ILike(m.Name, term) ||
                EF.Functions.ILike(m.Description, term) ||
                EF.Functions.ILike(m.MealType.Name, term));
        }

        var totalCount = await query.CountAsync();
        var meals = await query
            .OrderBy(m => m.Restaurant.Name)
            .ThenBy(m => m.MealType.Name)
            .ThenBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = meals.Select(MealMapper.ToBrowseResponse).ToList();

        return Ok(new PagedResult<MealBrowseResponse>(items, totalCount, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var meal = await db.Meals
            .Include(m => m.Restaurant)
            .Include(m => m.MealType)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (meal is null)
            return NotFound();

        return Ok(MealMapper.ToBrowseResponse(meal));
    }
}
