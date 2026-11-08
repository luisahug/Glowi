using Glowi.Services;

namespace Glowi.Models.ViewModels.Agendamento;

public class AgendarViewModel
{
    public Servico Servico { get; set; } = null!;
    public DateOnly Data { get; set; }
    public DateOnly Hoje { get; set; }
    public DateOnly Maximo { get; set; }
    public List<DateTime> Horarios { get; set; } = new();

    // agendamentos em aberto do(a) cliente nesse dia (com qualquer profissional): horários que se sobrepõem a eles não podem ser marcados
    public List<Models.Agendamento> MeusAgendamentosNoDia { get; set; } = new();

    public Models.Agendamento? ConflitoCom(DateTime inicioBrasilia)
    {
        var inicio = Horario.ParaUtc(inicioBrasilia);
        var fim = Horario.ParaUtc(inicioBrasilia.AddMinutes(Servico.DuracaoMinutos));
        return MeusAgendamentosNoDia.FirstOrDefault(a => a.Inicio < fim && inicio < a.Fim);
    }
}