using System.Security.Claims;
using Quillow.API.GraphQL.Mutations;
using Quillow.Core.Entities;

namespace Quillow.API.GraphQL.Types;

/// <summary>
/// GraphQL শেখার পয়েন্ট: Authentication Input & Payload Types
/// 
/// Register ও Login এর জন্য clean, strongly-typed inputs এবং
/// response এ JWT Token + User Data + Errors (Payload Pattern).
/// </summary>
public record RegisterInput(
    string UserName,
    string Email,
    string Password,
    string DisplayName,
    string? Bio = null
);

public record LoginInput(
    string Email,
    string Password
);

public record AuthPayload(
    string? Token,
    User? User,
    IReadOnlyList<UserError>? Errors
);

/// <summary>
/// ClaimsPrincipal থেকে সহজে current user এর তথ্য বের করার helper extensions।
/// GraphQL resolver এ [Authorize] থাকলে `ClaimsPrincipal claimsPrincipal` সরাসরি inject হয়!
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        return Guid.TryParse(idClaim, out var id) ? id : null;
    }

    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;
    }

    public static bool IsInRoleCustom(this ClaimsPrincipal principal, string role)
    {
        return principal.IsInRole(role);
    }
}
