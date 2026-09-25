using Quillow.Core.Entities;

namespace Quillow.API.GraphQL.Types;

/// <summary>
/// GraphQL শেখার পয়েন্ট: INTERFACE TYPE
/// 
/// Interface হলো একটা contract — যেসব type এটা implement করে, 
/// তাদের অবশ্যই এই fields থাকতে হবে।
/// 
/// query {
///   posts {
///     ... on ITimestamped {     ← Interface দিয়ে query!
///       createdAt
///       updatedAt
///     }
///     title
///   }
/// }
/// 
/// এটা কাজে লাগে যখন তুমি different types কে একই ভাবে handle করতে চাও।
/// যেমন: Post, Comment, User সবারই CreatedAt, UpdatedAt আছে।
/// </summary>
[InterfaceType("ITimestamped")]
public interface ITimestamped
{
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
}

/// <summary>
/// Node interface — Relay specification এর জন্য।
/// প্রতিটা entity কে globally unique ID দিয়ে identify করা যায়।
/// </summary>
[InterfaceType("INode")]
public interface INode
{
    Guid Id { get; }
}
