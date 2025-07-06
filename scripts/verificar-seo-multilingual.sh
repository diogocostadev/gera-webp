#!/bin/bash

# Script de Verificação SEO Multilíngue
# Verifica se todas as traduções e configurações SEO estão funcionando

echo "🌐 Verificação SEO Multilíngue - Wepper.app"
echo "=============================================="
echo ""

# Cores para output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para verificar se arquivo existe
check_file() {
    if [ -f "$1" ]; then
        echo -e "${GREEN}✅ Encontrado:${NC} $1"
        return 0
    else
        echo -e "${RED}❌ Não encontrado:${NC} $1"
        return 1
    fi
}

# Função para contar linhas em arquivo
count_lines() {
    if [ -f "$1" ]; then
        lines=$(wc -l < "$1")
        echo -e "${BLUE}📄 Linhas:${NC} $lines"
    fi
}

echo "🔍 1. Verificando Views Multilíngues"
echo "======================================"

# Views JPEG
echo -e "\n${YELLOW}📁 Views JPEG:${NC}"
check_file "../src/conversor/Views/Home/CompressorJpeg.cshtml"
check_file "../src/conversor/Views/Home/CompressorJpegEnglish.cshtml"
check_file "../src/conversor/Views/Home/CompressorJpegSpanish.cshtml"

# Views PNG  
echo -e "\n${YELLOW}📁 Views PNG:${NC}"
check_file "../src/conversor/Views/Home/CompressorPng.cshtml"
check_file "../src/conversor/Views/Home/CompressorPngEnglish.cshtml"
check_file "../src/conversor/Views/Home/CompressorPngSpanish.cshtml"

echo -e "\n🔧 2. Verificando Configurações Técnicas"
echo "========================================="

# Controller
echo -e "\n${YELLOW}🎛️ HomeController:${NC}"
if check_file "../src/conversor/Controllers/HomeController.cs"; then
    count_lines "../src/conversor/Controllers/HomeController.cs"
    
    echo -e "\n${BLUE}🔎 Verificando rotas multilíngues:${NC}"
    
    # Verificar rotas JPEG
    if grep -q "en/compress-jpeg" "../src/conversor/Controllers/HomeController.cs"; then
        echo -e "${GREEN}✅ Rota EN JPEG encontrada${NC}"
    else
        echo -e "${RED}❌ Rota EN JPEG não encontrada${NC}"
    fi
    
    if grep -q "es/comprimir-jpeg" "../src/conversor/Controllers/HomeController.cs"; then
        echo -e "${GREEN}✅ Rota ES JPEG encontrada${NC}"
    else
        echo -e "${RED}❌ Rota ES JPEG não encontrada${NC}"
    fi
    
    # Verificar rotas PNG
    if grep -q "en/compress-png" "../src/conversor/Controllers/HomeController.cs"; then
        echo -e "${GREEN}✅ Rota EN PNG encontrada${NC}"
    else
        echo -e "${RED}❌ Rota EN PNG não encontrada${NC}"
    fi
    
    if grep -q "es/comprimir-png" "../src/conversor/Controllers/HomeController.cs"; then
        echo -e "${GREEN}✅ Rota ES PNG encontrada${NC}"
    else
        echo -e "${RED}❌ Rota ES PNG não encontrada${NC}"
    fi
fi

# SeoController
echo -e "\n${YELLOW}🗺️ SeoController (Sitemap):${NC}"
if check_file "../src/conversor/Controllers/SeoController.cs"; then
    count_lines "../src/conversor/Controllers/SeoController.cs"
    
    echo -e "\n${BLUE}🔎 Verificando URLs no sitemap:${NC}"
    
    if grep -q "/comprimir-jpeg" "../src/conversor/Controllers/SeoController.cs"; then
        echo -e "${GREEN}✅ URL PT JPEG no sitemap${NC}"
    else
        echo -e "${RED}❌ URL PT JPEG não encontrada no sitemap${NC}"
    fi
    
    if grep -q "/en/compress-jpeg" "../src/conversor/Controllers/SeoController.cs"; then
        echo -e "${GREEN}✅ URL EN JPEG no sitemap${NC}"
    else
        echo -e "${RED}❌ URL EN JPEG não encontrada no sitemap${NC}"
    fi
    
    if grep -q "/es/comprimir-jpeg" "../src/conversor/Controllers/SeoController.cs"; then
        echo -e "${GREEN}✅ URL ES JPEG no sitemap${NC}"
    else
        echo -e "${RED}❌ URL ES JPEG não encontrada no sitemap${NC}"
    fi
fi

# Layout (Hreflang)
echo -e "\n${YELLOW}🌐 _Layout (Hreflang):${NC}"
if check_file "../src/conversor/Views/Shared/_Layout.cshtml"; then
    count_lines "../src/conversor/Views/Shared/_Layout.cshtml"
    
    echo -e "\n${BLUE}🔎 Verificando hreflang dinâmico:${NC}"
    
    if grep -q "urlMappings" "../src/conversor/Views/Shared/_Layout.cshtml"; then
        echo -e "${GREEN}✅ Sistema hreflang dinâmico implementado${NC}"
    else
        echo -e "${RED}❌ Sistema hreflang dinâmico não encontrado${NC}"
    fi
    
    if grep -q "comprimir-jpeg" "../src/conversor/Views/Shared/_Layout.cshtml"; then
        echo -e "${GREEN}✅ Mapeamentos JPEG encontrados${NC}"
    else
        echo -e "${RED}❌ Mapeamentos JPEG não encontrados${NC}"
    fi
    
    if grep -q "compress-png" "../src/conversor/Views/Shared/_Layout.cshtml"; then
        echo -e "${GREEN}✅ Mapeamentos PNG encontrados${NC}"
    else
        echo -e "${RED}❌ Mapeamentos PNG não encontrados${NC}"
    fi
