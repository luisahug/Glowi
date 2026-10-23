using Microsoft.AspNetCore.Identity;

namespace Glowi.Services;

// Mensagens de erro do Identity (cadastro, senha, email) em português
public class IdentityErrosPtBr : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => Erro(nameof(DefaultError), "Ocorreu um erro desconhecido.");
    public override IdentityError ConcurrencyFailure() => Erro(nameof(ConcurrencyFailure), "Os dados foram alterados por outra operação. Tente novamente.");
    public override IdentityError PasswordMismatch() => Erro(nameof(PasswordMismatch), "Senha incorreta.");
    public override IdentityError InvalidToken() => Erro(nameof(InvalidToken), "Token inválido.");
    public override IdentityError LoginAlreadyAssociated() => Erro(nameof(LoginAlreadyAssociated), "Já existe um usuário com esse login.");
    public override IdentityError InvalidUserName(string? userName) => Erro(nameof(InvalidUserName), $"O nome de usuário '{userName}' é inválido.");
    public override IdentityError InvalidEmail(string? email) => Erro(nameof(InvalidEmail), $"O email '{email}' é inválido.");
    public override IdentityError DuplicateUserName(string userName) => Erro(nameof(DuplicateUserName), $"Já existe uma conta com o email '{userName}'.");
    public override IdentityError DuplicateEmail(string email) => Erro(nameof(DuplicateEmail), $"Já existe uma conta com o email '{email}'.");
    public override IdentityError InvalidRoleName(string? role) => Erro(nameof(InvalidRoleName), $"O perfil '{role}' é inválido.");
    public override IdentityError DuplicateRoleName(string role) => Erro(nameof(DuplicateRoleName), $"O perfil '{role}' já existe.");
    public override IdentityError UserAlreadyHasPassword() => Erro(nameof(UserAlreadyHasPassword), "O usuário já tem uma senha.");
    public override IdentityError UserLockoutNotEnabled() => Erro(nameof(UserLockoutNotEnabled), "O bloqueio não está habilitado para este usuário.");
    public override IdentityError UserAlreadyInRole(string role) => Erro(nameof(UserAlreadyInRole), $"O usuário já pertence ao perfil '{role}'.");
    public override IdentityError UserNotInRole(string role) => Erro(nameof(UserNotInRole), $"O usuário não pertence ao perfil '{role}'.");
    public override IdentityError PasswordTooShort(int length) => Erro(nameof(PasswordTooShort), $"A senha deve ter pelo menos {length} caracteres.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Erro(nameof(PasswordRequiresUniqueChars), $"A senha deve ter pelo menos {uniqueChars} caracteres diferentes.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Erro(nameof(PasswordRequiresNonAlphanumeric), "A senha deve ter pelo menos um símbolo (ex.: ! @ # $).");
    public override IdentityError PasswordRequiresDigit() => Erro(nameof(PasswordRequiresDigit), "A senha deve ter pelo menos um número (0-9).");
    public override IdentityError PasswordRequiresLower() => Erro(nameof(PasswordRequiresLower), "A senha deve ter pelo menos uma letra minúscula (a-z).");
    public override IdentityError PasswordRequiresUpper() => Erro(nameof(PasswordRequiresUpper), "A senha deve ter pelo menos uma letra maiúscula (A-Z).");
    public override IdentityError RecoveryCodeRedemptionFailed() => Erro(nameof(RecoveryCodeRedemptionFailed), "Falha ao usar o código de recuperação.");

    private static IdentityError Erro(string code, string description) => new() { Code = code, Description = description };
}
