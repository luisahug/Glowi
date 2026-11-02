using System.ComponentModel.DataAnnotations;

namespace Glowi.Models
{
    // período em que a profissional não atende (folga, férias, compromisso). Sai do cálculo de horários livres.
    public class BloqueioAgenda
    {
        public int BloqueioAgendaId { get; set; }

        public int ProfissionalId { get; set; }
        public Profissional? Profissional { get; set; }

        // em UTC, como os agendamentos
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }

        [Display(Name = "Motivo")]
        [MaxLength(120, ErrorMessage = "O motivo deve ter no máximo 120 caracteres.")]
        public string? Motivo { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
