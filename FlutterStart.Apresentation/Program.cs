using FlutterStart.Application.Services;
using Microsoft.Extensions.FileProviders;
using FlutterStart.Application.Interfaces;
using FlutterStart.Infrastructure.Settings;
using FlutterStart.Infrastructure.Repository;
using FlutterStart.Infrastructure.Repository.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
configDependencyInjection(builder);
builder.Services.Configure<YtDlpSettings>(builder.Configuration.GetSection("YtDlpSettings"));

void configDependencyInjection(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IProcessRunner, ProcessRunner>();
    builder.Services.AddScoped<IUrlConversionService, UrlConversionService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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

app.Run();
