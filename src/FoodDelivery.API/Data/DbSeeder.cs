using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FoodDelivery.API.Helpers;
using FoodDelivery.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Data;

public static class DbSeeder
{
    private const string DefaultSeedPassword = "Password123!";

    private sealed record AccountRow(string Name, string Email, string Role);
    private sealed record RestaurantRow(string Name, string Description, string ImageUrl, string OwnerName, string OwnerEmail);
    private sealed record MealRow(string RestaurantName, string Name, string Description, decimal Price, string? ImageUrl);

    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedAccountsAsync(db);
        await SeedRestaurantsAsync(db);
        await SeedMealsAsync(db);
        await BackfillMealImagesAsync(db);
    }

    private static async Task SeedAccountsAsync(AppDbContext db)
    {
        foreach (var row in ReadCsv<AccountRow>("users.csv"))
        {
            if (!Enum.TryParse<UserRole>(row.Role, ignoreCase: true, out var role))
                continue;

            await EnsureUserAsync(db, row.Name.Trim(), row.Email.Trim().ToLowerInvariant(), role);
        }
    }

    private static async Task SeedRestaurantsAsync(AppDbContext db)
    {
        if (await db.Restaurants.AnyAsync())
            return;

        var restaurants = new List<Restaurant>();

        foreach (var row in ReadCsv<RestaurantRow>("restaurants.csv"))
        {
            var ownerEmail = row.OwnerEmail.Trim().ToLowerInvariant();
            var owner = await EnsureUserAsync(
                db,
                row.OwnerName.Trim(),
                ownerEmail,
                UserRole.Owner);

            restaurants.Add(new Restaurant
            {
                Name = row.Name.Trim(),
                Description = row.Description.Trim(),
                ImageUrl = row.ImageUrl.Trim(),
                OwnerId = owner.Id
            });
        }

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
                ImageUrl = ResolveMealImageUrl(m),
                RestaurantId = restaurantIdsByName[m.RestaurantName]
            })
            .ToList();

        db.Meals.AddRange(meals);
        await db.SaveChangesAsync();
    }

    private static async Task BackfillMealImagesAsync(AppDbContext db)
    {
        var meals = await db.Meals
            .Where(m => m.ImageUrl == "")
            .ToListAsync();

        if (meals.Count == 0)
            return;

        foreach (var meal in meals)
            meal.ImageUrl = MealImageUrlBuilder.Build(meal.Name, meal.Id);

        await db.SaveChangesAsync();
    }

    private static async Task<User> EnsureUserAsync(AppDbContext db, string name, string email, UserRole role)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (existing is not null)
            return existing;

        var user = new User
        {
            Name = name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultSeedPassword),
            Role = role
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static string ResolveMealImageUrl(MealRow row) =>
        string.IsNullOrWhiteSpace(row.ImageUrl)
            ? MealImageUrlBuilder.Build(row.Name, MealImageUrlBuilder.SeedFrom(row.RestaurantName, row.Name))
            : row.ImageUrl.Trim();

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
