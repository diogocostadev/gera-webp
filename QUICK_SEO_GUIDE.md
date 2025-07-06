# ⚡ Guia Rápido SEO Multilíngue - Wepper.app

> **🔴 OBRIGATÓRIO** - Consulte antes de implementar qualquer nova funcionalidade

## 🚀 Processo em 5 Passos

### 1️⃣ **Usar Templates**
```bash
# Copie os templates para sua nova funcionalidade
cp .seo-templates/view-template-portuguese.cshtml src/conversor/Views/Home/SuaFuncionalidade.cshtml
cp .seo-templates/view-template-english.cshtml src/conversor/Views/Home/SuaFuncionalidadeEnglish.cshtml
cp .seo-templates/view-template-spanish.cshtml src/conversor/Views/Home/SuaFuncionalidadeSpanish.cshtml
```

### 2️⃣ **Implementar Rotas**
```csharp
// HomeController.cs - SEMPRE as 3 rotas:
[HttpGet("sua-funcionalidade")]
public IActionResult SuaFuncionalidade() { SetCultureContent("pt-br"); return View(); }

[HttpGet("en/your-feature")]
public IActionResult SuaFuncionalidadeEnglish() { SetCultureContent("en"); return View(); }

[HttpGet("es/su-funcionalidad")]
public IActionResult SuaFuncionalidadeSpanish() { SetCultureContent("es"); return View(); }
```

### 3️⃣ **Personalizar Views**
```html
<!-- Substitua nos 3 templates: -->
[ACAO] → "Comprimir", "Converter", "Redimensionar"
[FORMATO] → "JPEG", "PNG", "WebP" 
[BENEFICIO_PRINCIPAL] → benefício específico da ferramenta
[FERRAMENTA_NOME] → nome da ferramenta
<!-- ... demais placeholders -->
```

### 4️⃣ **Atualizar Configurações**
```csharp
// _Layout.cshtml - Adicionar no urlMappings:
["/sua-funcionalidade"] = new() { 
    ["pt-br"] = "/sua-funcionalidade", 
    ["en"] = "/en/your-feature", 
    ["es"] = "/es/su-funcionalidad" 
}

// SeoController.cs - Adicionar no sitemap:
new { Url = "/sua-funcionalidade", Priority = "0.9", ChangeFreq = "weekly" },
new { Url = "/en/your-feature", Priority = "0.9", ChangeFreq = "weekly" },
new { Url = "/es/su-funcionalidad", Priority = "0.9", ChangeFreq = "weekly" }
```

### 5️⃣ **Validar**
```bash
# OBRIGATÓRIO antes de commit/deploy
./scripts/validar-nova-pagina.sh sua-funcionalidade
```

---

## 📝 Templates de Meta Tags OBRIGATÓRIAS

### **Compressão:**
```html
<!-- PT --> "Comprimir [Formato] Online Grátis - Reduzir Tamanho Sem Perder Qualidade"
<!-- EN --> "Compress [Format] Online Free - Reduce Size Maintaining Quality"  
<!-- ES --> "Comprimir [Formato] Online Gratis - Reducir Tamaño Sin Perder Calidad"
```

### **Conversão:**
```html
<!-- PT --> "Converter [De] para [Para] Online - Grátis e Rápido"
<!-- EN --> "Convert [From] to [To] Online Free - Fast & Secure"
<!-- ES --> "Convertir [De] a [Para] Online Gratis - Rápido y Seguro"
```

### **Redimensionamento:**
```html
<!-- PT --> "Redimensionar [Tipo] Online Grátis - Alterar Tamanho Sem Perder Qualidade"
<!-- EN --> "Resize [Type] Online Free - Change Size Maintaining Quality"
<!-- ES --> "Redimensionar [Tipo] Online Gratis - Cambiar Tamaño Sin Perder Calidad"
```

---

## 🎯 URLs Semânticas Corretas

```
✅ PORTUGUÊS: verbo-substantivo
   /comprimir-jpeg, /converter-png-webp, /redimensionar-imagem

✅ INGLÊS: verb-noun
   /en/compress-jpeg, /en/convert-png-webp, /en/resize-image

✅ ESPANHOL: verbo-sustantivo  
   /es/comprimir-jpeg, /es/convertir-png-webp, /es/redimensionar-imagen
```

---

## ✅ Checklist Validação Rápida

**Antes de cada commit:**
- [ ] 3 views criadas (PT/EN/ES)
- [ ] 3 rotas implementadas  
- [ ] Meta tags específicas por idioma
- [ ] Hreflang atualizado
- [ ] Sitemap atualizado
- [ ] Script validação executado ✅
- [ ] Conteúdo 100% traduzido

**Antes de cada deploy:**
- [ ] Teste manual das 3 URLs
- [ ] Mobile-friendly verificado
- [ ] Performance < 3 segundos

---

## 🚨 Regras OBRIGATÓRIAS

### **NUNCA:**
❌ Deixar texto em inglês nas views PT/ES  
❌ Usar URLs genéricas (/page1, /feature-x)  
❌ Esquecer meta tags específicas  
❌ Fazer deploy sem validação  

### **SEMPRE:**
✅ Usar templates como base  
✅ Implementar 3 idiomas completos  
✅ Seguir padrões de nomenclatura  
✅ Validar antes de commit/deploy  

---

## 📁 Arquivos para Consulta

- **`SEO_STANDARDS.md`** - Documentação completa
- **`.seo-templates/`** - Templates prontos
- **`scripts/validar-nova-pagina.sh`** - Validação automática
- **`.seo-checklist.yml`** - Configurações estruturadas

---

## 🆘 Comandos de Emergência

```bash
# Script não executa?
chmod +x scripts/*.sh

# Ver templates disponíveis
ls -la .seo-templates/

# Validação completa do sistema
./scripts/verificar-seo-multilingual.sh

# Ver regras completas
cat SEO_STANDARDS.md

# Debug do script
bash -x scripts/validar-nova-pagina.sh minha-funcionalidade
```

---

**📌 Dica:** Mantenha este arquivo sempre aberto durante o desenvolvimento!

**⚡ Lembre-se:** O script de validação é seu melhor amigo - use sempre antes de commit! 