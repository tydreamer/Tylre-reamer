using Microsoft.AspNetCore.Components.Forms;

namespace FoodDelivery.Web.Models.Images;

public class ImageUploadSelection
{
    public IBrowserFile? PendingFile { get; set; }
    public bool Removed { get; set; }
    public string? PendingPreviewDataUrl { get; set; }

    public bool HasPendingUpload => PendingFile is not null;
    public bool ShouldClear => Removed && PendingFile is null;
}
