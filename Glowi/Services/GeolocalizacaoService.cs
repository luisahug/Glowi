using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Glowi.Models;
using Glowi.Models.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Glowi.Services;

public record LocalEncontrado(double Latitude, double Longitude, string Rotulo);

public record EnderecoEncontrado(string? Cep, string? Logradouro, string? Numero, string? Bairro, string? Cidade, string? Uf, string Rotulo);

// ViaCEP (CEP -> endereço) e Nominatim/OpenStreetMap (endereço <-> coordenadas).
public class GeolocalizacaoService
{
    private const string ViaCep = "https://viacep.com.br/ws/";
    private const string Nominatim = "https://nominatim.openstreetmap.org/";
    private static readonly TimeSpan TempoCache = TimeSpan.FromDays(7);
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeolocalizacaoService> _logger;

    public GeolocalizacaoService(HttpClient http, IMemoryCache cache, ILogger<GeolocalizacaoService> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    public static string? SoDigitos(string? cep)
    {
        var digitos = new string((cep ?? "").Where(char.IsDigit).ToArray());
        return digitos.Length == 8 ? digitos : null;
    }
    public async Task<EnderecoEncontrado?> BuscarCepAsync(string? cep, CancellationToken ct = default)
    {
        var digitos = SoDigitos(cep);
        if (digitos == null)
            return null;

        return await ComCache($"cep:{digitos}", async () =>
        {
            var r = await ObterJson<ViaCepResposta>($"{ViaCep}{digitos}/json/", ct);
            if (r == null || r.Erro)
                return null;
            return new EnderecoEncontrado(digitos, Vazio(r.Logradouro), null, Vazio(r.Bairro), Vazio(r.Localidade), Vazio(r.Uf),
                Rotulo(r.Bairro, r.Localidade, r.Uf));
        });
    }
    public async Task<(double Latitude, double Longitude)?> GeocodificarAsync(Profissional p, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(p.Cidade))
            return null;

        var tentativas = new List<Dictionary<string, string?>>();
        if (!string.IsNullOrWhiteSpace(p.Logradouro))
        {
            if (!string.IsNullOrWhiteSpace(p.Numero))
                tentativas.Add(new() { ["street"] = $"{p.Numero} {p.Logradouro}", ["city"] = p.Cidade, ["state"] = p.Uf });
            tentativas.Add(new() { ["street"] = p.Logradouro, ["city"] = p.Cidade, ["state"] = p.Uf });
        }
        if (!string.IsNullOrWhiteSpace(p.Bairro))
            tentativas.Add(new() { ["q"] = string.Join(", ", new[] { p.Bairro, p.Cidade, p.Uf }.Where(s => !string.IsNullOrWhiteSpace(s))) });
        tentativas.Add(new() { ["city"] = p.Cidade, ["state"] = p.Uf });

        foreach (var parametros in tentativas)
        {
            var achado = await PesquisarAsync(parametros, ct);
            if (achado != null)
                return (achado.Latitude, achado.Longitude);
        }
        return null;
    }
    public async Task<LocalEncontrado?> BuscarLocalAsync(string? texto, CancellationToken ct = default)
    {
        texto = texto?.Trim();
        if (string.IsNullOrEmpty(texto) || texto.Length > 120)
            return null;

        if (SoDigitos(texto) != null && texto.Count(char.IsLetter) == 0)
        {
            var endereco = await BuscarCepAsync(texto, ct);
            if (endereco == null)
                return null;

            var p = new Profissional
            {
                UsuarioId = "",
                Logradouro = endereco.Logradouro,
                Bairro = endereco.Bairro,
                Cidade = endereco.Cidade,
                Uf = endereco.Uf
            };
            return await GeocodificarAsync(p, ct) is { } coords
                ? new LocalEncontrado(coords.Latitude, coords.Longitude, endereco.Rotulo)
                : null;
        }

        return await PesquisarAsync(new() { ["q"] = texto }, ct);
    }
    public async Task<EnderecoEncontrado?> ReversoAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        var chave = $"rev:{Math.Round(latitude, 4).ToString(Inv)},{Math.Round(longitude, 4).ToString(Inv)}";
        return await ComCache(chave, async () =>
        {
            var url = $"{Nominatim}reverse?format=jsonv2&addressdetails=1&zoom=18&accept-language=pt-BR" +
                      $"&lat={latitude.ToString(Inv)}&lon={longitude.ToString(Inv)}";
            var r = await ObterJson<NominatimLugar>(url, ct);
            if (r?.Address == null)
                return null;

            var a = r.Address;
            var bairro = a.Suburb ?? a.Neighbourhood ?? a.Quarter ?? a.CityDistrict;
            var cidade = a.City ?? a.Town ?? a.Village ?? a.Municipality;
            var uf = a.Uf;
            var numero = a.HouseNumber is { } n && System.Text.RegularExpressions.Regex.Match(n, @"\d{1,6}") is { Success: true } m ? m.Value : null;
            return new EnderecoEncontrado(SoDigitos(a.Postcode), a.Road, numero, bairro, cidade, uf, Rotulo(bairro, cidade, uf));
        });
    }
    public async Task<bool> AplicarEnderecoAsync(Profissional p, EnderecoInput e, CancellationToken ct = default)
    {
        string? Limpo(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        var antes = (p.Cep, p.Logradouro, p.Numero, p.Bairro, p.Cidade, p.Uf);

        p.Cep = SoDigitos(e.Cep);
        p.Logradouro = Limpo(e.Logradouro);
        p.Numero = Limpo(e.Numero);
        p.Complemento = Limpo(e.Complemento);
        p.Bairro = Limpo(e.Bairro);
        p.Cidade = Limpo(e.Cidade);
        p.Uf = Limpo(e.Uf)?.ToUpperInvariant();

        if (p.Cep != null && p.Cidade == null && await BuscarCepAsync(p.Cep, ct) is { } doCep)
        {
            p.Logradouro ??= doCep.Logradouro;
            p.Bairro ??= doCep.Bairro;
            p.Cidade = doCep.Cidade;
            p.Uf ??= doCep.Uf;
        }

        if (e.CoordenadasDoGps() is { } gps)
        {
            (p.Latitude, p.Longitude) = (gps.Latitude, gps.Longitude);
            return true;
        }

        if (p.Cidade == null)
        {
            (p.Latitude, p.Longitude) = (null, null);
            return e.Vazio;
        }

        var mudou = antes != (p.Cep, p.Logradouro, p.Numero, p.Bairro, p.Cidade, p.Uf);
        if (!mudou && p.TemLocalizacao)
            return true;

        if (await GeocodificarAsync(p, ct) is { } achado)
        {
            (p.Latitude, p.Longitude) = (achado.Latitude, achado.Longitude);
            return true;
        }

        (p.Latitude, p.Longitude) = (null, null);
        return false;
    }

    private async Task<LocalEncontrado?> PesquisarAsync(Dictionary<string, string?> parametros, CancellationToken ct)
    {
        var query = string.Join("&", parametros
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!.Trim())}"));
        var url = $"{Nominatim}search?format=jsonv2&addressdetails=1&limit=1&countrycodes=br&accept-language=pt-BR&{query}";

        return await ComCache($"busca:{query.ToLowerInvariant()}", async () =>
        {
            var lugares = await ObterJson<List<NominatimLugar>>(url, ct);
            var l = lugares?.FirstOrDefault();
            if (l == null ||
                !double.TryParse(l.Lat, NumberStyles.Float, Inv, out var lat) ||
                !double.TryParse(l.Lon, NumberStyles.Float, Inv, out var lng))
                return null;

            var a = l.Address;
            var rotulo = a == null
                ? l.Name ?? "Local escolhido"
                : Rotulo(a.Suburb ?? a.Neighbourhood ?? a.Quarter, a.City ?? a.Town ?? a.Village ?? a.Municipality, a.Uf);
            return new LocalEncontrado(lat, lng, rotulo);
        });
    }

    private async Task<T?> ComCache<T>(string chave, Func<Task<T?>> buscar) where T : class
    {
        if (_cache.TryGetValue(chave, out T? guardado))
            return guardado;

        var valor = await buscar();
        if (valor != null)
            _cache.Set(chave, valor, TempoCache);
        return valor;
    }

    private async Task<T?> ObterJson<T>(string url, CancellationToken ct) where T : class
    {
        try
        {
            using var resposta = await _http.GetAsync(url, ct);
            if (!resposta.IsSuccessStatusCode)
            {
                _logger.LogWarning("Geolocalização respondeu {Status} para {Url}", (int)resposta.StatusCode, url);
                return null;
            }
            await using var corpo = await resposta.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<T>(corpo, cancellationToken: ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Falha ao consultar geolocalização em {Url}", url);
            return null;
        }
    }

    private static string? Vazio(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string Rotulo(string? bairro, string? cidade, string? uf)
    {
        var local = cidade == null ? null : uf == null ? cidade : $"{cidade}/{uf}";
        var partes = new[] { bairro, local }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        return partes.Any() ? string.Join(", ", partes) : "Local escolhido";
    }

    private class ViaCepResposta
    {
        [JsonPropertyName("logradouro")] public string? Logradouro { get; set; }
        [JsonPropertyName("bairro")] public string? Bairro { get; set; }
        [JsonPropertyName("localidade")] public string? Localidade { get; set; }
        [JsonPropertyName("uf")] public string? Uf { get; set; }
        [JsonPropertyName("erro")] public JsonElement ErroBruto { get; set; }
        public bool Erro => ErroBruto.ValueKind == JsonValueKind.True ||
                            (ErroBruto.ValueKind == JsonValueKind.String && ErroBruto.GetString() == "true");
    }

    private class NominatimLugar
    {
        [JsonPropertyName("lat")] public string? Lat { get; set; }
        [JsonPropertyName("lon")] public string? Lon { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("address")] public NominatimEndereco? Address { get; set; }
    }

    private class NominatimEndereco
    {
        [JsonPropertyName("road")] public string? Road { get; set; }
        [JsonPropertyName("house_number")] public string? HouseNumber { get; set; }
        [JsonPropertyName("suburb")] public string? Suburb { get; set; }
        [JsonPropertyName("neighbourhood")] public string? Neighbourhood { get; set; }
        [JsonPropertyName("quarter")] public string? Quarter { get; set; }
        [JsonPropertyName("city_district")] public string? CityDistrict { get; set; }
        [JsonPropertyName("city")] public string? City { get; set; }
        [JsonPropertyName("town")] public string? Town { get; set; }
        [JsonPropertyName("village")] public string? Village { get; set; }
        [JsonPropertyName("municipality")] public string? Municipality { get; set; }
        [JsonPropertyName("postcode")] public string? Postcode { get; set; }
        [JsonPropertyName("ISO3166-2-lvl4")] public string? Iso { get; set; }
        public string? Uf => Iso is { Length: 5 } iso && iso.StartsWith("BR-") ? iso[3..] : null;
    }
}
