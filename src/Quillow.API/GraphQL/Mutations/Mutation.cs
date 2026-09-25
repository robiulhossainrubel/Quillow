using System.Security.Claims;
using System.Text.RegularExpressions;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Quillow.API.GraphQL.Subscriptions;
using Quillow.API.GraphQL.Types;
using Quillow.Core.Entities;
using Quillow.Core.Enums;
using Quillow.Infrastructure.Data;
using Quillow.Services.Auth;

using CoreTag = Quillow.Core.Entities.Tag;

namespace Quillow.API.GraphQL.Mutations;

/// <summary>
/// GraphQL শেখার পয়েন্ট: MUTATION — Data তৈরি/পরিবর্তন/মুছে ফেলার জন্য।
/// 
/// Phase 3 Updates:
/// - Authentication Mutations: Register ও Login (JWT Token generation)
/// - [Authorize] attribute দিয়ে resolver level protection
/// - Role-based authorization: [Authorize(Roles = new[] { "Admin" })]
/// - Current User context: ClaimsPrincipal থেকে স্বয়ংক্রিয়ভাবে User ID উদ্ধার
/// </summary>
public class Mutation
{
    // ========== Authentication Mutations ==========

    /// <summary>
    /// নতুন User রেজিস্ট্রেশন করা এবং সাথে সাথে JWT Access Token প্রদান করা।
    /// </summary>
    public async Task<AuthPayload> Register(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        [Service] IPasswordHasher passwordHasher,
        [Service] ITokenService tokenService,
        RegisterInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var errors = new List<UserError>();

        // Validations
        if (string.IsNullOrWhiteSpace(input.UserName))
            errors.Add(new UserError("Username is required", "VALIDATION_ERROR", "userName"));

        if (string.IsNullOrWhiteSpace(input.Email) || !input.Email.Contains('@'))
            errors.Add(new UserError("Valid email is required", "VALIDATION_ERROR", "email"));

        if (string.IsNullOrWhiteSpace(input.Password) || input.Password.Length < 6)
            errors.Add(new UserError("Password must be at least 6 characters", "VALIDATION_ERROR", "password"));

        if (string.IsNullOrWhiteSpace(input.DisplayName))
            errors.Add(new UserError("Display name is required", "VALIDATION_ERROR", "displayName"));

        if (errors.Any())
            return new AuthPayload(null, null, errors);

        // Check if email or username already exists
        var emailExists = await context.Users.AnyAsync(u => u.Email.ToLower() == input.Email.ToLower());
        if (emailExists)
            return new AuthPayload(null, null, [new UserError("Email is already registered", "DUPLICATE_EMAIL", "email")]);

        var userNameExists = await context.Users.AnyAsync(u => u.UserName.ToLower() == input.UserName.ToLower());
        if (userNameExists)
            return new AuthPayload(null, null, [new UserError("Username is already taken", "DUPLICATE_USERNAME", "userName")]);

        // Default role: Reader or Author
        var defaultRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Editor")
            ?? await context.Roles.FirstOrDefaultAsync(r => r.Name == "Reader");

        var user = new User
        {
            UserName = input.UserName.Trim(),
            Email = input.Email.Trim().ToLower(),
            DisplayName = input.DisplayName.Trim(),
            Bio = input.Bio?.Trim(),
            PasswordHash = passwordHasher.HashPassword(input.Password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var roles = new List<string>();
        if (defaultRole != null)
        {
            context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = defaultRole.Id });
            await context.SaveChangesAsync();
            roles.Add(defaultRole.Name);
        }

        var token = tokenService.GenerateAccessToken(user, roles);
        return new AuthPayload(token, user, null);
    }

