namespace FoodDelivery.API.Constants;

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

    public const string RoleRequired = "Role is required.";

    public const string DescriptionRequired = "Description is required.";
    public const string DescriptionTooLong = "Description cannot exceed 500 characters.";
    public const string ImageUrlTooLong = "Image URL cannot exceed 500 characters.";

    public const string PriceInvalid = "Price must be greater than 0.";
    public const string MealTypeRequired = "Meal type is required.";
    public const string MealTypeInvalid = "Invalid meal type.";

    public const string CodeRequired = "Code is required.";
    public const string CodeTooLong = "Code cannot exceed 10 characters.";
    public const string DiscountInvalid = "Discount must be between 1% and 100%.";

    public const string StatusRequired = "Status is required.";
    public const string ItemsRequired = "At least one item is required.";
    public const string QuantityInvalid = "Quantity must be between 1 and 100.";
    public const string TipInvalid = "Tip must be 0 or greater.";
}
