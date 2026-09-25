using Microsoft.EntityFrameworkCore;
using Quillow.API.GraphQL.Mutations;
using Quillow.API.GraphQL.Queries;
using Quillow.API.GraphQL.Types;
using Quillow.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ========== Database Setup ==========
// PostgreSQL + EF Core connection
builder.Services.AddPooledDbContextFactory<QuilllowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== GraphQL Setup (Hot Chocolate) ==========
// এখানেই সব GraphQL এর magic হয়!
builder.Services
    .AddGraphQLServer()

    // Query — data পড়ার জন্য (GET এর মতো)
    .AddQueryType<Query>()

    // Mutation — data তৈরি/পরিবর্তন/মুছে ফেলার জন্য (POST/PUT/DELETE এর মতো)
    .AddMutationType<Mutation>()

    // Object Types — কোন entity কিভাবে expose হবে
    .AddType<UserType>()
    .AddType<PostType>()
    .AddType<CategoryType>()
    .AddType<CommentType>()
    .AddType<TagType>()
    .AddType<LikeType>()
    .AddType<MediaType>()

    // Built-in features — Hot Chocolate এর power!
    .AddFiltering()    // where clause: posts(where: { status: { eq: PUBLISHED } })
    .AddSorting()      // order by: posts(order: { createdAt: DESC })
    .AddProjections(); // শুধু requested fields ই database থেকে আনবে

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
