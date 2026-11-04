using System.Globalization;

namespace Glowi.Services;
public record LocalizacaoCliente(double Latitude, double Longitude, string Rotulo, int RaioKm)
{
    public const string CookieLatitude = "glowi_lat";
    public const string CookieLongitude = "glowi_lng";
    public const string CookieRotulo = "glowi_local";
    public const string CookieRaio = "glowi_raio";

    public const int RaioPadraoKm = 15;
    public static readonly int[] OpcoesRaioKm = { 5, 10, 15, 25, 50, 100 };

    public static int LerRaio(HttpRequest request) =>
        int.TryParse(request.Cookies[CookieRaio], NumberStyles.Integer, CultureInfo.InvariantCulture, out var raio) &&
        OpcoesRaioKm.Contains(raio)
            ? raio
            : RaioPadraoKm;

    public static LocalizacaoCliente? Ler(HttpRequest request)
    {
        var inv = CultureInfo.InvariantCulture;
        if (!double.TryParse(request.Cookies[CookieLatitude], NumberStyles.Float, inv, out var lat) ||
            !double.TryParse(request.Cookies[CookieLongitude], NumberStyles.Float, inv, out var lng) ||
            lat is < -90 or > 90 || lng is < -180 or > 180)
            return null;

        var rotulo = request.Cookies[CookieRotulo];
        rotulo = string.IsNullOrWhiteSpace(rotulo) ? "Sua localização" : rotulo.Trim();
        if (rotulo.Length > 80) rotulo = rotulo[..80];

        return new LocalizacaoCliente(lat, lng, rotulo, LerRaio(request));
    }
    public (double LatMin, double LatMax, double LngMin, double LngMax) Caixa()
    {
        var dLat = RaioKm / 111.32;
        var dLng = RaioKm / (111.32 * Math.Max(Math.Cos(Latitude * Math.PI / 180), 0.01));
        return (Latitude - dLat, Latitude + dLat, Longitude - dLng, Longitude + dLng);
    }

    public double DistanciaKm(double latitude, double longitude) => Distancia.Km(Latitude, Longitude, latitude, longitude);
}

public static class Distancia
{
    private const double RaioTerraKm = 6371.0;

    // fórmula de Haversine: distância em linha reta sobre a superfície da Terra
    public static double Km(double lat1, double lng1, double lat2, double lng2)
    {
        double Rad(double graus) => graus * Math.PI / 180;
        var dLat = Rad(lat2 - lat1);
        var dLng = Rad(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return 2 * RaioTerraKm * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    public static string Formatar(double km) =>
        km < 1
            ? $"{Math.Max(100, Math.Round(km * 10) * 100):0} m"
            : km < 10
                ? km.ToString("0.0", new CultureInfo("pt-BR")) + " km"
                : $"{Math.Round(km):0} km";
}
