using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Quillow.Core.Entities;
using Quillow.Core.Enums;
using Quillow.Infrastructure.Data;

using CoreTag = Quillow.Core.Entities.Tag;

namespace Quillow.API.GraphQL.Mutations;

/// <summary>
/// GraphQL শেখার পয়েন্ট: MUTATION — Data তৈরি/পরিবর্তন/মুছে ফেলার জন্য।
/// 
/// REST API তে POST, PUT, DELETE ব্যবহার করতে,
/// GraphQL এ সব কিছু Mutation দিয়ে করা হয়:
/// 
/// mutation {
///   createPost(input: { title: "Hello", content: "World", categoryId: "..." }) {
///     post { id title slug }
///     errors { message code }
///   }
/// }
/// </summary>
public class Mutation
{
    // ========== Post Mutations ==========

    /// <summary>
    /// নতুন Post তৈরি করা।
    /// 
    /// শেখার পয়েন্ট: Input Type — CreatePostInput আলাদা type হিসেবে define করা হয়েছে
    /// যাতে client কে clear structure দেওয়া যায়।
    /// 
    /// Payload Pattern — response এ post ও errors দুইটাই থাকে,
    /// যাতে partial success handle করা যায়।
    /// </summary>
    public async Task<CreatePostPayload> CreatePost(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        CreatePostInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        // Validation
        var errors = new List<UserError>();

        if (string.IsNullOrWhiteSpace(input.Title))
            errors.Add(new UserError("Title is required", "VALIDATION_ERROR", "title"));

        if (string.IsNullOrWhiteSpace(input.Content))
            errors.Add(new UserError("Content is required", "VALIDATION_ERROR", "content"));

        var categoryExists = await context.Categories.AnyAsync(c => c.Id == input.CategoryId);
        if (!categoryExists)
            errors.Add(new UserError("Category not found", "NOT_FOUND", "categoryId"));

        var authorExists = await context.Users.AnyAsync(u => u.Id == input.AuthorId);
        if (!authorExists)
            errors.Add(new UserError("Author not found", "NOT_FOUND", "authorId"));

        if (errors.Count > 0)
            return new CreatePostPayload(null, errors);

        var post = new Post
        {
            Title = input.Title,
            Slug = GenerateSlug(input.Title),
            Content = input.Content,
            Excerpt = input.Excerpt,
            Status = input.Status ?? PostStatus.Draft,
            AuthorId = input.AuthorId,
            CategoryId = input.CategoryId
        };

        if (post.Status == PostStatus.Published)
            post.PublishedAt = DateTime.UtcNow;

        context.Posts.Add(post);

        if (input.TagIds?.Count > 0)
        {
            foreach (var tagId in input.TagIds)
            {
                context.PostTags.Add(new PostTag { PostId = post.Id, TagId = tagId });
            }
        }

        await context.SaveChangesAsync();

        var createdPost = await context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstAsync(p => p.Id == post.Id);

        return new CreatePostPayload(createdPost, null);
    }

    /// <summary>
    /// Post update করা — শুধু যেসব field পাঠানো হবে সেগুলোই update হবে।
    /// </summary>
    public async Task<UpdatePostPayload> UpdatePost(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        Guid id,
        UpdatePostInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var post = await context.Posts.FindAsync(id);

        if (post == null)
            return new UpdatePostPayload(null, [new UserError("Post not found", "NOT_FOUND", "id")]);

        if (input.Title != null)
        {
            post.Title = input.Title;
            post.Slug = GenerateSlug(input.Title);
        }

        if (input.Content != null)
            post.Content = input.Content;

        if (input.Excerpt != null)
            post.Excerpt = input.Excerpt;

        if (input.CategoryId.HasValue)
            post.CategoryId = input.CategoryId.Value;

        if (input.Status.HasValue)
        {
            post.Status = input.Status.Value;
            if (input.Status == PostStatus.Published && post.PublishedAt == null)
                post.PublishedAt = DateTime.UtcNow;
        }

        if (input.IsFeatured.HasValue)
            post.IsFeatured = input.IsFeatured.Value;

        await context.SaveChangesAsync();

        var updatedPost = await context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstAsync(p => p.Id == id);

        return new UpdatePostPayload(updatedPost, null);
    }

