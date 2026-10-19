using Glowi.Data;
using Glowi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Controllers;

[Authorize(Roles = "Profissional")]
public class DisponibilidadesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DisponibilidadesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<Profissional> ProfissionalLogadoAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Profissionais
            .Where(p => p.UsuarioId == userId)
            .FirstAsync();
    }

    public async Task<IActionResult> Index()
    {
        var profissional = await ProfissionalLogadoAsync();

        var horarios = await _context.Disponibilidades
            .Where(d => d.ProfissionalId == profissional.ProfissionalId)
            .OrderBy(d => d.HoraInicio)
            .ToListAsync();

        return View(horarios);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("DiaSemana,HoraInicio,HoraFim")] Disponibilidade nova)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Preencha o dia e os dois horários.";
            return RedirectToAction(nameof(Index));
        }

        if (nova.HoraFim <= nova.HoraInicio)
        {
            TempData["Erro"] = "O horário de fim precisa ser depois do horário de início.";
            return RedirectToAction(nameof(Index));
        }

        var profissional = await ProfissionalLogadoAsync();

        //não deixa cadastrar um intervalo que se sobrepõe a outro do mesmo dia
        var isHorarioPreenchido = await _context.Disponibilidades.AnyAsync(d =>
            d.ProfissionalId == profissional.ProfissionalId &&
            d.DiaSemana == nova.DiaSemana &&
            nova.HoraInicio < d.HoraFim &&
            d.HoraInicio < nova.HoraFim);

        if (isHorarioPreenchido)
        {
            TempData["Erro"] = "Esse horário se sobrepõe a outro que você já cadastrou nesse dia.";
            return RedirectToAction(nameof(Index));
        }

        nova.ProfissionalId = profissional.ProfissionalId;
        _context.Disponibilidades.Add(nova);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Horário adicionado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var profissional = await ProfissionalLogadoAsync();

        var horario = await _context.Disponibilidades.FirstOrDefaultAsync(d => d.DisponibilidadeId == id && d.ProfissionalId == profissional.ProfissionalId);

        if (horario == null)
            return NotFound();

        _context.Disponibilidades.Remove(horario);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Horário removido.";
        return RedirectToAction(nameof(Index));
    }
}