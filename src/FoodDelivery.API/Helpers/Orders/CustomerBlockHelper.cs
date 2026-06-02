using FoodDelivery.API.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Helpers.Orders;

public static class CustomerBlockHelper
{
    public static Task<bool> IsBlockedFromOwnerAsync(
        AppDbContext db,
        int ownerId,
        int customerId) =>
        db.OwnerCustomerBlocks.AnyAsync(b => b.OwnerId == ownerId && b.CustomerId == customerId);

    public static Task<bool> HasOrderedFromOwnerAsync(
        AppDbContext db,
        int ownerId,
        int customerId) =>
        db.Orders.AnyAsync(o => o.CustomerId == customerId && o.Restaurant.OwnerId == ownerId);
}
