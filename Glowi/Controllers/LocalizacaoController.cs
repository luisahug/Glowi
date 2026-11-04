using System.Globalization;
using Glowi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Glowi.Controllers
{
    [AllowAnonymous]
    [EnableRateLimiting(PoliticaLimite)]
    [Route("Localizacao")]
    public class LocalizacaoController : Controller
    {
        public const string PoliticaLimite = "geolocalizacao";

        private readonly GeolocalizacaoService _geo;

        public LocalizacaoController(GeolocalizacaoService geo) => _geo = geo;

        [HttpGet("Cep")]
        public async Task<IActionResult> Cep(string? cep, CancellationToken ct)
        {
            var endereco = await _geo.BuscarCepAsync(cep, ct);
            return endereco == null ? NotFound(new { mensagem = "CEP não encontrado." }) : Json(endereco);
        }

        [HttpGet("Buscar")]
        public async Task<IActionResult> Buscar(string? q, CancellationToken ct)
        {
            var local = await _geo.BuscarLocalAsync(q, ct);
            return local == null ? NotFound(new { mensagem = "Não encontramos esse local. Tente o CEP ou \"bairro, cidade\"." }) : Json(local);
        }

        [HttpGet("Reverso")]
        public async Task<IActionResult> Reverso(string? lat, string? lng, CancellationToken ct)
        {
            var inv = CultureInfo.InvariantCulture;
            if (!double.TryParse(lat, NumberStyles.Float, inv, out var latitude) ||
                !double.TryParse(lng, NumberStyles.Float, inv, out var longitude) ||
                latitude is < -90 or > 90 || longitude is < -180 or > 180)
                return BadRequest(new { mensagem = "Coordenadas inválidas." });

            var endereco = await _geo.ReversoAsync(latitude, longitude, ct);
            return endereco == null ? NotFound(new { mensagem = "Não foi possível identificar o endereço." }) : Json(endereco);
        }
    }
}
