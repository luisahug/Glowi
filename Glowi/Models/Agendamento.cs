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

    // preço do serviço no momento da reserva (não muda se a profissional alterar o serviço depois)
    public decimal Preco { get; set; }

    public StatusAgendamento Status { get; set; } = StatusAgendamento.Pendente;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // não é coluna no banco: concluído = confirmado e o horário já passou
    public bool Concluido => Status == StatusAgendamento.Confirmado && Fim <= DateTime.UtcNow;

    // pendente ou confirmado e ainda não começou: pode cancelar
    public bool EmAberto =>
        (Status == StatusAgendamento.Pendente || Status == StatusAgendamento.Confirmado) && Inicio > DateTime.UtcNow;
}