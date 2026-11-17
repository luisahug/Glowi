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

    public async Task<List<DateTime>> ObterHorariosLivres(int servicoId, DateOnly data) =>
        (await ObterHorariosDoDia(servicoId, data)).Where(h => h.Livre).Select(h => h.Inicio).ToList();

    // todos os horários de atendimento do dia (já respeitando antecedência mínima e bloqueios),
    // marcando quais estão livres e quais estão ocupados por outro agendamento (esses podem entrar na lista de espera)
    public async Task<List<HorarioDoDia>> ObterHorariosDoDia(int servicoId, DateOnly data)
    {
        var servico = await _context.Servico
            .Include(s => s.Profissional)
            .FirstOrDefaultAsync(s => s.ServicoId == servicoId);

        if (servico?.Profissional == null)
            return new List<HorarioDoDia>();

        var prof = servico.Profissional;

        var disponibilidade = await _context.Disponibilidades
            .Where(d => d.ProfissionalId == prof.ProfissionalId && d.DiaSemana == data.DayOfWeek)
            .OrderBy(d => d.HoraInicio)
            .ToListAsync();

        var inicioDia = Horario.ParaUtc(data.ToDateTime(TimeOnly.MinValue));
        var fimDia = Horario.ParaUtc(data.AddDays(1).ToDateTime(TimeOnly.MinValue));

        var agendados = (await _context.Agendamentos
                .Where(a => a.ProfissionalId == prof.ProfissionalId
                         && a.Status != StatusAgendamento.Cancelado
                         && a.Status != StatusAgendamento.Recusado
                         && a.Inicio < fimDia
                         && a.Fim > inicioDia)
                .Select(a => new { a.Inicio, a.Fim })
                .ToListAsync())
            .Select(a => (Inicio: Horario.ParaBrasilia(a.Inicio), Fim: Horario.ParaBrasilia(a.Fim)))
            .ToList();

        var bloqueios = (await _context.BloqueiosAgenda
                .Where(b => b.ProfissionalId == prof.ProfissionalId
                         && b.Inicio < fimDia
                         && b.Fim > inicioDia)
                .Select(b => new { b.Inicio, b.Fim })
                .ToListAsync())
            .Select(b => (Inicio: Horario.ParaBrasilia(b.Inicio), Fim: Horario.ParaBrasilia(b.Fim)))
            .ToList();

        return CalcularHorarios(servico, prof, data, disponibilidade, agendados, bloqueios);
    }

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

        var agendados = await _context.Agendamentos
                .Where(a => profIds.Contains(a.ProfissionalId)
                         && a.Status != StatusAgendamento.Cancelado
                         && a.Status != StatusAgendamento.Recusado
                         && a.Inicio < fimDia
                         && a.Fim > inicioDia)
                .Select(a => new { a.ProfissionalId, a.Inicio, a.Fim })
                .ToListAsync();

        var bloqueios = await _context.BloqueiosAgenda
                .Where(b => profIds.Contains(b.ProfissionalId)
                         && b.Inicio < fimDia
                         && b.Fim > inicioDia)
                .Select(b => new { b.ProfissionalId, b.Inicio, b.Fim })
                .ToListAsync();

        var ocupados = agendados
            .ToLookup(a => a.ProfissionalId, a => (Inicio: Horario.ParaBrasilia(a.Inicio), Fim: Horario.ParaBrasilia(a.Fim)));
        var bloqueados = bloqueios
            .ToLookup(b => b.ProfissionalId, b => (Inicio: Horario.ParaBrasilia(b.Inicio), Fim: Horario.ParaBrasilia(b.Fim)));

        return lista.ToDictionary(
            s => s.ServicoId,
            s =>
            {
                var profId = s.Profissional!.ProfissionalId;
                return CalcularHorarios(s, s.Profissional, data, disponibilidades[profId], ocupados[profId].ToList(), bloqueados[profId].ToList())
                    .Where(h => h.Livre)
                    .Select(h => h.Inicio)
                    .Where(t => (de == null || TimeOnly.FromDateTime(t) >= de) &&
                                (ate == null || TimeOnly.FromDateTime(t) < ate))
                    .ToList();
            });
    }

    private static List<HorarioDoDia> CalcularHorarios(
        Servico servico, Profissional prof, DateOnly data, IEnumerable<Disponibilidade> disponibilidade,
        List<(DateTime Inicio, DateTime Fim)> agendados, List<(DateTime Inicio, DateTime Fim)> bloqueios)
    {
        var duracao = TimeSpan.FromMinutes(servico.DuracaoMinutos);
        var intervalo = TimeSpan.FromMinutes(prof.IntervaloMinutos);
        var limiteMinimo = Horario.Agora.AddHours(prof.AntecedenciaMinimaHoras);

        var horarios = new List<HorarioDoDia>();

        foreach (var faixa in disponibilidade)
        {
            var inicio = data.ToDateTime(faixa.HoraInicio);
            var fimFaixa = data.ToDateTime(faixa.HoraFim);

            for (var t = inicio; t + duracao <= fimFaixa; t += intervalo)
            {
                if (t < limiteMinimo)
                    continue;

                var fimServico = t + duracao;

                // bloqueado: a profissional não atende, então nem aparece (nem para lista de espera)
                if (bloqueios.Any(b => t < b.Fim && b.Inicio < fimServico))
                    continue;

                var ocupado = agendados.Any(o => t < o.Fim && o.Inicio < fimServico);
                horarios.Add(new HorarioDoDia(t, !ocupado));
            }
        }

        return horarios;
    }

    public Task<List<Agendamento>> AgendamentosDoClienteNoPeriodo(int clienteId, DateTime inicioUtc, DateTime fimUtc) =>
        _context.Agendamentos
            .Include(a => a.Servico)
            .Include(a => a.Profissional).ThenInclude(p => p!.Usuario)
            .Where(a => a.ClienteId == clienteId
                     && (a.Status == StatusAgendamento.Pendente || a.Status == StatusAgendamento.Confirmado)
                     && a.Inicio < fimUtc && inicioUtc < a.Fim)
            .OrderBy(a => a.Inicio)
            .ToListAsync();

    public static string MensagemConflitoCliente(Agendamento a) =>
        $"Você já tem um agendamento nesse horário: {a.Servico?.Nome} com {a.Profissional?.Usuario?.Nome}, " +
        $"{Horario.ParaBrasilia(a.Inicio):dd/MM 'às' HH:mm}. Para marcar este, cancele primeiro esse agendamento em Meus agendamentos.";

    public async Task<string?> Agendar(int clienteId, int servicoId, DateTime inicioBrasilia)
    {
        var livres = await ObterHorariosLivres(servicoId, DateOnly.FromDateTime(inicioBrasilia));
        if (!livres.Contains(inicioBrasilia))
            return "Esse horário não está mais disponível. Escolha outro.";

        var servico = await _context.Servico
            .Include(s => s.Profissional)
            .FirstAsync(s => s.ServicoId == servicoId);

        var conflitos = await AgendamentosDoClienteNoPeriodo(clienteId,
            Horario.ParaUtc(inicioBrasilia), Horario.ParaUtc(inicioBrasilia.AddMinutes(servico.DuracaoMinutos)));
        if (conflitos.Count > 0)
            return MensagemConflitoCliente(conflitos[0]);

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
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23P01" })
        {
            _context.Entry(agendamento).State = EntityState.Detached;
            return "Esse horário acabou de ser reservado. Escolha outro.";
        }

        await AtualizarListaEsperaAposAgendar(agendamento);
        return null;
    }

    // ---------- lista de espera (horários desejados) ----------

    public async Task<string?> EntrarNaListaEspera(int clienteId, int servicoId, DateTime inicioBrasilia)
    {
        var servico = await _context.Servico
            .Include(s => s.Profissional)
            .FirstOrDefaultAsync(s => s.ServicoId == servicoId);
        if (servico?.Profissional == null)
            return "Serviço não encontrado.";

        var horario = (await ObterHorariosDoDia(servicoId, DateOnly.FromDateTime(inicioBrasilia)))
            .FirstOrDefault(h => h.Inicio == inicioBrasilia);

        if (horario == null)
            return "Esse horário não está disponível para lista de espera.";
        if (horario.Livre)
            return "Esse horário acabou de liberar! Você já pode agendar.";

        var inicio = Horario.ParaUtc(inicioBrasilia);
        var fim = Horario.ParaUtc(inicioBrasilia.AddMinutes(servico.DuracaoMinutos));

        var conflitos = await AgendamentosDoClienteNoPeriodo(clienteId, inicio, fim);
        if (conflitos.Count > 0)
            return MensagemConflitoCliente(conflitos[0]);

        var jaNaLista = await _context.HorariosDesejados
            .AnyAsync(h => h.ClienteId == clienteId && h.ServicoId == servicoId && h.Inicio == inicio);
        if (jaNaLista)
            return null;

        _context.HorariosDesejados.Add(new HorarioDesejado
        {
            ClienteId = clienteId,
            ServicoId = servicoId,
            ProfissionalId = servico.Profissional.ProfissionalId,
            Inicio = inicio,
            Fim = fim
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        // clique duplo: o índice único já barrou a segunda inserção
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
        }

        return null;
    }

    // chamado depois de salvar um agendamento como Cancelado ou Recusado:
    // quem esperava um horário que coincide com ele, e que agora está livre de verdade, é avisado(a).
    public async Task LiberarListaEspera(Agendamento agendamento)
    {
        var agora = DateTime.UtcNow;

        var esperando = await _context.HorariosDesejados
            .Where(h => h.ProfissionalId == agendamento.ProfissionalId
                     && h.LiberadoEm == null
                     && h.Inicio > agora
                     && h.Inicio < agendamento.Fim && agendamento.Inicio < h.Fim)
            .ToListAsync();

        if (esperando.Count == 0)
            return;

        // o horário pode continuar ocupado por outro agendamento ou já ter caído na antecedência mínima
        foreach (var grupo in esperando.GroupBy(h => (h.ServicoId, Dia: DateOnly.FromDateTime(Horario.ParaBrasilia(h.Inicio)))))
        {
            var livres = await ObterHorariosLivres(grupo.Key.ServicoId, grupo.Key.Dia);
            foreach (var h in grupo.Where(h => livres.Contains(Horario.ParaBrasilia(h.Inicio))))
            {
                h.LiberadoEm = agora;
                h.ClienteViuAviso = false;
            }
        }

        await _context.SaveChangesAsync();
    }

    // o período voltou a ficar ocupado (novo agendamento ou bloqueio): avisos de "liberou" nele voltam a "aguardando"
    public async Task VoltarParaEspera(int profissionalId, DateTime inicioUtc, DateTime fimUtc)
    {
        var liberados = await _context.HorariosDesejados
            .Where(h => h.ProfissionalId == profissionalId
                     && h.LiberadoEm != null
                     && h.Inicio < fimUtc && inicioUtc < h.Fim)
            .ToListAsync();

        if (liberados.Count == 0)
            return;

        foreach (var h in liberados)
        {
            h.LiberadoEm = null;
            h.ClienteViuAviso = false;
        }

        await _context.SaveChangesAsync();
    }

    private async Task AtualizarListaEsperaAposAgendar(Agendamento agendamento)
    {
        // a cliente conseguiu o horário que esperava: sai da lista
        var proprio = await _context.HorariosDesejados
            .Where(h => h.ClienteId == agendamento.ClienteId
                     && h.ServicoId == agendamento.ServicoId
                     && h.Inicio == agendamento.Inicio)
            .ToListAsync();

        if (proprio.Count > 0)
        {
            _context.HorariosDesejados.RemoveRange(proprio);
            await _context.SaveChangesAsync();
        }

        await VoltarParaEspera(agendamento.ProfissionalId, agendamento.Inicio, agendamento.Fim);
    }
}

public record HorarioDoDia(DateTime Inicio, bool Livre);
