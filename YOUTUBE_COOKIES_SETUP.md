# Como Configurar Cookies do YouTube para yt-dlp

## Problema
O YouTube está bloqueando requisições do servidor com erro 429 e solicitando autenticação via cookies.

## Solução: Configurar Cookies

### Método 1: Extrair Cookies do Navegador (Recomendado)

1. **Instale uma extensão para extrair cookies:**
   - Chrome: "Get cookies.txt LOCALLY" ou "cookies.txt"
   - Firefox: "cookies.txt" extension

2. **Extrair cookies do YouTube:**
   - Acesse youtube.com no seu navegador
   - Faça login na sua conta
   - Use a extensão para baixar cookies.txt
   - Salve o arquivo como `cookies.txt`

### Método 2: Usar yt-dlp para extrair cookies automaticamente

```bash
# Extrair cookies do Chrome
yt-dlp --cookies-from-browser chrome --write-pages --skip-download "https://www.youtube.com/watch?v=dQw4w9WgXcQ"

# Extrair cookies do Firefox
yt-dlp --cookies-from-browser firefox --write-pages --skip-download "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
```

### Método 3: Formato manual do cookies.txt

Se você quiser criar manualmente, o formato deve ser:
```
# Netscape HTTP Cookie File
# This is a generated file!  Do not edit.

.youtube.com	TRUE	/	FALSE	1234567890	cookie_name	cookie_value
```

## Configuração no Projeto

### Local (Desenvolvimento)
1. Coloque o arquivo `cookies.txt` na raiz do projeto
2. O código irá detectar automaticamente

### Servidor (Render)
1. **Opção 1 - Via variável de ambiente:**
   - Encode o conteúdo do cookies.txt em base64
   - Adicione como variável de ambiente `YOUTUBE_COOKIES_BASE64`
   - O código irá decodificar e criar o arquivo

2. **Opção 2 - Build time:**
   - Inclua o arquivo no build do Docker
   - Adicione no Dockerfile: `COPY cookies.txt /app/cookies.txt`

## Implementação Automática de Cookies via Variável de Ambiente

Vou adicionar uma função para ler cookies de variável de ambiente:

```csharp
public string GetCookiesArg()
{
    // Primeiro, tentar obter cookies de variável de ambiente
    var cookiesFromEnv = Environment.GetEnvironmentVariable("YOUTUBE_COOKIES_BASE64");
    if (!string.IsNullOrEmpty(cookiesFromEnv))
    {
        try
        {
            var cookiesContent = Convert.FromBase64String(cookiesFromEnv);
            var cookiesPath = Path.Combine(Path.GetTempPath(), "cookies.txt");
            File.WriteAllBytes(cookiesPath, cookiesContent);
            _logger.LogInformation("Cookies criados a partir da variável de ambiente: {CookiesPath}", cookiesPath);
            return $"--cookies \"{cookiesPath}\"";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar cookies da variável de ambiente");
        }
    }
    
    // Resto do código existente...
}
```

## Testando Cookies

Para testar se os cookies estão funcionando:

```bash
yt-dlp --cookies cookies.txt --simulate "https://www.youtube.com/watch?v=VIDEO_ID"
```

## Dicas Importantes

1. **Renovação**: Cookies expiram, você precisará renovar periodicamente
2. **Segurança**: Nunca commite cookies.txt no git (adicione ao .gitignore)
3. **Rate Limiting**: Mesmo com cookies, evite muitas requisições simultâneas
4. **IP Blocking**: Se o IP do Render for bloqueado, considere usar proxies

## Alternativas se Cookies Não Funcionarem

1. **Use proxies rotativos**
2. **Implemente cache para evitar downloads repetidos**
3. **Use serviços de proxy dedicados**
4. **Considere usar YouTube Data API para metadados**
