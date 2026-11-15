using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glowi.Data.Migrations
{
    /// <inheritdoc />
    public partial class TecnicasServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Tecnicas",
                table: "Servico",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'");

            // Leva a técnica única que já estava cadastrada para a nova lista.
            migrationBuilder.Sql(
                "UPDATE \"Servico\" SET \"Tecnicas\" = ARRAY[btrim(\"Tecnica\")] " +
                "WHERE \"Tecnica\" IS NOT NULL AND btrim(\"Tecnica\") <> '';");

            migrationBuilder.DropColumn(
                name: "Tecnica",
                table: "Servico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tecnica",
                table: "Servico",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE \"Servico\" SET \"Tecnica\" = left(\"Tecnicas\"[1], 60) " +
                "WHERE cardinality(\"Tecnicas\") > 0;");

            migrationBuilder.DropColumn(
                name: "Tecnicas",
                table: "Servico");
        }
    }
}
