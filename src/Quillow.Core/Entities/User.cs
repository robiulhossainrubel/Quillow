namespace Quillow.Core.Entities;

/// <summary>
/// ব্যবহারকারী — Blog এর লেখক ও পাঠক।
/// Navigation properties গুলো EF Core এর relationship define করে,
/// এবং GraphQL এ nested query enable করে:
/// query { user(id: "...") { displayName posts { title } comments { content } } }
/// </summary>
public class User : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? Bio { get; set; }

    // Navigation Properties
    public ICollection<Post> Posts { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
