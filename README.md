# Glowi
Sistema de agendamento de serviços de beleza

## Configuração local

### Pré-requisitos
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ (ou outro editor + terminal)

### 1. Pegue as senhas passadas no grupo

| Chave | O que é |
|---|---|
| `ConnectionStrings:DefaultConnection` | Connection string do banco Postgres (Supabase) |
| `Supabase:ServiceKey` | Chave `service_role` do Supabase, usada para enviar imagens ao bucket |

Os valores que não são secretos (`Supabase:Url`, `Supabase:Bucket`) já estão no `appsettings.json`.

### 2. Salve os segredos na sua máquina (user-secrets)
Os segredos ficam num arquivo fora do projeto, no seu usuário do Windows (`%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json`). O git nunca vê esse arquivo.

**Pelo Visual Studio:** clique com o botão direito no projeto **Glowi** → **Gerenciar Segredos do Usuário** e cole:

```json
{
  "ConnectionStrings:DefaultConnection": "Host=[HOST];Port=5432;Database=postgres;Username=[USERNAME];Password=[SENHA];SSL Mode=Require;Trust Server Certificate=true",
  "Supabase:ServiceKey": "[SENHA BUCKET]"
}
```

**Pelo terminal:** rode **dentro da pasta `Glowi\Glowi`** (a que tem o `Glowi.csproj`):

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=[HOST];Password=[SENHA]"
dotnet user-secrets set "Supabase:ServiceKey" "sb_secret_..."
```

Para conferir o que está salvo: `dotnet user-secrets list`.

### 3. Rode o projeto
Abra a solution `Glowi\Glowi.sln` no Visual Studio e rode, ou no terminal (pasta `Glowi\Glowi`):

```
dotnet run
```

### Problemas comuns
- **"Connection string 'DefaultConnection' não configurada"**: os user-secrets não foram configurados (passo 2).
- **`dotnet user-secrets` dá erro de projeto não encontrado**: o comando foi rodado fora da pasta `Glowi\Glowi`.
- **Nunca coloque segredos no `appsettings.json`**: ele é versionado. Os campos secretos ficam vazios lá de propósito.

## Produção
Os user-secrets só valem em desenvolvimento. No servidor, configure os mesmos valores como variáveis de ambiente (seções separadas por `__`):

```
ConnectionStrings__DefaultConnection=...
Supabase__ServiceKey=...
```
