namespace Glowi.Models.ViewModels.Profissionais;

public class BuscaViewModel
{    public static readonly List<(string Chave, string Nome, TimeOnly? De, TimeOnly? Ate)> Turnos = new()
    {
        ("manha", "Manhã", null, new TimeOnly(12, 0)),
        ("tarde", "Tarde", new TimeOnly(12, 0), new TimeOnly(18, 0)),
        ("noite", "Noite", new TimeOnly(18, 0), null)
    };

    public string? Busca { get; set; }
    public string? Tipo { get; set; }
    public bool VerServicos { get; set; }
    public DateOnly? Data { get; set; }
    public string? Turno { get; set; }
    public DateOnly Hoje { get; set; }
    public DateOnly Maximo { get; set; }
    public Dictionary<int, DateTime> PrimeiroLivreServico { get; set; } = new();
    public Dictionary<int, DateTime> PrimeiroLivreProfissional { get; set; } = new();
    public Glowi.Services.LocalizacaoCliente? Local { get; set; }
    public int RaioKm { get; set; } = Glowi.Services.LocalizacaoCliente.RaioPadraoKm;
    public Dictionary<int, double> DistanciaKm { get; set; } = new();

    public string? DistanciaTexto(int profissionalId) =>
        Local == null ? null
        : DistanciaKm.TryGetValue(profissionalId, out var km) ? Glowi.Services.Distancia.Formatar(km)
        : "distância não informada";
    public HashSet<int> Favoritas { get; set; } = new();
    public List<string> Tipos { get; set; } = new();

    public List<Profissional> Profissionais { get; set; } = new();
    public List<Servico> Servicos { get; set; } = new();

    public string? NomeTurno => Turnos.FirstOrDefault(t => t.Chave == Turno).Nome;
    public List<(string Chave, string Rotulo)> FiltrosAtivos
    {
        get
        {
            var filtros = new List<(string, string)>();
            if (!string.IsNullOrEmpty(Tipo)) filtros.Add(("tipo", Tipo));
            if (Data is DateOnly data)
            {
                filtros.Add(("data", data == Hoje ? "Hoje" : data == Hoje.AddDays(1) ? "Amanhã" : data.ToString("ddd, dd/MM")));
                if (NomeTurno != null) filtros.Add(("turno", NomeTurno));
            }
            return filtros;
        }
    }

    public bool TemFiltro => !string.IsNullOrEmpty(Busca) || FiltrosAtivos.Any();
    public int Total => VerServicos ? Servicos.Count : Profissionais.Count;
}
