using System.ComponentModel.DataAnnotations;

namespace Glowi.Models
{
    public class Servico
    {
        public int ServicoId { get; set; }

        public Profissional? Profissional { get; set; }

        [Required(ErrorMessage = "Informe o nome do serviço.")]
        public string Nome { get; set; }

        [Display(Name = "Duração (min)")]
        [Range(5, 480, ErrorMessage = "A duração deve ser entre 5 e 480 minutos.")]
        public int DuracaoMinutos { get; set; }

        [Display(Name = "Preço")]
        [Range(0.01, 10000, ErrorMessage = "Informe um preço válido.")]
        public decimal Preco { get; set; }
    }
}
