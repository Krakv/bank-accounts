using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bank_accounts.Migrations;

/// <inheritdoc />
public partial class AddIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_AccountId_Date",
            table: "Transactions",
            columns: new[] { "AccountId", "Date" });

        migrationBuilder.CreateIndex(
                name: "IX_Transactions_Date",
                table: "Transactions",
                column: "Date")
            .Annotation("Npgsql:IndexMethod", "GIST");

        migrationBuilder.CreateIndex(
                name: "IX_Accounts_OwnerId",
                table: "Accounts",
                column: "OwnerId")
            .Annotation("Npgsql:IndexMethod", "HASH");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Transactions_AccountId_Date",
            table: "Transactions");

        migrationBuilder.DropIndex(
            name: "IX_Transactions_Date",
            table: "Transactions");

        migrationBuilder.DropIndex(
            name: "IX_Accounts_OwnerId",
            table: "Accounts");
    }
}