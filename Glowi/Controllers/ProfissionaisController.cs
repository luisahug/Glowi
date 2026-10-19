using Glowi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Controllers
{
    [Authorize]
    public class ProfissionaisController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfissionaisController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var profissionais = await _context.Profissionais
                .Include(p => p.Usuario)
                .Include(p => p.Servicos)
                .Where(p => p.Servicos.Any())
                .OrderBy(p => p.Usuario!.Nome)
                .ToListAsync();

            return View(profissionais);
        }
        public async Task<IActionResult> Detalhes(int id)
        {
            var profissional = await _context.Profissionais
                .Include(p => p.Usuario)
                .Include(p => p.Servicos.OrderBy(s => s.Nome))
                .FirstOrDefaultAsync(p => p.ProfissionalId == id);

            if (profissional == null)
                return NotFound();

            return View(profissional);
        }

    }
}
