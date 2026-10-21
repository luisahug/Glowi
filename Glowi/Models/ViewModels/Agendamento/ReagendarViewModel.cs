namespace Glowi.Models.ViewModels.Agendamento;

public class ReagendarViewModel
{
    public Models.Agendamento Agendamento { get; set; } = null!;
    public DateOnly Data { get; set; }
    public DateOnly Hoje { get; set; }
    public DateOnly Maximo { get; set; }
    public List<DateTime> Horarios { get; set; } = new();
}
