#!/bin/bash

# Script para configurar cookies do YouTube para yt-dlp
# Este script ajuda a extrair e configurar cookies automaticamente

echo "=== Configurador de Cookies YouTube para yt-dlp ==="
echo ""

# Verificar se o yt-dlp está instalado
if ! command -v yt-dlp &> /dev/null; then
    echo "❌ yt-dlp não encontrado. Instale primeiro:"
    echo "pip install yt-dlp"
    exit 1
fi

echo "✅ yt-dlp encontrado"
echo ""

# Função para extrair cookies
extract_cookies() {
    local browser=$1
    local output_file="cookies.txt"
    
    echo "🔍 Tentando extrair cookies do $browser..."
    
    # Testar com um vídeo público
    local test_url="https://www.youtube.com/watch?v=dQw4w9WgXcQ"
    
    yt-dlp --cookies-from-browser "$browser" --cookies "$output_file" --write-pages --skip-download "$test_url" 2>/dev/null
    
    if [ -f "$output_file" ] && [ -s "$output_file" ]; then
        echo "✅ Cookies extraídos com sucesso para: $output_file"
        return 0
    else
        echo "❌ Falha ao extrair cookies do $browser"
        return 1
    fi
}

# Tentar extrair cookies de diferentes navegadores
browsers=("chrome" "firefox" "edge" "safari")
extracted=false

for browser in "${browsers[@]}"; do
    if extract_cookies "$browser"; then
        extracted=true
        break
    fi
done

if [ "$extracted" = false ]; then
    echo ""
    echo "❌ Não foi possível extrair cookies automaticamente."
    echo ""
    echo "📋 Métodos alternativos:"
    echo "1. Use uma extensão do navegador para exportar cookies:"
    echo "   - Chrome: 'Get cookies.txt LOCALLY'"
    echo "   - Firefox: 'cookies.txt extension'"
    echo ""
    echo "2. Salve o arquivo como 'cookies.txt' na raiz do projeto"
    echo ""
    echo "3. Para produção no Render, encode em base64:"
    echo "   base64 cookies.txt"
    echo "   E adicione como variável YOUTUBE_COOKIES_BASE64"
else
    echo ""
    echo "🎉 Configuração concluída!"
    echo ""
    echo "📋 Próximos passos:"
    echo "1. Para teste local: o arquivo cookies.txt já está configurado"
    echo "2. Para produção no Render:"
    echo "   - Execute: base64 cookies.txt"
    echo "   - Copie a saída"
    echo "   - Adicione como variável de ambiente YOUTUBE_COOKIES_BASE64"
    echo ""
    echo "🔄 Lembre-se de renovar os cookies periodicamente!"
fi

# Testar se os cookies funcionam
if [ -f "cookies.txt" ] && [ -s "cookies.txt" ]; then
    echo ""
    echo "🧪 Testando cookies..."
    
    test_url="https://www.youtube.com/watch?v=dQw4w9WgXcQ"
    if yt-dlp --cookies cookies.txt --simulate "$test_url" >/dev/null 2>&1; then
        echo "✅ Cookies funcionando corretamente!"
    else
        echo "⚠️  Cookies podem não estar funcionando. Verifique se você está logado no YouTube."
    fi
fi
