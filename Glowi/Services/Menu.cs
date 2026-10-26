using System.Security.Claims;

namespace Glowi.Services;

// Rotulo: nome curto da barra inferior (celular). Ativo: rotas que deixam o item destacado,
// "Controller" ou "Controller/Action" (Controller/Action vence quando dois itens batem).
public record ItemMenu(
    string? Controller, string? Action, string Icone, string Cor,
    string Titulo, string Rotulo, string Texto, string[] Ativo)
{
    public string? Area { get; init; }
    public string? Page { get; init; }
    public bool Pendentes { get; init; }
    public bool SoNoDesktop { get; init; }
}

public static class Menu
{
    // atalhos de cada perfil: aparecem na navegação e nos cards da tela inicial
    public static List<ItemMenu> Atalhos(ClaimsPrincipal user)
    {
        var itens = new List<ItemMenu>();

        if (user.IsInRole("Profissional"))
        {
            itens.Add(new("Atendimentos", "Agenda", "bi-calendar-week", "", "Agenda", "Agenda", "Sua semana e seu dia de atendimentos", new[] { "Atendimentos/Agenda" }));
            itens.Add(new("Atendimentos", "Index", "bi-bell", "icon-box-pink", "Agendamentos", "Pedidos", "Confirme ou recuse pedidos", new[] { "Atendimentos" }) { Pendentes = true });
            itens.Add(new("Servicos", "Index", "bi-stars", "icon-box-amber", "Meus serviços", "Serviços", "Cadastre preços e duração", new[] { "Servicos" }));
            itens.Add(new("Disponibilidades", "Index", "bi-clock", "icon-box-teal", "Meus horários", "Horários", "Defina quando você atende", new[] { "Disponibilidades" }));
        }

        if (user.IsInRole("Cliente"))
        {
            itens.Add(new("Profissionais", "Index", "bi-search-heart", "", "Agendar", "Agendar", "Encontre profissionais e serviços", new[] { "Profissionais", "Agendamentos" }));
            itens.Add(new("Agendamentos", "MeusAgendamentos", "bi-calendar-check", "icon-box-pink", "Meus agendamentos", "Agendamentos", "Acompanhe seus horários marcados", new[] { "Agendamentos/MeusAgendamentos" }));
        }

        return itens;
    }

    // navegação completa: Início + atalhos + Conta.
    // Com 4 atalhos o Início sai da barra do celular para caber (a logo no topo já leva ao início).
    public static List<ItemMenu> Navegacao(ClaimsPrincipal user)
    {
        var atalhos = Atalhos(user);

        var itens = new List<ItemMenu>
        {
            new("Home", "Index", "bi-house", "", "Início", "Início", "", new[] { "Home" }) { SoNoDesktop = atalhos.Count >= 4 }
        };
        itens.AddRange(atalhos);
        itens.Add(new(null, null, "bi-person", "", "Minha conta", "Conta", "", new[] { "Identity" })
        {
            Area = "Identity",
            Page = "/Account/Manage/Index"
        });

        return itens;
    }

    public static ItemMenu? ItemAtivo(IEnumerable<ItemMenu> itens, string? area, string? controller, string? action)
    {
        var rota = area ?? controller;
        var lista = itens.ToList();

        return lista.FirstOrDefault(i => i.Ativo.Contains($"{rota}/{action}"))
            ?? lista.FirstOrDefault(i => i.Ativo.Contains(rota));
    }
}
