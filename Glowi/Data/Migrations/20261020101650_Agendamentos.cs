using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Glowi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Agendamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AntecedenciaMinimaHoras",
                table: "Profissionais",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "IntervaloMinutos",
                table: "Profissionais",
                type: "integer",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.CreateTable(
                name: "Agendamentos",
                columns: table => new
                {
                    AgendamentoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    ProfissionalId = table.Column<int>(type: "integer", nullable: false),
                    ServicoId = table.Column<int>(type: "integer", nullable: false),
                    Inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendamentos", x => x.AgendamentoId);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "ProfissionalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Servico_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servico",
                        principalColumn: "ServicoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_ClienteId",
                table: "Agendamentos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_ProfissionalId_Inicio",
                table: "Agendamentos",
                columns: new[] { "ProfissionalId", "Inicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_ServicoId",
                table: "Agendamentos",
                column: "ServicoId");

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");
            migrationBuilder.Sql(@"
                ALTER TABLE ""Agendamentos""
                ADD CONSTRAINT ""SemConflitoDeHorario""
                EXCLUDE USING gist (
                    ""ProfissionalId"" WITH =,
                    tstzrange(""Inicio"", ""Fim"") WITH &&
                ) WHERE (""Status"" NOT IN (2, 3));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "AntecedenciaMinimaHoras",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "IntervaloMinutos",
                table: "Profissionais");

            migrationBuilder.Sql(@"ALTER TABLE ""Agendamentos"" DROP CONSTRAINT IF EXISTS ""SemConflitoDeHorario"";");
        }
    }
}
