using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Glowi.Services;

// mensagens padrão de validação/model binding (ex.: "The value '' is invalid.") em português
public static class ValidacaoPtBr
{
    public static void Configurar(DefaultModelBindingMessageProvider m)
    {
        m.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para {campo}.");
        m.SetMissingBindRequiredValueAccessor(campo => $"Informe o campo {campo}.");
        m.SetMissingKeyOrValueAccessor(() => "Informe um valor.");
        m.SetMissingRequestBodyRequiredValueAccessor(() => "Os dados da requisição não foram enviados.");
        m.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
        m.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor informado não é válido.");
        m.SetNonPropertyValueMustBeANumberAccessor(() => "Informe um número.");
        m.SetUnknownValueIsInvalidAccessor(campo => $"O valor informado não é válido para {campo}.");
        m.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
        m.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
        m.SetValueMustNotBeNullAccessor(_ => "Informe um valor.");
    }
}
