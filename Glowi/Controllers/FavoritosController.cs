using Glowi.Data;
using Glowi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Controllers;

[Authorize(Roles = "Cliente")]
public class FavoritosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<int> ClienteLogadoIdAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Clientes
            .Where(c => c.UsuarioId == userId)
            .Select(c => c.ClienteId)
            .FirstAsync();
    }

    public async Task<IActionResult> Index()
    {
        var clienteId = await ClienteLogadoIdAsync();

        var favoritas = await _context.Profissionais
            .Include(p => p.Usuario)
            .Include(p => p.Servicos.OrderBy(s => s.Nome))
            .Where(p => _context.Favoritos.Any(f => f.ClienteId == clienteId && f.ProfissionalId == p.ProfissionalId))
            .OrderBy(p => p.Usuario!.Nome)
            .ToListAsync();

        return View(favoritas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Favoritar(int profissionalId, string? returnUrl)
    {
        var clienteId = await ClienteLogadoIdAsync();

        if (!await _context.Profissionais.AnyAsync(p => p.ProfissionalId == profissionalId))
            return NotFound();

        var jaFavorita = await _context.Favoritos.AnyAsync(f => f.ClienteId == clienteId && f.ProfissionalId == profissionalId);
        if (!jaFavorita)
        {
            _context.Favoritos.Add(new Favorito { ClienteId = clienteId, ProfissionalId = profissionalId });
            try
            {
                await _context.SaveChangesAsync();
            }
            // clique duplo: o índice único já barrou a segunda inserção
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
            {
            }
        }

        TempData["Sucesso"] = "Adicionada às suas favoritas.";
        return Voltar(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desfavoritar(int profissionalId, string? returnUrl)
    {
        var clienteId = await ClienteLogadoIdAsync();

        var favorito = await _context.Favoritos
            .FirstOrDefaultAsync(f => f.ClienteId == clienteId && f.ProfissionalId == profissionalId);

        if (favorito != null)
        {
            _context.Favoritos.Remove(favorito);
            await _context.SaveChangesAsync();
        }

        TempData["Sucesso"] = "Removida das suas favoritas.";
        return Voltar(returnUrl);
    }

    private IActionResult Voltar(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }
}
