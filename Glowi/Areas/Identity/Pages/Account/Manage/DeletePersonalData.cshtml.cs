using System.ComponentModel.DataAnnotations;
using Glowi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Areas.Identity.Pages.Account.Manage;

public class DeletePersonalDataModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<DeletePersonalDataModel> _logger;

    public DeletePersonalDataModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<DeletePersonalDataModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Informe sua senha.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Usuário não encontrado.");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Usuário não encontrado.");

        if (!ModelState.IsValid)
            return Page();

        if (!await _userManager.CheckPasswordAsync(user, Input.Senha))
        {
            ModelState.AddModelError("Input.Senha", "Senha incorreta.");
            return Page();
        }

        IdentityResult resultado;
        try
        {
            resultado = await _userManager.DeleteAsync(user);
        }
        catch (DbUpdateException ex)
        {
            // Agendamentos usam DeleteBehavior.Restrict: conta com agendamentos não pode ser apagada
            _logger.LogWarning(ex, "Não foi possível excluir o usuário {UserId}.", user.Id);
            StatusMessage = "Erro: sua conta tem agendamentos registrados e não pode ser excluída. Fale com o suporte.";
            return RedirectToPage("./PersonalData");
        }

        if (!resultado.Succeeded)
        {
            StatusMessage = "Erro: não foi possível excluir sua conta.";
            return RedirectToPage("./PersonalData");
        }

        await _signInManager.SignOutAsync();
        _logger.LogInformation("Usuário {UserId} excluiu a própria conta.", user.Id);
        return Redirect("~/");
    }
}
