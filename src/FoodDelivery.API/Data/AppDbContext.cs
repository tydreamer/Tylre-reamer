using FoodDelivery.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<OwnerCustomerBlock> OwnerCustomerBlocks => Set<OwnerCustomerBlock>();
    public DbSet<Cuisine> Cuisines => Set<Cuisine>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MealType> MealTypes => Set<MealType>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<OwnerCustomerBlock>()
            .HasIndex(b => new { b.OwnerId, b.CustomerId })
            .IsUnique();

        modelBuilder.Entity<OwnerCustomerBlock>()
            .HasOne(b => b.Owner)
            .WithMany()
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OwnerCustomerBlock>()
            .HasOne(b => b.Customer)
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.GoogleSubjectId)
            .IsUnique();

        modelBuilder.Entity<Coupon>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.Tip)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Meal>()
            .Property(m => m.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Meal>()
            .Property(m => m.ImageUrl)
            .HasMaxLength(500);

        modelBuilder.Entity<MealType>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<Meal>()
            .HasOne(m => m.MealType)
            .WithMany(t => t.Meals)
            .HasForeignKey(m => m.MealTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cuisine>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<Restaurant>()
            .Property(r => r.ImageUrl)
            .HasMaxLength(500);

        modelBuilder.Entity<Restaurant>()
            .HasOne(r => r.Cuisine)
            .WithMany(c => c.Restaurants)
            .HasForeignKey(r => r.CuisineId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Coupon>()
            .Property(c => c.DiscountValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(t => t.Token)
            .IsUnique();

        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cart>()
            .HasIndex(c => c.CustomerId)
            .IsUnique();

        modelBuilder.Entity<Cart>()
            .HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cart>()
            .HasOne(c => c.Restaurant)
            .WithMany()
            .HasForeignKey(c => c.RestaurantId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CartItem>()
            .HasIndex(i => new { i.CartId, i.MealId })
            .IsUnique();

        modelBuilder.Entity<CartItem>()
            .HasOne(i => i.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(i => i.Meal)
            .WithMany()
            .HasForeignKey(i => i.MealId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
