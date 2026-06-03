namespace FoodDelivery.API.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }

    public string? ApiKey { get; set; }

    public string FromAddress { get; set; } = "noreply@fooddelivery.local";

    public string FromName { get; set; } = "Food Delivery";
}
