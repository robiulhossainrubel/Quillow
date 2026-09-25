using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Quillow.API.GraphQL.Mutations;
using Quillow.API.GraphQL.Types;
using Quillow.Infrastructure.Data;
using Quillow.Services.Auth;
using Xunit;

namespace Quillow.Tests;

public class MutationValidationTests
{
    private readonly IDbContextFactory<QuilllowDbContext> _contextFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public MutationValidationTests()
    {
        var options = new DbContextOptionsBuilder<QuilllowDbContext>()
            .UseInMemoryDatabase($"TestValidationDb_{Guid.NewGuid()}")
            .Options;

        // Custom factory for testing
        _contextFactory = new TestDbContextFactory(options);
        _passwordHasher = new PasswordHasher();
        _tokenService = new TokenService(new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Jwt:Secret", "Quillow_Super_Secure_JWT_Secret_Key_2026_DotNet10_HotChocolate_32Chars!"},
                {"Jwt:Issuer", "QuillowAPI"},
                {"Jwt:Audience", "QuillowClient"}
            })
            .Build());
    }

    [Fact]
    public async Task Register_WithInvalidEmailOrShortPassword_ShouldReturnValidationErrors()
    {
        // Arrange
        var mutation = new Mutation();
        var invalidInput = new RegisterInput(
            UserName: "testuser",
            Email: "not-an-email",
            Password: "123", // too short (< 6)
            DisplayName: "Test User",
            Bio: null
        );

        // Act
        var result = await mutation.Register(_contextFactory, _passwordHasher, _tokenService, invalidInput);

        // Assert
        Assert.Null(result.Token);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Field == "email");
        Assert.Contains(result.Errors, e => e.Field == "password");
    }

    [Fact]
    public async Task Register_WithValidDetails_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var mutation = new Mutation();
        var validInput = new RegisterInput(
            UserName: "validuser",
            Email: "valid@quillow.com",
            Password: "StrongPassword@123",
            DisplayName: "Valid User",
            Bio: "Fullstack .NET & GraphQL developer"
        );

        // Act
        var result = await mutation.Register(_contextFactory, _passwordHasher, _tokenService, validInput);

        // Assert
        Assert.NotNull(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal("validuser", result.User.UserName);
        Assert.Equal("valid@quillow.com", result.User.Email);
        Assert.Null(result.Errors);
    }
}

public class TestDbContextFactory : IDbContextFactory<QuilllowDbContext>
{
    private readonly DbContextOptions<QuilllowDbContext> _options;

    public TestDbContextFactory(DbContextOptions<QuilllowDbContext> options)
    {
        _options = options;
    }

    public QuilllowDbContext CreateDbContext()
    {
        return new QuilllowDbContext(_options);
    }

    public Task<QuilllowDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateDbContext());
    }
}
