using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Glowi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgendamentoPreco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Preco",
                table: "Agendamentos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // agendamentos que já existiam recebem o preço atual do serviço
            migrationBuilder.Sql(@"
                UPDATE ""Agendamentos"" a
                SET ""Preco"" = s.""Preco""
                FROM ""Servico"" s
                WHERE s.""ServicoId"" = a.""ServicoId"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Preco",
                table: "Agendamentos");
        }
    }
}
