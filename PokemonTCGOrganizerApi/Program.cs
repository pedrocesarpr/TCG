using PokemonTCGOrganizer.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logging para debug no Render
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configurar a porta do Render ANTES do Build
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco de dados SQLite
builder.Services.AddDbContext<PokemonDbContext>(options =>
    options.UseSqlite("Data Source=pokemoncards.db"));

builder.Services.AddScoped<CardScraper>();

var app = builder.Build();

// Garante que o banco existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PokemonDbContext>();
    db.Database.EnsureCreated(); // cria banco e tabelas se não existirem
}

//// Swagger apenas no ambiente Dev
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors("AllowAll");

// Exibir erros detalhados (no Render aparece nos logs)
app.UseDeveloperExceptionPage();

app.UseAuthorization();

app.MapControllers();

// Healthcheck simples
app.MapGet("/ping", () => Results.Ok(new { status = "pong" }));

app.Run();
