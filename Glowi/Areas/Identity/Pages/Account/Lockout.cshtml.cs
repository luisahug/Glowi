using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Glowi.Areas.Identity.Pages.Account;

// substitui a página padrão (em inglês) do Identity, aberta quando a conta é bloqueada por tentativas de login
public class LockoutModel : PageModel
{
    public void OnGet()
    {
    }
}
