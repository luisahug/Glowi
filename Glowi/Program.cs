using Glowi.Data;
using Glowi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' não configurada. Em dev: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"...\"");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<Glowi.Services.IdentityErrosPtBr>();
builder.Services.AddControllersWithViews(options =>
{
    Glowi.Services.ValidacaoPtBr.Configurar(options.ModelBindingMessageProvider);
    options.ModelMetadataDetailsProviders.Add(new Glowi.Services.MensagensValidacaoPtBr());
});

builder.Services.AddScoped<Glowi.Services.AgendaService>();
builder.Services.Configure<Glowi.Services.SupabaseStorageOptions>(
    builder.Configuration.GetSection(Glowi.Services.SupabaseStorageOptions.Secao));
builder.Services.AddHttpClient<Glowi.Services.SupabaseStorageService>();

// geolocalização (ViaCEP + Nominatim/OpenStreetMap). O Nominatim exige User-Agent identificando o app.
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<Glowi.Services.GeolocalizacaoService>(http =>
{
    http.Timeout = TimeSpan.FromSeconds(8);
    http.DefaultRequestHeaders.UserAgent.ParseAdd("Glowi/1.0 (agendamento de servicos de beleza)");
});
builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opcoes.AddPolicy(Glowi.Controllers.LocalizacaoController.PoliticaLimite, contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

var ptBR = new CultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(ptBR),
    SupportedCultures = new[] { ptBR },
    SupportedUICultures = new[] { ptBR }
});

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Cliente", "Profissional" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
}

app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Status/500");
    app.UseHsts();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
