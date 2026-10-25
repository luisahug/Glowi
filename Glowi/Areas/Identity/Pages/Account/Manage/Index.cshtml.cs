using System.ComponentModel.DataAnnotations;
using Glowi.Data;
using Glowi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Glowi.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public string Email { get; set; } = string.Empty;
    public bool EhProfissional { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Informe seu nome.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefone inválido.")]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [Display(Name = "Especialidade")]
        public string? Especialidade { get; set; }

        [Display(Name = "Sobre você")]
        public string? Biografia { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Usuário não encontrado.");

        Email = user.Email ?? string.Empty;
        EhProfissional = User.IsInRole("Profissional");
        Input.Nome = user.Nome;

        if (EhProfissional)
        {
            var p = await _context.Profissionais.FirstOrDefaultAsync(p => p.UsuarioId == user.Id);
            Input.Telefone = p?.Telefone;
            Input.Especialidade = p?.Especialidade;
            Input.Biografia = p?.Biografia;
        }
        else
        {
            var c = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
            Input.Telefone = c?.Telefone;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Usuário não encontrado.");

        Email = user.Email ?? string.Empty;
        EhProfissional = User.IsInRole("Profissional");

        if (EhProfissional && string.IsNullOrWhiteSpace(Input.Especialidade))
            ModelState.AddModelError("Input.Especialidade", "Informe sua especialidade.");

        if (!ModelState.IsValid)
            return Page();

        user.Nome = Input.Nome.Trim();
        var resultado = await _userManager.UpdateAsync(user);
        if (!resultado.Succeeded)
        {
            StatusMessage = "Erro: não foi possível salvar seu perfil.";
            return RedirectToPage();
        }

        var telefone = string.IsNullOrWhiteSpace(Input.Telefone) ? null : Input.Telefone.Trim();

        if (EhProfissional)
        {
            var p = await _context.Profissionais.FirstOrDefaultAsync(p => p.UsuarioId == user.Id);
            if (p == null)
            {
                p = new Profissional { UsuarioId = user.Id };
                _context.Profissionais.Add(p);
            }
            p.Telefone = telefone;
            p.Especialidade = Input.Especialidade?.Trim();
            p.Biografia = string.IsNullOrWhiteSpace(Input.Biografia) ? null : Input.Biografia.Trim();
        }
        else
        {
            var c = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
            if (c == null)
            {
                c = new Cliente { UsuarioId = user.Id };
                _context.Clientes.Add(c);
            }
            c.Telefone = telefone;
        }

        await _context.SaveChangesAsync();
        await _signInManager.RefreshSignInAsync(user);

        StatusMessage = "Perfil atualizado.";
        return RedirectToPage();
    }
}
