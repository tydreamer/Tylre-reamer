using System.Security.Claims;
using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs;
using FoodDelivery.API.Helpers;
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
            .OrderByDescending(u => u.CreatedAt)
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
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (currentRole == "Owner")
        {
            if (user.Role != UserRole.Customer)
                return BadRequest("Owners can only block customers.");

            if (!await CustomerBlockHelper.HasOrderedFromOwnerAsync(db, currentUserId, id))
                return BadRequest("You can only block customers who have ordered from your restaurants.");

            if (await CustomerBlockHelper.IsBlockedFromOwnerAsync(db, currentUserId, id))
                return NoContent();

            db.OwnerCustomerBlocks.Add(new OwnerCustomerBlock
            {
                OwnerId = currentUserId,
                CustomerId = id
            });
            await db.SaveChangesAsync();
            return NoContent();
        }

        user.IsBlocked = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}/unblock")]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Unblock(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        var currentRole = User.FindFirstValue(ClaimTypes.Role);
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (currentRole == "Owner")
        {
            if (user.Role != UserRole.Customer)
                return BadRequest("Owners can only unblock customers.");

            var block = await db.OwnerCustomerBlocks
                .FirstOrDefaultAsync(b => b.OwnerId == currentUserId && b.CustomerId == id);

            if (block is null)
                return NotFound("This customer is not blocked from your restaurants.");

            db.OwnerCustomerBlocks.Remove(block);
            await db.SaveChangesAsync();
            return NoContent();
        }

        user.IsBlocked = false;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
