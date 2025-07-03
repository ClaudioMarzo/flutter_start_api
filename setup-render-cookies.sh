#!/bin/bash

# Script para configurar cookies do YouTube no Render
# Este script deve ser executado no ambiente de produção

echo "=== Configuração de Cookies do YouTube para Render ==="

# Verificar se a variável de ambiente YOUTUBE_COOKIES_BASE64 está definida
if [ -n "$YOUTUBE_COOKIES_BASE64" ]; then
    echo "✓ Variável de ambiente YOUTUBE_COOKIES_BASE64 encontrada"
    
    # Decodificar e salvar cookies
    echo "$YOUTUBE_COOKIES_BASE64" | base64 -d > /app/cookies.txt
    
    if [ -f "/app/cookies.txt" ] && [ -s "/app/cookies.txt" ]; then
        echo "✓ Arquivo de cookies criado com sucesso em /app/cookies.txt"
        echo "Tamanho do arquivo: $(wc -c < /app/cookies.txt) bytes"
        
        # Verificar formato dos cookies
        if head -1 /app/cookies.txt | grep -q "# Netscape HTTP Cookie File"; then
            echo "✓ Formato de cookies válido detectado"
        else
            echo "⚠ Aviso: Formato de cookies pode não ser válido"
        fi
    else
        echo "✗ Falha ao criar arquivo de cookies"
        exit 1
    fi
else
    echo "⚠ Variável YOUTUBE_COOKIES_BASE64 não encontrada"
    echo "Para configurar:"
    echo "1. Exporte seus cookies do Chrome/Firefox para um arquivo cookies.txt"
    echo "2. Codifique em base64: cat cookies.txt | base64 -w 0"
    echo "3. Defina a variável de ambiente YOUTUBE_COOKIES_BASE64 no Render"
fi

echo "=== Configuração concluída ==="
