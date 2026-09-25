namespace Glowi.Services;

public static class Horario
{
    private static readonly TimeZoneInfo Brasilia =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    // o que foi escolhido na tela > para salvar no banco
    public static DateTime ParaUtc(DateTime horaBrasilia) =>
        TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(horaBrasilia, DateTimeKind.Unspecified), Brasilia);

    // o que veio do banco > para mostrar na tela
    public static DateTime ParaBrasilia(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc), Brasilia);

    public static DateTime Agora => ParaBrasilia(DateTime.UtcNow);
}