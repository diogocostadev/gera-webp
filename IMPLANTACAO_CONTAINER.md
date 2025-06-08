# Guia de Implantação em Container - WebP Converter

## Visão Geral

Este documento fornece instruções completas para implantar o **WebP Converter** em ambiente containerizado usando Docker, incluindo mapeamento de volumes, configurações de segurança e instruções passo a passo

## Estrutura do Projeto

```
/
├── src/
│   └── conversor/
│       ├── Dockerfile
│       ├── wwwroot/
│       │   ├── uploads/        # Arquivos enviados pelos usuários
│       │   ├── convertidos/    # Arquivos convertidos
│       │   ├── css/           # Estilos CSS
│       │   ├── js/            # Scripts JavaScript
│       │   ├── img/           # Imagens do site
│       │   └── lib/           # Bibliotecas JavaScript
│       ├── Views/             # Templates Razor
│       ├── Controllers/       # Controladores MVC
│       └── Models/           # Modelos de dados
└── docs/                     # Documentação
```

## Volumes Críticos a Mapear

### 1. Pasta de Uploads (`wwwroot/uploads/`)
- **Função**: Armazena arquivos enviados pelos usuários
- **Importância**: CRÍTICA - dados temporários que precisam persistir entre restarts
- **Mapeamento**: `/host/uploads:/app/wwwroot/uploads`

### 2. Pasta de Convertidos (`wwwroot/convertidos/`)
- **Função**: Armazena arquivos WebP convertidos
- **Importância**: CRÍTICA - arquivos de saída que usuários baixam
- **Mapeamento**: `/host/convertidos:/app/wwwroot/convertidos`

### 3. Logs (opcional)
- **Função**: Logs da aplicação
- **Importância**: MÉDIA - para debugging e monitoramento
- **Mapeamento**: `/host/logs:/app/logs`

## Preparação do Ambiente Host

### 1. Criar Diretórios no Host

```bash
# Criar estrutura de diretórios
sudo mkdir -p /opt/webp-converter/{uploads,convertidos,logs}

# Definir permissões apropriadas (usuário da aplicação é 1654)
sudo chown -R 1654:1654 /opt/webp-converter/
sudo chmod -R 755 /opt/webp-converter/
```

### 2. Configurar Limpeza Automática (Opcional)

```bash
# Criar script de limpeza
sudo tee /opt/webp-converter/cleanup.sh << 'EOF'
#!/bin/bash
# Limpar arquivos com mais de 1 hora
find /opt/webp-converter/uploads -type f -mmin +60 -delete
find /opt/webp-converter/convertidos -type f -mmin +60 -delete
EOF

sudo chmod +x /opt/webp-converter/cleanup.sh

# Adicionar ao crontab (executar a cada 30 minutos)
echo "*/30 * * * * /opt/webp-converter/cleanup.sh" | sudo crontab -
```

## Build da Imagem

### 1. Build Local

```bash
# Navegar para o diretório do projeto
cd src/

# Build da imagem
docker build -t webp-converter:latest -f conversor/Dockerfile .
```

### 2. Build Multi-arquitetura (Opcional)

```bash
# Para suportar AMD64 e ARM64
docker buildx build --platform linux/amd64,linux/arm64 -t webp-converter:latest -f conversor/Dockerfile . --push
```

## Execução em Produção

### 1. Docker Run (Básico)

```bash
docker run -d \
  --name webp-converter \
  --restart unless-stopped \
  -p 8080:8080 \
  -v /opt/webp-converter/uploads:/app/wwwroot/uploads \
  -v /opt/webp-converter/convertidos:/app/wwwroot/convertidos \
  -v /opt/webp-converter/logs:/app/logs \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8080 \
  --memory=512m \
  --cpus=1.0 \
  webp-converter:latest
```

### 2. Docker Compose (Recomendado)

Criar arquivo `docker-compose.yml`:

```yaml
version: '3.8'

services:
  webp-converter:
    image: webp-converter:latest
    container_name: webp-converter
    restart: unless-stopped
    ports:
      - "8080:8080"
    volumes:
      - /opt/webp-converter/uploads:/app/wwwroot/uploads
      - /opt/webp-converter/convertidos:/app/wwwroot/convertidos
      - /opt/webp-converter/logs:/app/logs
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - TZ=America/Sao_Paulo
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: '1.0'
        reservations:
          memory: 256M
          cpus: '0.5'
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### 3. Execução com Docker Compose

```bash
# Subir o serviço
docker-compose up -d

