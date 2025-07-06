#!/bin/bash

# Script para verificar configuração do arquivo ads.txt
# Usado para resolver problemas do Google AdSense

echo "🔍 Verificando configuração do arquivo ads.txt..."

ADS_TXT_FILE="../src/conversor/wwwroot/ads.txt"

# Verificar se o arquivo existe
if [ ! -f "$ADS_TXT_FILE" ]; then
    echo "❌ Erro: Arquivo ads.txt não encontrado!"
    echo "📍 Localização esperada: $ADS_TXT_FILE"
    exit 1
fi

# Verificar conteúdo do arquivo
CONTENT=$(cat "$ADS_TXT_FILE")
echo "📄 Conteúdo atual do ads.txt:"
echo "$CONTENT"

# Verificar se contém placeholder
if echo "$CONTENT" | grep -q "pub-XXXXXXXXX"; then
    echo "⚠️  AVISO: Arquivo contém placeholder!"
    echo "🔧 Você precisa substituir 'pub-XXXXXXXXX' pelo seu Publisher ID real do Google AdSense"
    echo "📖 Consulte docs/CONFIGURACAO_ADS_TXT.md para instruções detalhadas"
    exit 1
fi

# Verificar formato básico
if echo "$CONTENT" | grep -q "google.com.*pub-.*DIRECT.*f08c47fec0942fa0"; then
    echo "✅ Arquivo ads.txt está configurado corretamente!"
    echo "🚀 Após o deploy, verifique em: https://wepper.app/ads.txt"
else
    echo "❌ Erro: Formato do arquivo ads.txt incorreto!"
    echo "📋 Formato esperado: google.com, pub-1234567890123456, DIRECT, f08c47fec0942fa0"
    echo "📖 Consulte docs/CONFIGURACAO_ADS_TXT.md para instruções"
    exit 1
fi

echo "✅ Verificação concluída com sucesso!"
echo "📊 Monitore o console do Google AdSense nas próximas 24-48 horas" 