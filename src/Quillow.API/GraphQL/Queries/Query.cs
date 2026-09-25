using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using Quillow.API.GraphQL.Types;
using Quillow.Core.Entities;
using Quillow.Core.Enums;
using Quillow.Infrastructure.Data;

using CoreTag = Quillow.Core.Entities.Tag;

namespace Quillow.API.GraphQL.Queries;

/// <summary>
/// GraphQL শেখার পয়েন্ট: QUERY — Data পড়ার (Read) জন্য।
/// 
/// Phase 2 Updates:
/// - Include() সরিয়ে দেওয়া হয়েছে কারণ DataLoader handle করবে
/// - Search query যোগ করা হয়েছে (Union Type)
/// - Pagination ready
/// </summary>
public class Query
{
    /// <summary>
    /// সব posts — DataLoader দিয়ে related data load হবে, তাই Include দরকার নেই!
    /// 
    /// আগে: 
    ///   context.Posts.Include(p => p.Author).Include(p => p.Category)... = অনেক JOIN
    /// 
    /// এখন:
    ///   context.Posts.ToListAsync() = শুধু Posts টেবিল query
    ///   DataLoader automatically Author, Category, Comments আলাদা batch query তে আনবে
    /// </summary>
    [UseFiltering]
    [UseSorting]
    public async Task<List<Post>> GetPosts(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Posts.ToListAsync();
    }

    /// <summary>
    /// ID দিয়ে একটা post খুঁজে বের করা।
    /// </summary>
    public async Task<Post?> GetPostById(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Posts.FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Slug দিয়ে post খোঁজা — frontend এ URL /blog/my-first-post থেকে data fetch করতে।
    /// </summary>
    public async Task<Post?> GetPostBySlug(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        string slug)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Posts.FirstOrDefaultAsync(p => p.Slug == slug);
    }

    /// <summary>
    /// Featured posts — homepage এ দেখানোর জন্য।
    /// </summary>
    public async Task<List<Post>> GetFeaturedPosts(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Posts
            .Where(p => p.IsFeatured && p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    // ========== User Queries ==========

    [UseFiltering]
    [UseSorting]
    public async Task<List<User>> GetUsers(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.ToListAsync();
    }

    public async Task<User?> GetUserById(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// GraphQL শেখার পয়েন্ট: [Authorize] দিয়ে Query সুরক্ষিত করা।
    /// 
    /// Header এ `Authorization: Bearer <token>` থাকলে
    /// ClaimsPrincipal থেকে স্বয়ংক্রিয়ভাবে current user এর ডেটা রিটার্ন করবে।
    /// 
    /// query {
    ///   me {
    ///     id
    ///     displayName
    ///     email
    ///     roles
    ///   }
    /// }
    /// </summary>
    [Authorize]
    public async Task<User?> GetMe(
        ClaimsPrincipal claimsPrincipal,
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory)
    {
        var userId = claimsPrincipal.GetUserId();
        if (userId == null) return null;

        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId.Value);
    }

    // ========== Category Queries ==========

    [UseFiltering]
    [UseSorting]
    public async Task<List<Category>> GetCategories(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.ParentCategory)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryBySlug(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        string slug)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    // ========== Tag Queries ==========

    [UseFiltering]
    [UseSorting]
    public async Task<List<CoreTag>> GetTags(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Tags.ToListAsync();
    }

    // ========== Comment Queries ==========

    [UseSorting]
    public async Task<List<Comment>> GetCommentsByPostId(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        Guid postId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Comments
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .Include(c => c.Author)
            .Include(c => c.Replies).ThenInclude(r => r.Author)
            .ToListAsync();
    }

    // ========== Search Query (Union Type!) ==========

    /// <summary>
    /// GraphQL শেখার পয়েন্ট: UNION TYPE দিয়ে Global Search
    /// 
    /// এটা একসাথে Posts, Users, Categories তে search করে।
    /// Result যেকোনো type হতে পারে — তাই Union Type ব্যবহার!
    /// 
    /// query {
    ///   search(term: "GraphQL") {
    ///     ... on Post { title excerpt }
    ///     ... on User { displayName email }
    ///     ... on Category { name description }
    ///   }
    /// }
    /// </summary>
    [GraphQLType(typeof(ListType<SearchResultType>))]
    public async Task<List<object>> Search(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        string term)
    {
        return await SearchQueryExtensions.SearchAsync(contextFactory, term);
    }

    // ========== Statistics Query ==========

    /// <summary>
    /// Blog এর overall statistics — dashboard এ দেখানোর জন্য।
    /// </summary>
    public async Task<BlogStats> GetStats(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        return new BlogStats(
            TotalPosts: await context.Posts.CountAsync(),
            PublishedPosts: await context.Posts.CountAsync(p => p.Status == PostStatus.Published),
            DraftPosts: await context.Posts.CountAsync(p => p.Status == PostStatus.Draft),
            TotalUsers: await context.Users.CountAsync(),
            TotalComments: await context.Comments.CountAsync(),
            TotalLikes: await context.Likes.CountAsync(),
            TotalCategories: await context.Categories.CountAsync(),
            TotalTags: await context.Tags.CountAsync()
        );
    }

    // ========== Media Queries (Phase 7) ==========

    /// <summary>
    /// সব upload করা media files fetch করা।
    /// </summary>
    [UseFiltering]
    [UseSorting]
    public async Task<List<Media>> GetMedia(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Media
            .Include(m => m.UploadedBy)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Media?> GetMediaById(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Media
            .Include(m => m.UploadedBy)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // ========== Offset Pagination (Phase 5) ==========

    /// <summary>
    /// GraphQL শেখার পয়েন্ট: OFFSET PAGINATION (Skip & Take)
    /// </summary>
    public async Task<PostPaginationResult> GetPostsOffset(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        int skip = 0,
        int take = 10)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var total = await context.Posts.CountAsync();
        var posts = await context.Posts
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return new PostPaginationResult(posts, total, skip, take, skip + take < total);
    }
}

/// <summary>
/// Offset Pagination payload
/// </summary>
public record PostPaginationResult(
    List<Post> Items,
    int TotalCount,
    int Skip,
    int Take,
    bool HasNextPage
);

/// <summary>
/// Blog statistics — computed query result type।
/// </summary>
public record BlogStats(
    int TotalPosts,
    int PublishedPosts,
    int DraftPosts,
    int TotalUsers,
    int TotalComments,
    int TotalLikes,
    int TotalCategories,
    int TotalTags
);
