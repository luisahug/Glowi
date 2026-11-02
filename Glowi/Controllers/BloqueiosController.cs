using Glowi.Data;
using Glowi.Models;
using Glowi.Models.Enum;
using Glowi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Controllers;

[Authorize(Roles = "Profissional")]
public class BloqueiosController : Controller
{
    private const int DiasMaximosNoFuturo = 365;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BloqueiosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<int> ProfissionalLogadoIdAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Profissionais
            .Where(p => p.UsuarioId == userId)
            .Select(p => p.ProfissionalId)
            .FirstAsync();
    }

    public async Task<IActionResult> Index()
    {
        var profissionalId = await ProfissionalLogadoIdAsync();
        var agora = DateTime.UtcNow;

        // só os que ainda não terminaram
        var bloqueios = await _context.BloqueiosAgenda
            .Where(b => b.ProfissionalId == profissionalId && b.Fim > agora)
            .OrderBy(b => b.Inicio)
            .ToListAsync();

        return View(bloqueios);
    }

    // dia inteiro: bloqueia de 00:00 de dataInicio até 00:00 do dia seguinte a dataFim.
    // senão: de dataInicio horaInicio até dataFim horaFim.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DateOnly? dataInicio, DateOnly? dataFim, TimeOnly? horaInicio, TimeOnly? horaFim, bool diaInteiro, string? motivo)
    {
        motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        if (dataInicio == null || (!diaInteiro && (horaInicio == null || horaFim == null)))
        {
            TempData["Erro"] = "Preencha a data e os horários do bloqueio.";
            return RedirectToAction(nameof(Index));
        }

        var fimData = dataFim ?? dataInicio.Value;
        var inicioBrasilia = dataInicio.Value.ToDateTime(diaInteiro ? TimeOnly.MinValue : horaInicio!.Value);
        var fimBrasilia = diaInteiro
            ? fimData.AddDays(1).ToDateTime(TimeOnly.MinValue)
            : fimData.ToDateTime(horaFim!.Value);

        var erro =
            fimBrasilia <= inicioBrasilia ? "O fim do bloqueio precisa ser depois do início." :
            fimBrasilia <= Horario.Agora ? "Esse período já passou." :
            DateOnly.FromDateTime(inicioBrasilia) > DateOnly.FromDateTime(Horario.Agora).AddDays(DiasMaximosNoFuturo) ? "Só é possível bloquear até um ano à frente." :
            motivo?.Length > 120 ? "O motivo deve ter no máximo 120 caracteres." :
            null;

        if (erro != null)
        {
            TempData["Erro"] = erro;
            return RedirectToAction(nameof(Index));
        }

        var profissionalId = await ProfissionalLogadoIdAsync();
        var inicio = Horario.ParaUtc(inicioBrasilia);
        var fim = Horario.ParaUtc(fimBrasilia);

        //não deixa cadastrar um bloqueio que se sobrepõe a outro
        var isPeriodoBloqueado = await _context.BloqueiosAgenda.AnyAsync(b =>
            b.ProfissionalId == profissionalId && b.Inicio < fim && inicio < b.Fim);

        if (isPeriodoBloqueado)
        {
            TempData["Erro"] = "Esse período se sobrepõe a outro bloqueio que você já cadastrou.";
            return RedirectToAction(nameof(Index));
        }

        _context.BloqueiosAgenda.Add(new BloqueioAgenda
        {
            ProfissionalId = profissionalId,
            Inicio = inicio,
            Fim = fim,
            Motivo = motivo
        });
        await _context.SaveChangesAsync();

        // agendamentos já marcados no período não são cancelados automaticamente
        var agendamentosNoPeriodo = await _context.Agendamentos.CountAsync(a =>
            a.ProfissionalId == profissionalId
            && (a.Status == StatusAgendamento.Pendente || a.Status == StatusAgendamento.Confirmado)
            && a.Inicio < fim && inicio < a.Fim);

        TempData["Sucesso"] = agendamentosNoPeriodo == 0
            ? "Período bloqueado. As clientes não verão horários livres nele."
            : $"Período bloqueado. Atenção: você já tem {agendamentosNoPeriodo} agendamento(s) nesse período, que continuam marcados.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var profissionalId = await ProfissionalLogadoIdAsync();

        var bloqueio = await _context.BloqueiosAgenda.FirstOrDefaultAsync(b => b.BloqueioAgendaId == id && b.ProfissionalId == profissionalId);

        if (bloqueio == null)
            return NotFound();

        _context.BloqueiosAgenda.Remove(bloqueio);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Bloqueio removido. Os horários voltaram a ficar disponíveis.";
        return RedirectToAction(nameof(Index));
    }
}
