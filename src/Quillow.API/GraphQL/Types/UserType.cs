using Quillow.Core.Entities;

namespace Quillow.API.GraphQL.Types;

/// <summary>
/// GraphQL শেখার পয়েন্ট: Object Type Definition
/// 
/// এটা হলো GraphQL এর "Type" — বলে দেয় কোন entity র কোন fields client access করতে পারবে।
/// Hot Chocolate তে ObjectType<T> inherit করে type define করা যায়।
/// 
/// এখানে আমরা:
/// 1. PasswordHash field ignore করেছি (security)
/// 2. Description দিয়েছি (GraphQL Playground এ দেখাবে)
/// 3. Computed fields যোগ করেছি (postCount, commentCount)
/// </summary>
public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor
            .Description("একজন Blog ব্যবহারকারী — লেখক বা পাঠক");

        // PasswordHash কখনোই GraphQL দিয়ে expose করবো না!
        descriptor
            .Field(u => u.PasswordHash)
            .Ignore();

        descriptor
            .Field(u => u.Id)
            .Description("User এর unique identifier");

        descriptor
            .Field(u => u.DisplayName)
            .Description("User এর প্রদর্শনযোগ্য নাম");

        descriptor
            .Field(u => u.Posts)
            .Description("এই User এর লেখা সব posts");

        descriptor
            .Field(u => u.Comments)
            .Description("এই User এর করা সব comments");

        // Computed field — database এ নেই, কিন্তু GraphQL এ আছে!
        descriptor
            .Field("postCount")
            .Description("এই User মোট কতটা post লিখেছে")
            .ResolveWith<UserResolvers>(r => r.GetPostCount(default!));

        descriptor
            .Field("roles")
            .Description("User এর assigned roles")
            .Resolve(ctx =>
            {
                var user = ctx.Parent<User>();
                return user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList() 
                    ?? new List<string>();
            });
    }

    /// <summary>
    /// Computed fields এর resolvers — এগুলো database column না, 
    /// run-time এ calculate হয়।
    /// </summary>
    private class UserResolvers
    {
        public int GetPostCount([Parent] User user) => user.Posts?.Count ?? 0;
        public int GetCommentCount([Parent] User user) => user.Comments?.Count ?? 0;
    }
}