    /// <summary>
    /// User Login — Email ও Password যাচাই করে JWT Token ফেরত দেওয়া।
    /// </summary>
    public async Task<AuthPayload> Login(
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        [Service] IPasswordHasher passwordHasher,
        [Service] ITokenService tokenService,
        LoginInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password))
        {
            return new AuthPayload(null, null, [
                new UserError("Email and password are required", "VALIDATION_ERROR")
            ]);
        }

        var user = await context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == input.Email.ToLower());

        if (user == null)
        {
            return new AuthPayload(null, null, [
                new UserError("Invalid email or password", "INVALID_CREDENTIALS")
            ]);
        }

        // Demo seed user compatibility: যদি password "hashed_password_here" হয়,
        // তবে "Password@123" বা "Admin@123" গ্রহণ করে автоматически BCrypt hash এ রূপান্তর করবে
        var isPasswordValid = false;
        if (user.PasswordHash == "hashed_password_here" && 
            (input.Password == "Password@123" || input.Password == "Admin@123"))
        {
            user.PasswordHash = passwordHasher.HashPassword(input.Password);
            await context.SaveChangesAsync();
            isPasswordValid = true;
        }
        else
        {
            isPasswordValid = passwordHasher.VerifyPassword(input.Password, user.PasswordHash);
        }

        if (!isPasswordValid)
        {
            return new AuthPayload(null, null, [
                new UserError("Invalid email or password", "INVALID_CREDENTIALS")
            ]);
        }

        var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(r => !string.IsNullOrEmpty(r)).ToList();
        var token = tokenService.GenerateAccessToken(user, roles);

        return new AuthPayload(token, user, null);
    }

    // ========== Post Mutations ==========

    /// <summary>
    /// নতুন Post তৈরি করা।
    /// 
    /// 🔐 [Authorize] — শুধু লগইন করা ব্যবহারকারী পোস্ট করতে পারবেন।
    /// AuthorId ক্লায়েন্ট থেকে পাঠানোর প্রয়োজন নেই, JWT Token থেকে স্বয়ংক্রিয়ভাবে নেওয়া হয়।
    /// </summary>
    [Authorize]
    public async Task<CreatePostPayload> CreatePost(
        ClaimsPrincipal claimsPrincipal,
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        CreatePostInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var currentUserId = claimsPrincipal.GetUserId();
        var authorId = input.AuthorId ?? currentUserId;

        if (authorId == null)
            return new CreatePostPayload(null, [new UserError("Authentication required", "UNAUTHORIZED")]);

        var errors = new List<UserError>();

        if (string.IsNullOrWhiteSpace(input.Title))
            errors.Add(new UserError("Title is required", "VALIDATION_ERROR", "title"));

        if (string.IsNullOrWhiteSpace(input.Content))
            errors.Add(new UserError("Content is required", "VALIDATION_ERROR", "content"));

        var categoryExists = await context.Categories.AnyAsync(c => c.Id == input.CategoryId);
        if (!categoryExists)
            errors.Add(new UserError("Category not found", "NOT_FOUND", "categoryId"));

        if (errors.Any())
            return new CreatePostPayload(null, errors);

        var slug = GenerateSlug(input.Title);
        var baseSlug = slug;
        var count = 1;
        while (await context.Posts.AnyAsync(p => p.Slug == slug))
        {
            slug = $"{baseSlug}-{count++}";
        }

        var post = new Post
        {
            Title = input.Title,
            Slug = slug,
            Content = input.Content,
            Excerpt = input.Excerpt ?? (input.Content.Length > 200 ? input.Content[..200] + "..." : input.Content),
            AuthorId = authorId.Value,
            CategoryId = input.CategoryId,
            Status = input.Status ?? PostStatus.Draft,
            PublishedAt = (input.Status == PostStatus.Published) ? DateTime.UtcNow : null
        };

        if (input.TagIds != null && input.TagIds.Any())
        {
            foreach (var tagId in input.TagIds)
            {
                post.PostTags.Add(new PostTag { PostId = post.Id, TagId = tagId });
            }
        }

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var createdPost = await context.Posts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .FirstAsync(p => p.Id == post.Id);

        return new CreatePostPayload(createdPost, null);
    }

    /// <summary>
    /// Post update করা — শুধু পোস্টের লেখক বা Admin আপডেট করতে পারবেন।
    /// </summary>
    [Authorize]
    public async Task<UpdatePostPayload> UpdatePost(
        ClaimsPrincipal claimsPrincipal,
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        [Service] ITopicEventSender eventSender,
        Guid id,
        UpdatePostInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var post = await context.Posts.FindAsync(id);

        if (post == null)
            return new UpdatePostPayload(null, [new UserError("Post not found", "NOT_FOUND", "id")]);

        var currentUserId = claimsPrincipal.GetUserId();
        var isAdmin = claimsPrincipal.IsInRole("Admin");

        // Authorization check: Only author or Admin can edit!
        if (post.AuthorId != currentUserId && !isAdmin)
        {
            return new UpdatePostPayload(null, [
                new UserError("You are not authorized to update this post", "FORBIDDEN")
            ]);
        }

        var wasPublished = post.Status == PostStatus.Published;

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

        // 🔔 Subscription event publish if newly published
        if (!wasPublished && updatedPost.Status == PostStatus.Published)
        {
            await eventSender.SendAsync("PostPublished", updatedPost);
        }

        return new UpdatePostPayload(updatedPost, null);
    }

    /// <summary>
    /// Post delete করা — শুধু পোস্টের লেখক বা Admin ডিলিট করতে পারবেন।
    /// </summary>
    [Authorize]
    public async Task<DeletePostPayload> DeletePost(
        ClaimsPrincipal claimsPrincipal,
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var post = await context.Posts.FindAsync(id);

        if (post == null)
            return new DeletePostPayload(false, [new UserError("Post not found", "NOT_FOUND", "id")]);

        var currentUserId = claimsPrincipal.GetUserId();
        var isAdmin = claimsPrincipal.IsInRole("Admin");

        // Authorization check: Only author or Admin can delete!
        if (post.AuthorId != currentUserId && !isAdmin)
        {
            return new DeletePostPayload(false, [
                new UserError("You are not authorized to delete this post", "FORBIDDEN")
            ]);
        }

        context.Posts.Remove(post);
        await context.SaveChangesAsync();

        return new DeletePostPayload(true, null);
    }

    // ========== Comment Mutations ==========

    /// <summary>
    /// নতুন কমেন্ট যোগ করা — লগইন করা ইউজারের ID স্বয়ংক্রিয়ভাবে নেওয়া হবে।
    /// </summary>
    [Authorize]
    public async Task<AddCommentPayload> AddComment(
        ClaimsPrincipal claimsPrincipal,
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        [Service] ITopicEventSender eventSender,
        AddCommentInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var currentUserId = claimsPrincipal.GetUserId();
        var authorId = input.AuthorId ?? currentUserId;

        if (authorId == null)
            return new AddCommentPayload(null, [new UserError("Authentication required", "UNAUTHORIZED")]);

        var postExists = await context.Posts.AnyAsync(p => p.Id == input.PostId);
        if (!postExists)
            return new AddCommentPayload(null, [new UserError("Post not found", "NOT_FOUND", "postId")]);

        var comment = new Comment
        {
            Content = input.Content,
            PostId = input.PostId,
            AuthorId = authorId.Value,
            ParentCommentId = input.ParentCommentId
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        var createdComment = await context.Comments
            .Include(c => c.Author)
            .Include(c => c.Post)
            .FirstAsync(c => c.Id == comment.Id);

        // 🔔 Subscription event publish!
        await eventSender.SendAsync($"CommentAdded_{input.PostId}", createdComment);

        return new AddCommentPayload(createdComment, null);
    }

    // ========== Like Mutations ==========

    /// <summary>
    /// Toggle Like — লগইন করা ইউজারের ভিত্তিতে লাইক/আনলাইক।
    /// </summary>
    [Authorize]
    public async Task<ToggleLikePayload> ToggleLike(
        ClaimsPrincipal claimsPrincipal,
        [Service] IDbContextFactory<QuilllowDbContext> contextFactory,
        [Service] ITopicEventSender eventSender,
        Guid postId,
        Guid? userId = null)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var currentUserId = claimsPrincipal.GetUserId();
        var effectiveUserId = userId ?? currentUserId;

        if (effectiveUserId == null)
            return new ToggleLikePayload(false, "Authentication required");

        var existingLike = await context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == effectiveUserId.Value);

        var user = await context.Users.FindAsync(effectiveUserId.Value);
        var userDisplayName = user?.DisplayName ?? "Unknown";

        if (existingLike != null)
        {
            context.Likes.Remove(existingLike);
            await context.SaveChangesAsync();
            var totalLikes = await context.Likes.CountAsync(l => l.PostId == postId);

            // 🔔 Like subscription event
            await eventSender.SendAsync($"PostLiked_{postId}",
                new LikeNotification(postId, effectiveUserId.Value, userDisplayName, false, totalLikes));

            return new ToggleLikePayload(false, "Post unliked");
        }

        var like = new Like { PostId = postId, UserId = effectiveUserId.Value };
        context.Likes.Add(like);
        await context.SaveChangesAsync();
        var totalLikesCount = await context.Likes.CountAsync(l => l.PostId == postId);

        // 🔔 Like subscription event
        await eventSender.SendAsync($"PostLiked_{postId}",
            new LikeNotification(postId, effectiveUserId.Value, userDisplayName, true, totalLikesCount));

        return new ToggleLikePayload(true, "Post liked");
    }

    // ========== Category Mutations ==========

    /// <summary>
    /// শুধু Admin রোল থাকা ব্যবহারকারী নতুন Category তৈরি করতে পারবেন!
    /// </summary>
    [Authorize(Roles = new[] { "Admin" })]
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

    [Authorize]
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
    Guid? AuthorId,
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
    Guid? AuthorId,
    Guid? ParentCommentId
);

public record CreateCategoryInput(
    string Name,
    string? Description,
    Guid? ParentCategoryId
);

// ========== Payload Types ==========

public record CreatePostPayload(
    Post? Post,
    IReadOnlyList<UserError>? Errors
);

public record UpdatePostPayload(
    Post? Post,
    IReadOnlyList<UserError>? Errors
);

public record DeletePostPayload(
    bool Success,
    IReadOnlyList<UserError>? Errors
);

public record AddCommentPayload(
    Comment? Comment,
    IReadOnlyList<UserError>? Errors
);

public record ToggleLikePayload(
    bool IsLiked,
    string Message
);

public record UserError(
    string Message,
    string Code,
    string? Field = null
);
