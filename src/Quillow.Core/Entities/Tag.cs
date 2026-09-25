namespace Quillow.Core.Entities;

/// <summary>
/// Tag — Post এ multiple tags থাকতে পারে (Many-to-Many relationship)।
/// GraphQL শেখার পয়েন্ট: Many-to-Many relation কিভাবে GraphQL এ handle করে।
/// </summary>
public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<PostTag> PostTags { get; set; } = [];
}
