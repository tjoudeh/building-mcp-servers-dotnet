# BitOfTech Expenses, an MCP server in .NET

Source code for the blog series **Building MCP Servers in .NET for AI Agents** on
[bitoftech.net](https://bitoftech.net).

The sample is a Model Context Protocol server for expense reports and approvals. It starts as a
console application with two tools and in-memory data, and ends as a secured service running in
Azure that four different AI clients can call.

## The series

| Part | Post | Tag |
|---|---|---|
| 1 | What is the Model Context Protocol? An Introduction for .NET Developers | no code |
| 2 | Building Your First MCP Server in C# | `part-02` |
| 3 | MCP Tools, Resources and Prompts in .NET | `part-03` |
| 4 | Wiring MCP Tools to Azure SQL and External APIs | `part-04` |
| 5 | Hosting an MCP Server in ASP.NET Core with Streamable HTTP | `part-05` |
| 6 | Building an MCP Client and AI Agent in .NET | `part-06` |
| 7 | Securing an MCP Server with Microsoft Entra ID | `part-07` |
| 8 | Deploying an MCP Server to Azure Container Apps | `part-08` |
| 9 | Connecting an MCP Server to Microsoft Foundry Agent Service | `part-09` |
| 10 | Observability, Cost Control and MCP Security | `part-10` |

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
| `convert_currency` | Part 4 |
| `create_expense_report` | Part 4 |
| `submit_expense_report` | Part 4 |
| `approve_expense_report` | Part 7 |
| `notify_employee` | Part 8 |

Resources:

| Resource | Added in |
|---|---|
| `expenses://policy` | Part 3 |
| `expenses://reports/{reportId}` | Part 3 |

Prompts:

| Prompt | Added in |
|---|---|
| `review_expense_report` | Part 3 |

## Clients

The same server, unchanged, is called by all four of these:

| Client | From |
|---|---|
| VS Code Copilot | Part 2 |
| `BitOfTech.Expenses.Agent`, written in the series | Part 6 |
| Claude | Part 8 |
| Microsoft Foundry Agent Service | Part 9 |

## Prerequisites

1. The [.NET 10 SDK](https://dotnet.microsoft.com/download).
2. VS Code with the GitHub Copilot extension. The free Copilot tier is enough.
3. An Azure subscription, from Part 4 onwards, for Azure SQL on the free offer. Parts 1 to 3
   need nothing but the SDK and VS Code. A `docker-compose.yml` is included if you would rather
   run SQL Server locally.
4. A Microsoft Entra tenant where you are allowed to register applications, from Part 7 onwards.

## Running it

**Start at `part-02` or `part-03`.** Those two tags are self contained. Clone, build, open the
folder in VS Code, and the `bitoftech-expenses` server appears in the MCP server list. Start it,
switch Copilot Chat to Agent mode, and ask something like:

```
Which expense reports has Sara submitted?
```

```bash
git clone https://github.com/tjoudeh/building-mcp-servers-dotnet.git
cd building-mcp-servers-dotnet
git checkout part-02
dotnet build
```

From Part 4 onwards the code expects resources that belong to whoever is running it. `main` and
the later tags carry my endpoints, my tenant id and my client ids in `.vscode/mcp.json` and in
each `appsettings.json`. You cannot reach any of them.

So treat Part 4 and later as code to read alongside the post, and create your own resources by
following the Azure CLI commands in it. Then put your own values in the configuration files.

## Project layout

```
src/BitOfTech.Expenses.Mcp/     the MCP server
  Models/                       expense report, line and employee
  Data/                         EF Core context, queries and seed data
  Migrations/                   EF Core migrations, from Part 4
  Contracts/                    the shapes returned to the model
  Tools/                        tools the model can call
  Resources/                    the expense policy and report resources
  Prompts/                      the policy review prompt
  Services/                     Frankfurter currency client and email sender
  Auth/                         Microsoft Entra ID authentication, from Part 7
src/BitOfTech.Expenses.Agent/   the .NET agent, hand written loop and agent framework
src/BitOfTech.Expenses.FoundryAgent/   talks to a Foundry hosted agent, from Part 9
.vscode/mcp.json                VS Code MCP configuration
docker-compose.yml              local SQL Server, if you would rather not use Azure SQL
Dockerfile                      used from Part 8
BitOfTech.Expenses.slnx         the solution
```

## Packages

`src/BitOfTech.Expenses.Mcp`

| Package | Version |
|---|---|
| `ModelContextProtocol` | 2.2.0 |
| `ModelContextProtocol.AspNetCore` | 2.2.0 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.12 |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.12 |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.12 |
| `Azure.Communication.Email` | 1.1.0 |
| `Azure.Monitor.OpenTelemetry.AspNetCore` | 1.6.0 |

`src/BitOfTech.Expenses.Agent`

| Package | Version |
|---|---|
| `ModelContextProtocol.Core` | 2.2.0 |
| `Microsoft.Extensions.AI` | 10.10.0 |
| `Microsoft.Extensions.AI.OpenAI` | 10.10.0 |
| `Microsoft.Agents.AI` | 1.22.0 |
| `Azure.AI.OpenAI` | 2.9.0-beta.1 |
| `Azure.Identity` | 1.21.0 |
| `Microsoft.Extensions.Configuration.Json` | 10.0.12 |

`src/BitOfTech.Expenses.FoundryAgent`

| Package | Version |
|---|---|
| `Azure.AI.Projects` | 2.0.1 |
| `Azure.AI.Extensions.OpenAI` | 2.0.0 |
| `Azure.Identity` | 1.21.0 |
| `Microsoft.Extensions.Configuration.Json` | 10.0.12 |

MCP and its SDKs move quickly. If something here no longer matches the current SDK, the posts
were written against the versions above.

## Licence

MIT. See [LICENSE](LICENSE).
