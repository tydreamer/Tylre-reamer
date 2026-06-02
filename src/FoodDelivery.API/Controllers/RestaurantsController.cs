using System.Security.Claims;
using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs;
using FoodDelivery.API.Helpers;
using FoodDelivery.API.Constants;
using FoodDelivery.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] int? ownerId = null,
        [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 12;

        var query = db.Restaurants
            .Include(r => r.Cuisine)
            .AsQueryable();

        if (ownerId.HasValue)
            query = query.Where(r => r.OwnerId == ownerId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(r =>
                EF.Functions.ILike(r.Name, term) ||
                EF.Functions.ILike(r.Description, term) ||
                EF.Functions.ILike(r.Cuisine.Name, term));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = items.Select(RestaurantMapper.ToResponse).ToList();
        return Ok(new PagedResult<RestaurantResponse>(responses, totalCount, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var restaurant = await db.Restaurants
            .Include(r => r.Cuisine)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant is null) return NotFound();
        return Ok(RestaurantMapper.ToResponse(restaurant));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateRestaurantRequest req)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (!await db.Cuisines.AnyAsync(c => c.Id == req.CuisineId))
            return BadRequest(new { error = ValidationMessages.CuisineInvalid });

        var restaurant = new Restaurant
        {
            Name = req.Name,
            Description = req.Description,
            ImageUrl = req.ImageUrl ?? string.Empty,
            CuisineId = req.CuisineId,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            OwnerId = ownerId
        };
        db.Restaurants.Add(restaurant);
        await db.SaveChangesAsync();

        await db.Entry(restaurant).Reference(r => r.Cuisine).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = restaurant.Id },
            RestaurantMapper.ToResponse(restaurant));
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateRestaurantRequest req)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var restaurant = await db.Restaurants.FindAsync(id);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != ownerId) return Forbid();

        if (!await db.Cuisines.AnyAsync(c => c.Id == req.CuisineId))
            return BadRequest(new { error = ValidationMessages.CuisineInvalid });

        restaurant.Name = req.Name;
        restaurant.Description = req.Description;
        restaurant.ImageUrl = req.ImageUrl ?? string.Empty;
        restaurant.CuisineId = req.CuisineId;
        restaurant.Latitude = req.Latitude;
        restaurant.Longitude = req.Longitude;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var restaurant = await db.Restaurants.FindAsync(id);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != ownerId) return Forbid();

        db.Restaurants.Remove(restaurant);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
