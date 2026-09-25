using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quillow.API.GraphQL.DataLoaders;
using Quillow.API.GraphQL.Mutations;
using Quillow.API.GraphQL.Queries;
using Quillow.API.GraphQL.Subscriptions;
using Quillow.API.GraphQL.Types;
using Quillow.Core.Entities;
using Quillow.Infrastructure.Data;
using Quillow.Services.Auth;
using Xunit;

namespace Quillow.Tests;

public class GraphQLSchemaTests
{
    [Fact]
    public async Task GraphQLSchema_ShouldBuildSuccessfully_WithAllTypesQueriesMutationsAndSubscriptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // In-memory DbContext
        services.AddPooledDbContextFactory<QuilllowDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddHttpContextAccessor();
        services.AddAuthorization();

        // GraphQL Server Configuration matching Program.cs
        services.AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddSubscriptionType<Subscription>()
            .AddType<UserType>()
            .AddType<PostType>()
            .AddType<CategoryType>()
            .AddType<CommentType>()
            .AddType<TagType>()
            .AddType<LikeType>()
            .AddType<MediaType>()
            .AddType<UploadType>()
            .AddType<SearchResultType>()
            .AddType<ObjectType<User>>()
            .AddDataLoader<AuthorByIdDataLoader>()
            .AddDataLoader<CategoryByIdDataLoader>()
            .AddDataLoader<CommentsByPostIdDataLoader>()
            .AddDataLoader<LikeCountByPostIdDataLoader>()
            .AddDataLoader<TagsByPostIdDataLoader>()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .AddInMemorySubscriptions();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var schema = await serviceProvider.GetSchemaAsync();

        // Assert
        Assert.NotNull(schema);

        // Verify Root Operation Types
        Assert.NotNull(schema.QueryType);
        Assert.NotNull(schema.MutationType);
        Assert.NotNull(schema.SubscriptionType);

        // Verify Core Queries exist
        Assert.True(schema.QueryType.Fields.ContainsName("posts"));
        Assert.True(schema.QueryType.Fields.ContainsName("postById"));
        Assert.True(schema.QueryType.Fields.ContainsName("postBySlug"));
        Assert.True(schema.QueryType.Fields.ContainsName("search"));
        Assert.True(schema.QueryType.Fields.ContainsName("stats"));
        Assert.True(schema.QueryType.Fields.ContainsName("media"));

        // Verify Core Mutations exist
        Assert.True(schema.MutationType.Fields.ContainsName("register"));
        Assert.True(schema.MutationType.Fields.ContainsName("login"));
        Assert.True(schema.MutationType.Fields.ContainsName("createPost"));
        Assert.True(schema.MutationType.Fields.ContainsName("addComment"));
        Assert.True(schema.MutationType.Fields.ContainsName("toggleLike"));
        Assert.True(schema.MutationType.Fields.ContainsName("uploadMedia"));

        // Verify Subscriptions exist
        Assert.True(schema.SubscriptionType.Fields.ContainsName("onPostPublished"));
        Assert.True(schema.SubscriptionType.Fields.ContainsName("onCommentAdded"));
        Assert.True(schema.SubscriptionType.Fields.ContainsName("onPostLiked"));

        // Verify Union & Types exist in Schema
        Assert.True(schema.Types.ContainsName("SearchResult"));
        Assert.True(schema.Types.ContainsName("Upload"));
    }
}
