using System.ComponentModel.DataAnnotations;

namespace Glowi.Models
{
    public class ServicoImagem
    {
        public const int MaximoPorServico = 10;

        public int ServicoImagemId { get; set; }

        public int ServicoId { get; set; }
        public Servico? Servico { get; set; }

        [Required, MaxLength(300)]
        public string CaminhoBucket { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        [MaxLength(120, ErrorMessage = "A descrição deve ter no máximo 120 caracteres.")]
        public string? Descricao { get; set; }

        public int Ordem { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
