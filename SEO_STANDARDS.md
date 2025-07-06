# 🌐 Padrões SEO Multilíngue - Wepper.app

## 📋 REGRAS OBRIGATÓRIAS

> **⚠️ IMPORTANTE:** Toda nova página/funcionalidade DEVE seguir estas regras.  
> **🚫 PULL REQUESTS** que não seguirem serão rejeitados.

## 🌍 Idiomas Suportados

| Idioma | Código | Prefixo URL | Exemplo |
|--------|--------|-------------|---------|
| Português | `pt-br` | `/` | `/comprimir-jpeg` |
| Inglês | `en` | `/en/` | `/en/compress-jpeg` |
| Espanhol | `es` | `/es/` | `/es/comprimir-jpeg` |

## 🔧 Checklist OBRIGATÓRIO para Novas Páginas

### ✅ **1. Rotas no Controller**
```csharp
// SEMPRE implementar as 3 rotas:
[HttpGet("nova-funcionalidade")]                    // Português
[HttpGet("en/new-feature")]                         // Inglês  
[HttpGet("es/nueva-funcionalidad")]                 // Espanhol

// Configuração de idioma OBRIGATÓRIA:
SetCultureContent("pt-br");  // ou "en" ou "es"
```

### ✅ **2. Views Traduzidas**
```
Views/Home/
├── NovaFuncionalidade.cshtml           # Português (original)
├── NovaFuncionalidadeEnglish.cshtml    # Inglês (tradução)
└── NovaFuncionalidadeSpanish.cshtml    # Espanhol (tradução)
```

### ✅ **3. Meta Tags Específicas**
```html
@{
    ViewData["Title"] = "Título específico do idioma";
    ViewData["Description"] = "Descrição específica do idioma (máx 160 chars)";
    ViewData["Keywords"] = "palavras, chave, específicas, idioma";
}
```

### ✅ **4. Hreflang no _Layout.cshtml**
```csharp
// Adicionar mapeamento no urlMappings:
["/nova-funcionalidade"] = new() { 
    ["pt-br"] = "/nova-funcionalidade", 
    ["en"] = "/en/new-feature", 
    ["es"] = "/es/nueva-funcionalidad" 
}
```

### ✅ **5. Sitemap.xml Atualizado**
```csharp
// Adicionar no SeoController.cs:
new { Url = "/nova-funcionalidade", Priority = "0.9", ChangeFreq = "weekly" },
new { Url = "/en/new-feature", Priority = "0.9", ChangeFreq = "weekly" },
new { Url = "/es/nueva-funcionalidad", Priority = "0.9", ChangeFreq = "weekly" }
```

## 📝 Padrões de Tradução

### **🇧🇷 Português (Padrão)**
- Tom: Informal e acessível
- Foco: Gratuito, rápido, seguro
- Call-to-action: "Começar agora", "Baixar grátis"

### **🇺🇸 Inglês**
- Tom: Profissional mas amigável
- Foco: Free, fast, secure, no watermarks
- Call-to-action: "Start now", "Download free"

### **🇪🇸 Espanhol**
- Tom: Caloroso e próximo
- Foco: Gratis, rápido, seguro, sin marcas
- Call-to-action: "Comenzar ahora", "Descargar gratis"

## 🎯 Templates de Meta Tags

### **Compressão/Otimização**
```html
<!-- Português -->
Title: "[Formato] Online Grátis - [Ação] Sem Perder Qualidade"
Description: "[Ferramenta] [formato] grátis. [Benefício principal] mantendo qualidade. Processamento por lotes até 100MB."

<!-- Inglês -->
Title: "[Action] [Format] Online Free - [Main Benefit]"
Description: "Free [format] [tool] online. [Main benefit] while maintaining quality. Batch processing up to 100MB."

<!-- Espanhol -->
Title: "[Acción] [Formato] Online Gratis - [Beneficio Principal]"
Description: "[Herramienta] [formato] gratis. [Beneficio principal] manteniendo calidad. Procesamiento por lotes hasta 100MB."
```

### **Conversão/Transformação**
```html
<!-- Português -->
Title: "Converter [De] para [Para] Online - Grátis e Rápido"
Description: "Conversor [de] para [para] gratuito. Conversão rápida e segura. Suporte a lotes, sem marcas d'água."

<!-- Inglês -->
Title: "Convert [From] to [To] Online Free - Fast & Secure"
Description: "Free [from] to [to] converter online. Fast and secure conversion. Batch support, no watermarks."

<!-- Espanhol -->
Title: "Convertir [De] a [Para] Online Gratis - Rápido y Seguro"
Description: "Convertidor [de] a [para] gratis. Conversión rápida y segura. Soporte por lotes, sin marcas de agua."
```

## 📊 Estrutura de Conteúdo Obrigatória

### **🔥 Hero Section**
```html
<h1>[Ação] [Formato] Online</h1>
<p>[Benefício principal] mantendo [qualidade específica]. [Características únicas].</p>
```

### **💎 Benefits Section (4 cards sempre)**
1. **Benefit 1**: Característica principal (ex: preservar transparência)
2. **Benefit 2**: Performance (ex: carregamento rápido)
3. **Benefit 3**: Mobile-friendly (ex: otimizado para móvel)
4. **Benefit 4**: Batch processing (ex: múltiplos arquivos)

