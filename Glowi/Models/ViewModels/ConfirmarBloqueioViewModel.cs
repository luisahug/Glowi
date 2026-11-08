namespace Glowi.Models.ViewModels;

// tela de aviso quando o período que a profissional quer bloquear já tem agendamentos marcados.
// guarda o que ela preencheu para reenviar o formulário com cancelarAgendamentos=true.
public class ConfirmarBloqueioViewModel
{
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFim { get; set; }
    public bool DiaInteiro { get; set; }
    public string? Motivo { get; set; }

    public List<Models.Agendamento> Agendamentos { get; set; } = new();
}
