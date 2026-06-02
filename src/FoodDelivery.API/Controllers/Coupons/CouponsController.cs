using System.Security.Claims;
using FoodDelivery.API.Data;
using FoodDelivery.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers.Coupons;

[ApiController]
[Route("api/restaurants/{restaurantId}/coupons")]
[Authorize(Roles = "Owner")]
public class CouponsController(AppDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(int restaurantId)
    {
        var restaurant = await db.Restaurants.FindAsync(restaurantId);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != CurrentUserId) return Forbid();

        var coupons = await db.Coupons
            .Where(c => c.RestaurantId == restaurantId)
            .Select(c => new CouponResponse(c.Id, c.Code, c.DiscountValue, c.ExpiresAt, c.IsActive))
            .ToListAsync();

        return Ok(coupons);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int restaurantId, CreateCouponRequest req)
    {
        var restaurant = await db.Restaurants.FindAsync(restaurantId);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != CurrentUserId) return Forbid();

        if (await db.Coupons.AnyAsync(c => c.Code == req.Code))
            return Conflict("Coupon code already exists.");

        var coupon = new Coupon
        {
            Code = req.Code,
            DiscountType = DiscountType.Percentage,
            DiscountValue = req.DiscountValue,
            RestaurantId = restaurantId,
            ExpiresAt = req.ExpiresAt
        };

        db.Coupons.Add(coupon);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { restaurantId },
            new CouponResponse(coupon.Id, coupon.Code, coupon.DiscountValue, coupon.ExpiresAt, coupon.IsActive));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int restaurantId, int id)
    {
        var restaurant = await db.Restaurants.FindAsync(restaurantId);
        if (restaurant is null) return NotFound();
        if (restaurant.OwnerId != CurrentUserId) return Forbid();

        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId);
        if (coupon is null) return NotFound();

        db.Coupons.Remove(coupon);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
