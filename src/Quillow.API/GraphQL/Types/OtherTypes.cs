using Quillow.Core.Entities;

using CoreTag = Quillow.Core.Entities.Tag;

namespace Quillow.API.GraphQL.Types;

public class TagType : ObjectType<CoreTag>
{
    protected override void Configure(IObjectTypeDescriptor<CoreTag> descriptor)
    {
        descriptor.Description("Post এর Tag — একটি post এ একাধিক tag থাকতে পারে");

        descriptor.Field(t => t.Id).Description("Tag এর unique identifier");
        descriptor.Field(t => t.Name).Description("Tag এর নাম");
        descriptor.Field(t => t.Slug).Description("URL-friendly slug");
        descriptor.Field(t => t.PostTags).Description("এই tag এ tagged posts");
    }
}

public class LikeType : ObjectType<Like>
{
    protected override void Configure(IObjectTypeDescriptor<Like> descriptor)
    {
        descriptor.Description("Post Like — কে কখন like করেছে");

        descriptor.Field(l => l.Id).Description("Like এর unique identifier");
        descriptor.Field(l => l.Post).Description("কোন post like করা হয়েছে");
        descriptor.Field(l => l.User).Description("কে like করেছে");
        descriptor.Field(l => l.CreatedAt).Description("কখন like করা হয়েছে");
    }
}

public class MediaType : ObjectType<Media>
{
    protected override void Configure(IObjectTypeDescriptor<Media> descriptor)
    {
        descriptor.Description("Media file — images, videos ইত্যাদি");

        descriptor.Field(m => m.Id).Description("Media এর unique identifier");
        descriptor.Field(m => m.FileName).Description("File এর নাম");
        descriptor.Field(m => m.Url).Description("File এর URL");
        descriptor.Field(m => m.MimeType).Description("File type (e.g., image/png)");
        descriptor.Field(m => m.Size).Description("File size in bytes");
        descriptor.Field(m => m.UploadedBy).Description("কে upload করেছে");
        descriptor.Field(m => m.CreatedAt).Description("কখন upload করা হয়েছে");
    }
}
