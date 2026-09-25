namespace Quillow.Core.Entities;

/// <summary>
/// User ও Role এর মধ্যে Many-to-Many join table।
/// একজন User এর একাধিক Role থাকতে পারে।
/// </summary>
public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