    /// <summary>
    /// Post delete করা।
    /// </summary>
    public async Task<DeletePostPayload> DeletePost(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var post = await context.Posts.FindAsync(id);

        if (post == null)
            return new DeletePostPayload(false, [new UserError("Post not found", "NOT_FOUND", "id")]);

        context.Posts.Remove(post);
        await context.SaveChangesAsync();

        return new DeletePostPayload(true, null);
    }

    // ========== Comment Mutations ==========

    public async Task<AddCommentPayload> AddComment(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        AddCommentInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var postExists = await context.Posts.AnyAsync(p => p.Id == input.PostId);
        if (!postExists)
            return new AddCommentPayload(null, [new UserError("Post not found", "NOT_FOUND", "postId")]);

        var comment = new Comment
        {
            Content = input.Content,
            PostId = input.PostId,
            AuthorId = input.AuthorId,
            ParentCommentId = input.ParentCommentId
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        var createdComment = await context.Comments
            .Include(c => c.Author)
            .Include(c => c.Post)
            .FirstAsync(c => c.Id == comment.Id);

        return new AddCommentPayload(createdComment, null);
    }

    // ========== Like Mutations ==========

    /// <summary>
    /// Toggle Like — like করা থাকলে unlike করবে, না থাকলে like করবে।
    /// </summary>
    public async Task<ToggleLikePayload> ToggleLike(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        Guid postId,
        Guid userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var existingLike = await context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        if (existingLike != null)
        {
            context.Likes.Remove(existingLike);
            await context.SaveChangesAsync();
            return new ToggleLikePayload(false, "Post unliked");
        }

        var like = new Like { PostId = postId, UserId = userId };
        context.Likes.Add(like);
        await context.SaveChangesAsync();

        return new ToggleLikePayload(true, "Post liked");
    }

    // ========== Category Mutations ==========

    public async Task<Category> CreateCategory(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        CreateCategoryInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var category = new Category
        {
            Name = input.Name,
            Slug = GenerateSlug(input.Name),
            Description = input.Description,
            ParentCategoryId = input.ParentCategoryId
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return category;
    }

    // ========== Tag Mutations ==========

    public async Task<CoreTag> CreateTag(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        string name)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var tag = new CoreTag
        {
            Name = name,
            Slug = GenerateSlug(name)
        };

        context.Tags.Add(tag);
        await context.SaveChangesAsync();

        return tag;
    }

    // ========== Helper Methods ==========

    private static string GenerateSlug(string text)
    {
        var slug = text.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\u0980-\u09FF\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}

// ========== Input Types ==========

public record CreatePostInput(
    string Title,
    string Content,
    string? Excerpt,
    Guid AuthorId,
    Guid CategoryId,
    PostStatus? Status,
    List<Guid>? TagIds
);

public record UpdatePostInput(
    string? Title,
    string? Content,
    string? Excerpt,
    Guid? CategoryId,
    PostStatus? Status,
    bool? IsFeatured
);

public record AddCommentInput(
    string Content,
    Guid PostId,
    Guid AuthorId,
    Guid? ParentCommentId
);

public record CreateCategoryInput(
    string Name,
    string? Description,
    Guid? ParentCategoryId
);

// ========== Payload Types ==========

public record CreatePostPayload(Post? Post, List<UserError>? Errors);
public record UpdatePostPayload(Post? Post, List<UserError>? Errors);
public record DeletePostPayload(bool Success, List<UserError>? Errors);
public record AddCommentPayload(Comment? Comment, List<UserError>? Errors);
public record ToggleLikePayload(bool IsLiked, string Message);
public record UserError(string Message, string Code, string? Field);
