using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Glowi.Data.Migrations
{
    /// <inheritdoc />
    public partial class ListaEspera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HorariosDesejados",
                columns: table => new
                {
                    HorarioDesejadoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    ServicoId = table.Column<int>(type: "integer", nullable: false),
                    ProfissionalId = table.Column<int>(type: "integer", nullable: false),
                    Inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LiberadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClienteViuAviso = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosDesejados", x => x.HorarioDesejadoId);
                    table.ForeignKey(
                        name: "FK_HorariosDesejados_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HorariosDesejados_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "ProfissionalId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HorariosDesejados_Servico_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servico",
                        principalColumn: "ServicoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorariosDesejados_ClienteId_ServicoId_Inicio",
                table: "HorariosDesejados",
                columns: new[] { "ClienteId", "ServicoId", "Inicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HorariosDesejados_ProfissionalId_Inicio",
                table: "HorariosDesejados",
                columns: new[] { "ProfissionalId", "Inicio" });

            migrationBuilder.CreateIndex(
                name: "IX_HorariosDesejados_ServicoId",
                table: "HorariosDesejados",
                column: "ServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorariosDesejados");
        }
    }
}
