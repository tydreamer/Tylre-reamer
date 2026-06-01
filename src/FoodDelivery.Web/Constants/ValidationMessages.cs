namespace FoodDelivery.Web.Constants;

public static class ValidationMessages
{
    public const string NameRequired = "Name is required.";
    public const string NameTooLong = "Name cannot exceed 100 characters.";

    public const string EmailRequired = "Email is required.";
    public const string EmailInvalid = "Enter a valid email address.";
    public const string EmailTooLong = "Email cannot exceed 255 characters.";

    public const string PasswordRequired = "Password is required.";
    public const string PasswordTooShort = "Password must be at least 6 characters.";
    public const string PasswordTooLong = "Password cannot exceed 100 characters.";
    public const string PasswordTooWeak = "Password does not meet strength requirements";
    public const string PasswordConfirmRequired = "Please confirm your password.";
    public const string PasswordConfirmMismatch = "Passwords do not match.";

    public const string CodeRequired = "Code is required.";
    public const string CodeTooLong = "Code cannot exceed 10 characters.";
    public const string DiscountRequired = "Discount is required.";
    public const string DiscountInvalid = "Discount must be between 1% and 100%.";
    public const string MealTypeRequired = "Meal type is required.";
}
