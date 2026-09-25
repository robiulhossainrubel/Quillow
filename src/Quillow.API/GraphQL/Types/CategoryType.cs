using Quillow.Core.Entities;

namespace Quillow.API.GraphQL.Types;

public class CategoryType : ObjectType<Category>
{
    protected override void Configure(IObjectTypeDescriptor<Category> descriptor)
    {
        descriptor.Description("Blog Post এর Category — hierarchical (parent-child) structure");

        descriptor.Field(c => c.Id).Description("Category এর unique identifier");
        descriptor.Field(c => c.Name).Description("Category এর নাম");
        descriptor.Field(c => c.Slug).Description("URL-friendly slug");
        descriptor.Field(c => c.Description).Description("Category এর বিবরণ");
        descriptor.Field(c => c.ParentCategory).Description("Parent category (যদি থাকে)");
        descriptor.Field(c => c.SubCategories).Description("Sub-categories");
        descriptor.Field(c => c.Posts).Description("এই category র সব posts");

        descriptor
            .Field("postCount")
            .Description("এই category তে মোট কতটা post আছে")
            .ResolveWith<CategoryResolvers>(r => r.GetPostCount(default!));
    }

    private class CategoryResolvers
    {
        public int GetPostCount([Parent] Category category) => category.Posts?.Count ?? 0;
    }
}
