using System.Security.Claims;
using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs;
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
    public async Task<IActionResult> GetAll()
    {
        var restaurants = await db.Restaurants
            .Select(r => new RestaurantResponse(r.Id, r.Name, r.Description, r.ImageUrl, r.OwnerId))
            .ToListAsync();
        return Ok(restaurants);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var restaurant = await db.Restaurants.FindAsync(id);
        if (restaurant is null) return NotFound();
        return Ok(new RestaurantResponse(restaurant.Id, restaurant.Name, restaurant.Description, restaurant.ImageUrl, restaurant.OwnerId));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateRestaurantRequest req)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var restaurant = new Restaurant
        {
            Name = req.Name,
            Description = req.Description,
            ImageUrl = req.ImageUrl,
            OwnerId = ownerId
        };
        db.Restaurants.Add(restaurant);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = restaurant.Id },
            new RestaurantResponse(restaurant.Id, restaurant.Name, restaurant.Description, restaurant.ImageUrl, restaurant.OwnerId));
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateRestaurantRequest req)
    {
        var ownerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var restaurant = await db.Restaurants.FindAsync(id);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != ownerId) return Forbid();

        restaurant.Name = req.Name;
        restaurant.Description = req.Description;
        restaurant.ImageUrl = req.ImageUrl;
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
