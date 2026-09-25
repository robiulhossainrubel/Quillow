using Quillow.Core.Entities;

namespace Quillow.API.GraphQL.Types;

/// <summary>
/// GraphQL শেখার পয়েন্ট: Post Type — মূল Object Type
/// 
/// এখানে শিখবে:
/// 1. Field descriptions
/// 2. Nested types (Author, Category, Comments, Tags)
/// 3. Computed fields (likeCount, commentCount, readingTime)
/// </summary>
public class PostType : ObjectType<Post>
{
    protected override void Configure(IObjectTypeDescriptor<Post> descriptor)
    {
        descriptor
            .Description("একটি Blog Post — Quillow এর মূল content unit");

        descriptor.Field(p => p.Id).Description("Post এর unique identifier");
        descriptor.Field(p => p.Title).Description("Post এর শিরোনাম");
        descriptor.Field(p => p.Slug).Description("URL-friendly slug (e.g., 'graphql-ki-ebong-keno-shikhbo')");
        descriptor.Field(p => p.Content).Description("Post এর পূর্ণ content (Markdown/HTML)");
        descriptor.Field(p => p.Excerpt).Description("Post এর সংক্ষিপ্ত বিবরণ");
        descriptor.Field(p => p.Status).Description("Post এর অবস্থা: DRAFT, PUBLISHED, বা ARCHIVED");
        descriptor.Field(p => p.IsFeatured).Description("Featured post কিনা (homepage এ দেখাবে)");
        descriptor.Field(p => p.ViewCount).Description("কতবার দেখা হয়েছে");
        descriptor.Field(p => p.PublishedAt).Description("কখন publish করা হয়েছে");

        descriptor.Field(p => p.Author).Description("Post এর লেখক");
        descriptor.Field(p => p.Category).Description("Post এর category");
        descriptor.Field(p => p.Comments).Description("Post এ করা comments");
        descriptor.Field(p => p.PostTags).Description("Post এ থাকা tags");

        // Computed fields
        descriptor
            .Field("likeCount")
            .Description("Post এ মোট কতটা like আছে")
            .ResolveWith<PostResolvers>(r => r.GetLikeCount(default!));

        descriptor
            .Field("commentCount")
            .Description("Post এ মোট কতটা comment আছে")
            .ResolveWith<PostResolvers>(r => r.GetCommentCount(default!));

        descriptor
            .Field("readingTime")
            .Type<StringType>()
            .Description("আনুমানিক পড়তে কত সময় লাগবে")
            .ResolveWith<PostResolvers>(r => r.GetReadingTime(default!));
    }

    private class PostResolvers
    {
        public int GetLikeCount([Parent] Post post) => post.Likes?.Count ?? 0;
        public int GetCommentCount([Parent] Post post) => post.Comments?.Count ?? 0;

        public string GetReadingTime([Parent] Post post)
        {
            var wordCount = post.Content?.Split(' ').Length ?? 0;
            var minutes = Math.Max(1, wordCount / 200); // 200 words per minute
            return $"{minutes} min read";
        }
    }
}
