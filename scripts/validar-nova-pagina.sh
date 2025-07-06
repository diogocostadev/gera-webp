#!/bin/bash

# 🌐 Script de Validação SEO Multilíngue - Wepper.app
# Verifica se uma nova página segue todas as regras obrigatórias

set -e

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Contadores
TOTAL_CHECKS=0
PASSED_CHECKS=0
FAILED_CHECKS=0

# Função para mostrar header
show_header() {
    echo -e "${BLUE}=================================="
    echo -e "🌐 VALIDADOR SEO MULTILÍNGUE"
    echo -e "Wepper.app - Nova Página"
    echo -e "==================================${NC}\n"
}

# Função para mostrar check
check_item() {
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✅ $2${NC}"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        echo -e "${RED}❌ $2${NC}"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
    fi
}

# Função para mostrar warning
warning_item() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Função para mostrar resumo final
show_summary() {
    echo -e "\n${BLUE}=================================="
    echo -e "📊 RESUMO DA VALIDAÇÃO"
    echo -e "==================================${NC}"
    echo -e "Total de verificações: ${TOTAL_CHECKS}"
    echo -e "${GREEN}✅ Aprovadas: ${PASSED_CHECKS}${NC}"
    echo -e "${RED}❌ Falharam: ${FAILED_CHECKS}${NC}"
    
    if [ $FAILED_CHECKS -eq 0 ]; then
        echo -e "\n${GREEN}🎉 PARABÉNS! Sua página está 100% conforme os padrões SEO multilíngue!${NC}"
        echo -e "${GREEN}✅ Pode fazer deploy com segurança!${NC}\n"
        return 0
    else
        echo -e "\n${RED}🚫 ATENÇÃO! Sua página NÃO está conforme os padrões.${NC}"
        echo -e "${RED}❌ Corrija os itens falharam antes do deploy!${NC}\n"
        return 1
    fi
}

# Verificar parâmetros
if [ $# -eq 0 ]; then
    echo -e "${RED}❌ Erro: Especifique a funcionalidade para validar${NC}"
    echo -e "Uso: $0 <nome-funcionalidade>"
    echo -e "Exemplo: $0 comprimir-jpeg"
    echo -e "Exemplo: $0 converter-webp"
    exit 1
fi

FUNCIONALIDADE=$1
show_header

echo -e "${BLUE}🔍 Validando funcionalidade: ${FUNCIONALIDADE}${NC}\n"

# ================================
# 1. VERIFICAR VIEWS
# ================================
echo -e "${BLUE}📄 1. VERIFICANDO VIEWS TRADUZIDAS${NC}"

# Mapear funcionalidade para nome real das views
case "$FUNCIONALIDADE" in
    "comprimir-jpeg")
        VIEW_BASE="CompressorJpeg"
        ;;
    "comprimir-png")
        VIEW_BASE="CompressorPng"
        ;;
    *)
        # Para novas funcionalidades, usar o formato padrão
        VIEW_BASE=$(echo "$FUNCIONALIDADE" | sed 's/-//g' | sed 's/\b\w/\U&/g')
        ;;
esac

# Verificar view português
VIEW_PT="src/conversor/Views/Home/${VIEW_BASE}.cshtml"
if [ -f "$VIEW_PT" ]; then
    check_item 0 "View português existe: ${VIEW_PT}"
else
    check_item 1 "View português NÃO existe: ${VIEW_PT}"
fi

# Verificar view inglês  
VIEW_EN="src/conversor/Views/Home/${VIEW_BASE}English.cshtml"
if [ -f "$VIEW_EN" ]; then
    check_item 0 "View inglês existe: ${VIEW_EN}"
else
    check_item 1 "View inglês NÃO existe: ${VIEW_EN}"
fi

# Verificar view espanhol
VIEW_ES="src/conversor/Views/Home/${VIEW_BASE}Spanish.cshtml"
if [ -f "$VIEW_ES" ]; then
    check_item 0 "View espanhol existe: ${VIEW_ES}"
else
    check_item 1 "View espanhol NÃO existe: ${VIEW_ES}"
fi

# ================================
# 2. VERIFICAR ROTAS NO CONTROLLER
# ================================
echo -e "\n${BLUE}🛣️  2. VERIFICANDO ROTAS NO CONTROLLER${NC}"

CONTROLLER="src/conversor/Controllers/HomeController.cs"
if [ -f "$CONTROLLER" ]; then
    # Verificar rota português
    if grep -q "HttpGet.*\"${FUNCIONALIDADE}\"" "$CONTROLLER"; then
        check_item 0 "Rota português encontrada: /${FUNCIONALIDADE}"
    else
        check_item 1 "Rota português NÃO encontrada: /${FUNCIONALIDADE}"
    fi
    
    # Verificar rota inglês
    if grep -q "HttpGet.*\"en/.*\"" "$CONTROLLER" | grep -q "${FUNCIONALIDADE}"; then
        check_item 0 "Rota inglês encontrada"
    else
        check_item 1 "Rota inglês NÃO encontrada"
    fi
    
    # Verificar rota espanhol
    if grep -q "HttpGet.*\"es/.*\"" "$CONTROLLER" | grep -q "${FUNCIONALIDADE}"; then
        check_item 0 "Rota espanhol encontrada"
    else
        check_item 1 "Rota espanhol NÃO encontrada"
    fi
