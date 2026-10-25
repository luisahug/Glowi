namespace Glowi.Models.ViewModels.Agendamento;

public class AgendaViewModel
{
    public bool Semanal { get; set; }
    public DateOnly Data { get; set; }
    public DateOnly Hoje { get; set; }

    // dias exibidos (1 na visão diária, 7 na semanal), cada um com seus agendamentos em ordem
    public List<DiaAgenda> Dias { get; set; } = new();

    public DateOnly Anterior => Data.AddDays(Semanal ? -7 : -1);
    public DateOnly Proximo => Data.AddDays(Semanal ? 7 : 1);

    public int Total => Dias.Sum(d => d.Agendamentos.Count);
    public decimal FaturamentoPrevisto => Dias
        .SelectMany(d => d.Agendamentos)
        .Where(a => a.Status == Enum.StatusAgendamento.Confirmado)
        .Sum(a => a.Preco);
}

public class DiaAgenda
{
    public DateOnly Dia { get; set; }
    public List<Models.Agendamento> Agendamentos { get; set; } = new();
}
