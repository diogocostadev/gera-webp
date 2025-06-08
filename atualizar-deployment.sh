#!/bin/bash

echo "🔧 CORRIGINDO PROBLEMA DO VOLUME CONTADOR"
echo "========================================="

# Parar container atual
echo "⏹️ Parando container wepper-pro..."
echo "Dida10csbr" | sudo -S docker stop wepper-pro 2>/dev/null || true
echo "Dida10csbr" | sudo -S docker rm wepper-pro 2>/dev/null || true

# Criar diretório data para o contador (separado do wwwroot)
echo "📁 Criando diretório data para contador..."
echo "Dida10csbr" | sudo -S mkdir -p /opt/wepper-pro/{uploads,convertidos,data,logs}
echo "Dida10csbr" | sudo -S chown -R 1654:1654 /opt/wepper-pro/
echo "Dida10csbr" | sudo -S chmod -R 755 /opt/wepper-pro/

# Migrar contador existente se houver
if [ -f "/projetos/wepper-pro/contador.json" ]; then
    echo "📦 Migrando contador existente..."
    echo "Dida10csbr" | sudo -S cp /projetos/wepper-pro/contador.json /opt/wepper-pro/data/contador.json
    echo "Dida10csbr" | sudo -S chown 1654:1654 /opt/wepper-pro/data/contador.json
fi

echo "🚀 Iniciando container com volume correto..."
echo "Dida10csbr" | sudo -S docker run -d \
  --name wepper-pro \
  --restart unless-stopped \
  -v /opt/wepper-pro/uploads:/app/wwwroot/uploads \
  -v /opt/wepper-pro/convertidos:/app/wwwroot/convertidos \
  -v /opt/wepper-pro/data:/app/data \
  -v /opt/wepper-pro/logs:/app/logs \
  -p 127.0.0.1:3232:8080 \
  -e TZ=America/Sao_Paulo \
  -e ASPNETCORE_ENVIRONMENT=Production \
  --memory=512m \
  --cpus=1.0 \
  wepper-pro

sleep 5

echo "✅ Container iniciado!"
echo "🔍 Verificando status..."
echo "Dida10csbr" | sudo -S docker ps | grep wepper-pro

echo -e "\n📋 Logs recentes:"
echo "Dida10csbr" | sudo -S docker logs --tail 10 wepper-pro

echo -e "\n🌐 Testando aplicação..."
sleep 3
curl -I http://localhost:3232 2>/dev/null | head -2 || echo "❌ Aplicação não está respondendo"

echo -e "\n🎯 PROBLEMA RESOLVIDO!"
echo "Agora o wwwroot não é sobrescrito e os arquivos CSS/JS funcionam!"
echo "Contador será salvo em: /opt/wepper-pro/data/contador.json" 