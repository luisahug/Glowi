namespace Glowi.Models
{
    public class Profissional
    {
        public string UsuarioId { get; set; }
        public ApplicationUser? Usuario { get; set; } //nome e email aqui
        public int ProfissionalId { get; set; }
        public string? Telefone { get; set; }
        public string? Especialidade { get; set; }
        public string? Biografia { get; set; }
        public int IntervaloMinutos { get; set; } = 15;
        public int AntecedenciaMinimaHoras { get; set; } = 2;
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool TemLocalizacao => Latitude != null && Longitude != null;

        public string? BairroCidade =>
            string.Join(", ", new[] { Bairro, Cidade is null ? null : Uf is null ? Cidade : $"{Cidade}/{Uf}" }
                .Where(s => !string.IsNullOrWhiteSpace(s))) is { Length: > 0 } texto ? texto : null;

        public string? EnderecoCompleto
        {
            get
            {
                var rua = string.IsNullOrWhiteSpace(Logradouro) ? null
                    : string.IsNullOrWhiteSpace(Numero) ? Logradouro : $"{Logradouro}, {Numero}";
                if (rua != null && !string.IsNullOrWhiteSpace(Complemento))
                    rua += $" - {Complemento}";

                var cep = Cep is { Length: 8 } c ? $"CEP {c[..5]}-{c[5..]}" : null;

                var texto = string.Join(", ", new[] { rua, BairroCidade, cep }.Where(s => !string.IsNullOrWhiteSpace(s)));
                return texto.Length > 0 ? texto : null;
            }
        }

        public ICollection<Servico> Servicos { get; set; } = new List<Servico>();
    }
}
