namespace Glowi.Models.ViewModels.Agendamento;

public class ConfirmarAgendamentoViewModel
{
    public Servico Servico { get; set; } = null!;
    public DateTime Inicio { get; set; }

    // agendamentos em aberto do(a) cliente que coincidem com esse horário (com qualquer profissional)
    public List<Models.Agendamento> Conflitos { get; set; } = new();
}