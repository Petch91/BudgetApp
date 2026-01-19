using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDateFinBis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateFin",
                table: "Transactions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateFin",
                table: "Transactions");
        }
    }
}
