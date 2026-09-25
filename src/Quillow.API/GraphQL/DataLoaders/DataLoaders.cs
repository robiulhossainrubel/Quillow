using Microsoft.EntityFrameworkCore;
using Quillow.Core.Entities;
using Quillow.Infrastructure.Data;

namespace Quillow.API.GraphQL.DataLoaders;

/// <summary>
/// GraphQL শেখার পয়েন্ট: DataLoader — N+1 Problem সমাধান।
/// 
/// ❌ সমস্যা (N+1):
/// যদি 10টা post আনি, প্রতিটার author আলাদা query তে আসে = 1 + 10 = 11 queries!
/// query {
///   posts {                    ← 1 query: SELECT * FROM Posts
///     author { displayName }   ← 10 queries: SELECT * FROM Users WHERE Id = @id (প্রতিটার জন্য!)
///   }
/// }
/// 
/// ✅ DataLoader দিয়ে সমাধান:
/// সব author IDs জমা করে, একবারেই সব authors আনে = মোট 2 queries!
/// SELECT * FROM Posts
/// SELECT * FROM Users WHERE Id IN (@id1, @id2, @id3, ...) ← একটাই query!
/// 
/// BatchDataLoader: Multiple IDs দিলে একবারে সব data দেয় (batch by key)
/// GroupDataLoader: একটা key র জন্য multiple results দেয় (batch by group)
/// </summary>
public class AuthorByIdDataLoader : BatchDataLoader<Guid, User>
{
    private readonly IDbContextFactory<QuilllowDbContext> _contextFactory;

    public AuthorByIdDataLoader(
        IDbContextFactory<QuilllowDbContext> contextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// এই method একবারে সব author IDs নিয়ে একটাই database query চালায়।
    /// Hot Chocolate automatically সব resolver থেকে IDs collect করে, তারপর এখানে পাঠায়।
    /// </summary>
    protected override async Task<IReadOnlyDictionary<Guid, User>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Users
            .Where(u => keys.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);
    }
}

/// <summary>
/// Post এর Category load করার জন্য DataLoader।
/// </summary>
public class CategoryByIdDataLoader : BatchDataLoader<Guid, Category>
{
    private readonly IDbContextFactory<QuilllowDbContext> _contextFactory;

    public CategoryByIdDataLoader(
        IDbContextFactory<QuilllowDbContext> contextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _contextFactory = contextFactory;
    }

    protected override async Task<IReadOnlyDictionary<Guid, Category>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Categories
            .Where(c => keys.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);
    }
}

/// <summary>
/// GroupDataLoader — একটা PostId র জন্য multiple comments return করে।
/// 
/// BatchDataLoader: 1 key → 1 result (Author by ID)
/// GroupDataLoader: 1 key → many results (Comments by PostId)
/// </summary>
public class CommentsByPostIdDataLoader : GroupedDataLoader<Guid, Comment>
{
    private readonly IDbContextFactory<QuilllowDbContext> _contextFactory;

    public CommentsByPostIdDataLoader(
        IDbContextFactory<QuilllowDbContext> contextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _contextFactory = contextFactory;
    }

    protected override async Task<ILookup<Guid, Comment>> LoadGroupedBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var comments = await context.Comments
            .Where(c => keys.Contains(c.PostId))
            .Include(c => c.Author)
            .ToListAsync(cancellationToken);

        return comments.ToLookup(c => c.PostId);
    }
}

/// <summary>
/// Post এর Like count load করার DataLoader — count aggregation এর জন্য।
/// </summary>
public class LikeCountByPostIdDataLoader : BatchDataLoader<Guid, int>
{
    private readonly IDbContextFactory<QuilllowDbContext> _contextFactory;

    public LikeCountByPostIdDataLoader(
        IDbContextFactory<QuilllowDbContext> contextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _contextFactory = contextFactory;
    }

    protected override async Task<IReadOnlyDictionary<Guid, int>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Likes
            .Where(l => keys.Contains(l.PostId))
            .GroupBy(l => l.PostId)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Count(),
                cancellationToken);
    }
}

/// <summary>
/// Post এর Tags load করার DataLoader।
/// </summary>
public class TagsByPostIdDataLoader : GroupedDataLoader<Guid, Core.Entities.Tag>
{
    private readonly IDbContextFactory<QuilllowDbContext> _contextFactory;

    public TagsByPostIdDataLoader(
        IDbContextFactory<QuilllowDbContext> contextFactory,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _contextFactory = contextFactory;
    }

    protected override async Task<ILookup<Guid, Core.Entities.Tag>> LoadGroupedBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var postTags = await context.PostTags
            .Where(pt => keys.Contains(pt.PostId))
            .Include(pt => pt.Tag)
            .ToListAsync(cancellationToken);

        return postTags.ToLookup(pt => pt.PostId, pt => pt.Tag);
    }
}
