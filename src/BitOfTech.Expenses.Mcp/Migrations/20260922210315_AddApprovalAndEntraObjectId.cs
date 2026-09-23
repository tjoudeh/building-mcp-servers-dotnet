using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitOfTech.Expenses.Mcp.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalAndEntraObjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedByEmployeeId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ApprovedOn",
                table: "Reports",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntraObjectId",
                table: "Employees",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                column: "EntraObjectId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                column: "EntraObjectId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 3,
                column: "EntraObjectId",
                value: null);

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Email", "EntraObjectId", "Name" },
                values: new object[] { 4, "taiseer.joudeh@bitoftech.net", null, "Taiseer Joudeh" });

            migrationBuilder.UpdateData(
                table: "Reports",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApprovedByEmployeeId", "ApprovedOn" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Reports",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApprovedByEmployeeId", "ApprovedOn" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Reports",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApprovedByEmployeeId", "ApprovedOn" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Reports",
                keyColumn: "Id",
                keyValue: 1004,
                columns: new[] { "ApprovedByEmployeeId", "ApprovedOn" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Reports",
                keyColumn: "Id",
                keyValue: 1005,
                columns: new[] { "ApprovedByEmployeeId", "ApprovedOn" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EntraObjectId",
                table: "Employees",
                column: "EntraObjectId",
                unique: true,
                filter: "[EntraObjectId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_EntraObjectId",
                table: "Employees");

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "ApprovedByEmployeeId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "EntraObjectId",
                table: "Employees");
        }
    }
}
