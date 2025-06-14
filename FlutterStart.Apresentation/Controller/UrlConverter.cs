using System.Net;
using FlutterStart.Application.DTO;
using FlutterStart.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

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
            return BadRequest(ModelState);
        }
        if (string.IsNullOrWhiteSpace(input.Url))
        {
            return BadRequest(new { message = "URL inválida" });
        }

        try
        {
            var result = await _conversionService.ConvertUrlAsync(input);
            if (!result.Success)
            {
                return StatusCode(500, result);
            }
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

    [HttpGet("download-file/{*fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        try
        {
            // Decode de URL (caso venha com %2F etc)
            var decoded = WebUtility.UrlDecode(fileName ?? "");

            // Se vier a URL completa, remove tudo antes de "/downloads/"
            var relativePathIndex = decoded.IndexOf("/downloads/", StringComparison.OrdinalIgnoreCase);
            if (relativePathIndex >= 0)
            {
                decoded = decoded.Substring(relativePathIndex + "/downloads/".Length);
            }

            if (decoded.Contains("..") || Path.IsPathRooted(decoded))
                return BadRequest("Nome de arquivo inválido.");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
            var filePath = Path.Combine(folderPath, decoded);

            if (!filePath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Acesso não autorizado.");

            if (!System.IO.File.Exists(filePath))
                return NotFound("Arquivo não encontrado.");

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
                contentType = "application/octet-stream";

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, Path.GetFileName(filePath));
        }
        catch
        {
            return BadRequest("Erro ao processar a requisição.");
        }
    }
}
