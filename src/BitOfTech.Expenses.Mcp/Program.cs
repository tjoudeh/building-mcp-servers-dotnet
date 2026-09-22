using BitOfTech.Expenses.Mcp.Data;
using BitOfTech.Expenses.Mcp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // The working directory belongs to whoever started us, so appsettings.json has to be
    // read from the folder the binary is in rather than the current directory.
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddDbContext<ExpensesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ExpensesDb")));

builder.Services.AddHttpClient<FrankfurterClient>(client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.dev/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();