### **📋 How-to Section (4 steps sempre)**
1. **Step 1**: Selecionar/Upload
2. **Step 2**: Configurar/Escolher
3. **Step 3**: Processar/Converter
4. **Step 4**: Baixar/Salvar

### **❓ FAQ Section (mínimo 5 perguntas)**
1. Pergunta sobre qualidade/perda
2. Pergunta sobre limite de tamanho
3. Pergunta sobre segurança/privacidade
4. Pergunta sobre processamento em lote
5. Pergunta específica da funcionalidade

## 🌐 URLs Semânticas

### **Padrões de Nomenclatura:**
```
Português: verbo-substantivo (comprimir-jpeg, converter-webp)
Inglês: verb-noun (compress-jpeg, convert-webp)
Espanhol: verbo-substantivo (comprimir-jpeg, convertir-webp)
```

### **Exemplos Corretos:**
```
✅ /comprimir-jpeg → /en/compress-jpeg → /es/comprimir-jpeg
✅ /redimensionar-imagem → /en/resize-image → /es/redimensionar-imagen
✅ /remover-metadados → /en/remove-metadata → /es/eliminar-metadatos
```

### **Exemplos INCORRETOS:**
```
❌ /compress-jpg (usar jpeg)
❌ /imagen-resize (ordem incorreta em espanhol)
❌ /en/comprimir-jpeg (não traduzido)
```

## 🔍 Keywords por Categoria

### **Compressão:**
- **PT**: comprimir, otimizar, reduzir tamanho, compressor
- **EN**: compress, optimize, reduce size, compressor
- **ES**: comprimir, optimizar, reducir tamaño, compresor

### **Conversão:**
- **PT**: converter, transformar, mudar formato, conversor
- **EN**: convert, transform, change format, converter
- **ES**: convertir, transformar, cambiar formato, convertidor

### **Redimensionamento:**
- **PT**: redimensionar, alterar tamanho, ajustar dimensões
- **EN**: resize, change size, adjust dimensions
- **ES**: redimensionar, cambiar tamaño, ajustar dimensiones

## 🚀 Integração com Layout

### **Breadcrumbs Automáticos:**
```html
<nav aria-label="breadcrumb">
    <ol class="breadcrumb">
        <li><a href="/@(cultura == "en" ? "en/" : cultura == "es" ? "es/" : "")">
            @(cultura == "en" ? "Home" : cultura == "es" ? "Inicio" : "Início")</a></li>
        <li class="active">@ViewData["Title"]</li>
    </ol>
</nav>
```

### **CTA Buttons Localizados:**
```html
<!-- Português -->
<button>Processar Agora</button>

<!-- Inglês -->
<button>Process Now</button>

<!-- Espanhol -->
<button>Procesar Ahora</button>
```

## 📈 Métricas e Validação

### **Ferramentas de Teste Obrigatórias:**
1. **Google Search Console** - Indexação
2. **PageSpeed Insights** - Performance
3. **Mobile-Friendly Test** - Responsividade
4. **hreflang.org** - Validação hreflang
5. **Schema.org Validator** - Estruturação

### **KPIs por Idioma:**
- **Tempo de carregamento** < 3 segundos
- **Core Web Vitals** todas verdes
- **Indexação** dentro de 7 dias
- **Mobile usability** sem erros

## 🔧 Automação e Ferramentas

### **Scripts de Validação:**
- `scripts/verificar-seo-multilingual.sh` - Verificação completa
- `scripts/validar-nova-pagina.sh` - Para novas implementações
- `scripts/gerar-sitemap.sh` - Atualização automática sitemap

### **Templates Prontos:**
- `.seo-templates/` - Templates para novas views
- `.seo-templates/controller-routes.txt` - Rotas padrão
- `.seo-templates/meta-tags.txt` - Meta tags base

## ⚠️ Checklist Final PRÉ-DEPLOY

Antes de fazer deploy de qualquer nova funcionalidade:

- [ ] 3 views criadas (PT/EN/ES)
- [ ] 3 rotas implementadas no controller
- [ ] Meta tags específicas para cada idioma
- [ ] Hreflang mapeamento atualizado
- [ ] Sitemap.xml incluindo novas URLs
- [ ] Conteúdo 100% traduzido (não apenas template)
- [ ] URLs semânticas corretas
- [ ] Breadcrumbs funcionando
- [ ] FAQ com mínimo 5 perguntas
- [ ] 4 benefits e 4 steps implementados
- [ ] Script de validação executado ✅
- [ ] Teste manual das 3 URLs
- [ ] Mobile-friendly verificado

## 🎯 Responsáveis

| Área | Responsável | Validação |
|------|-------------|-----------|
| **Rotas/Controller** | Dev Backend | Compilação + teste manual |
| **Views/Tradução** | Dev Frontend | Validação visual + conteúdo |
| **SEO Técnico** | DevOps/SEO | Scripts + ferramentas Google |
| **Conteúdo** | Content/Marketing | Revisão linguística |

---

## 📞 Suporte

**Dúvidas sobre implementação?**
1. Consulte os templates em `.seo-templates/`
2. Execute `scripts/verificar-seo-multilingual.sh`
3. Teste com URLs de exemplo existentes
4. Documente novos padrões encontrados

---

**Última atualização**: $(date +"%Y-%m-%d")  
**Versão**: 1.0  
**Status**: 🟢 Ativo e obrigatório 