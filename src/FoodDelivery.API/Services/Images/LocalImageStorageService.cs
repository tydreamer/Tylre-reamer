using FoodDelivery.API.Helpers.Images;
using FoodDelivery.API.Options;
using Microsoft.Extensions.Options;

namespace FoodDelivery.API.Services.Images;

public class LocalImageStorageService(
    IWebHostEnvironment environment,
    IOptions<ImageUploadOptions> options) : IImageStorageService
{
    private readonly ImageUploadOptions _options = options.Value;

    public async Task<string> SaveRestaurantImageAsync(
        int restaurantId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(GetUploadRoot(), "restaurants", restaurantId.ToString());
        return await SaveAsync(directory, "cover", file, cancellationToken);
    }

    public async Task<string> SaveMealImageAsync(
        int restaurantId,
        int mealId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(
            GetUploadRoot(),
            "restaurants",
            restaurantId.ToString(),
            "meals");
        return await SaveAsync(directory, mealId.ToString(), file, cancellationToken);
    }

    public void DeleteIfStored(string? imageUrl)
    {
        if (!TryGetAbsolutePath(imageUrl, out var absolutePath))
            return;

        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
    }

    private async Task<string> SaveAsync(
        string directory,
        string fileNameWithoutExtension,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(directory);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var physicalPath = Path.Combine(directory, fileNameWithoutExtension + extension);

        foreach (var existing in Directory.EnumerateFiles(directory, fileNameWithoutExtension + ".*"))
            File.Delete(existing);

        await using var stream = File.Create(physicalPath);
        await file.CopyToAsync(stream, cancellationToken);

        var relativePath = physicalPath
            [GetUploadRoot().Length..]
            .Replace('\\', '/')
            .TrimStart('/');

        return $"{_options.PublicPathPrefix.TrimEnd('/')}/{relativePath}";
    }

    private string GetUploadRoot()
    {
        var root = Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(environment.ContentRootPath, _options.RootPath);

        Directory.CreateDirectory(root);
        return Path.GetFullPath(root);
    }

    private bool TryGetAbsolutePath(string? imageUrl, out string absolutePath)
    {
        absolutePath = string.Empty;
        if (string.IsNullOrWhiteSpace(imageUrl))
            return false;

        var prefix = _options.PublicPathPrefix.TrimEnd('/');
        if (!imageUrl.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase) &&
            !imageUrl.Equals(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var relative = imageUrl[prefix.Length..].TrimStart('/');
        absolutePath = Path.GetFullPath(Path.Combine(GetUploadRoot(), relative.Replace('/', Path.DirectorySeparatorChar)));
        var uploadRoot = GetUploadRoot();
        if (!absolutePath.StartsWith(uploadRoot, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}
