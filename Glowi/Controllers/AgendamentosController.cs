using Glowi.Data;
using Glowi.Models;
using Glowi.Models.ViewModels.Agendamento;
using Glowi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Glowi.Controllers;

[Authorize(Roles = "Cliente")]
public class AgendamentosController : Controller
{
    private const int DiasMaximosNoFuturo = 30;
    private const string FormatoInicio = "yyyy-MM-ddTHH:mm";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AgendaService _agenda;

    public AgendamentosController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        AgendaService agenda)
    {
        _context = context;
        _userManager = userManager;
        _agenda = agenda;
    }

    private async Task<int> ClienteLogadoIdAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Clientes
            .Where(c => c.UsuarioId == userId)
            .Select(c => c.ClienteId)
            .FirstAsync();
    }

    private Task<Servico?> BuscarServico(int servicoId) =>
        _context.Servico
            .Include(s => s.Profissional)
            .ThenInclude(p => p!.Usuario)
            .FirstOrDefaultAsync(s => s.ServicoId == servicoId);

    private static bool TentarLerInicio(string? texto, out DateTime inicio) =>
        DateTime.TryParseExact(texto, FormatoInicio, CultureInfo.InvariantCulture, DateTimeStyles.None, out inicio);

    public async Task<IActionResult> CriarNovoAgendamento(int servicoId, DateOnly? data)
    {
        var servico = await BuscarServico(servicoId);
        if (servico == null)
            return NotFound();

        var hoje = DateOnly.FromDateTime(Horario.Agora);
        var maximo = hoje.AddDays(DiasMaximosNoFuturo);

        var dia = data ?? hoje;
        if (dia < hoje) dia = hoje;
        if (dia > maximo) dia = maximo;

        var vm = new AgendarViewModel
        {
            Servico = servico,
            Data = dia,
            Hoje = hoje,
            Maximo = maximo,
            Horarios = await _agenda.ObterHorariosLivres(servicoId, dia)
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> RevisarAgendamento(int servicoId, string inicio)
    {
        if (!TentarLerInicio(inicio, out var dataHora))
            return BadRequest();

        var servico = await BuscarServico(servicoId);
        if (servico == null)
            return NotFound();

        return View(new ConfirmarAgendamentoViewModel { Servico = servico, Inicio = dataHora });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarAgendamento(int servicoId, string inicio)
    {
        if (!TentarLerInicio(inicio, out var dataHora))
            return BadRequest();

        var clienteId = await ClienteLogadoIdAsync();
        var erro = await _agenda.Agendar(clienteId, servicoId, dataHora);

        if (erro != null)
        {
            TempData["Erro"] = erro;
            return RedirectToAction(nameof(CriarNovoAgendamento), new
            {
                servicoId,
                data = DateOnly.FromDateTime(dataHora).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            });
        }

        return RedirectToAction(nameof(FinalizarAgendamento), new { servicoId, inicio });
    }

    public async Task<IActionResult> FinalizarAgendamento(int servicoId, string inicio)
    {
        if (!TentarLerInicio(inicio, out var dataHora))
            return BadRequest();

        var servico = await BuscarServico(servicoId);
        if (servico == null)
            return NotFound();

        return View(new ConfirmarAgendamentoViewModel { Servico = servico, Inicio = dataHora });
    }
}