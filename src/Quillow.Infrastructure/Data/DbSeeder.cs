using Microsoft.EntityFrameworkCore;
using Quillow.Core.Entities;
using Quillow.Core.Enums;

namespace Quillow.Infrastructure.Data;

/// <summary>
/// Database এ initial seed data দেওয়ার জন্য।
/// প্রথমবার run করলে কিছু sample data থাকবে যাতে GraphQL Playground এ test করতে পারো।
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(QuilllowDbContext context)
    {
        // Already seeded হলে skip করো
        if (await context.Users.AnyAsync())
            return;

        // ===== Roles =====
        var adminRole = new Role { Name = "Admin" };
        var editorRole = new Role { Name = "Editor" };
        var readerRole = new Role { Name = "Reader" };
        context.Roles.AddRange(adminRole, editorRole, readerRole);

        // ===== Users =====
        var admin = new User
        {
            UserName = "admin",
            Email = "admin@quillow.com",
            PasswordHash = "hashed_password_here", // পরে proper hashing করবো
            DisplayName = "Quillow Admin",
            Bio = "Quillow Blog Platform এর Admin"
        };

        var editor = new User
        {
            UserName = "robiulhossain",
            Email = "robiul@quillow.com",
            PasswordHash = "hashed_password_here",
            DisplayName = "Robiul Hossain",
            Bio = "GraphQL শিখছি, একজন passionate developer"
        };

        var reader = new User
        {
            UserName = "reader01",
            Email = "reader@quillow.com",
            PasswordHash = "hashed_password_here",
            DisplayName = "Regular Reader",
            Bio = "I love reading tech blogs!"
        };

        context.Users.AddRange(admin, editor, reader);

        // ===== User Roles =====
        context.UserRoles.AddRange(
            new UserRole { UserId = admin.Id, RoleId = adminRole.Id },
            new UserRole { UserId = editor.Id, RoleId = editorRole.Id },
            new UserRole { UserId = reader.Id, RoleId = readerRole.Id }
        );

        // ===== Categories =====
        var techCategory = new Category
        {
            Name = "Technology",
            Slug = "technology",
            Description = "Technology related blog posts"
        };

        var webDevCategory = new Category
        {
            Name = "Web Development",
            Slug = "web-development",
            Description = "Everything about web development",
            ParentCategoryId = techCategory.Id
        };

        var graphqlCategory = new Category
        {
            Name = "GraphQL",
            Slug = "graphql",
            Description = "GraphQL tutorials and guides",
            ParentCategoryId = webDevCategory.Id
        };

        var lifestyleCategory = new Category
        {
            Name = "Lifestyle",
            Slug = "lifestyle",
            Description = "Lifestyle and productivity tips"
        };

        context.Categories.AddRange(techCategory, webDevCategory, graphqlCategory, lifestyleCategory);

        // ===== Tags =====
        var dotnetTag = new Tag { Name = ".NET", Slug = "dotnet" };
        var graphqlTag = new Tag { Name = "GraphQL", Slug = "graphql" };
        var csharpTag = new Tag { Name = "C#", Slug = "csharp" };
        var beginnerTag = new Tag { Name = "Beginner", Slug = "beginner" };
        var tutorialTag = new Tag { Name = "Tutorial", Slug = "tutorial" };

        context.Tags.AddRange(dotnetTag, graphqlTag, csharpTag, beginnerTag, tutorialTag);

        // ===== Posts =====
        var post1 = new Post
        {
            Title = "GraphQL কি এবং কেন শিখবো?",
            Slug = "graphql-ki-ebong-keno-shikhbo",
            Content = "GraphQL হলো একটা query language যেটা Facebook তৈরি করেছে...",
            Excerpt = "GraphQL এর basic concepts ও কেন REST এর চেয়ে ভালো তা জানুন",
            Status = PostStatus.Published,
            IsFeatured = true,
            ViewCount = 150,
            PublishedAt = DateTime.UtcNow.AddDays(-5),
            AuthorId = editor.Id,
            CategoryId = graphqlCategory.Id
        };

        var post2 = new Post
        {
            Title = "Hot Chocolate দিয়ে .NET এ GraphQL Server",
            Slug = "hot-chocolate-diye-dotnet-e-graphql-server",
            Content = "Hot Chocolate হলো .NET এর জন্য সবচেয়ে popular GraphQL server...",
            Excerpt = ".NET Core দিয়ে কিভাবে GraphQL API বানাবেন শিখুন",
            Status = PostStatus.Published,
            IsFeatured = true,
            ViewCount = 230,
            PublishedAt = DateTime.UtcNow.AddDays(-3),
            AuthorId = editor.Id,
            CategoryId = graphqlCategory.Id
        };

        var post3 = new Post
        {
            Title = "EF Core দিয়ে Database Design",
            Slug = "ef-core-diye-database-design",
            Content = "Entity Framework Core হলো .NET এর official ORM...",
            Excerpt = "EF Core দিয়ে কিভাবে clean database design করবেন",
            Status = PostStatus.Draft,
            AuthorId = editor.Id,
            CategoryId = webDevCategory.Id
        };

        context.Posts.AddRange(post1, post2, post3);

        // ===== PostTags =====
        context.PostTags.AddRange(
            new PostTag { PostId = post1.Id, TagId = graphqlTag.Id },
            new PostTag { PostId = post1.Id, TagId = beginnerTag.Id },
            new PostTag { PostId = post1.Id, TagId = tutorialTag.Id },
            new PostTag { PostId = post2.Id, TagId = graphqlTag.Id },
            new PostTag { PostId = post2.Id, TagId = dotnetTag.Id },
            new PostTag { PostId = post2.Id, TagId = csharpTag.Id },
            new PostTag { PostId = post3.Id, TagId = dotnetTag.Id },
            new PostTag { PostId = post3.Id, TagId = csharpTag.Id }
        );

        // ===== Comments =====
        var comment1 = new Comment
        {
            Content = "অসাধারণ পোস্ট! GraphQL শেখা শুরু করবো।",
            PostId = post1.Id,
            AuthorId = reader.Id
        };

        var reply1 = new Comment
        {
            Content = "ধন্যবাদ! কোনো প্রশ্ন থাকলে জানাবেন।",
            PostId = post1.Id,
            AuthorId = editor.Id,
            ParentCommentId = comment1.Id
        };

        var comment2 = new Comment
        {
            Content = "Hot Chocolate সত্যিই দারুণ library!",
            PostId = post2.Id,
            AuthorId = reader.Id
        };

        context.Comments.AddRange(comment1, reply1, comment2);

        // ===== Likes =====
        context.Likes.AddRange(
            new Like { PostId = post1.Id, UserId = reader.Id },
            new Like { PostId = post1.Id, UserId = admin.Id },
            new Like { PostId = post2.Id, UserId = reader.Id }
        );

        await context.SaveChangesAsync();
    }
}
