using System.Security.Claims;
using FoodDelivery.API.Constants;
using FoodDelivery.API.Data;
using FoodDelivery.API.Hubs;
using FoodDelivery.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers.Orders;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(AppDbContext db, IHubContext<OrderHub> hub) : ControllerBase
{
    protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected string CurrentUserRole => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var userId = CurrentUserId;
        var role = CurrentUserRole;

        var query = db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Meal)
            .Include(o => o.Restaurant)
            .Include(o => o.Customer)
            .Include(o => o.Coupon)
            .Include(o => o.StatusHistory)
            .AsQueryable();

        query = role == "Owner"
            ? query.Where(o => o.Restaurant.OwnerId == userId)
            : query.Where(o => o.CustomerId == userId);

        query = OrderQuerySort.Apply(query, sortBy, sortDesc);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = new List<OrderResponse>();
        foreach (var item in items)
            responses.Add(await MapToResponseAsync(item));

        return Ok(new PagedResult<OrderResponse>(responses, totalCount, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Meal)
            .Include(o => o.Restaurant)
            .Include(o => o.Customer)
            .Include(o => o.Coupon)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();
        if (!CanAccessOrder(order)) return Forbid();

        return Ok(await MapToResponseAsync(order));
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Place(PlaceOrderRequest req)
    {
        var customerId = CurrentUserId;

        var customer = await db.Users.FindAsync(customerId);
        if (customer is null || customer.IsBlocked)
            return Forbid();

        var restaurant = await db.Restaurants
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == req.RestaurantId);
        if (restaurant is null) return NotFound("Restaurant not found.");

        if (await CustomerBlockHelper.IsBlockedFromOwnerAsync(db, restaurant.OwnerId, customerId))
            return BadRequest(ValidationMessages.CustomerBlockedFromRestaurant);

        var mealIds = req.Items.Select(i => i.MealId).ToList();
        var meals = await db.Meals
            .Where(m => mealIds.Contains(m.Id) && m.RestaurantId == req.RestaurantId)
            .ToListAsync();

        if (meals.Count != req.Items.Count)
            return BadRequest("One or more meals are invalid.");

        decimal subtotal = req.Items.Sum(i => meals.First(m => m.Id == i.MealId).Price * i.Quantity);
        decimal discount = 0;

        Coupon? coupon = null;
        if (!string.IsNullOrWhiteSpace(req.CouponCode))
        {
            var validation = await CouponValidator.ValidateAsync(
                db, req.RestaurantId, req.CouponCode, subtotal);

            if (validation.Status != CouponValidationStatus.Valid || validation.Coupon is null)
                return BadRequest(CouponValidator.ErrorMessage(validation.Status));

            coupon = validation.Coupon;
            discount = validation.DiscountAmount;
        }

        var total = subtotal - discount + req.Tip;

        var order = new Order
        {
            CustomerId = customerId,
            RestaurantId = req.RestaurantId,
            Tip = req.Tip,
            CouponId = coupon?.Id,
            TotalPrice = total,
            Items = req.Items.Select(i => new OrderItem
            {
                MealId = i.MealId,
                Quantity = i.Quantity,
                UnitPrice = meals.First(m => m.Id == i.MealId).Price
            }).ToList(),
            StatusHistory = [new OrderStatusHistory { Status = OrderStatus.Placed }]
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var created = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Meal)
            .Include(o => o.Restaurant)
            .Include(o => o.Customer)
            .Include(o => o.Coupon)
            .Include(o => o.StatusHistory)
            .FirstAsync(o => o.Id == order.Id);

        var notification = new OrderStatusNotification(order.Id, restaurant.Name, OrderStatus.Placed.ToString());
        await hub.Clients.Group($"user-{restaurant.OwnerId}")
            .SendAsync("OrderStatusChanged", notification);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, await MapToResponseAsync(created));
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest req)
    {
        var order = await db.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();
        if (!CanAccessOrder(order)) return Forbid();

        if (!Enum.TryParse<OrderStatus>(req.Status, ignoreCase: true, out var newStatus))
            return BadRequest("Invalid status.");

        if (!IsValidTransition(order.Status, newStatus, CurrentUserRole))
            return BadRequest($"Cannot transition from {order.Status} to {newStatus}.");

        order.Status = newStatus;
        order.StatusHistory.Add(new OrderStatusHistory { Status = newStatus });
        await db.SaveChangesAsync();

        var notification = new OrderStatusNotification(order.Id, order.Restaurant.Name, newStatus.ToString());
        await hub.Clients
            .Groups($"user-{order.CustomerId}", $"user-{order.Restaurant.OwnerId}")
            .SendAsync("OrderStatusChanged", notification);

        return NoContent();
    }

    [HttpPost("{id}/duplicate")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Duplicate(int id)
    {
        var customer = await db.Users.FindAsync(CurrentUserId);
        if (customer is null || customer.IsBlocked)
            return Forbid();

        var original = await db.Orders
            .Include(o => o.Items)
            .Include(o => o.Restaurant)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == CurrentUserId);

        if (original is null) return NotFound();

        if (await CustomerBlockHelper.IsBlockedFromOwnerAsync(db, original.Restaurant.OwnerId, CurrentUserId))
            return BadRequest(ValidationMessages.CustomerBlockedFromRestaurant);

        var meals = await db.Meals
            .Where(m => original.Items.Select(i => i.MealId).Contains(m.Id))
            .ToListAsync();

        var order = new Order
        {
            CustomerId = original.CustomerId,
            RestaurantId = original.RestaurantId,
            Tip = original.Tip,
            TotalPrice = meals.Sum(m => original.Items.First(i => i.MealId == m.Id).Quantity * m.Price) + original.Tip,
            Items = original.Items.Select(i => new OrderItem
            {
                MealId = i.MealId,
                Quantity = i.Quantity,
                UnitPrice = meals.FirstOrDefault(m => m.Id == i.MealId)?.Price ?? i.UnitPrice
            }).ToList(),
            StatusHistory = [new OrderStatusHistory { Status = OrderStatus.Placed }]
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, new { order.Id });
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
            (OrderStatus.Processing, OrderStatus.InRoute, "Owner") => true,
            (OrderStatus.InRoute, OrderStatus.Delivered, "Owner") => true,
            (OrderStatus.Delivered, OrderStatus.Received, "Customer") => true,
            _ => false
        };
    }

    protected async Task<OrderResponse> MapToResponseAsync(Order o)
    {
        var blockedFromOwner = false;
        if (CurrentUserRole == "Owner")
        {
            blockedFromOwner = await CustomerBlockHelper.IsBlockedFromOwnerAsync(
                db, CurrentUserId, o.CustomerId);
        }

        return new OrderResponse(
            o.Id,
            o.CustomerId,
            o.Customer.Name,
            o.RestaurantId,
            o.Restaurant.Name,
            o.Status.ToString(),
            o.Tip,
            o.TotalPrice,
            o.CreatedAt,
            o.Items.Select(i => new OrderItemResponse(i.MealId, i.Meal.Name, i.Quantity, i.UnitPrice)).ToList(),
            o.StatusHistory.OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryResponse(h.Status.ToString(), h.ChangedAt)).ToList(),
            blockedFromOwner,
            o.Coupon?.Code,
            GetCouponDiscount(o));
    }

    private static decimal? GetCouponDiscount(Order o)
    {
        if (o.Coupon is null)
            return null;

        var subtotal = o.Items.Sum(i => i.UnitPrice * i.Quantity);
        return Math.Max(0, subtotal - (o.TotalPrice - o.Tip));
    }
}
