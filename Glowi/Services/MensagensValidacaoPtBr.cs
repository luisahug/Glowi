using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Glowi.Services;

public class MensagensValidacaoPtBr : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var atributo in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            if (!string.IsNullOrEmpty(atributo.ErrorMessage) || atributo.ErrorMessageResourceType != null)
                continue;

            var mensagem = atributo switch
            {
                RequiredAttribute => "Informe o campo {0}.",
                StringLengthAttribute { MinimumLength: > 0 } => "O campo {0} deve ter entre {2} e {1} caracteres.",
                StringLengthAttribute => "O campo {0} deve ter no máximo {1} caracteres.",
                MaxLengthAttribute => "O campo {0} deve ter no máximo {1} caracteres.",
                MinLengthAttribute => "O campo {0} deve ter pelo menos {1} caracteres.",
                RangeAttribute => "O campo {0} deve estar entre {1} e {2}.",
                EmailAddressAttribute => "Informe um email válido.",
                PhoneAttribute => "Informe um telefone válido.",
                UrlAttribute => "Informe um endereço web válido.",
                CompareAttribute => "{0} e {1} não conferem.",
                RegularExpressionAttribute => "O campo {0} não está no formato correto.",
                _ => null
            };

            if (mensagem != null)
                atributo.ErrorMessage = mensagem;
        }
    }
}
