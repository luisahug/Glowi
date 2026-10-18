namespace Glowi.Models
{
    public class Cliente
    {
        public string UsuarioId { get; set; }
        public int ClienteId { get; set; }
        public ApplicationUser? Usuario { get; set; } //nome e email aqui
        public string? Telefone { get; set; }
    }
}
