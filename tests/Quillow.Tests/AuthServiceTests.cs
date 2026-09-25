using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Quillow.Core.Entities;
using Quillow.Services.Auth;
using Xunit;

namespace Quillow.Tests;

public class AuthServiceTests
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthServiceTests()
    {
        _passwordHasher = new PasswordHasher();

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Secret", "Quillow_Super_Secure_JWT_Secret_Key_2026_DotNet10_HotChocolate_32Chars!"},
            {"Jwt:Issuer", "QuillowAPI"},
            {"Jwt:Audience", "QuillowClient"},
            {"Jwt:ExpiryMinutes", "60"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _tokenService = new TokenService(configuration);
    }

    [Fact]
    public void PasswordHasher_ShouldHashAndVerifyPasswordSuccessfully()
    {
        // Arrange
        const string rawPassword = "SecurePassword@2026";

        // Act
        var hash = _passwordHasher.HashPassword(rawPassword);
        var isValid = _passwordHasher.VerifyPassword(rawPassword, hash);
        var isInvalid = _passwordHasher.VerifyPassword("WrongPassword123", hash);

        // Assert
        Assert.NotEmpty(hash);
        Assert.NotEqual(rawPassword, hash);
        Assert.True(isValid);
        Assert.False(isInvalid);
    }

    [Fact]
    public void TokenService_ShouldGenerateValidJwtTokenWithCorrectClaims()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "developer@quillow.com",
            DisplayName = "Robiul Developer",
            UserName = "robiuldev"
        };
        var roles = new List<string> { "Admin", "Author" };

        // Act
        var token = _tokenService.GenerateAccessToken(user, roles);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));

        var jwt = handler.ReadJwtToken(token);
        Assert.Equal("QuillowAPI", jwt.Issuer);
        Assert.Contains("QuillowClient", jwt.Audiences);

        var idClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
        Assert.Equal(user.Id.ToString(), idClaim);

        var roleClaims = jwt.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Contains("Admin", roleClaims);
        Assert.Contains("Author", roleClaims);
    }
}
