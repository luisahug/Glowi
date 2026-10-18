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
    }
}
