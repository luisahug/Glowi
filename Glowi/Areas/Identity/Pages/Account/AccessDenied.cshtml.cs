using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Glowi.Areas.Identity.Pages.Account;

// substitui a página padrão (em inglês) do Identity, aberta quando o usuário não tem o perfil exigido
public class AccessDeniedModel : PageModel
{
    public void OnGet()
    {
    }
}
