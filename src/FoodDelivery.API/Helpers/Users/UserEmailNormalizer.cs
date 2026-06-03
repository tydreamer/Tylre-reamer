namespace FoodDelivery.API.Helpers.Users;

public static class UserEmailNormalizer
{
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
