using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BitOfTech.Expenses.Mcp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubmittedOn = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpenseReportId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncurredOn = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lines_Reports_ExpenseReportId",
                        column: x => x.ExpenseReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Email", "Name" },
                values: new object[,]
                {
                    { 1, "sara.haddad@bitoftech.net", "Sara Haddad" },
                    { 2, "omar.nasser@bitoftech.net", "Omar Nasser" },
                    { 3, "lina.farah@bitoftech.net", "Lina Farah" }
                });

            migrationBuilder.InsertData(
                table: "Reports",
                columns: new[] { "Id", "Currency", "EmployeeId", "Status", "SubmittedOn", "Title", "TotalAmount" },
                values: new object[,]
                {
                    { 1001, "EUR", 1, "Submitted", new DateOnly(2026, 9, 2), "Berlin customer workshop", 842.50m },
                    { 1002, "USD", 1, "Approved", new DateOnly(2026, 8, 14), "Azure certification exam", 165.00m },
                    { 1003, "GBP", 2, "Submitted", new DateOnly(2026, 9, 9), "London team offsite", 1205.75m },
                    { 1004, "GBP", 3, "Rejected", new DateOnly(2026, 8, 28), "Mobile and broadband, August", 88.40m },
                    { 1005, "EUR", 2, "Draft", null, "Client dinner, Amsterdam", 210.00m }
                });

            migrationBuilder.InsertData(
                table: "Lines",
                columns: new[] { "Id", "Amount", "Category", "Description", "ExpenseReportId", "IncurredOn" },
                values: new object[,]
                {
                    { 1, 410.00m, "Travel", "Return flight LHR to BER", 1001, new DateOnly(2026, 8, 31) },
                    { 2, 312.50m, "Accommodation", "Hotel, two nights", 1001, new DateOnly(2026, 8, 31) },
                    { 3, 120.00m, "Travel", "Airport transfers", 1001, new DateOnly(2026, 9, 1) },
                    { 4, 165.00m, "Training", "AZ-204 exam voucher", 1002, new DateOnly(2026, 8, 12) },
                    { 5, 96.75m, "Travel", "Train tickets", 1003, new DateOnly(2026, 9, 7) },
                    { 6, 689.00m, "Accommodation", "Hotel, three nights", 1003, new DateOnly(2026, 9, 7) },
                    { 7, 420.00m, "Meals", "Team meals", 1003, new DateOnly(2026, 9, 8) },
                    { 8, 48.40m, "Communications", "Mobile plan", 1004, new DateOnly(2026, 8, 25) },
                    { 9, 40.00m, "Communications", "Home broadband", 1004, new DateOnly(2026, 8, 25) },
                    { 10, 210.00m, "Meals", "Dinner for four", 1005, new DateOnly(2026, 9, 15) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lines_ExpenseReportId",
                table: "Lines",
                column: "ExpenseReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_EmployeeId",
                table: "Reports",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status",
                table: "Reports",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Lines");

            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
