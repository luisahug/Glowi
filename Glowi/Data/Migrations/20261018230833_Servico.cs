using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Glowi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Servico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servico",
                columns: table => new
                {
                    ServicoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfissionalId = table.Column<int>(type: "integer", nullable: true),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "integer", nullable: false),
                    Preco = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servico", x => x.ServicoId);
                    table.ForeignKey(
                        name: "FK_Servico_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "ProfissionalId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Servico_ProfissionalId",
                table: "Servico",
                column: "ProfissionalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Servico");
        }
    }
}
