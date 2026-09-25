using Microsoft.EntityFrameworkCore;
using Quillow.Core.Entities;
using Quillow.Core.Enums;
using Quillow.Infrastructure.Data;

using CoreTag = Quillow.Core.Entities.Tag;

namespace Quillow.API.GraphQL.Queries;

/// <summary>
/// GraphQL শেখার পয়েন্ট: QUERY — Data পড়ার (Read) জন্য।
/// 
/// REST API তে যেখানে GET /api/posts, GET /api/posts/1 ইত্যাদি endpoint লাগতো,
/// GraphQL এ একটাই endpoint (/graphql) — client বলে কী data চাই:
/// 
/// query {
///   posts { id title author { displayName } }
/// }
/// 
/// Hot Chocolate v16+ এ DbContext inject করার জন্য Service attribute ব্যবহার করা হয়।
/// IDbContextFactory থেকে context তৈরি হয়, যাতে প্রতিটা request এ আলাদা context পাই।
/// </summary>
public class Query
{
    /// <summary>
    /// সব posts পাওয়ার জন্য — filtering, sorting সহ।
    /// 
    /// Example:
    /// query {
    ///   posts(where: { status: { eq: PUBLISHED } }, order: { createdAt: DESC }) {
    ///     id title slug excerpt author { displayName }
    ///   }
    /// }
    /// </summary>
    [UseFiltering]
    [UseSorting]
    public async Task<List<Post>> GetPosts(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .ToListAsync();
    }

    /// <summary>
    /// ID দিয়ে একটা post খুঁজে বের করা।
    /// 
    /// Example:
    /// query {
    ///   postById(id: "some-guid-here") {
    ///     title content author { displayName } comments { content author { displayName } }
    ///   }
    /// }
    /// </summary>
    public async Task<Post?> GetPostById(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Comments).ThenInclude(c => c.Author)
            .Include(p => p.Comments).ThenInclude(c => c.Replies)
            .Include(p => p.Likes)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Slug দিয়ে post খোঁজা — frontend এ URL /blog/my-first-post থেকে data fetch করতে কাজে লাগবে।
    /// </summary>
    public async Task<Post?> GetPostBySlug(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        string slug)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Comments).ThenInclude(c => c.Author)
            .Include(p => p.Comments).ThenInclude(c => c.Replies)
            .Include(p => p.Likes)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug);
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
            .Include(p => p.Author)
            .Include(p => p.Category)
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
        return await context.Users
            .Include(u => u.Posts)
            .Include(u => u.Comments)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<User?> GetUserById(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Users
            .Include(u => u.Posts)
            .Include(u => u.Comments)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    // ========== Category Queries ==========

    [UseFiltering]
    [UseSorting]
    public async Task<List<Category>> GetCategories(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Categories
            .Include(c => c.Posts)
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
            .Include(c => c.Posts).ThenInclude(p => p.Author)
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
        return await context.Tags
            .Include(t => t.PostTags)
                .ThenInclude(pt => pt.Post)
            .ToListAsync();
    }

    // ========== Comment Queries ==========

    /// <summary>
    /// একটা post এর সব top-level comments (replies না) পাওয়ার জন্য।
    /// </summary>
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
}
