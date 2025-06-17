# Sistema de Versionamento - GeraWebP

## 🏷️ Versão Implementada

O sistema de versionamento foi implementado no projeto GeraWebP com as seguintes funcionalidades:

### ✅ Funcionalidades Implementadas

1. **Versão no Layout Principal**
   - Exibição da versão no rodapé de todas as páginas
   - Design discreto e responsivo
   - Tooltip com informações detalhadas
   - Compatibilidade com modo escuro

2. **Endpoint de API para Versão**
   - Rota: `/version` ou `/api/version`
   - Retorna informações completas em JSON
   - Inclui dados do build, framework e ambiente

3. **Configuração no Projeto**
   - Versão definida no arquivo `.csproj`
   - Versionamento automático baseado no assembly
   - Informações de copyright e produto

## 📍 Localizações dos Arquivos

### Arquivos Modificados:
- `src/conversor/GeraWebP.csproj` - Configuração de versão
- `src/conversor/Views/Shared/_Layout.cshtml` - Exibição no layout
- `src/conversor/Controllers/HomeController.cs` - Endpoint de API
- `src/conversor/wwwroot/css/site.css` - Estilos para a versão

## 🎨 Design da Versão

### No Layout:
```html
<footer class="build-version">
    <div title="Versão da aplicação: 1.0.0 | Build: 2025.06.16">
        Wepper v1.0.0
    </div>
</footer>
```

### Características Visuais:
- Fonte monoespaçada para melhor legibilidade
- Cor discreta que não interfere no design
- Efeito hover sutil para interatividade
- Responsive design para todos os dispositivos
- Acessibilidade completa (high contrast, reduced motion)

## 🔗 API de Versão

### Endpoint: `GET /version` ou `GET /api/version`

### Resposta JSON:
```json
{
    "Application": "GeraWebP",
    "Version": "1.0.0.0",
    "ShortVersion": "1.0.0",
    "BuildDate": "2025-06-16",
    "BuildTime": "2025-06-16 15:30:00 UTC",
    "Framework": "9.0.0",
    "Platform": "Microsoft Windows NT 10.0.19045.0",
    "Environment": "Production"
}
```

### Campos Retornados:
- **Application**: Nome da aplicação
- **Version**: Versão completa (4 dígitos)
- **ShortVersion**: Versão resumida (3 dígitos)
- **BuildDate**: Data do build
- **BuildTime**: Data e hora completa do build
- **Framework**: Versão do .NET
- **Platform**: Sistema operacional
- **Environment**: Ambiente de execução (Development/Staging/Production)

## 🛠️ Como Atualizar a Versão

### 1. Atualizando a Versão Principal:
Edite o arquivo `src/conversor/GeraWebP.csproj`:

```xml
<PropertyGroup>
    <Version>1.1.0</Version>
    <AssemblyVersion>1.1.0.0</AssemblyVersion>
    <FileVersion>1.1.0.0</FileVersion>
</PropertyGroup>
```

### 2. Recompile a Aplicação:
```bash
cd src/conversor
dotnet build --configuration Release
```

### 3. Verificação:
- A versão será automaticamente atualizada no rodapé
- O endpoint `/version` refletirá as mudanças
- A data de build será atualizada automaticamente

## 📱 Responsividade

O sistema de versão é totalmente responsivo:

- **Desktop**: Versão completa visível
- **Tablet**: Versão otimizada
- **Mobile**: Versão compacta com fonte menor
- **High Contrast**: Suporte para modo de alto contraste
- **Dark Mode**: Cores adaptadas para tema escuro

## 🔧 Manutenção

### Logs e Monitoramento:
- Erros na obtenção de versão são logados automaticamente
- Fallbacks seguros em caso de problemas
- Não afeta o funcionamento da aplicação principal

### Atualizações Futuras:
- Adicionar número de build automático
- Integração com CI/CD para versionamento automático
- Histórico de versões em página dedicada
- Notificações de atualizações para usuários

## ✨ Benefícios

1. **Transparência**: Usuários podem ver a versão atual
2. **Suporte**: Facilita identificação de problemas
3. **Desenvolvimento**: Tracking de deploys e builds
4. **API**: Integração com ferramentas de monitoramento
5. **UX**: Design não intrusivo mas informativo
