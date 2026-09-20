# BitOfTech Expenses, an MCP server in .NET

Source code for the blog series **Building MCP Servers in .NET for AI Agents** on
[bitoftech.net](https://bitoftech.net).

The sample is a Model Context Protocol server for expense reports and approvals. It starts as a
console application with two tools and in-memory data, and ends as a secured service running in
Azure that three different AI clients can call.

## The series

| Part | Post | Tag |
|---|---|---|
| 1 | What is the Model Context Protocol? An Introduction for .NET Developers | no code |
| 2 | Building Your First MCP Server in C# | `part-02` |
| 3 | MCP Tools, Resources and Prompts in .NET | `part-03` |
| 4 | Wiring MCP Tools to Azure SQL and External APIs | |
| 5 | Hosting an MCP Server in ASP.NET Core with Streamable HTTP | |
| 6 | Building an MCP Client and AI Agent in .NET | |
| 7 | Securing an MCP Server with Microsoft Entra ID | |
| 8 | Deploying an MCP Server to Azure Container Apps | |
| 9 | Connecting an MCP Server to Microsoft Foundry Agent Service | |
| 10 | Observability, Cost Control and MCP Security | |

`main` always holds the latest state. To get the code exactly as it appears in a given post,
check out that post's tag:

```bash
git checkout part-02
```

## What the server exposes

Tools:

| Tool | Added in |
|---|---|
| `search_expense_reports` | Part 2 |
| `get_expense_report` | Part 2 |

Resources:

| Resource | Added in |
|---|---|
| `expenses://policy` | Part 3 |
| `expenses://reports/{reportId}` | Part 3 |

Prompts:

| Prompt | Added in |
|---|---|
| `review_expense_report` | Part 3 |

## Prerequisites

1. The [.NET 10 SDK](https://dotnet.microsoft.com/download).
2. VS Code with the GitHub Copilot extension. The free Copilot tier is enough.
3. An Azure subscription, from Part 4 onwards, for Azure SQL on the free offer. Parts 1 to 3
   need nothing but the SDK and VS Code. A `docker-compose.yml` is included if you would rather
   run SQL Server locally.

## Running it

```bash
git clone https://github.com/tjoudeh/building-mcp-servers-dotnet.git
cd building-mcp-servers-dotnet
dotnet build
```

Open the folder in VS Code. The repository includes `.vscode/mcp.json`, so the
`bitoftech-expenses` server appears in the MCP server list. Start it, switch Copilot Chat to
Agent mode, and ask something like:

```
Which expense reports has Sara submitted?
```

## Project layout

```
src/BitOfTech.Expenses.Mcp/     the MCP server
  Models/                       expense report, line and employee
  Data/                         in-memory store, becomes EF Core in Part 4
  Tools/                        tools the model can call
  Resources/                    the expense policy and report resources
  Prompts/                      the policy review prompt
.vscode/mcp.json                VS Code MCP configuration
```

## Packages

| Package | Version |
|---|---|
| `ModelContextProtocol` | 2.2.0 |
| `Microsoft.Extensions.Hosting` | 10.0.12 |

MCP and its SDKs move quickly. If something here no longer matches the current SDK, the posts
were written against the versions above.

## Licence

MIT. See [LICENSE](LICENSE).
