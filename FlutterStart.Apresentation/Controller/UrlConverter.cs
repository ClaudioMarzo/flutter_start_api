using System.Net;
using Microsoft.AspNetCore.Mvc;
using FlutterStart.Application.DTO;
using Microsoft.AspNetCore.StaticFiles;
using FlutterStart.Application.Interfaces;

namespace FlutterStart.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class UrlConverterController : ControllerBase
{
    private readonly IUrlConversionService _conversionService;
    private readonly ILogger<UrlConverterController> _logger;

    public UrlConverterController(IUrlConversionService conversionService, ILogger<UrlConverterController> logger)
    {
        _conversionService = conversionService;
        _logger = logger;
    }


    [HttpPost("convert")]
    public async Task<IActionResult> ConvertToUrl([FromBody] InputConvertDto input)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Modelo inválido recebido em ConvertToUrl");
            return BadRequest(ModelState);
        }
        if (string.IsNullOrWhiteSpace(input.Url))
        {
            _logger.LogWarning("URL inválida recebida em ConvertToUrl");
            return BadRequest(new { message = "URL inválida" });
        }

        try
        {
            _logger.LogInformation("Iniciando conversão para URL: {Url}", input.Url);
            var result = await _conversionService.ConvertUrlAsync(input);
            if (!result.Success)
            {
                _logger.LogError("Erro ao converter URL: {ErrorMessage}", result.Error);
                return StatusCode(500, result);
            }
            _logger.LogInformation("Conversão concluída com sucesso: {Result}", result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Input inválido em ConvertToUrl");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno no ConvertToUrl");
            return StatusCode(500, new { error = "Erro interno", details = ex.Message });
        }
    }

    [HttpGet("download/{*filePath}")]
    public IActionResult DownloadFile(string filePath)
    {
        _logger.LogInformation("Iniciando download do arquivo com caminho: {FilePath}", filePath);
        try
        {
            var decoded = WebUtility.UrlDecode(filePath ?? "");

            if (string.IsNullOrWhiteSpace(decoded) || decoded.Contains("..") || Path.IsPathRooted(decoded))
            {
                _logger.LogWarning("Caminho de arquivo inválido ou potencialmente malicioso: {DecodedPath}", decoded);
                return BadRequest("Caminho de arquivo inválido.");
            }
            
            // Certificando-se que está pegando o caminho relativo, removendo qualquer referência a "downloads/" que possa ter restado
            var parts = decoded.Split(new[] { "downloads/" }, StringSplitOptions.RemoveEmptyEntries);
            var cleanPath = parts[parts.Length - 1];
            
            // Construindo o caminho completo do arquivo no sistema de arquivos
            var downloadsBaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
            var fullFilePath = Path.Combine(downloadsBaseFolder, cleanPath);
            
            _logger.LogInformation("Caminho completo do arquivo: {FullFilePath}", fullFilePath);

            // Verificar se o arquivo está realmente dentro da pasta de downloads (segurança)
            if (!fullFilePath.StartsWith(downloadsBaseFolder, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Acesso não autorizado ao caminho: {FullFilePath}", fullFilePath);
                return BadRequest("Acesso não autorizado.");
            }
            
            if (!System.IO.File.Exists(fullFilePath))
            {
                _logger.LogWarning("Arquivo não encontrado: {FullFilePath}", fullFilePath);
                return NotFound("Arquivo não encontrado.");
            }
            
            // Determinando o tipo de conteúdo com base na extensão
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullFilePath, out var contentType))
                contentType = "application/octet-stream";
            
            // Obter o nome do arquivo para exibição no download
            var fileName = Path.GetFileName(fullFilePath);
            
            _logger.LogInformation("Retornando arquivo: {FileName}, ContentType: {ContentType}", fileName, contentType);
            
            // Usando FileStreamResult para melhor performance com arquivos grandes
            var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(fileStream, contentType)
            {
                FileDownloadName = fileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar download do arquivo");
            return BadRequest("Erro ao processar a requisição.");
        }
    }
}
