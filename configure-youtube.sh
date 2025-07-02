#!/bin/bash

echo "🚀 Configurador Automático de Cookies para YouTube"
echo "================================================="
echo ""

# Verificar se estamos no diretório correto
if [ ! -f "FlutterStartAPI.sln" ]; then
    echo "❌ Execute este script na raiz do projeto (onde está o FlutterStartAPI.sln)"
    exit 1
fi

echo "✅ Diretório correto detectado"
echo ""

# Função para instalar yt-dlp se não existir
install_ytdlp() {
    echo "🔧 Verificando yt-dlp..."
    if ! command -v yt-dlp &> /dev/null; then
        echo "📦 Instalando yt-dlp..."
        if command -v pip3 &> /dev/null; then
            pip3 install yt-dlp
        elif command -v pip &> /dev/null; then
            pip install yt-dlp
        else
            echo "❌ pip não encontrado. Instale o Python e pip primeiro."
            echo "Ubuntu/Debian: sudo apt install python3-pip"
            echo "Em seguida execute: pip3 install yt-dlp"
            exit 1
        fi
    fi
    
    if command -v yt-dlp &> /dev/null; then
        echo "✅ yt-dlp instalado: $(yt-dlp --version)"
    else
        echo "❌ Falha ao instalar yt-dlp"
        exit 1
    fi
}

# Função para extrair cookies do navegador
extract_cookies() {
    local browser=$1
    local cookies_file="cookies.txt"
    
    echo "🍪 Tentando extrair cookies do $browser..."
    
    # Usar um vídeo público para teste
    local test_url="https://www.youtube.com/watch?v=dQw4w9WgXcQ"
    
    if yt-dlp --cookies-from-browser "$browser" --cookies "$cookies_file" --simulate "$test_url" >/dev/null 2>&1; then
        if [ -f "$cookies_file" ] && [ -s "$cookies_file" ]; then
            echo "✅ Cookies extraídos com sucesso do $browser!"
            return 0
        fi
    fi
    
    echo "❌ Falha ao extrair cookies do $browser"
    return 1
}

# Função para testar cookies
test_cookies() {
    if [ ! -f "cookies.txt" ] || [ ! -s "cookies.txt" ]; then
        return 1
    fi
    
    echo "🧪 Testando cookies..."
    local test_url="https://www.youtube.com/watch?v=dQw4w9WgXcQ"
    
    if yt-dlp --cookies cookies.txt --simulate "$test_url" >/dev/null 2>&1; then
        echo "✅ Cookies funcionando!"
        return 0
    else
        echo "⚠️  Cookies podem não estar funcionando perfeitamente"
        return 1
    fi
}

# Função para gerar base64 para Render
generate_base64() {
    if [ -f "cookies.txt" ] && [ -s "cookies.txt" ]; then
        echo ""
        echo "🔑 Gerando Base64 para variável de ambiente no Render:"
        echo "=================================================="
        local base64_content=$(base64 -w 0 cookies.txt)
        echo "$base64_content"
        echo ""
        echo "📋 COMO USAR NO RENDER:"
        echo "1. Copie o texto Base64 acima"
        echo "2. No Render, vá em Environment Variables"
        echo "3. Adicione: YOUTUBE_COOKIES_BASE64 = [cole o texto aqui]"
        echo ""
    fi
}

# Função principal
main() {
    install_ytdlp
    
    echo ""
    echo "🍪 Extraindo cookies do navegador..."
    echo "=================================="
    
    # Tentar extrair de diferentes navegadores
    local browsers=("chrome" "firefox" "edge" "safari")
    local extracted=false
    
    for browser in "${browsers[@]}"; do
        if extract_cookies "$browser"; then
            extracted=true
            break
        fi
    done
    
    if [ "$extracted" = false ]; then
        echo ""
        echo "❌ Não foi possível extrair cookies automaticamente"
        echo ""
        echo "📋 MÉTODOS ALTERNATIVOS:"
        echo "1. Instale uma extensão do navegador:"
        echo "   - Chrome: 'Get cookies.txt LOCALLY'"
        echo "   - Firefox: 'cookies.txt extension'"
        echo "2. Acesse youtube.com e faça login"
        echo "3. Use a extensão para baixar cookies.txt"
        echo "4. Salve como 'cookies.txt' na raiz deste projeto"
        echo ""
        echo "5. Execute este script novamente depois de obter os cookies"
        exit 1
    fi
    
    # Testar cookies
    test_cookies
    
    # Gerar base64 para produção
    generate_base64
    
    echo ""
    echo "🎉 CONFIGURAÇÃO CONCLUÍDA!"
    echo "========================="
    echo "✅ Cookies configurados para desenvolvimento local"
    echo "✅ Base64 gerado para produção no Render"
    echo ""
    echo "🔄 PRÓXIMOS PASSOS:"
    echo "1. Teste sua aplicação localmente"
    echo "2. Configure a variável YOUTUBE_COOKIES_BASE64 no Render"
    echo "3. Faça o deploy"
    echo ""
    echo "⚠️  LEMBRE-SE:"
    echo "- Cookies expiram, renove periodicamente"
    echo "- Nunca commite cookies.txt no git"
    echo "- Se der erro 429, aguarde alguns minutos"
}

# Executar
main
