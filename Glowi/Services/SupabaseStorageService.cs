using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Glowi.Services;
public class ImagemInvalidaException : Exception
{
    public ImagemInvalidaException(string mensagem) : base(mensagem) { }
}
public class SupabaseStorageService
{
    public const long TamanhoMaximoBytes = 5 * 1024 * 1024;

    private static readonly Dictionary<string, (string Extensao, Func<byte[], bool> Assinatura)> Formatos = new()
    {
        ["image/jpeg"] = (".jpg", b => b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF),
        ["image/png"] = (".png", b => b.Length >= 4 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47),
        ["image/webp"] = (".webp", b => b.Length >= 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
                                                       && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P')
    };

    private readonly HttpClient _http;
    private readonly SupabaseStorageOptions _opcoes;

    public SupabaseStorageService(HttpClient http, IOptions<SupabaseStorageOptions> opcoes)
    {
        _http = http;
        _opcoes = opcoes.Value;
    }
    public async Task<string> EnviarAsync(IFormFile arquivo, string pasta, CancellationToken ct = default)
    {
        if (arquivo == null || arquivo.Length == 0)
            throw new ImagemInvalidaException("Selecione uma imagem.");

        if (arquivo.Length > TamanhoMaximoBytes)
            throw new ImagemInvalidaException($"A imagem deve ter no máximo {TamanhoMaximoBytes / 1024 / 1024} MB.");

        var tipo = arquivo.ContentType?.ToLowerInvariant() ?? "";
        if (!Formatos.TryGetValue(tipo, out var formato))
            throw new ImagemInvalidaException("Formato não suportado. Envie JPG, PNG ou WEBP.");

        await using var stream = arquivo.OpenReadStream();
        var cabecalho = new byte[12];
        var lidos = await stream.ReadAsync(cabecalho, ct);
        if (!formato.Assinatura(cabecalho[..lidos]))
            throw new ImagemInvalidaException("O arquivo não é uma imagem válida.");
        stream.Position = 0;

        var caminho = $"{pasta.Trim('/')}/{Guid.NewGuid():N}{formato.Extensao}";

        using var conteudo = new StreamContent(stream);
        conteudo.Headers.ContentType = new MediaTypeHeaderValue(tipo);

        using var requisicao = NovaRequisicao(HttpMethod.Post, caminho);
        requisicao.Content = conteudo;

        using var resposta = await _http.SendAsync(requisicao, ct);
        await GarantirSucesso(resposta, "enviar a imagem", ct);

        return caminho;
    }

    public async Task ApagarAsync(string? caminho, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(caminho))
            return;

        using var requisicao = NovaRequisicao(HttpMethod.Delete, caminho);
        using var resposta = await _http.SendAsync(requisicao, ct);

        if (resposta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return;
        await GarantirSucesso(resposta, "apagar a imagem", ct);
    }
    public string? UrlPublica(string? caminho) =>
        string.IsNullOrWhiteSpace(caminho) ? null : _opcoes.UrlPublica(caminho);

    private HttpRequestMessage NovaRequisicao(HttpMethod metodo, string caminho)
    {
        if (string.IsNullOrWhiteSpace(_opcoes.Url) || string.IsNullOrWhiteSpace(_opcoes.Bucket) || string.IsNullOrWhiteSpace(_opcoes.ServiceKey))
            throw new InvalidOperationException(
                "Supabase não configurado: preencha Supabase:Url, Supabase:Bucket e Supabase:ServiceKey (ver README).");

        var url = $"{_opcoes.Url.TrimEnd('/')}/storage/v1/object/{_opcoes.Bucket}/{caminho.TrimStart('/')}";
        var requisicao = new HttpRequestMessage(metodo, url);

        requisicao.Headers.Add("apikey", _opcoes.ServiceKey);
        if (_opcoes.ServiceKey.StartsWith("eyJ"))
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opcoes.ServiceKey);

        return requisicao;
    }

    private static async Task GarantirSucesso(HttpResponseMessage resposta, string acao, CancellationToken ct)
    {
        if (resposta.IsSuccessStatusCode)
            return;

        var corpo = await resposta.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException(
            $"Falha ao {acao} no Supabase Storage ({(int)resposta.StatusCode}): {corpo}", null, resposta.StatusCode);
    }
}
