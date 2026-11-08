using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glowi.Data.Migrations
{
    /// <inheritdoc />
    public partial class CancelamentoPorBloqueio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanceladoPorBloqueio",
                table: "Agendamentos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ClienteViuCancelamento",
                table: "Agendamentos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanceladoPorBloqueio",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "ClienteViuCancelamento",
                table: "Agendamentos");
        }
    }
}
