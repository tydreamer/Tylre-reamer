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
[Authorize]
public class OrdersController(AppDbContext db) : ControllerBase
{
    protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected string CurrentUserRole => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = CurrentUserId;
        var role = CurrentUserRole;

        var query = db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Meal)
            .Include(o => o.Restaurant)
            .Include(o => o.StatusHistory)
            .AsQueryable();

        query = role == "Owner"
            ? query.Where(o => o.Restaurant.OwnerId == userId)
            : query.Where(o => o.CustomerId == userId);

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Meal)
            .Include(o => o.Restaurant)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();
        if (!CanAccessOrder(order)) return Forbid();

        return Ok(MapToResponse(order));
    }

    protected bool CanAccessOrder(Order order)
    {
        var userId = CurrentUserId;
        return CurrentUserRole == "Owner"
            ? order.Restaurant.OwnerId == userId
            : order.CustomerId == userId;
    }

    protected static bool IsValidTransition(OrderStatus current, OrderStatus next, string role)
    {
        return (current, next, role) switch
        {
            (OrderStatus.Placed, OrderStatus.Cancelled, "Customer") => true,
            (OrderStatus.Placed, OrderStatus.Cancelled, "Owner") => true,
            (OrderStatus.Placed, OrderStatus.Processing, "Owner") => true,
            (OrderStatus.Processing, OrderStatus.Cancelled, "Customer") => true,
            (OrderStatus.Processing, OrderStatus.InRoute, "Owner") => true,
            (OrderStatus.InRoute, OrderStatus.Delivered, "Owner") => true,
            (OrderStatus.Delivered, OrderStatus.Received, "Customer") => true,
            _ => false
        };
    }

    protected static OrderResponse MapToResponse(Order o) => new(
        o.Id,
        o.CustomerId,
        o.RestaurantId,
        o.Restaurant.Name,
        o.Status.ToString(),
        o.Tip,
        o.TotalPrice,
        o.CreatedAt,
        o.Items.Select(i => new OrderItemResponse(i.MealId, i.Meal.Name, i.Quantity, i.UnitPrice)).ToList(),
        o.StatusHistory.OrderBy(h => h.ChangedAt).Select(h => new OrderStatusHistoryResponse(h.Status.ToString(), h.ChangedAt)).ToList()
    );
}
