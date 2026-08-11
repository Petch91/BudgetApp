using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalRefToTransactionVariable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalRef",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ExternalRef",
                table: "Transactions",
                column: "ExternalRef");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_ExternalRef",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ExternalRef",
                table: "Transactions");
        }
    }
}
