using Glowi.Data;
using Glowi.Models.Enum;
using Glowi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Glowi.ViewComponents;
public class NavegacaoViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NavegacaoViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var usuario = UserClaimsPrincipal;
        var itens = Menu.Navegacao(usuario);

        var rota = ViewContext.RouteData.Values;
        var ativo = Menu.ItemAtivo(itens, rota["area"]?.ToString(), rota["controller"]?.ToString(), rota["action"]?.ToString());

        var pendentes = 0;
        if (usuario.IsInRole("Profissional"))
        {
            var userId = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
            var agora = DateTime.UtcNow;
            pendentes = await _context.Agendamentos
                .CountAsync(a => a.Profissional!.UsuarioId == userId
                              && a.Status == StatusAgendamento.Pendente
                              && a.Fim > agora);
        }

        return View((Itens: itens, Ativo: ativo, Pendentes: pendentes));
    }
}
