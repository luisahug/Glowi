namespace Glowi.Services;

public static class WhatsApp
{
    public static string? Link(string? telefone, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return null;

        var digitos = new string(telefone.Where(char.IsDigit).ToArray());
        if (digitos.Length < 10)
            return null;

        if (!digitos.StartsWith("55") || digitos.Length <= 11)
            digitos = "55" + digitos;

        return $"https://wa.me/{digitos}?text={Uri.EscapeDataString(mensagem)}";
    }
}
