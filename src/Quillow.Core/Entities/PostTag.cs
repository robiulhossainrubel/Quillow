namespace Quillow.Core.Entities;

/// <summary>
/// Post ও Tag এর মধ্যে Many-to-Many join table।
/// EF Core শেখার পয়েন্ট: Composite Primary Key, Join Entity।
/// </summary>
public class PostTag
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
