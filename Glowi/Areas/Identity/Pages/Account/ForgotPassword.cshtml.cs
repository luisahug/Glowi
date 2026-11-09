using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Glowi.Areas.Identity.Pages.Account;

// substitui a página padrão (em inglês) do Identity. O app não envia emails, então não há link de
// redefinição: a página só orienta o(a) usuário(a) a pedir ajuda.
public class ForgotPasswordModel : PageModel
{
    public void OnGet()
    {
    }
}
