using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glowi.Data.Migrations
{
    /// <inheritdoc />
    public partial class TecnicaServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tecnica",
                table: "Servico",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tecnica",
                table: "Servico");
        }
    }
}
