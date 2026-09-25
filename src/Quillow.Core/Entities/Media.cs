namespace Quillow.Core.Entities;

/// <summary>
/// Media — File upload (image, video, etc.) track করার জন্য।
/// GraphQL শেখার পয়েন্ট: File Upload mutation শেখা যাবে।
/// </summary>
public class Media : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }

    // Foreign Keys
    public Guid? PostId { get; set; }
    public Guid UploadedById { get; set; }

    // Navigation Properties
    public Post? Post { get; set; }
    public User UploadedBy { get; set; } = null!;
}
