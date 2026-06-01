using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FoodDelivery.API.Constants;
using FoodDelivery.API.Helpers;
using FoodDelivery.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Data;

public static class DbSeeder
{
    private const string DefaultSeedPassword = "Password123!";

    private sealed record AccountRow(string Name, string Email, string Role);
    private sealed record RestaurantRow(string Name, string Description, string ImageUrl, string OwnerName, string OwnerEmail);
    private sealed record MealRow(string RestaurantName, string Name, string Description, decimal Price, string? ImageUrl, string? MealType);

    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedMealTypesAsync(db);
        await SeedAccountsAsync(db);
        await SeedRestaurantsAsync(db);
        await SeedMealsAsync(db);
        await BackfillMealImagesAsync(db);
    }

    private static async Task SeedMealTypesAsync(AppDbContext db)
    {
        if (await db.MealTypes.AnyAsync())
            return;

        foreach (var name in MealTypeNames.All)
            db.MealTypes.Add(new MealType { Name = name });

        await db.SaveChangesAsync();
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

        var mealTypeIdsByName = await db.MealTypes.ToDictionaryAsync(t => t.Name, t => t.Id);
        var defaultMealTypeId = mealTypeIdsByName[MealTypeNames.Lunch];

        var meals = ReadCsv<MealRow>("meals.csv")
            .Where(m => restaurantIdsByName.ContainsKey(m.RestaurantName))
            .Select(m => new Meal
            {
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                ImageUrl = ResolveMealImageUrl(m),
                RestaurantId = restaurantIdsByName[m.RestaurantName],
                MealTypeId = ResolveMealTypeId(m, mealTypeIdsByName, defaultMealTypeId)
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

    private static int ResolveMealTypeId(
        MealRow row,
        Dictionary<string, int> mealTypeIdsByName,
        int defaultMealTypeId)
    {
        if (!string.IsNullOrWhiteSpace(row.MealType)
            && mealTypeIdsByName.TryGetValue(row.MealType.Trim(), out var id))
            return id;

        return InferMealTypeId(row, mealTypeIdsByName, defaultMealTypeId);
    }

    private static int InferMealTypeId(
        MealRow row,
        Dictionary<string, int> mealTypeIdsByName,
        int defaultMealTypeId)
    {
        if (row.RestaurantName.Contains("Breakfast", StringComparison.OrdinalIgnoreCase))
            return mealTypeIdsByName[MealTypeNames.Breakfast];

        var name = row.Name.ToLowerInvariant();
        if (name.Contains("cake") || name.Contains("pie") || name.Contains("cookie")
            || name.Contains("brownie") || name.Contains("tiramisu") || name.Contains("cheesecake")
            || name.Contains("sundae") || name.Contains("gelato") || name.Contains("baklava")
            || name.Contains("churro") || name.Contains("mochi") || name.Contains("panna cotta"))
            return mealTypeIdsByName[MealTypeNames.Dessert];

        if (name.Contains("soup") || name.Contains("wings") || name.Contains("nachos")
            || name.Contains("spring roll") || name.Contains("edamame") || name.Contains("oyster")
            || name.Contains("bruschetta") || name.Contains("samosa") || name.Contains("calamari")
            || name.Contains("dip") || name.Contains("salad") && !name.Contains("bowl"))
            return mealTypeIdsByName[MealTypeNames.Appetizers];

        if (name.Contains("coffee") || name.Contains("pancake") || name.Contains("omelette")
            || name.Contains("eggs") || name.Contains("waffle") || name.Contains("burrito")
            && row.RestaurantName.Contains("Coffee", StringComparison.OrdinalIgnoreCase))
            return mealTypeIdsByName[MealTypeNames.Breakfast];

        return mealTypeIdsByName.GetValueOrDefault(MealTypeNames.Dinner, defaultMealTypeId);
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
