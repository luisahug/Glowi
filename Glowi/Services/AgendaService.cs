using Glowi.Data;
using Glowi.Models;
using Glowi.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Services;

public class AgendaService
{
    private readonly ApplicationDbContext _context;

    public AgendaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DateTime>> ObterHorariosLivres(int servicoId, DateOnly data)
    {
        var servico = await _context.Servico
            .Include(s => s.Profissional)
            .FirstOrDefaultAsync(s => s.ServicoId == servicoId);

        if (servico?.Profissional == null)
            return new List<DateTime>();

        var prof = servico.Profissional;

        var disponibilidade = await _context.Disponibilidades
            .Where(d => d.ProfissionalId == prof.ProfissionalId && d.DiaSemana == data.DayOfWeek)
            .OrderBy(d => d.HoraInicio)
            .ToListAsync();

        var inicioDia = Horario.ParaUtc(data.ToDateTime(TimeOnly.MinValue));
        var fimDia = Horario.ParaUtc(data.AddDays(1).ToDateTime(TimeOnly.MinValue));

        var ocupados = (await _context.Agendamentos
                .Where(a => a.ProfissionalId == prof.ProfissionalId
                         && a.Status != StatusAgendamento.Cancelado
                         && a.Status != StatusAgendamento.Recusado
                         && a.Inicio < fimDia
                         && a.Fim > inicioDia)
                .Select(a => new { a.Inicio, a.Fim })
                .ToListAsync())
            .Select(a => (Inicio: Horario.ParaBrasilia(a.Inicio), Fim: Horario.ParaBrasilia(a.Fim)))
            .ToList();

        return CalcularLivres(servico, prof, data, disponibilidade, ocupados);
    }

    // versão em lote (busca do cliente): horários livres de vários serviços no mesmo dia, com 2 consultas no total.
    // de/ate filtram pelo horário de início (ate exclusivo). Os serviços precisam vir com Profissional carregado.
    public async Task<Dictionary<int, List<DateTime>>> ObterHorariosLivres(
        IEnumerable<Servico> servicos, DateOnly data, TimeOnly? de = null, TimeOnly? ate = null)
    {
        var lista = servicos.Where(s => s.Profissional != null).DistinctBy(s => s.ServicoId).ToList();
        var profIds = lista.Select(s => s.Profissional!.ProfissionalId).Distinct().ToList();

        var disponibilidades = (await _context.Disponibilidades
                .Where(d => profIds.Contains(d.ProfissionalId) && d.DiaSemana == data.DayOfWeek)
                .OrderBy(d => d.HoraInicio)
                .ToListAsync())
            .ToLookup(d => d.ProfissionalId);

        var inicioDia = Horario.ParaUtc(data.ToDateTime(TimeOnly.MinValue));
        var fimDia = Horario.ParaUtc(data.AddDays(1).ToDateTime(TimeOnly.MinValue));

        var ocupados = (await _context.Agendamentos
                .Where(a => profIds.Contains(a.ProfissionalId)
                         && a.Status != StatusAgendamento.Cancelado
                         && a.Status != StatusAgendamento.Recusado
                         && a.Inicio < fimDia
                         && a.Fim > inicioDia)
                .Select(a => new { a.ProfissionalId, a.Inicio, a.Fim })
                .ToListAsync())
            .ToLookup(a => a.ProfissionalId, a => (Inicio: Horario.ParaBrasilia(a.Inicio), Fim: Horario.ParaBrasilia(a.Fim)));

        return lista.ToDictionary(
            s => s.ServicoId,
            s =>
            {
                var profId = s.Profissional!.ProfissionalId;
                return CalcularLivres(s, s.Profissional, data, disponibilidades[profId], ocupados[profId].ToList())
                    .Where(t => (de == null || TimeOnly.FromDateTime(t) >= de) &&
                                (ate == null || TimeOnly.FromDateTime(t) < ate))
                    .ToList();
            });
    }

    private static List<DateTime> CalcularLivres(
        Servico servico, Profissional prof, DateOnly data,
        IEnumerable<Disponibilidade> disponibilidade, List<(DateTime Inicio, DateTime Fim)> ocupados)
    {
        var duracao = TimeSpan.FromMinutes(servico.DuracaoMinutos);
        var intervalo = TimeSpan.FromMinutes(prof.IntervaloMinutos);
        var limiteMinimo = Horario.Agora.AddHours(prof.AntecedenciaMinimaHoras);

        var livres = new List<DateTime>();

        foreach (var faixa in disponibilidade)
        {
            var inicio = data.ToDateTime(faixa.HoraInicio);
            var fimFaixa = data.ToDateTime(faixa.HoraFim);

            for (var t = inicio; t + duracao <= fimFaixa; t += intervalo)
            {
                if (t < limiteMinimo)
                    continue;

                var fimServico = t + duracao;
                var conflita = ocupados.Any(o => t < o.Fim && o.Inicio < fimServico);

                if (!conflita)
                    livres.Add(t);
            }
        }

        return livres;
    }

    public async Task<string?> Agendar(int clienteId, int servicoId, DateTime inicioBrasilia)
    {
        var livres = await ObterHorariosLivres(servicoId, DateOnly.FromDateTime(inicioBrasilia));
        if (!livres.Contains(inicioBrasilia))
            return "Esse horário não está mais disponível. Escolha outro.";

        var servico = await _context.Servico
            .Include(s => s.Profissional)
            .FirstAsync(s => s.ServicoId == servicoId);

        var agendamento = new Agendamento
        {
            ClienteId = clienteId,
            ProfissionalId = servico.Profissional!.ProfissionalId,
            ServicoId = servicoId,
            Inicio = Horario.ParaUtc(inicioBrasilia),
            Fim = Horario.ParaUtc(inicioBrasilia.AddMinutes(servico.DuracaoMinutos)),
            Preco = servico.Preco,
            Status = StatusAgendamento.Pendente
        };

        _context.Agendamentos.Add(agendamento);

        try
        {
            await _context.SaveChangesAsync();
            return null;
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23P01" })
        {
            _context.Entry(agendamento).State = EntityState.Detached;
            return "Esse horário acabou de ser reservado. Escolha outro.";
        }
    }
}