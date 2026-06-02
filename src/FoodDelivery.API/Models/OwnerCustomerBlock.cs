namespace FoodDelivery.API.Models;

/// <summary>
/// Owner-scoped block: customer can still sign in but cannot order from this owner's restaurants.
/// </summary>
public class OwnerCustomerBlock
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public int CustomerId { get; set; }
    public User Customer { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
