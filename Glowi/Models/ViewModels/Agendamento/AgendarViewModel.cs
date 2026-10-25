namespace Glowi.Models.ViewModels.Agendamento;

public class AgendarViewModel
{
    public Servico Servico { get; set; } = null!;
    public DateOnly Data { get; set; }
    public DateOnly Hoje { get; set; }
    public DateOnly Maximo { get; set; }
    public List<DateTime> Horarios { get; set; } = new();
}