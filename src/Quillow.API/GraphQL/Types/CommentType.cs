using Quillow.Core.Entities;

namespace Quillow.API.GraphQL.Types;

public class CommentType : ObjectType<Comment>
{
    protected override void Configure(IObjectTypeDescriptor<Comment> descriptor)
    {
        descriptor.Description("Post এ করা Comment — nested replies সহ");

        descriptor.Field(c => c.Id).Description("Comment এর unique identifier");
        descriptor.Field(c => c.Content).Description("Comment এর content");
        descriptor.Field(c => c.Author).Description("কে comment করেছে");
        descriptor.Field(c => c.Post).Description("কোন post এ comment করা হয়েছে");
        descriptor.Field(c => c.ParentComment).Description("Parent comment (reply হলে)");
        descriptor.Field(c => c.Replies).Description("এই comment এর replies");
        descriptor.Field(c => c.CreatedAt).Description("কখন comment করা হয়েছে");

        descriptor
            .Field("replyCount")
            .Description("এই comment এ কতটা reply আছে")
            .ResolveWith<CommentResolvers>(r => r.GetReplyCount(default!));
    }

    private class CommentResolvers
    {
        public int GetReplyCount([Parent] Comment comment) => comment.Replies?.Count ?? 0;
    }
}
