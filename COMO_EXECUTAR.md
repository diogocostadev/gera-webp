# Como Executar o WebP Converter

## Problema Identificado e Solução

### Problema
O macOS usa a porta 5000 por padrão para o AirPlay/AirTunes, causando conflito com aplicações ASP.NET Core.

### Solução Implementada
Configuramos o sistema para usar a porta **5050** em vez da porta 5000.

## Instruções de Execução

### 1. Navegar para o diretório do projeto
```bash
cd src/conversor
```

### 2. Executar o sistema
```bash
dotnet run --urls "http://localhost:5050"
```

### 3. Acessar no navegador
Abra seu navegador e acesse: **http://localhost:5050**

## Configuração Permanente

O arquivo `appsettings.Development.json` foi configurado com:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Urls": "http://localhost:5050;https://localhost:5051"
}
```

## Verificação de Funcionamento

### Testar se o sistema está rodando:
```bash
curl -I http://localhost:5050
```

### Testar se os arquivos CSS estão sendo servidos:
```bash
curl -I http://localhost:5050/css/site.css
```

## Estrutura de Arquivos Importantes

- **Views/Home/Index.cshtml** - Página principal (auto-contida, sem layout)
- **Views/Home/Privacidade.cshtml** - Página de privacidade (auto-contida, sem layout)
- **wwwroot/css/site.css** - Estilos principais do sistema
- **Controllers/HomeController.cs** - Controlador principal
- **Program.cs** - Configuração da aplicação

## Notas Importantes

1. **Layout**: As páginas Index e Privacidade são auto-contidas (não usam `_Layout.cshtml`)
2. **Porta**: Sistema configurado para porta 5050 para evitar conflito com AirPlay
3. **Arquivos Estáticos**: Configurados corretamente em `Program.cs` com `app.UseStaticFiles()`
4. **SignalR**: Configurado para atualizações em tempo real do progresso

## Troubleshooting

### Se a porta 5050 estiver ocupada:
```bash
# Verificar processos na porta
lsof -i :5050

# Usar porta alternativa
dotnet run --urls "http://localhost:5051"
```

### Se os arquivos CSS não carregarem:
1. Verificar se `app.UseStaticFiles()` está em `Program.cs`
2. Verificar se os arquivos estão em `wwwroot/css/`
3. Verificar permissões dos arquivos

### Logs de Debug:
```bash
# Executar com logs detalhados
dotnet run --urls "http://localhost:5050" --verbosity detailed
``` 