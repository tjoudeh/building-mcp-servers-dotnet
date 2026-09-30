using Azure.Monitor.OpenTelemetry.AspNetCore;
using BitOfTech.Expenses.Mcp.Auth;
using BitOfTech.Expenses.Mcp.Data;
using BitOfTech.Expenses.Mcp.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // The working directory belongs to whoever started us, so appsettings.json has to be
    // read from the folder the binary is in rather than the current directory.
    ContentRootPath = AppContext.BaseDirectory
});

// Container Apps terminates TLS at its ingress and forwards over plain HTTP. Without this the
// server builds http:// URLs, and the 401 would point MCP clients at an address Entra will not
// issue tokens for. The known network lists are cleared because ingress is the only route in.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddDbContext<ExpensesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ExpensesDb")));

// The MCP SDK already emits a span per JSON-RPC message on this source, so there is nothing
// to instrument by hand. Skipped when the connection string is absent, which is how the
// server runs locally against docker-compose.
var appInsights = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

if (!string.IsNullOrWhiteSpace(appInsights))
{
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing.AddSource("Experimental.ModelContextProtocol"))
        .WithMetrics(metrics => metrics.AddMeter("Experimental.ModelContextProtocol"))
        .UseAzureMonitor(options => options.ConnectionString = appInsights);
}

builder.Services.AddHttpClient<FrankfurterClient>(client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.dev/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddSingleton<EmailSender>();

builder.Services.AddEntraAuthentication(builder.Configuration);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly()
    // Drops any tool, resource or prompt the caller is not allowed to use before the list is sent.
    .AddAuthorizationFilters();

var app = builder.Build();

// Must run before anything that reads the scheme, which includes the MCP challenge handler.
app.UseForwardedHeaders();

app.UseAuthentication();
app.UseAuthorization();

app.MapMcp("/mcp").RequireAuthorization();

app.Run();
