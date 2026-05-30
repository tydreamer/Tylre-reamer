using System.Security.Claims;
using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs;
using FoodDelivery.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = db.Users.Where(u => u.Role != UserRole.Admin);
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserResponse(u.Id, u.Name, u.Email, u.Role.ToString(), u.IsBlocked))
            .ToListAsync();

        return Ok(new PagedResult<UserResponse>(items, totalCount, page, pageSize));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateUserRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict("Email already in use.");

        if (!Enum.TryParse<UserRole>(req.Role, ignoreCase: true, out var role) || role == UserRole.Admin)
            return BadRequest("Invalid role. Use 'Customer' or 'Owner'.");

        var user = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = role
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new UserResponse(user.Id, user.Name, user.Email, user.Role.ToString(), user.IsBlocked));
    }

    [HttpPut("{id}/block")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Block(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (user.Role == UserRole.Admin)
            return BadRequest("Admins cannot be blocked.");

        var currentRole = User.FindFirstValue(ClaimTypes.Role);
        if (currentRole == "Owner" && user.Role != UserRole.Customer)
            return BadRequest("Owners can only block customers.");

        user.IsBlocked = true;
        await db.SaveChangesAsync();

        return NoContent();
    }
}
