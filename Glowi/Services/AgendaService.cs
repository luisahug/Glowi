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
        var duracao = TimeSpan.FromMinutes(servico.DuracaoMinutos);
        var intervalo = TimeSpan.FromMinutes(prof.IntervaloMinutos);
        var limiteMinimo = Horario.Agora.AddHours(prof.AntecedenciaMinimaHoras);

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

        var servico = await _context.Servico.FirstAsync(s => s.ServicoId == servicoId);

        var agendamento = new Agendamento
        {
            ClienteId = clienteId,
            ProfissionalId = servico.Profissional.ProfissionalId,
            ServicoId = servicoId,
            Inicio = Horario.ParaUtc(inicioBrasilia),
            Fim = Horario.ParaUtc(inicioBrasilia.AddMinutes(servico.DuracaoMinutos)),
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