else
    check_item 1 "Controller não encontrado: ${CONTROLLER}"
fi

# ================================
# 3. VERIFICAR META TAGS
# ================================
echo -e "\n${BLUE}🏷️  3. VERIFICANDO META TAGS${NC}"

# Verificar meta tags nas views
for view in "$VIEW_PT" "$VIEW_EN" "$VIEW_ES"; do
    if [ -f "$view" ]; then
        if grep -q "ViewData\[\"Title\"\]" "$view"; then
            check_item 0 "Meta Title encontrado em $(basename $view)"
        else
            check_item 1 "Meta Title NÃO encontrado em $(basename $view)"
        fi
        
        if grep -q "ViewData\[\"Description\"\]" "$view"; then
            check_item 0 "Meta Description encontrado em $(basename $view)"
        else
            check_item 1 "Meta Description NÃO encontrado em $(basename $view)"
        fi
        
        if grep -q "ViewData\[\"Keywords\"\]" "$view"; then
            check_item 0 "Meta Keywords encontrado em $(basename $view)"
        else
            check_item 1 "Meta Keywords NÃO encontrado em $(basename $view)"
        fi
    fi
done

# ================================
# 4. VERIFICAR HREFLANG
# ================================
echo -e "\n${BLUE}🌐 4. VERIFICANDO HREFLANG${NC}"

LAYOUT="src/conversor/Views/Shared/_Layout.cshtml"
if [ -f "$LAYOUT" ]; then
    if grep -q "${FUNCIONALIDADE}" "$LAYOUT"; then
        check_item 0 "Mapeamento hreflang encontrado no Layout"
    else
        check_item 1 "Mapeamento hreflang NÃO encontrado no Layout"
    fi
else
    check_item 1 "Layout não encontrado: ${LAYOUT}"
fi

# ================================
# 5. VERIFICAR SITEMAP
# ================================
echo -e "\n${BLUE}🗺️  5. VERIFICANDO SITEMAP${NC}"

SEO_CONTROLLER="src/conversor/Controllers/SeoController.cs"
if [ -f "$SEO_CONTROLLER" ]; then
    if grep -q "${FUNCIONALIDADE}" "$SEO_CONTROLLER"; then
        check_item 0 "URLs encontradas no Sitemap"
    else
        check_item 1 "URLs NÃO encontradas no Sitemap"
    fi
else
    check_item 1 "SeoController não encontrado: ${SEO_CONTROLLER}"
fi

# ================================
# 6. VERIFICAR ESTRUTURA DE CONTEÚDO
# ================================
echo -e "\n${BLUE}📝 6. VERIFICANDO ESTRUTURA DE CONTEÚDO${NC}"

# Verificar H1 nas views
for view in "$VIEW_PT" "$VIEW_EN" "$VIEW_ES"; do
    if [ -f "$view" ]; then
        if grep -q "<h1>" "$view"; then
            check_item 0 "H1 encontrado em $(basename $view)"
        else
            check_item 1 "H1 NÃO encontrado em $(basename $view)"
        fi
        
        if grep -q "benefits-section" "$view"; then
            check_item 0 "Seção Benefits encontrada em $(basename $view)"
        else
            check_item 1 "Seção Benefits NÃO encontrada em $(basename $view)"
        fi
        
        if grep -q "howto-section" "$view"; then
            check_item 0 "Seção How-to encontrada em $(basename $view)"
        else
            check_item 1 "Seção How-to NÃO encontrada em $(basename $view)"
        fi
        
        if grep -q "faq-section" "$view"; then
            check_item 0 "Seção FAQ encontrada em $(basename $view)"
        else
            check_item 1 "Seção FAQ NÃO encontrada em $(basename $view)"
        fi
    fi
done

# ================================
# 7. VERIFICAR BREADCRUMBS
# ================================
echo -e "\n${BLUE}🧭 7. VERIFICANDO BREADCRUMBS${NC}"

for view in "$VIEW_PT" "$VIEW_EN" "$VIEW_ES"; do
    if [ -f "$view" ]; then
        if grep -q "breadcrumb" "$view"; then
            check_item 0 "Breadcrumbs encontrado em $(basename $view)"
        else
            check_item 1 "Breadcrumbs NÃO encontrado em $(basename $view)"
        fi
    fi
done

# ================================
# 8. VERIFICAÇÕES ADICIONAIS
# ================================
echo -e "\n${BLUE}⚙️  8. VERIFICAÇÕES ADICIONAIS${NC}"

# Verificar se as palavras não estão hardcoded em inglês nas views PT/ES
if [ -f "$VIEW_PT" ]; then
    if grep -qi "click\|download\|select\|upload" "$VIEW_PT"; then
        warning_item "View português pode ter texto em inglês - revisar manualmente"
    fi
fi

if [ -f "$VIEW_ES" ]; then
    if grep -qi "click\|download\|select\|upload" "$VIEW_ES"; then
        warning_item "View espanhol pode ter texto em inglês - revisar manualmente"
    fi
fi

# Verificar se existe duplicação de código
echo -e "\n${BLUE}🔍 ANÁLISE DE QUALIDADE${NC}"
warning_item "Revisar manualmente se o conteúdo está 100% traduzido (não apenas template)"
warning_item "Verificar se as URLs semânticas estão corretas para cada idioma"
warning_item "Testar manualmente as 3 URLs no navegador após deploy"

# Mostrar resumo final
show_summary 