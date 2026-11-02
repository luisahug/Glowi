namespace Glowi.Models
{
    // profissional que a cliente marcou como favorita (cada par cliente × profissional aparece uma vez só)
    public class Favorito
    {
        public int FavoritoId { get; set; }

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int ProfissionalId { get; set; }
        public Profissional? Profissional { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
