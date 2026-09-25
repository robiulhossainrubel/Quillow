using Microsoft.EntityFrameworkCore;
using Quillow.API.GraphQL.Mutations;
using Quillow.API.GraphQL.Queries;
using Quillow.API.GraphQL.Subscriptions;
using Quillow.API.GraphQL.Types;
using Quillow.Core.Entities;
using Quillow.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ========== Database Setup ==========
builder.Services.AddPooledDbContextFactory<QuilllowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== GraphQL Setup (Hot Chocolate) ==========
builder.Services
    .AddGraphQLServer()

    // Query — data পড়ার জন্য
    .AddQueryType<Query>()

    // Mutation — data পরিবর্তন করার জন্য
    .AddMutationType<Mutation>()

    // 🆕 Subscription — real-time WebSocket data!
    .AddSubscriptionType<Subscription>()

    // Object Types
    .AddType<UserType>()
    .AddType<PostType>()
    .AddType<CategoryType>()
    .AddType<CommentType>()
    .AddType<TagType>()
    .AddType<LikeType>()
    .AddType<MediaType>()

    // 🆕 Union Type — Search result (Post | User | Category)
    .AddType<SearchResultType>()
    .AddType<ObjectType<User>>()

    // 🆕 DataLoaders — automatically registered!
    .AddDataLoader<Quillow.API.GraphQL.DataLoaders.AuthorByIdDataLoader>()
    .AddDataLoader<Quillow.API.GraphQL.DataLoaders.CategoryByIdDataLoader>()
    .AddDataLoader<Quillow.API.GraphQL.DataLoaders.CommentsByPostIdDataLoader>()
    .AddDataLoader<Quillow.API.GraphQL.DataLoaders.LikeCountByPostIdDataLoader>()
    .AddDataLoader<Quillow.API.GraphQL.DataLoaders.TagsByPostIdDataLoader>()

    // Built-in features
    .AddFiltering()
    .AddSorting()
    .AddProjections()

    // 🆕 In-Memory Pub/Sub for Subscriptions
    .AddInMemorySubscriptions();

// CORS — frontend (Next.js) থেকে access করার জন্য
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ========== Database Migration & Seed ==========
// Development এ automatically migration run করবে ও seed data দেবে
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<QuilllowDbContext>>();
    using var context = await factory.CreateDbContextAsync();

    await context.Database.MigrateAsync();
    await DbSeeder.SeedAsync(context);
}

app.UseCors();

// 🆕 WebSocket support — Subscription এর জন্য দরকার!
app.UseWebSockets();

// ========== GraphQL Endpoint ==========
// এই একটা endpoint এ সব query, mutation, subscription হবে!
// Browser এ http://localhost:5049/graphql গেলে Banana Cake Pop (GraphQL Playground) পাবে
app.MapGraphQL();

Console.WriteLine("""

    ╔══════════════════════════════════════════════════════════╗
    ║                                                          ║
    ║   🐦 Quillow Blog API is running!                        ║
    ║                                                          ║
    ║   GraphQL Playground: http://localhost:5049/graphql       ║
    ║                                                          ║
    ║   Try this query:                                        ║
    ║   query { posts { id title slug author { displayName } } ║
    ║                                                          ║
    ╚══════════════════════════════════════════════════════════╝

""");

app.Run();
