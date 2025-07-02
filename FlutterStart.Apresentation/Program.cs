using HealthChecks.UI.Client;
using Microsoft.EntityFrameworkCore;
using FlutterStart.Application.Mapping;
using FlutterStart.Application.Services;
using Microsoft.Extensions.FileProviders;
using FlutterStart.Application.Interfaces;
using FlutterStart.Infrastructure.Context;
using FlutterStart.Infrastructure.Services;
using FlutterStart.Infrastructure.Settings;
using FlutterStart.Infrastructure.Repository;
using FlutterStart.Application.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using FlutterStart.Infrastructure.Services.Interfaces;
using FlutterStart.Infrastructure.Repository.Interfaces;

// Garante que a pasta wwwroot e wwwroot/images existam antes de criar o builder
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}
var imagensPath = Path.Combine(wwwrootPath, "images");
if (!Directory.Exists(imagensPath))
{
    Directory.CreateDirectory(imagensPath);
}

var builder = WebApplication.CreateBuilder(args);
string environment  = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
string connectionsStrings = environment  == "Development" ? "ConnectionStrings:DefaultConnection" : "ConnectionStrings:ProductionConnection";
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

// Configura as URLs para desenvolvimento e produção
if (environment != "Development")
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Adiciona Swagger apenas em ambiente de desenvolvimento
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    { 
        Title = "FlutterStart API", 
        Version = "v1",
        Description = "API para o aplicativo FlutterStart"
    });
});

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
    builder.Services.AddHostedService<DownloadCleanupService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IBookService, BookService>();
    builder.Services.AddScoped<IProcessRunner, ProcessRunner>();
    builder.Services.AddScoped<IBookRepository, BookRepository>();
    builder.Services.AddScoped<IAuthRepository, AuthRepository>();
    builder.Services.AddScoped<IFileStorageService, FileStorageService>();
    builder.Services.AddScoped<IUrlConversionService, UrlConversionService>();
    builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
    builder.Services.AddScoped<IMovieService, MovieService>();
    builder.Services.AddScoped<IMovieRepository, MovieRepository>();
}

var app = builder.Build();

// Executa as migrations automaticamente ao iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlutterStartDbContext>();
    db.Database.Migrate();
}


// Configuração do Swagger em todos os ambientes
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlutterStart API V1");
    c.RoutePrefix = string.Empty;
});


var downloadsPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
Directory.CreateDirectory(downloadsPath);

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "downloads")),
    RequestPath = "/downloads"
});

// app.UseHttpsRedirection();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
