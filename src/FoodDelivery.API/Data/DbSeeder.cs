using FoodDelivery.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Restaurants.AnyAsync())
            return;

        var owner = await db.Users.FirstOrDefaultAsync(u => u.Email == "owner@example.com");
        if (owner is null)
        {
            owner = new User
            {
                Name = "Sample Owner",
                Email = "owner@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = UserRole.Owner
            };
            db.Users.Add(owner);
            await db.SaveChangesAsync();
        }

        db.Restaurants.AddRange(
            new Restaurant
            {
                Name = "Bella Napoli",
                Description = "Wood-fired Neapolitan pizza and fresh pasta.",
                ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=600",
                OwnerId = owner.Id
            },
            new Restaurant
            {
                Name = "Sakura Sushi",
                Description = "Authentic Japanese sushi and ramen bowls.",
                ImageUrl = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=600",
                OwnerId = owner.Id
            },
            new Restaurant
            {
                Name = "El Toro Taqueria",
                Description = "Street-style tacos, burritos, and quesadillas.",
                ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=600",
                OwnerId = owner.Id
            });

        await db.SaveChangesAsync();
    }
}
