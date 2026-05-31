namespace FoodDelivery.API.Helpers;

public static class MealImageUrlBuilder
{
    public static string Build(string mealName, int lockSeed)
    {
        var tag = mealName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(WordLetters)
            .FirstOrDefault(w => w.Length >= 3);

        tag = string.IsNullOrEmpty(tag) ? "food" : tag.ToLowerInvariant();
        var lockId = Math.Clamp(Math.Abs(lockSeed), 100, 9999);
        return $"https://loremflickr.com/600/400/food,{tag}?lock={lockId}";
    }

    public static int SeedFrom(string restaurantName, string mealName) =>
        HashCode.Combine(restaurantName, mealName);

    private static string WordLetters(string word) =>
        new string(word.Where(char.IsLetter).ToArray());
}
