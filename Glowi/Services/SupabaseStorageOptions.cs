namespace Glowi.Services;
public class SupabaseStorageOptions
{
    public const string Secao = "Supabase";

    public string Url { get; set; } = string.Empty;

    public string ServiceKey { get; set; } = string.Empty;

    // nome exato do bucket no Supabase: a URL pública diferencia maiúsculas de minúsculas
    public string Bucket { get; set; } = string.Empty;

    public string UrlPublica(string caminho) =>
        $"{Url.TrimEnd('/')}/storage/v1/object/public/{Bucket}/{caminho.TrimStart('/')}";
}
