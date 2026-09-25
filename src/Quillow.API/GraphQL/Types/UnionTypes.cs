using Microsoft.EntityFrameworkCore;
using Quillow.Core.Entities;
using Quillow.Infrastructure.Data;

namespace Quillow.API.GraphQL.Types;

/// <summary>
/// GraphQL শেখার পয়েন্ট: UNION TYPE
/// 
/// Union হলো "অনেকগুলো type এর মধ্যে যেকোনো একটা" — OR condition এর মতো।
/// এটা search functionality তে অসাধারণ কাজে লাগে!
/// 
/// query {
///   search(term: "GraphQL") {
///     ... on Post {          ← যদি Post হয়
///       title
///       excerpt
///     }
///     ... on User {          ← যদি User হয়
///       displayName
///       email
///     }
///     ... on Category {      ← যদি Category হয়
///       name
///       description
///     }
///   }
/// }
/// 
/// Union vs Interface:
/// - Interface: সব type এ common fields থাকতে হয়
/// - Union: type গুলো সম্পূর্ণ আলাদা হতে পারে, common field দরকার নেই
/// </summary>
public class SearchResultType : UnionType
{
    protected override void Configure(IUnionTypeDescriptor descriptor)
    {
        descriptor.Name("SearchResult");
        descriptor.Type<PostType>();
        descriptor.Type<ObjectType<User>>();
        descriptor.Type<CategoryType>();
    }
}

/// <summary>
/// Search query — সব entity তে একসাথে search করে Union type এ result দেয়।
/// </summary>
public static class SearchQueryExtensions
{
    public static async Task<List<object>> SearchAsync(
        IDbContextFactory<QuilllowDbContext> contextFactory,
        string term)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var results = new List<object>();

        // Posts এ search
        var posts = await context.Posts
            .Where(p => p.Title.Contains(term) || p.Content.Contains(term) || p.Excerpt!.Contains(term))
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Take(10)
            .ToListAsync();
        results.AddRange(posts);

        // Users এ search
        var users = await context.Users
            .Where(u => u.DisplayName.Contains(term) || u.UserName.Contains(term) || u.Bio!.Contains(term))
            .Take(5)
            .ToListAsync();
        results.AddRange(users);

        // Categories এ search
        var categories = await context.Categories
            .Where(c => c.Name.Contains(term) || c.Description!.Contains(term))
            .Take(5)
            .ToListAsync();
        results.AddRange(categories);

        return results;
    }
}
