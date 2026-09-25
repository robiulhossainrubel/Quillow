using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using Quillow.Core.Entities;

namespace Quillow.API.GraphQL.Subscriptions;

/// <summary>
/// GraphQL শেখার পয়েন্ট: SUBSCRIPTION — Real-time Data!
/// 
/// Query = GET (একবার data আনো)
/// Mutation = POST/PUT/DELETE (data পরিবর্তন করো)  
/// Subscription = WebSocket (live data পাও, ক্রমাগত!)
/// 
/// কিভাবে কাজ করে:
/// 1. Client subscribe করে: "আমি এই post এর নতুন comments দেখতে চাই"
/// 2. Server WebSocket connection খোলা রাখে
/// 3. যখন কেউ comment করে, server তৎক্ষণাৎ সেই data client কে পাঠায়
/// 4. Client real-time update দেখে!
/// 
/// Frontend এ এরকম ব্যবহার হবে:
/// 
/// subscription {
///   onCommentAdded(postId: "some-id") {
///     id
///     content
///     author { displayName avatar }
///     createdAt
///   }
/// }
/// 
/// useSubscription(ON_COMMENT_ADDED, { variables: { postId } })
/// </summary>
public class Subscription
{
    /// <summary>
    /// নতুন comment আসলে real-time notify করবে।
    /// 
    /// যখন কেউ AddComment mutation চালায়, সেই comment 
    /// subscribe করা সবার কাছে তৎক্ষণাৎ চলে যাবে।
    /// </summary>
    [Subscribe]
    [Topic("CommentAdded_{postId}")]
    public Comment OnCommentAdded(
        Guid postId,
        [EventMessage] Comment comment)
    {
        return comment;
    }

    /// <summary>
    /// নতুন post publish হলে notify করবে।
    /// সব subscriber কে নতুন published post দেখাবে।
    /// </summary>
    [Subscribe]
    [Topic("PostPublished")]
    public Post OnPostPublished([EventMessage] Post post)
    {
        return post;
    }

    /// <summary>
    /// কেউ post like করলে notify করবে।
    /// </summary>
    [Subscribe]
    [Topic("PostLiked_{postId}")]
    public LikeNotification OnPostLiked(
        Guid postId,
        [EventMessage] LikeNotification notification)
    {
        return notification;
    }
}

/// <summary>
/// Like notification payload — subscription এ পাঠানো হবে।
/// </summary>
public record LikeNotification(
    Guid PostId,
    Guid UserId,
    string UserDisplayName,
    bool IsLiked,
    int TotalLikes
);
