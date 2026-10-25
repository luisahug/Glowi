namespace Glowi.Models.ViewModels.Agendamento;

public class MeusAgendamentosViewModel
{
    // só usado na tela da profissional: pedidos esperando confirmar/recusar
    public List<Models.Agendamento> Pendentes { get; set; } = new();
    public List<Models.Agendamento> Proximos { get; set; } = new();
    public List<Models.Agendamento> Historico { get; set; } = new();
}
