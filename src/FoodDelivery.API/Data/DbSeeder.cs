using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FoodDelivery.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Data;

public static class DbSeeder
{
    private sealed record RestaurantRow(string Name, string Description, string ImageUrl);
    private sealed record MealRow(string RestaurantName, string Name, string Description, decimal Price);

    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedAdminAsync(db);
        await SeedRestaurantsAsync(db);
        await SeedMealsAsync(db);
    }

    private static async Task SeedAdminAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
            return;

        db.Users.Add(new User
        {
            Name = "Administrator",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = UserRole.Admin
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedRestaurantsAsync(AppDbContext db)
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

        var restaurants = ReadCsv<RestaurantRow>("restaurants.csv")
            .Select(r => new Restaurant
            {
                Name = r.Name,
                Description = r.Description,
                ImageUrl = r.ImageUrl,
                OwnerId = owner.Id
            })
            .ToList();

        if (restaurants.Count == 0)
            return;

        db.Restaurants.AddRange(restaurants);
        await db.SaveChangesAsync();
    }

    private static async Task SeedMealsAsync(AppDbContext db)
    {
        if (await db.Meals.AnyAsync())
            return;

        var restaurantIdsByName = await db.Restaurants
            .ToDictionaryAsync(r => r.Name, r => r.Id);

        if (restaurantIdsByName.Count == 0)
            return;

        var meals = ReadCsv<MealRow>("meals.csv")
            .Where(m => restaurantIdsByName.ContainsKey(m.RestaurantName))
            .Select(m => new Meal
            {
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                RestaurantId = restaurantIdsByName[m.RestaurantName]
            })
            .ToList();

        db.Meals.AddRange(meals);
        await db.SaveChangesAsync();
    }

    private static List<T> ReadCsv<T>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", fileName);
        if (!File.Exists(path))
            return [];

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        return csv.GetRecords<T>().ToList();
    }
}
