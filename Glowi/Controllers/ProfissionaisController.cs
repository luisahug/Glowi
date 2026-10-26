using Glowi.Data;
using Glowi.Models;
using Glowi.Models.ViewModels.Profissionais;
using Glowi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Controllers
{
    [Authorize]
    public class ProfissionaisController : Controller
    {
        private const int DiasMaximosNoFuturo = 30;

        private readonly ApplicationDbContext _context;
        private readonly AgendaService _agenda;

        public ProfissionaisController(ApplicationDbContext context, AgendaService agenda)
        {
            _context = context;
            _agenda = agenda;
        }

        private static string Contem(string texto) =>
            "%" + texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";

        private static IEnumerable<Servico> ServicosRelevantes(Profissional p, string? busca)
        {
            var encontrados = busca == null
                ? new List<Servico>()
                : p.Servicos.Where(s => s.Nome.Contains(busca, StringComparison.OrdinalIgnoreCase)).ToList();
            return encontrados.Any() ? encontrados : p.Servicos;
        }

        public async Task<IActionResult> Index(string? busca, string? tipo, string? ver, DateOnly? data, string? turno)
        {
            busca = string.IsNullOrWhiteSpace(busca) ? null : busca.Trim();
            tipo = string.IsNullOrWhiteSpace(tipo) ? null : tipo.Trim();

            var hoje = DateOnly.FromDateTime(Horario.Agora);
            var maximo = hoje.AddDays(DiasMaximosNoFuturo);
            if (data < hoje || data > maximo) data = null;

            var faixaTurno = BuscaViewModel.Turnos.FirstOrDefault(t => t.Chave == turno);
            if (data == null || faixaTurno.Chave == null) turno = null;

            var vm = new BuscaViewModel
            {
                Busca = busca,
                Tipo = tipo,
                VerServicos = ver == "servicos",
                Data = data,
                Turno = turno,
                Hoje = hoje,
                Maximo = maximo
            };

            var especialidades = await _context.Profissionais
                .Where(p => p.Servicos.Any() && p.Especialidade != null && p.Especialidade.Trim() != "")
                .Select(p => p.Especialidade!.Trim())
                .ToListAsync();

            vm.Tipos = especialidades
                .GroupBy(e => e.ToLower())
                .Select(g => g.First())
                .OrderBy(e => e)
                .ToList();

            var padrao = busca == null ? null : Contem(busca);

            if (vm.VerServicos)
            {
                var servicos = _context.Servico
                    .Include(s => s.Profissional)
                    .ThenInclude(p => p!.Usuario)
                    .AsQueryable();

                if (tipo != null)
                    servicos = servicos.Where(s => EF.Functions.ILike(s.Profissional!.Especialidade!.Trim(), tipo));

                if (padrao != null)
                    servicos = servicos.Where(s =>
                        EF.Functions.ILike(s.Nome, padrao) ||
                        EF.Functions.ILike(s.Profissional!.Usuario!.Nome, padrao));

                vm.Servicos = await servicos
                    .OrderBy(s => s.Nome)
                    .ThenBy(s => s.Preco)
                    .ToListAsync();

                if (data is DateOnly dia)
                {
                    var livres = await _agenda.ObterHorariosLivres(vm.Servicos, dia, faixaTurno.De, faixaTurno.Ate);
                    vm.PrimeiroLivreServico = livres
                        .Where(l => l.Value.Any())
                        .ToDictionary(l => l.Key, l => l.Value.Min());
                    vm.Servicos = vm.Servicos.Where(s => vm.PrimeiroLivreServico.ContainsKey(s.ServicoId)).ToList();
                }
            }
            else
            {
                var profissionais = _context.Profissionais
                    .Include(p => p.Usuario)
                    .Include(p => p.Servicos.OrderBy(s => s.Nome))
                    .Where(p => p.Servicos.Any());

                if (tipo != null)
                    profissionais = profissionais.Where(p => EF.Functions.ILike(p.Especialidade!.Trim(), tipo));

                if (padrao != null)
                    profissionais = profissionais.Where(p =>
                        EF.Functions.ILike(p.Usuario!.Nome, padrao) ||
                        EF.Functions.ILike(p.Especialidade!, padrao) ||
                        p.Servicos.Any(s => EF.Functions.ILike(s.Nome, padrao)));

                vm.Profissionais = await profissionais
                    .OrderBy(p => p.Usuario!.Nome)
                    .ToListAsync();

                if (data is DateOnly dia)
                {
                    var relevantes = vm.Profissionais.ToDictionary(p => p.ProfissionalId, p => ServicosRelevantes(p, busca).ToList());
                    var livres = await _agenda.ObterHorariosLivres(relevantes.Values.SelectMany(s => s), dia, faixaTurno.De, faixaTurno.Ate);

                    foreach (var (profissionalId, servicosDaProfissional) in relevantes)
                    {
                        var horarios = servicosDaProfissional
                            .SelectMany(s => livres.GetValueOrDefault(s.ServicoId) ?? new List<DateTime>())
                            .ToList();
                        if (horarios.Any())
                            vm.PrimeiroLivreProfissional[profissionalId] = horarios.Min();
                    }

                    vm.Profissionais = vm.Profissionais.Where(p => vm.PrimeiroLivreProfissional.ContainsKey(p.ProfissionalId)).ToList();
                }
            }

            return View(vm);
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
