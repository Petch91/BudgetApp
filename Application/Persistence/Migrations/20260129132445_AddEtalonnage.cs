using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEtalonnage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EcheancesRestantes",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEchelonne",
                table: "Transactions",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantParEcheance",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NombreEcheances",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE Transactions SET IsEchelonne = 0 WHERE IsEchelonne IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EcheancesRestantes",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsEchelonne",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "MontantParEcheance",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "NombreEcheances",
                table: "Transactions");
        }
    }
}
