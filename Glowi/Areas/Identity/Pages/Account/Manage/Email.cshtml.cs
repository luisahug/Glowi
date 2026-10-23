using System.ComponentModel.DataAnnotations;
using Glowi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Glowi.Areas.Identity.Pages.Account.Manage;

// O app não envia emails, então a troca é feita direto (sem link de confirmação).
// O email também é o nome de usuário usado no login, por isso os dois mudam juntos.
public class EmailModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public EmailModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string Email { get; set; } = string.Empty;

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Informe o novo email.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        [Display(Name = "Novo email")]
        public string NovoEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe sua senha.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha atual")]
        public string Senha { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Usuário não encontrado.");

        Email = user.Email ?? string.Empty;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Usuário não encontrado.");

        Email = user.Email ?? string.Empty;
        if (!ModelState.IsValid)
            return Page();

        var novoEmail = Input.NovoEmail.Trim();
        if (string.Equals(novoEmail, Email, StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = "Esse já é o seu email atual.";
            return RedirectToPage();
        }

        if (!await _userManager.CheckPasswordAsync(user, Input.Senha))
        {
            ModelState.AddModelError("Input.Senha", "Senha incorreta.");
            return Page();
        }

        if (await _userManager.FindByEmailAsync(novoEmail) != null)
        {
            ModelState.AddModelError("Input.NovoEmail", "Já existe uma conta com esse email.");
            return Page();
        }

        var resultado = await _userManager.SetEmailAsync(user, novoEmail);
        if (resultado.Succeeded)
            resultado = await _userManager.SetUserNameAsync(user, novoEmail);

        if (!resultado.Succeeded)
        {
            StatusMessage = "Erro: não foi possível alterar o email.";
            return RedirectToPage();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Email alterado. Use o novo email para entrar.";
        return RedirectToPage();
    }
}
