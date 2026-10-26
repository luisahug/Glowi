namespace Glowi.Models.ViewModels.Profissionais;

public class BuscaViewModel
{
    // turnos do filtro de agenda: o horário de início do atendimento precisa estar em [De, Ate)
    public static readonly List<(string Chave, string Nome, TimeOnly? De, TimeOnly? Ate)> Turnos = new()
    {
        ("manha", "Manhã", null, new TimeOnly(12, 0)),
        ("tarde", "Tarde", new TimeOnly(12, 0), new TimeOnly(18, 0)),
        ("noite", "Noite", new TimeOnly(18, 0), null)
    };

    public string? Busca { get; set; }
    public string? Tipo { get; set; }
    public bool VerServicos { get; set; }

    // filtro de agenda: só aparece quem tem horário livre nesse dia (e turno, se escolhido)
    public DateOnly? Data { get; set; }
    public string? Turno { get; set; }
    public DateOnly Hoje { get; set; }
    public DateOnly Maximo { get; set; }

    // primeiro horário livre no período filtrado, por ServicoId e por ProfissionalId
    public Dictionary<int, DateTime> PrimeiroLivreServico { get; set; } = new();
    public Dictionary<int, DateTime> PrimeiroLivreProfissional { get; set; } = new();

    // tipos de profissional (Especialidade) disponíveis para filtrar
    public List<string> Tipos { get; set; } = new();

    public List<Profissional> Profissionais { get; set; } = new();
    public List<Servico> Servicos { get; set; } = new();

    public string? NomeTurno => Turnos.FirstOrDefault(t => t.Chave == Turno).Nome;

    // filtros do painel que estão aplicados: (parâmetro na URL, texto exibido)
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
