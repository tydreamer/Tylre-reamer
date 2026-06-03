using FoodDelivery.API.Options;

namespace FoodDelivery.API.Helpers.Images;

public static class ImageFileValidator
{
    public static bool TryValidate(IFormFile file, ImageUploadOptions options, out string error)
    {
        error = string.Empty;

        if (file.Length == 0)
        {
            error = "Image file is empty.";
            return false;
        }

        if (file.Length > options.MaxFileBytes)
        {
            error = $"Image must be {options.MaxFileBytes / (1024 * 1024)} MB or smaller.";
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) ||
            !options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            error = "Image must be a JPG, PNG, or WebP file.";
            return false;
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            error = "Uploaded file must be an image.";
            return false;
        }

        return true;
    }
}
