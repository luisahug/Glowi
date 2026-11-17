namespace Glowi.Models
{
    // lista de espera: a cliente quer um horário que está ocupado e pede para ser avisada se ele liberar.
    // quando um agendamento que ocupava esse horário é cancelado ou recusado, LiberadoEm é preenchido
    // e a cliente vê o aviso em "Meus agendamentos".
    public class HorarioDesejado
    {
        public int HorarioDesejadoId { get; set; }

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public int ServicoId { get; set; }
        public Servico? Servico { get; set; }

        public int ProfissionalId { get; set; }
        public Profissional? Profissional { get; set; }

        // em UTC, como os agendamentos
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        // preenchido quando o horário liberou; volta a nulo se alguém agendar nele antes da cliente
        public DateTime? LiberadoEm { get; set; }

        // a cliente já viu o aviso de que o horário liberou
        public bool ClienteViuAviso { get; set; }

        public bool Liberado => LiberadoEm != null;
    }
}
