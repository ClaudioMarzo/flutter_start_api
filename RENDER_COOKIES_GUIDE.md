# Guia de Configuração de Cookies para Render

## Problema
O YouTube está bloqueando downloads com a mensagem "Sign in to confirm you're not a bot", exigindo autenticação via cookies.

## Solução
Configure cookies do seu navegador no ambiente do Render usando variáveis de ambiente.

## Passos para Configuração

### 1. Exportar Cookies do Navegador

#### Opção A: Usando Extensão do Chrome/Edge
1. Instale a extensão "Get cookies.txt LOCALLY" ou similar
2. Acesse youtube.com e faça login
3. Use a extensão para exportar cookies como arquivo `cookies.txt`

#### Opção B: Usando Firefox
1. Instale o addon "cookies.txt"
2. Acesse youtube.com e faça login
3. Exporte os cookies para arquivo `cookies.txt`

#### Opção C: Manualmente (Developer Tools)
1. Abra youtube.com e faça login
2. Pressione F12 (Developer Tools)
3. Vá para Application/Storage > Cookies > https://www.youtube.com
4. Copie os cookies relevantes (veja COOKIES_MANUAL.md para detalhes)

### 2. Preparar Cookies para o Render

#### No Linux/macOS:
```bash
# Codificar cookies em base64
cat cookies.txt | base64 -w 0 > cookies_base64.txt

# O conteúdo de cookies_base64.txt será usado na variável de ambiente
```

#### No Windows (PowerShell):
```powershell
# Codificar cookies em base64
[Convert]::ToBase64String([IO.File]::ReadAllBytes("cookies.txt")) | Out-File -Encoding ascii cookies_base64.txt
```

### 3. Configurar no Render

1. Acesse seu dashboard do Render
2. Vá para seu serviço web
3. Clique em "Environment"
4. Adicione uma nova variável de ambiente:
   - **Nome**: `YOUTUBE_COOKIES_BASE64`
   - **Valor**: Cole o conteúdo do arquivo `cookies_base64.txt` (toda a string em uma linha)

### 4. Fazer Deploy

Após configurar a variável de ambiente, faça o deploy do seu serviço. O script `setup-render-cookies.sh` será executado automaticamente e criará o arquivo de cookies.

## Verificação

Para verificar se os cookies foram configurados corretamente, verifique os logs do seu serviço no Render. Você deve ver mensagens como:

```
✓ Variável de ambiente YOUTUBE_COOKIES_BASE64 encontrada
✓ Arquivo de cookies criado com sucesso em /app/cookies.txt
✓ Formato de cookies válido detectado
```

## Problemas Comuns

### Cookies Expirados
- Os cookies do YouTube expiram periodicamente
- Se começar a dar erro novamente, repita o processo de exportação

### Formato Inválido
- Certifique-se de que o arquivo cookies.txt está no formato Netscape
- A primeira linha deve ser: `# Netscape HTTP Cookie File`

### Variável de Ambiente Muito Grande
- Se a variável de ambiente for muito grande, considere usar apenas os cookies essenciais
- Remova cookies de outros domínios que não sejam youtube.com

## Cookies Essenciais

Os cookies mais importantes para o YouTube são:
- `VISITOR_INFO1_LIVE`
- `YSC`
- `GPS`
- `PREF`
- `LOGIN_INFO` (se estiver logado)

## Segurança

⚠️ **Importante**: Os cookies contêm informações de autenticação. Mantenha-os seguros:
- Não compartilhe o conteúdo da variável YOUTUBE_COOKIES_BASE64
- Use apenas em ambientes seguros
- Considere usar uma conta dedicada para este propósito

## Alternativas

Se não conseguir configurar cookies, considere:
1. Usar proxies rotativos
2. Implementar delays maiores entre requests
3. Usar serviços de terceiros para downloads
4. Configurar múltiplas instâncias com IPs diferentes
