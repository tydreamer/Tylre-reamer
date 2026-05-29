using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FoodDelivery.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Data;

public static class DbSeeder
{
    private sealed record RestaurantRow(string Name, string Description, string ImageUrl);

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

        var csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "restaurants.csv");
        if (!File.Exists(csvPath))
            return;

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        var rows = csv.GetRecords<RestaurantRow>();

        var restaurants = rows.Select(r => new Restaurant
        {
            Name = r.Name,
            Description = r.Description,
            ImageUrl = r.ImageUrl,
            OwnerId = owner.Id
        });

        db.Restaurants.AddRange(restaurants);
        await db.SaveChangesAsync();
    }
}
