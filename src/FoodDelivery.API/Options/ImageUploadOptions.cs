namespace FoodDelivery.API.Options;

public class ImageUploadOptions
{
    public const string SectionName = "Uploads";

    public string RootPath { get; set; } = "uploads";
    public string PublicPathPrefix { get; set; } = "/uploads";
    public long MaxFileBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
