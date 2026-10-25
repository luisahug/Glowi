namespace Glowi.Models
{
    public class Disponibilidade
    {
        public int DisponibilidadeId { get; set; }

        public int ProfissionalId { get; set; }
        public Profissional? Profissional { get; set; }

        public DayOfWeek DiaSemana { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFim { get; set; }
    }
}