# Verificar logs
docker-compose logs -f webp-converter

# Parar o serviço
docker-compose down
```

## Configuração com Reverse Proxy (Nginx)

### 1. Configuração Nginx

```nginx
server {
    listen 80;
    server_name seu-dominio.com www.seu-dominio.com;
    
    # Redirecionar para HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name seu-dominio.com www.seu-dominio.com;
    
    # Configurações SSL
    ssl_certificate /path/to/certificate.crt;
    ssl_certificate_key /path/to/private.key;
    
    # Tamanho máximo de upload (50MB)
    client_max_body_size 50M;
    
    # Timeout para uploads grandes
    proxy_read_timeout 300;
    proxy_connect_timeout 300;
    proxy_send_timeout 300;
    
    location / {
        proxy_pass http://localhost:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
    
    # Cache para arquivos estáticos
    location ~* \.(css|js|jpg|jpeg|png|gif|ico|svg|webp)$ {
        proxy_pass http://localhost:8080;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

## Monitoramento e Logs

### 1. Verificar Status

```bash
# Status do container
docker ps | grep webp-converter

# Logs em tempo real
docker logs -f webp-converter

# Uso de recursos
docker stats webp-converter
```

### 2. Health Check

```bash
# Verificar saúde da aplicação
curl -f http://localhost:8080/health || echo "Aplicação não está respondendo"
```

### 3. Backup dos Volumes

```bash
#!/bin/bash
# Script de backup
BACKUP_DIR="/backup/webp-converter/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

# Backup dos uploads e convertidos (se necessário)
tar -czf "$BACKUP_DIR/uploads.tar.gz" -C /opt/webp-converter uploads/
tar -czf "$BACKUP_DIR/convertidos.tar.gz" -C /opt/webp-converter convertidos/

# Manter apenas os últimos 7 dias de backup
find /backup/webp-converter -type d -mtime +7 -exec rm -rf {} +
```

## Segurança

### 1. Usuário Não-Root
O container já executa com usuário não-root (UID 1654).

### 2. Rede Isolada

```yaml
networks:
  webp-network:
    driver: bridge
    internal: true  # Isolar da internet se necessário
```

### 3. Variáveis de Ambiente Sensíveis

```bash
# Use arquivo .env para variáveis sensíveis
echo "ASPNETCORE_ENVIRONMENT=Production" > .env
echo "API_KEY=sua-chave-secreta" >> .env
```

## Troubleshooting

### 1. Problemas de Permissão

```bash
# Verificar permissões dos volumes
ls -la /opt/webp-converter/

# Corrigir permissões se necessário
sudo chown -R 1654:1654 /opt/webp-converter/
```

### 2. Container Não Inicia

```bash
# Verificar logs de erro
docker logs webp-converter

# Verificar se as portas estão disponíveis
netstat -tlnp | grep :8080
```

### 3. Alto Uso de Memória

```bash
# Monitorar uso de memória
docker stats --no-stream webp-converter

# Ajustar limite de memória no docker-compose.yml
```

## Atualizações

### 1. Atualizar Imagem

```bash
# Pull nova versão
docker pull webp-converter:latest

# Recriar container
docker-compose up -d --force-recreate
```

### 2. Backup Antes da Atualização

```bash
# Sempre fazer backup antes de atualizações
./backup-script.sh
```

## Comandos Úteis

```bash
# Entrar no container
docker exec -it webp-converter /bin/bash

# Verificar espaço em disco dos volumes
du -sh /opt/webp-converter/*

# Limpar arquivos temporários manualmente
find /opt/webp-converter/uploads -type f -mmin +60 -delete
find /opt/webp-converter/convertidos -type f -mmin +60 -delete

# Reiniciar apenas o serviço
docker-compose restart webp-converter
```

---

**Nota Importante**: Sempre teste a implantação em ambiente de desenvolvimento antes de aplicar em produção. Monitore os logs durante as primeiras horas após a implantação. 