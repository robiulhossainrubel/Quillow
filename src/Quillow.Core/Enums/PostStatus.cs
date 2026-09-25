namespace Quillow.Core.Enums;

/// <summary>
/// Post এর status — GraphQL শেখার পয়েন্ট: এটা GraphQL Enum Type হিসেবে expose হবে।
/// query { posts(where: { status: { eq: PUBLISHED } }) { title } }
/// </summary>
public enum PostStatus
{
    Draft,
    Published,
    Archived
}
