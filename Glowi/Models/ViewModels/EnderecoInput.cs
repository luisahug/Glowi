using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Glowi.Models.ViewModels;

public class EnderecoInput
{
    [Display(Name = "CEP")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Informe um CEP válido (ex.: 80010-000).")]
    public string? Cep { get; set; }

    [Display(Name = "Rua")]
    [StringLength(150, ErrorMessage = "A rua deve ter no máximo {1} caracteres.")]
    public string? Logradouro { get; set; }

    [Display(Name = "Número")]
    [StringLength(10, ErrorMessage = "O número deve ter no máximo {1} caracteres.")]
    public string? Numero { get; set; }

    [Display(Name = "Complemento")]
    [StringLength(60, ErrorMessage = "O complemento deve ter no máximo {1} caracteres.")]
    public string? Complemento { get; set; }

    [Display(Name = "Bairro")]
    [StringLength(80, ErrorMessage = "O bairro deve ter no máximo {1} caracteres.")]
    public string? Bairro { get; set; }

    [Display(Name = "Cidade")]
    [StringLength(80, ErrorMessage = "A cidade deve ter no máximo {1} caracteres.")]
    public string? Cidade { get; set; }

    [Display(Name = "UF")]
    [RegularExpression(@"^[A-Za-z]{2}$", ErrorMessage = "Informe a sigla do estado (ex.: PR).")]
    public string? Uf { get; set; }

    public string? Latitude { get; set; }
    public string? Longitude { get; set; }

    public bool SemCoordenadas { get; set; }

    public bool Vazio =>
        new[] { Cep, Logradouro, Numero, Bairro, Cidade, Uf }.All(string.IsNullOrWhiteSpace);

    public (double Latitude, double Longitude)? CoordenadasDoGps()
    {
        if (double.TryParse(Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
            double.TryParse(Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var lng) &&
            lat is >= -90 and <= 90 && lng is >= -180 and <= 180)
            return (lat, lng);
        return null;
    }

    public static EnderecoInput De(Profissional p) => new()
    {
        Cep = p.Cep is { Length: 8 } cep ? $"{cep[..5]}-{cep[5..]}" : p.Cep,
        Logradouro = p.Logradouro,
        Numero = p.Numero,
        Complemento = p.Complemento,
        Bairro = p.Bairro,
        Cidade = p.Cidade,
        Uf = p.Uf,
        SemCoordenadas = !p.TemLocalizacao && !string.IsNullOrWhiteSpace(p.Cidade)
    };
}
