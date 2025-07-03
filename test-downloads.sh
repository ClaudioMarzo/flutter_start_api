#!/bin/bash

# Script de teste para validar downloads de vídeo
echo "=== Teste de Download de Vídeo ==="

# Configurações
TEST_URL="https://www.youtube.com/watch?v=dQw4w9WgXcQ"  # Never Gonna Give You Up (teste)
DOWNLOAD_DIR="./test_downloads"
YT_DLP_PATH="./yt-dlp_linux"

# Criar diretório de teste
mkdir -p "$DOWNLOAD_DIR"
echo "Diretório de teste criado: $DOWNLOAD_DIR"

# Tornar yt-dlp executável
chmod +x "$YT_DLP_PATH"

echo ""
echo "=== Teste 1: Download MP4 (formato preferencial) ==="
$YT_DLP_PATH \
    --user-agent "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36" \
    --extractor-args "youtube:player_client=android" \
    --add-header "Accept-Language:en-US,en;q=0.9" \
    -f "best[ext=mp4][height<=720]/bestvideo[ext=mp4][height<=720]+bestaudio[ext=m4a]/best[height<=720]" \
    --merge-output-format mp4 \
    --embed-metadata \
    -o "$DOWNLOAD_DIR/test_mp4_%(id)s.%(ext)s" \
    "$TEST_URL"

if [ $? -eq 0 ]; then
    echo "✓ Download MP4 bem-sucedido"
    ls -la "$DOWNLOAD_DIR"/test_mp4_*
else
    echo "✗ Falha no download MP4"
fi

echo ""
echo "=== Teste 2: Download MP3 (áudio) ==="
$YT_DLP_PATH \
    --user-agent "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36" \
    --extractor-args "youtube:player_client=android" \
    --extract-audio \
    --audio-format mp3 \
    --audio-quality 0 \
    --embed-metadata \
    -o "$DOWNLOAD_DIR/test_mp3_%(id)s.%(ext)s" \
    "$TEST_URL"

if [ $? -eq 0 ]; then
    echo "✓ Download MP3 bem-sucedido"
    ls -la "$DOWNLOAD_DIR"/test_mp3_*
else
    echo "✗ Falha no download MP3"
fi

echo ""
echo "=== Teste 3: Verificar formato de saída ==="
for file in "$DOWNLOAD_DIR"/*; do
    if [ -f "$file" ]; then
        echo "Arquivo: $(basename "$file")"
        echo "Tamanho: $(du -h "$file" | cut -f1)"
        if command -v file &> /dev/null; then
            echo "Tipo: $(file "$file" | cut -d: -f2-)"
        fi
        echo "---"
    fi
done

echo ""
echo "=== Limpeza ==="
# Remover arquivos de teste (descomente se quiser manter os arquivos)
# rm -rf "$DOWNLOAD_DIR"
echo "Arquivos de teste mantidos em: $DOWNLOAD_DIR"

echo ""
echo "=== Teste concluído ==="
