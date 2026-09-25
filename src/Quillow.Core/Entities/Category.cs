namespace Quillow.Core.Entities;

/// <summary>
/// Post এর Category — self-referencing relationship আছে (parent-child)।
/// GraphQL শেখার পয়েন্ট: recursive/nested types শেখা যাবে।
/// query { categories { name subCategories { name } } }
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Self-referencing relationship (Parent Category)
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    // Navigation Properties
    public ICollection<Category> SubCategories { get; set; } = [];
    public ICollection<Post> Posts { get; set; } = [];
}
