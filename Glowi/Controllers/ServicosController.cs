using Glowi.Data;
using Glowi.Models;
using Glowi.Services;
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
        private readonly SupabaseStorageService _storage;
        private readonly ILogger<ServicosController> _logger;

        public ServicosController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SupabaseStorageService storage,
            ILogger<ServicosController> logger)
        {
            _context = context;
            _userManager = userManager;
            _storage = storage;
            _logger = logger;
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
                .Include(s => s.Imagens)
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

            TempData["Sucesso"] = "Serviço criado. Toque em \"Imagens\" para adicionar fotos dele.";
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

            var servico = await _context.Servico
                .Include(s => s.Imagens)
                .FirstOrDefaultAsync(s => s.ServicoId == id && s.Profissional == profisisonal);

            if (servico == null)
                return NotFound();

            var caminhos = servico.Imagens.Select(i => i.CaminhoBucket).ToList();

            _context.Servico.Remove(servico);
            await _context.SaveChangesAsync();

            foreach (var caminho in caminhos)
                await ApagarDoBucket(caminho);

            TempData["Sucesso"] = "Serviço excluído.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Imagens(int id)
        {
            var profissional = await ProfissionalLogadoAsync();

            var servico = await _context.Servico
                .Include(s => s.Imagens.OrderBy(i => i.Ordem))
                .FirstOrDefaultAsync(s => s.ServicoId == id && s.Profissional == profissional);

            if (servico == null)
                return NotFound();

            return View(servico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarImagem(int id, IFormFile? arquivo, string? descricao)
        {
            var profissional = await ProfissionalLogadoAsync();

            var servico = await _context.Servico
                .Include(s => s.Imagens)
                .FirstOrDefaultAsync(s => s.ServicoId == id && s.Profissional == profissional);

            if (servico == null)
                return NotFound();

            descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();

            if (servico.Imagens.Count >= ServicoImagem.MaximoPorServico)
                TempData["Erro"] = $"Cada serviço pode ter no máximo {ServicoImagem.MaximoPorServico} imagens.";
            else if (descricao?.Length > 120)
                TempData["Erro"] = "A descrição deve ter no máximo 120 caracteres.";
            else
            {
                try
                {
                    var caminho = await _storage.EnviarAsync(arquivo!, $"servicos/{servico.ServicoId}");

                    servico.Imagens.Add(new ServicoImagem
                    {
                        CaminhoBucket = caminho,
                        Descricao = descricao,
                        Ordem = servico.Imagens.Select(i => i.Ordem).DefaultIfEmpty(0).Max() + 1
                    });

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch
                    {
                        await ApagarDoBucket(caminho);
                        throw;
                    }

                    TempData["Sucesso"] = "Imagem adicionada.";
                }
                catch (ImagemInvalidaException ex)
                {
                    TempData["Erro"] = ex.Message;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Erro ao enviar imagem do serviço {ServicoId}", servico.ServicoId);
                    TempData["Erro"] = "Não foi possível enviar a imagem agora. Tente de novo em instantes.";
                }
            }

            return RedirectToAction(nameof(Imagens), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverImagem(int id)
        {
            var profissional = await ProfissionalLogadoAsync();

            var imagem = await _context.ServicoImagens
                .FirstOrDefaultAsync(i => i.ServicoImagemId == id && i.Servico!.Profissional == profissional);

            if (imagem == null)
                return NotFound();

            _context.ServicoImagens.Remove(imagem);
            await _context.SaveChangesAsync();
            await ApagarDoBucket(imagem.CaminhoBucket);

            TempData["Sucesso"] = "Imagem removida.";
            return RedirectToAction(nameof(Imagens), new { id = imagem.ServicoId });
        }
        private async Task ApagarDoBucket(string caminho)
        {
            try
            {
                await _storage.ApagarAsync(caminho);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível apagar {Caminho} do bucket", caminho);
            }
        }
    }
}
