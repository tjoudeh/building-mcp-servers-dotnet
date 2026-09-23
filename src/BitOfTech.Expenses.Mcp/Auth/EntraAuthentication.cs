using Microsoft.AspNetCore.Authentication.JwtBearer;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;

namespace BitOfTech.Expenses.Mcp.Auth;

public static class EntraAuthentication
{
    public const string CanApprovePolicy = "CanApprove";

    public static IServiceCollection AddEntraAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var tenantId = configuration["Entra:TenantId"]!;
        var clientId = configuration["Entra:ClientId"]!;
        var serverUrl = configuration["Entra:ServerUrl"]!;

        var issuer = $"https://login.microsoftonline.com/{tenantId}/v2.0";

        // The resource identifier is the MCP endpoint itself, not the host. RFC 9728 requires it
        // to match the URL the metadata document is served from, and the SDK serves that at
        // /.well-known/oauth-protected-resource/mcp to mirror the endpoint path.
        var resource = $"{serverUrl}/mcp";

        services
            .AddAuthentication(options =>
            {
                // Two schemes doing two jobs. JWT bearer reads the token on every request.
                // The MCP scheme owns the 401, because only it knows how to write the
                // WWW-Authenticate header the protocol expects.
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = issuer;

                // For a v2 token the audience is the resource's client id, not its App ID URI.
                options.Audience = clientId;

                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidIssuer = issuer;
            })
            .AddMcp(options =>
            {
                options.ResourceMetadata = new ProtectedResourceMetadata
                {
                    Resource = resource,
                    AuthorizationServers = { issuer },
                    // Entra resolves an unqualified scope name against Microsoft Graph, so the
                    // scope has to carry the Application ID URI that owns it.
                    ScopesSupported = [$"{resource}/Expenses.Access"],
                    BearerMethodsSupported = ["header"]
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(CanApprovePolicy, policy => policy.RequireRole("Expenses.Approver"));

        return services;
    }
}
