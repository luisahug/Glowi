using Glowi.Data;
using Glowi.Models;
using Glowi.Models.Enum;
using Glowi.Models.ViewModels.Agendamento;
using Glowi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Controllers;

[Authorize(Roles = "Profissional")]
public class AtendimentosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AtendimentosController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
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

    private async Task<Agendamento?> BuscarAgendamento(int id)
    {
        var profissionalId = await ProfissionalLogadoIdAsync();
        return await _context.Agendamentos
            .Include(a => a.Servico)
            .Include(a => a.Cliente).ThenInclude(c => c!.Usuario)
            .FirstOrDefaultAsync(a => a.AgendamentoId == id && a.ProfissionalId == profissionalId);
    }

    public async Task<IActionResult> Index()
    {
        var profissionalId = await ProfissionalLogadoIdAsync();
        var agora = DateTime.UtcNow;

        var agendamentos = await _context.Agendamentos
            .Include(a => a.Servico)
            .Include(a => a.Cliente).ThenInclude(c => c!.Usuario)
            .Where(a => a.ProfissionalId == profissionalId)
            .ToListAsync();

        var vm = new MeusAgendamentosViewModel
        {
            Pendentes = agendamentos
                .Where(a => a.Status == StatusAgendamento.Pendente && a.Fim > agora)
                .OrderBy(a => a.Inicio)
                .ToList(),
            Proximos = agendamentos
                .Where(a => a.Status == StatusAgendamento.Confirmado && a.Fim > agora)
                .OrderBy(a => a.Inicio)
                .ToList(),
            Historico = agendamentos
                .Where(a => a.Fim <= agora
                         || a.Status == StatusAgendamento.Cancelado
                         || a.Status == StatusAgendamento.Recusado)
                .OrderByDescending(a => a.Inicio)
                .ToList()
        };

        return View(vm);
    }

    public async Task<IActionResult> Agenda(DateOnly? data, string? visao)
    {
        var profissionalId = await ProfissionalLogadoIdAsync();
        var hoje = DateOnly.FromDateTime(Horario.Agora);
        var semanal = visao == "semana";

        var dia = data ?? hoje;
        var primeiroDia = semanal ? dia.AddDays(-(((int)dia.DayOfWeek + 6) % 7)) : dia;
        var quantidadeDias = semanal ? 7 : 1;

        var inicio = Horario.ParaUtc(primeiroDia.ToDateTime(TimeOnly.MinValue));
        var fim = Horario.ParaUtc(primeiroDia.AddDays(quantidadeDias).ToDateTime(TimeOnly.MinValue));

        var agendamentos = await _context.Agendamentos
            .Include(a => a.Servico)
            .Include(a => a.Cliente).ThenInclude(c => c!.Usuario)
            .Where(a => a.ProfissionalId == profissionalId
                     && a.Inicio >= inicio && a.Inicio < fim
                     && (a.Status == StatusAgendamento.Pendente || a.Status == StatusAgendamento.Confirmado))
            .OrderBy(a => a.Inicio)
            .ToListAsync();

        var vm = new AgendaViewModel
        {
            Semanal = semanal,
            Data = primeiroDia,
            Hoje = hoje,
            Dias = Enumerable.Range(0, quantidadeDias)
                .Select(i => primeiroDia.AddDays(i))
                .Select(d => new DiaAgenda
                {
                    Dia = d,
                    Agendamentos = agendamentos
                        .Where(a => DateOnly.FromDateTime(Horario.ParaBrasilia(a.Inicio)) == d)
                        .ToList()
                })
                .ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Confirmar(int id, string? returnUrl) =>
        ResponderPedido(id, StatusAgendamento.Confirmado, "Agendamento confirmado.", returnUrl);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Recusar(int id, string? returnUrl) =>
        ResponderPedido(id, StatusAgendamento.Recusado, "Pedido recusado.", returnUrl);

    private async Task<IActionResult> ResponderPedido(int id, StatusAgendamento novoStatus, string mensagem, string? returnUrl)
    {
        var agendamento = await BuscarAgendamento(id);
        if (agendamento == null)
            return NotFound();

        if (agendamento.Status != StatusAgendamento.Pendente || agendamento.Fim <= DateTime.UtcNow)
            TempData["Erro"] = "Esse pedido não está mais pendente.";
        else
        {
            agendamento.Status = novoStatus;
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = mensagem;
        }

        if (Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }
}
