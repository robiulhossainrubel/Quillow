using Quillow.API.GraphQL.DataLoaders;
using Quillow.Core.Entities;

namespace Quillow.API.GraphQL.Types;

/// <summary>
/// GraphQL শেখার পয়েন্ট: Post Type — এবার DataLoader দিয়ে optimize করা!
/// 
/// আগে Include() দিয়ে সব data একবারে আনতাম (eager loading)।
/// এখন DataLoader দিয়ে শুধু যখন দরকার তখনই আনবে (lazy but batched)।
/// 
/// এর ফলে:
/// - N+1 problem সমাধান হবে
/// - শুধু requested fields এর data ই fetch হবে
/// - Performance অনেক ভালো হবে
/// </summary>
public class PostType : ObjectType<Post>
{
    protected override void Configure(IObjectTypeDescriptor<Post> descriptor)
    {
        descriptor
            .Description("একটি Blog Post — Quillow এর মূল content unit");

        descriptor.Field(p => p.Id).Description("Post এর unique identifier");
        descriptor.Field(p => p.Title).Description("Post এর শিরোনাম");
        descriptor.Field(p => p.Slug).Description("URL-friendly slug");
        descriptor.Field(p => p.Content).Description("Post এর পূর্ণ content (Markdown/HTML)");
        descriptor.Field(p => p.Excerpt).Description("Post এর সংক্ষিপ্ত বিবরণ");
        descriptor.Field(p => p.Status).Description("Post এর অবস্থা: DRAFT, PUBLISHED, বা ARCHIVED");
        descriptor.Field(p => p.IsFeatured).Description("Featured post কিনা");
        descriptor.Field(p => p.ViewCount).Description("কতবার দেখা হয়েছে");
        descriptor.Field(p => p.PublishedAt).Description("কখন publish করা হয়েছে");
        descriptor.Field(p => p.CreatedAt).Description("কখন তৈরি করা হয়েছে");
        descriptor.Field(p => p.UpdatedAt).Description("সর্বশেষ কখন আপডেট হয়েছে");

        // ========== DataLoader দিয়ে Related Data Load ==========

        // Author — DataLoader ব্যবহার করে (N+1 সমাধান!)
        descriptor
            .Field(p => p.Author)
            .Description("Post এর লেখক (DataLoader দিয়ে optimized)")
            .ResolveWith<PostResolvers>(r => r.GetAuthorAsync(default!, default!));

        // Category — DataLoader ব্যবহার করে
        descriptor
            .Field(p => p.Category)
            .Description("Post এর category (DataLoader দিয়ে optimized)")
            .ResolveWith<PostResolvers>(r => r.GetCategoryAsync(default!, default!));

        // Comments — GroupedDataLoader ব্যবহার করে
        descriptor
            .Field(p => p.Comments)
            .Description("Post এ করা comments (DataLoader দিয়ে optimized)")
            .ResolveWith<PostResolvers>(r => r.GetCommentsAsync(default!, default!));

        // Tags — GroupedDataLoader ব্যবহার করে  
        descriptor
            .Field("tags")
            .Description("Post এর tags")
            .ResolveWith<PostResolvers>(r => r.GetTagsAsync(default!, default!));

        // PostTags ignore — tags field দিয়ে expose করছি
        descriptor.Field(p => p.PostTags).Ignore();

        // Foreign keys ignore — GraphQL এ এগুলো দরকার নেই
        descriptor.Field(p => p.AuthorId).Ignore();
        descriptor.Field(p => p.CategoryId).Ignore();

        // ========== Computed Fields ==========

        descriptor
            .Field("likeCount")
            .Type<IntType>()
            .Description("Post এ মোট কতটা like আছে (DataLoader দিয়ে optimized)")
            .ResolveWith<PostResolvers>(r => r.GetLikeCountAsync(default!, default!));

        descriptor
            .Field("commentCount")
            .Type<IntType>()
            .Description("Post এ মোট কতটা comment আছে")
            .ResolveWith<PostResolvers>(r => r.GetCommentCountAsync(default!, default!));

        descriptor
            .Field("readingTime")
            .Type<StringType>()
            .Description("আনুমানিক পড়তে কত সময় লাগবে")
            .ResolveWith<PostResolvers>(r => r.GetReadingTime(default!));
    }

    /// <summary>
    /// DataLoader-powered resolvers — এখানেই N+1 problem সমাধান হচ্ছে!
    /// </summary>
    private class PostResolvers
    {
        public async Task<User?> GetAuthorAsync(
            [Parent] Post post,
            AuthorByIdDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(post.AuthorId);
        }

        public async Task<Category?> GetCategoryAsync(
            [Parent] Post post,
            CategoryByIdDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(post.CategoryId);
        }

        public async Task<IEnumerable<Comment>> GetCommentsAsync(
            [Parent] Post post,
            CommentsByPostIdDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(post.Id);
        }

        public async Task<IEnumerable<Core.Entities.Tag>> GetTagsAsync(
            [Parent] Post post,
            TagsByPostIdDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(post.Id);
        }

        public async Task<int> GetLikeCountAsync(
            [Parent] Post post,
            LikeCountByPostIdDataLoader dataLoader)
        {
            return await dataLoader.LoadAsync(post.Id);
        }

        public async Task<int> GetCommentCountAsync(
            [Parent] Post post,
            CommentsByPostIdDataLoader dataLoader)
        {
            var comments = await dataLoader.LoadAsync(post.Id);
            return comments.Count();
        }

        public string GetReadingTime([Parent] Post post)
        {
            var wordCount = post.Content?.Split(' ').Length ?? 0;
            var minutes = Math.Max(1, wordCount / 200);
            return $"{minutes} min read";
        }
    }
}
