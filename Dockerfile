FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore before copying the rest of the source, so a code change does not invalidate the
# restore layer.
COPY src/BitOfTech.Expenses.Mcp/BitOfTech.Expenses.Mcp.csproj src/BitOfTech.Expenses.Mcp/
RUN dotnet restore src/BitOfTech.Expenses.Mcp/BitOfTech.Expenses.Mcp.csproj

COPY src/BitOfTech.Expenses.Mcp/ src/BitOfTech.Expenses.Mcp/
RUN dotnet publish src/BitOfTech.Expenses.Mcp/BitOfTech.Expenses.Mcp.csproj -c Release -o /app

# The ASP.NET Core runtime images already run as a non-root user and listen on 8080.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "BitOfTech.Expenses.Mcp.dll"]
