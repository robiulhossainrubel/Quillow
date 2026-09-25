namespace Quillow.Core.Entities;

/// <summary>
/// সব Entity র জন্য base class — common properties এখানে থাকবে।
/// GraphQL শেখার পয়েন্ট: এটা পরে Interface Type (ITimestamped) হিসেবে expose করবো।
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
