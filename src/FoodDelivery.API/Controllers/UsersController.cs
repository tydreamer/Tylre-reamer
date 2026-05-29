using System.Security.Claims;
using FoodDelivery.API.Data;
using FoodDelivery.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Owner")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpPut("{id}/block")]
    public async Task<IActionResult> Block(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (user.Role != UserRole.Customer) return BadRequest("Only customers can be blocked.");

        user.IsBlocked = true;
        await db.SaveChangesAsync();

        return NoContent();
    }
}
