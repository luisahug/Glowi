using Glowi.Data;
using Glowi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Controllers
{
    [Authorize(Roles = "Profissional")]
    public class ServicosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ServicosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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
        
            var servicos = await _context.Servico
                .Where(s => s.Profissional == profissional)
                .OrderBy(s => s.Nome)
                .ToListAsync();

            return View(servicos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,DuracaoMinutos,Preco")] Servico servico)
        {
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Não foi possível salvar. Confira os campos e tente de novo.";
                return RedirectToAction(nameof(Index));
            }

            servico.Profissional = await ProfissionalLogadoAsync();
            _context.Servico.Add(servico);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Serviço criado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Nome,DuracaoMinutos,Preco")] Servico dados)
        {
            var profissional = await ProfissionalLogadoAsync();

            var servico = await _context.Servico.FirstOrDefaultAsync(s => s.ServicoId == id && s.Profissional == profissional);

            if (servico == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Não foi possível salvar. Confira os campos e tente de novo.";
                return RedirectToAction(nameof(Index));
            }

            servico.Nome = dados.Nome;
            servico.DuracaoMinutos = dados.DuracaoMinutos;
            servico.Preco = dados.Preco;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Serviço atualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var profisisonal = await ProfissionalLogadoAsync();

            var servico = await _context.Servico.FirstOrDefaultAsync(s => s.ServicoId == id && s.Profissional == profisisonal);

            if (servico == null)
                return NotFound();

            _context.Servico.Remove(servico);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Serviço excluído.";
            return RedirectToAction(nameof(Index));
        }
    }
}
