using FoodDelivery.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers.Common;

[ApiController]
[Route("api/cuisines")]
public class CuisinesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var cuisines = await db.Cuisines
            .OrderBy(c => c.Id)
            .Select(c => new CuisineResponse(c.Id, c.Name))
            .ToListAsync();

        return Ok(cuisines);
    }
}