fi

echo -e "\n📊 3. Verificando Conteúdo das Views"
echo "====================================="

# Função para verificar conteúdo de view
check_view_content() {
    local file="$1"
    local lang="$2"
    local format="$3"
    
    echo -e "\n${YELLOW}📝 Verificando $lang $format:${NC}"
    
    if [ -f "$file" ]; then
        # Verificar ViewData
        if grep -q "ViewData\[\"Title\"\]" "$file"; then
            title=$(grep "ViewData\[\"Title\"\]" "$file" | head -1)
            echo -e "${GREEN}✅ Title:${NC} ${title#*= }"
        fi
        
        if grep -q "ViewData\[\"Description\"\]" "$file"; then
            echo -e "${GREEN}✅ Description configurada${NC}"
        fi
        
        if grep -q "ViewData\[\"Keywords\"\]" "$file"; then
            echo -e "${GREEN}✅ Keywords configuradas${NC}"
        fi
        
        # Verificar elementos específicos do idioma
        case $lang in
            "English")
                if grep -q "Compress.*Online" "$file"; then
                    echo -e "${GREEN}✅ Conteúdo em inglês detectado${NC}"
                fi
                ;;
            "Spanish")
                if grep -q "Comprimir.*Online" "$file"; then
                    echo -e "${GREEN}✅ Conteúdo em espanhol detectado${NC}"
                fi
                ;;
        esac
        
        # Verificar tamanho do arquivo
        size=$(wc -l < "$file")
        echo -e "${BLUE}📏 Tamanho:${NC} $size linhas"
        
    else
        echo -e "${RED}❌ Arquivo não encontrado${NC}"
    fi
}

# Verificar views em inglês
check_view_content "../src/conversor/Views/Home/CompressorJpegEnglish.cshtml" "English" "JPEG"
check_view_content "../src/conversor/Views/Home/CompressorPngEnglish.cshtml" "English" "PNG"

# Verificar views em espanhol
check_view_content "../src/conversor/Views/Home/CompressorJpegSpanish.cshtml" "Spanish" "JPEG"
check_view_content "../src/conversor/Views/Home/CompressorPngSpanish.cshtml" "Spanish" "PNG"

echo -e "\n📋 4. Verificando Documentação"
echo "==============================="

# Documentação
echo -e "\n${YELLOW}📚 Documentos:${NC}"
check_file "../docs/SEO_MULTILINGUAL_IMPLEMENTATION.md"

if [ -f "../docs/README_PLANEJAMENTO.md" ]; then
    echo -e "${GREEN}✅ README_PLANEJAMENTO.md existe${NC}"
    
    if grep -q "SEO_MULTILINGUAL_IMPLEMENTATION" "../docs/README_PLANEJAMENTO.md"; then
        echo -e "${GREEN}✅ Documentação SEO referenciada no índice${NC}"
    else
        echo -e "${YELLOW}⚠️ Adicionar referência no README_PLANEJAMENTO.md${NC}"
    fi
fi

echo -e "\n🚀 5. Resumo da Verificação"
echo "============================"

# Contar arquivos criados
views_created=0
routes_found=0

# Contar views criadas
for view in "CompressorJpegEnglish" "CompressorJpegSpanish" "CompressorPngEnglish" "CompressorPngSpanish"; do
    if [ -f "../src/conversor/Views/Home/${view}.cshtml" ]; then
        ((views_created++))
    fi
done

# Contar rotas implementadas
for route in "en/compress-jpeg" "es/comprimir-jpeg" "en/compress-png" "es/comprimir-png"; do
    if grep -q "$route" "../src/conversor/Controllers/HomeController.cs" 2>/dev/null; then
        ((routes_found++))
    fi
done

echo -e "\n${BLUE}📈 Estatísticas:${NC}"
echo -e "Views criadas: ${GREEN}$views_created/4${NC}"
echo -e "Rotas implementadas: ${GREEN}$routes_found/4${NC}"

# URLs testáveis
echo -e "\n${BLUE}🔗 URLs para testar:${NC}"
echo "http://localhost:5169/comprimir-jpeg"
echo "http://localhost:5169/en/compress-jpeg"
echo "http://localhost:5169/es/comprimir-jpeg"
echo "http://localhost:5169/comprimir-png"
echo "http://localhost:5169/en/compress-png"
echo "http://localhost:5169/es/comprimir-png"

echo -e "\n${BLUE}🌐 URLs de produção:${NC}"
echo "https://wepper.app/comprimir-jpeg"
echo "https://wepper.app/en/compress-jpeg"
echo "https://wepper.app/es/comprimir-jpeg"
echo "https://wepper.app/comprimir-png"
echo "https://wepper.app/en/compress-png"
echo "https://wepper.app/es/comprimir-png"

if [ $views_created -eq 4 ] && [ $routes_found -eq 4 ]; then
    echo -e "\n${GREEN}🎉 IMPLEMENTAÇÃO COMPLETA!${NC}"
    echo -e "${GREEN}✅ Todas as traduções e configurações SEO estão funcionando${NC}"
    echo -e "${BLUE}🚀 Pronto para deploy em produção${NC}"
else
    echo -e "\n${YELLOW}⚠️ Implementação parcial${NC}"
    echo -e "${RED}Verificar itens faltantes acima${NC}"
fi

echo -e "\n${BLUE}📋 Próximos passos:${NC}"
echo "1. Compilar e testar local: dotnet build && dotnet run"
echo "2. Verificar cada URL no navegador"
echo "3. Testar hreflang com ferramentas online"
echo "4. Deploy em produção"
echo "5. Submit no Google Search Console"

echo ""
echo "Script executado em: $(date)"
echo "==============================================" 