using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glowi.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnderecoProfissional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "Profissionais",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cep",
                table: "Profissionais",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Profissionais",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "Profissionais",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Profissionais",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                table: "Profissionais",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Profissionais",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Profissionais",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uf",
                table: "Profissionais",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Cep",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Logradouro",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Uf",
                table: "Profissionais");
        }
    }
}
