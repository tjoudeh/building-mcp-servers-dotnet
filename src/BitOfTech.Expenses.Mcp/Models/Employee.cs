namespace BitOfTech.Expenses.Mcp.Models;

public class Employee
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // The Entra ID object id (oid claim) of the person this row represents, when they have a sign-in.
    // Deliberately not the email, which an administrator can change.
    public string? EntraObjectId { get; set; }
}
