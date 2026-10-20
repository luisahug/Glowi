using Glowi.Models.Enum;

namespace Glowi.Models;

public class Agendamento
{
    public int AgendamentoId { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int ProfissionalId { get; set; }
    public Profissional? Profissional { get; set; }

    public int ServicoId { get; set; }
    public Servico? Servico { get; set; }

    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }

    public StatusAgendamento Status { get; set; } = StatusAgendamento.Pendente;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}