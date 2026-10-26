namespace Glowi.Models.ViewModels;

// Modal de confirmação de ação destrutiva (Views/Shared/_ModalConfirmacao).
// O botão que abre o modal informa, via data-*: data-acao (URL do POST), data-resumo, data-quando e data-pessoa.
public class ModalConfirmacaoViewModel
{
    public string Id { get; set; } = "modalConfirmacao";
    public string Titulo { get; set; } = "Tem certeza?";
    public string Texto { get; set; } = "Essa ação não pode ser desfeita.";
    public string Icone { get; set; } = "bi-exclamation-triangle";
    public string Manter { get; set; } = "Voltar";
    public string Confirmar { get; set; } = "Confirmar";
}
