using Glowi.Data;
using Glowi.Models;
using Glowi.Models.Enum;
using Glowi.Models.ViewModels.Agendamento;
using Glowi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Glowi.Controllers;

// "Meus agendamentos" do lado da profissional
[Authorize(Roles = "Profissional")]
public class AtendimentosController : Controller
{
    private const int DiasMaximosNoFuturo = 30;
    private const string FormatoInicio = "yyyy-MM-ddTHH:mm";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AgendaService _agenda;

    public AtendimentosController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        AgendaService agenda)
    {
        _context = context;
        _userManager = userManager;
        _agenda = agenda;
    }

    private async Task<int> ProfissionalLogadoIdAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Profissionais
            .Where(p => p.UsuarioId == userId)
            .Select(p => p.ProfissionalId)
            .FirstAsync();
    }

    // só encontra agendamentos da profissional logada
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Confirmar(int id) =>
        ResponderPedido(id, StatusAgendamento.Confirmado, "Agendamento confirmado.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Recusar(int id) =>
        ResponderPedido(id, StatusAgendamento.Recusado, "Pedido recusado.");

    private async Task<IActionResult> ResponderPedido(int id, StatusAgendamento novoStatus, string mensagem)
    {
        var agendamento = await BuscarAgendamento(id);
        if (agendamento == null)
            return NotFound();

        if (agendamento.Status != StatusAgendamento.Pendente || agendamento.Fim <= DateTime.UtcNow)
        {
            TempData["Erro"] = "Esse pedido não está mais pendente.";
            return RedirectToAction(nameof(Index));
        }

        agendamento.Status = novoStatus;
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = mensagem;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Reagendar(int id, DateOnly? data)
    {
        var agendamento = await BuscarAgendamento(id);
        if (agendamento == null)
            return NotFound();

        if (!agendamento.EmAberto)
        {
            TempData["Erro"] = "Esse agendamento não pode mais ser reagendado.";
            return RedirectToAction(nameof(Index));
        }

        var hoje = DateOnly.FromDateTime(Horario.Agora);
        var maximo = hoje.AddDays(DiasMaximosNoFuturo);

        var dia = data ?? DateOnly.FromDateTime(Horario.ParaBrasilia(agendamento.Inicio));
        if (dia < hoje) dia = hoje;
        if (dia > maximo) dia = maximo;

        var vm = new ReagendarViewModel
        {
            Agendamento = agendamento,
            Data = dia,
            Hoje = hoje,
            Maximo = maximo,
            Horarios = await _agenda.ObterHorariosLivres(agendamento.ServicoId, dia, agendamento.AgendamentoId)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reagendar(int id, string inicio)
    {
        if (!DateTime.TryParseExact(inicio, FormatoInicio, CultureInfo.InvariantCulture, DateTimeStyles.None, out var novoInicio))
            return BadRequest();

        var agendamento = await BuscarAgendamento(id);
        if (agendamento == null)
            return NotFound();

        if (!agendamento.EmAberto)
        {
            TempData["Erro"] = "Esse agendamento não pode mais ser reagendado.";
            return RedirectToAction(nameof(Index));
        }

        var erro = await _agenda.Reagendar(agendamento, novoInicio);
        if (erro != null)
        {
            TempData["Erro"] = erro;
            return RedirectToAction(nameof(Reagendar), new
            {
                id,
                data = DateOnly.FromDateTime(novoInicio).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            });
        }

        TempData["Sucesso"] = $"Agendamento remarcado para {novoInicio:dd/MM 'às' HH:mm}. Avise a cliente!";
        return RedirectToAction(nameof(Index));
    }
}
