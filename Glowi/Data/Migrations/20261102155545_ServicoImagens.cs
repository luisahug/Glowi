using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Glowi.Data.Migrations
{
    /// <inheritdoc />
    public partial class ServicoImagens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServicoImagens",
                columns: table => new
                {
                    ServicoImagemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServicoId = table.Column<int>(type: "integer", nullable: false),
                    CaminhoBucket = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicoImagens", x => x.ServicoImagemId);
                    table.ForeignKey(
                        name: "FK_ServicoImagens_Servico_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servico",
                        principalColumn: "ServicoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicoImagens_ServicoId_Ordem",
                table: "ServicoImagens",
                columns: new[] { "ServicoId", "Ordem" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicoImagens");
        }
    }
}
