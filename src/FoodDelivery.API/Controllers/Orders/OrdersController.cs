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
public class OrdersController(AppDbContext db, IHubContext<OrderHub> hub, OrderPricingService pricingService) : ControllerBase
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

        HashSet<int> blockedCustomerIds = [];
        if (role == "Owner")
        {
            var customerIds = items.Select(o => o.CustomerId).Distinct().ToList();
            blockedCustomerIds = (await db.OwnerCustomerBlocks
                .Where(b => b.OwnerId == userId && customerIds.Contains(b.CustomerId))
                .Select(b => b.CustomerId)
                .ToListAsync())
                .ToHashSet();
        }

        var responses = items
            .Select(o => MapToResponse(o, blockedCustomerIds.Contains(o.CustomerId)))
            .ToList();

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

        var blocked = CurrentUserRole == "Owner" &&
            await CustomerBlockHelper.IsBlockedFromOwnerAsync(db, CurrentUserId, order.CustomerId);
        return Ok(MapToResponse(order, blocked));
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

        var mealPrices = meals.ToDictionary(m => m.Id, m => m.Price);
        var priced = await pricingService.PriceAsync(
            req.Items.Select(i => (i.MealId, i.Quantity)).ToList(),
            mealPrices, req.RestaurantId, req.Tip, req.CouponCode);

        if (!string.IsNullOrWhiteSpace(req.CouponCode) && priced.Coupon is null)
            return BadRequest(CouponValidator.ErrorMessage(priced.CouponStatus ?? CouponValidationStatus.Invalid));

        var order = new Order
        {
            CustomerId = customerId,
            RestaurantId = req.RestaurantId,
            Tip = req.Tip,
            CouponId = priced.Coupon?.Id,
            TotalPrice = priced.Total,
            Items = req.Items.Select(i => new OrderItem
            {
                MealId = i.MealId,
                Quantity = i.Quantity,
                UnitPrice = mealPrices[i.MealId]
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

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapToResponse(created, false));
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

        var mealPrices = meals.ToDictionary(m => m.Id, m => m.Price);
        var priced = await pricingService.PriceAsync(
            original.Items.Select(i => (i.MealId, i.Quantity)).ToList(),
            mealPrices, original.RestaurantId, original.Tip, null);

        var order = new Order
        {
            CustomerId = original.CustomerId,
            RestaurantId = original.RestaurantId,
            Tip = original.Tip,
            TotalPrice = priced.Total,
            Items = original.Items.Select(i => new OrderItem
            {
                MealId = i.MealId,
                Quantity = i.Quantity,
                UnitPrice = mealPrices.GetValueOrDefault(i.MealId, i.UnitPrice)
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

    protected OrderResponse MapToResponse(Order o, bool blockedFromOwner) =>
        new(o.Id,
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

    private static decimal? GetCouponDiscount(Order o)
    {
        if (o.Coupon is null)
            return null;

        var subtotal = o.Items.Sum(i => i.UnitPrice * i.Quantity);
        return Math.Max(0, subtotal - (o.TotalPrice - o.Tip));
    }
}
