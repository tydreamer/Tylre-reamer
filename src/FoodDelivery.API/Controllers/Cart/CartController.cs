using System.Security.Claims;
using FoodDelivery.API.Constants;
using FoodDelivery.API.Data;
using FoodDelivery.API.DTOs.Cart;
using FoodDelivery.API.Helpers.Cart;
using FoodDelivery.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers.Cart;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class CartController(AppDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var cart = await LoadCartAsync(CurrentUserId);
        return Ok(cart is null ? CartMapper.Empty() : CartMapper.ToResponse(cart));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest req)
    {
        var meal = await db.Meals
            .Include(m => m.MealType)
            .Include(m => m.Restaurant)
            .FirstOrDefaultAsync(m => m.Id == req.MealId);
        if (meal is null)
            return NotFound("Meal not found.");

        var cart = await GetOrCreateCartAsync(CurrentUserId);

        if (cart.RestaurantId is not null && cart.RestaurantId != meal.RestaurantId)
            return BadRequest(ValidationMessages.CartRestaurantMismatch);

        if (cart.RestaurantId is null)
            cart.RestaurantId = meal.RestaurantId;

        var existing = cart.Items.FirstOrDefault(i => i.MealId == req.MealId);
        if (existing is not null)
            existing.Quantity += req.Quantity;
        else
            cart.Items.Add(new CartItem { MealId = req.MealId, Quantity = req.Quantity });

        cart.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var updated = await LoadCartAsync(CurrentUserId);
        return Ok(updated is null ? CartMapper.Empty() : CartMapper.ToResponse(updated));
    }

    [HttpPut("items/{mealId:int}")]
    public async Task<IActionResult> SetItemQuantity(int mealId, SetCartItemQuantityRequest req)
    {
        var cart = await LoadCartAsync(CurrentUserId);
        if (cart is null)
            return NotFound();

        var item = cart.Items.FirstOrDefault(i => i.MealId == mealId);
        if (item is null)
            return NotFound();

        if (req.Quantity <= 0)
            cart.Items.Remove(item);
        else
            item.Quantity = req.Quantity;

        if (cart.Items.Count == 0)
            cart.RestaurantId = null;

        cart.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var updated = await LoadCartAsync(CurrentUserId);
        return Ok(updated is null ? CartMapper.Empty() : CartMapper.ToResponse(updated));
    }

    [HttpDelete("items/{mealId:int}")]
    public async Task<IActionResult> RemoveItem(int mealId)
    {
        var cart = await LoadCartAsync(CurrentUserId);
        if (cart is null)
            return Ok(CartMapper.Empty());

        var item = cart.Items.FirstOrDefault(i => i.MealId == mealId);
        if (item is not null)
        {
            cart.Items.Remove(item);
            if (cart.Items.Count == 0)
                cart.RestaurantId = null;

            cart.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        var updated = await LoadCartAsync(CurrentUserId);
        return Ok(updated is null ? CartMapper.Empty() : CartMapper.ToResponse(updated));
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == CurrentUserId);

        if (cart is not null)
        {
            cart.Items.Clear();
            cart.RestaurantId = null;
            cart.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return Ok(CartMapper.Empty());
    }

    private async Task<Models.Cart?> LoadCartAsync(int customerId) =>
        await db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Meal).ThenInclude(m => m.MealType)
            .Include(c => c.Restaurant)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

    private async Task<Models.Cart> GetOrCreateCartAsync(int customerId)
    {
        var cart = await LoadCartAsync(customerId);
        if (cart is not null)
            return cart;

        cart = new Models.Cart { CustomerId = customerId };
        db.Carts.Add(cart);
        await db.SaveChangesAsync();
        return cart;
    }
}
