using Quillow.Core.Enums;

namespace Quillow.Core.Entities;

/// <summary>
/// Blog Post — এটাই মূল entity।
/// GraphQL শেখার পয়েন্ট:
/// - Object Type হিসেবে expose হবে
/// - Query, Mutation, Subscription সব এর মাধ্যমে access হবে
/// - Filtering, Sorting, Pagination সব এখানে শিখবে
/// </summary>
public class Post : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? CoverImage { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public bool IsFeatured { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public DateTime? PublishedAt { get; set; }

    // Foreign Keys
    public Guid AuthorId { get; set; }
    public Guid CategoryId { get; set; }

    // Navigation Properties
    public User Author { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<PostTag> PostTags { get; set; } = [];
    public ICollection<Media> Media { get; set; } = [];
}
