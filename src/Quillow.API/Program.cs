using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quillow.API.GraphQL.Mutations;
using Quillow.API.GraphQL.Queries;
using Quillow.API.GraphQL.Subscriptions;
using Quillow.API.GraphQL.Types;
using Quillow.Core.Entities;
using Quillow.Infrastructure.Data;
using HotChocolate.Authorization;
using Quillow.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

// ========== Database Setup ==========
builder.Services.AddPooledDbContextFactory<QuilllowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== Authentication & Security Setup ==========
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? "Quillow_Super_Secure_JWT_Secret_Key_2026_DotNet10_HotChocolate_32Chars!";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "QuillowAPI",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "QuillowClient",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();

// ========== GraphQL Setup (Hot Chocolate) ==========
builder.Services
    .AddGraphQLServer()
    // 🔐 Hot Chocolate ASP.NET Core Authorization Support
    .AddAuthorization()
    .AddErrorFilter(error =>
    {
        Console.WriteLine($"[GRAPHQL_ERROR] {error.Message} | {error.Exception?.Message} | {error.Exception?.InnerException?.Message}");
        return error;
    })
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment())

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

// 🔐 Authentication & Authorization middlewares
app.UseAuthentication();
app.UseAuthorization();

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
