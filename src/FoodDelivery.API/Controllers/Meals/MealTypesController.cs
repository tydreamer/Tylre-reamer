using FoodDelivery.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers.Meals;

[ApiController]
[Route("api/meal-types")]
public class MealTypesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var types = await db.MealTypes
            .OrderBy(t => t.Id)
            .Select(t => new MealTypeResponse(t.Id, t.Name))
            .ToListAsync();

        return Ok(types);
    }
}
