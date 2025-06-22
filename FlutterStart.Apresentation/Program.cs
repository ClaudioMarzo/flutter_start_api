using HealthChecks.UI.Client;
using Microsoft.EntityFrameworkCore;
using FlutterStart.Application.Services;
using Microsoft.Extensions.FileProviders;
using FlutterStart.Application.Interfaces;
using FlutterStart.Infrastructure.Context;
using FlutterStart.Infrastructure.Settings;
using FlutterStart.Infrastructure.Repository;
using FlutterStart.Infrastructure.Repository.Interfaces;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
string ENVIRONMENT = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
string connectionsStrings = ENVIRONMENT == "Development" ? "ConnectionStrings:DefaultConnection" : "ConnectionStrings:ProductionConnection";
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
configDataBase(builder);
configDependencyInjection(builder);
builder.Services.Configure<YtDlpSettings>(builder.Configuration.GetSection("YtDlpSettings"));

// Configuração da conexão com o banco de dados PostgreSQL
void configDataBase(WebApplicationBuilder serviceProvider)
{
    builder.Services.AddDbContext<FlutterStartDbContext>(options => options.UseNpgsql(
        builder.Configuration[connectionsStrings],
        options => options.SetPostgresVersion(new Version(15, 0, 0))
    ));
}

// Configuração da injeção de dependência
void configDependencyInjection(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IProcessRunner, ProcessRunner>();
    builder.Services.AddScoped<IUrlConversionService, UrlConversionService>();
    builder.Services.AddHostedService<DownloadCleanupService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection(); 
}

var downloadsPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
Directory.CreateDirectory(downloadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "downloads")),
    RequestPath = "/downloads"
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
