namespace Glowi.Models
{
    public class Profissional
    {
        public string UsuarioId { get; set; }
        public ApplicationUser? Usuario { get; set; } //nome e email aqui
        public int ProfissionalId { get; set; }
        public string? Telefone { get; set; }
        public string? Especialidade { get; set; }
        public string? Biografia { get; set; }

        // configurações da agenda (por enquanto fixas, mas no futuro serão editáveis pela profissional)
        public int IntervaloMinutos { get; set; } = 15;
        public int AntecedenciaMinimaHoras { get; set; } = 2;

        public ICollection<Servico> Servicos { get; set; } = new List<Servico>();
    }
}
