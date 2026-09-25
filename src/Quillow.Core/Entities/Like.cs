namespace Quillow.Core.Entities;

/// <summary>
/// Like — User একটা Post কে like করতে পারবে (toggle)।
/// GraphQL শেখার পয়েন্ট: Mutation (toggleLike) — same mutation দিয়ে like/unlike।
/// </summary>
public class Like : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }

    // Navigation Properties
    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
}
