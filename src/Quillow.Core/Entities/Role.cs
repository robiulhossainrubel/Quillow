namespace Quillow.Core.Entities;

/// <summary>
/// Role — User এর role (Admin, Editor, Reader)।
/// GraphQL শেখার পয়েন্ট: Authorization — role-based access control।
/// [Authorize(Roles = ["Admin"])] attribute দিয়ে resolver protect করা।
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
