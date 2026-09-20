using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BitOfTech.Expenses.Mcp.Data;

// dotnet ef needs a DbContext without running Program.cs, which would start the MCP
// server and take over stdio. This factory gives it one.
public class ExpensesDbContextFactory : IDesignTimeDbContextFactory<ExpensesDbContext>
{
    public ExpensesDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("ExpensesDb")
            ?? throw new InvalidOperationException("ConnectionStrings:ExpensesDb is not set.");

        var options = new DbContextOptionsBuilder<ExpensesDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ExpensesDbContext(options);
    }
}
