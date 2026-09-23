using System.Security.Claims;

namespace BitOfTech.Expenses.Mcp.Auth;

public static class CallerExtensions
{
    // Entra puts the stable user id in "oid". ASP.NET Core remaps it to this longer name unless
    // you turn claim mapping off, so check both rather than depending on that setting.
    private const string ObjectIdClaim = "oid";
    private const string ObjectIdClaimLong = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public static string? GetObjectId(this ClaimsPrincipal? user) =>
        user?.FindFirst(ObjectIdClaim)?.Value ?? user?.FindFirst(ObjectIdClaimLong)?.Value;
}
