using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SoloCerto.Api.Data;
using SoloCerto.Api.Infra;
using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Services;
using SoloCerto.Api.Services.Regras;

var builder = WebApplication.CreateBuilder(args);

// ---------- Banco de dados (EF Core + SQLite) ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SoloCerto") ?? "Data Source=solocerto.db"));

// ---------- Injeção de dependência ----------
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.Configure<ParametrosIrrigacao>(builder.Configuration.GetSection("Irrigacao"));

// Regras de irrigação (Strategy): para criar uma regra nova basta registrá-la aqui
builder.Services.AddSingleton<IRegraIrrigacao, RegraMedicaoIndisponivel>();
builder.Services.AddSingleton<IRegraIrrigacao, RegraUmidadeCritica>();
builder.Services.AddSingleton<IRegraIrrigacao, RegraChuvaPrevista>();
builder.Services.AddSingleton<IRegraIrrigacao, RegraUmidadeBaixa>();
builder.Services.AddSingleton<IRegraIrrigacao, RegraUmidadeAdequada>();
builder.Services.AddSingleton<IMotorRecomendacao, MotorRecomendacao>();

builder.Services.AddScoped<IAgricultorService, AgricultorService>();
builder.Services.AddScoped<IPlantacaoService, PlantacaoService>();
builder.Services.AddScoped<IMedicaoService, MedicaoService>();
builder.Services.AddScoped<IPrevisaoChuvaService, PrevisaoChuvaService>();
builder.Services.AddScoped<IRecomendacaoService, RecomendacaoService>();
builder.Services.AddScoped<IIndicadorService, IndicadorService>();

// ---------- Web API ----------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = contexto =>
        new BadRequestObjectResult(new ValidationProblemDetails(contexto.ModelState)
        {
            Title = "Dados inválidos",
            Status = StatusCodes.Status400BadRequest,
        }));
// O gerador do OpenAPI usa estas opções: enums aparecem como texto na documentação, igual à API
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorDeErros>();

// ---------- Documentação automática (OpenAPI + Scalar) ----------
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info.Title = "SoloCerto API";
        doc.Info.Version = "v1";
        doc.Info.Description =
            "Apoio à decisão de irrigação para pequenos produtores rurais: cadastro de agricultores e " +
            "plantações, registro de umidade do solo, previsões de chuva vindas do módulo de IA e " +
            "recomendações de irrigação. GS FIAP 2026.2 · CitizenTech.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();                         // /openapi/v1.json
app.MapScalarApiReference(o => o          // /scalar/v1 (documentação interativa)
    .WithTitle("SoloCerto API")
    .WithTheme(ScalarTheme.Kepler));
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.MapControllers();

// ---------- Migrações e dados de demonstração ----------
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
    if (app.Configuration.GetValue("SoloCerto:CarregarDadosDemonstracao", false))
        await DadosDemonstracao.CarregarAsync(db);
}

await app.RunAsync();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
