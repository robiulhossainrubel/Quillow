namespace Quillow.Core.Entities;

/// <summary>
/// Comment — Post এ comment করা যাবে, reply ও করা যাবে (nested comments)।
/// GraphQL শেখার পয়েন্ট:
/// - Subscription: নতুন comment আসলে real-time notify হবে
/// - Self-referencing: replies (parent-child comment)
/// </summary>
public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;

    // Foreign Keys
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? ParentCommentId { get; set; }

    // Navigation Properties
    public Post Post { get; set; } = null!;
    public User Author { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = [];
}
