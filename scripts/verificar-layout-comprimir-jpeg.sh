#!/bin/bash

# Script para verificar correções de layout na página comprimir-jpeg
# Verifica se os estilos CSS foram aplicados corretamente

echo "🔍 Verificando correções de layout - Página Comprimir JPEG..."

COMPRIMIR_JPEG_FILE="../src/conversor/Views/Home/CompressorJpeg.cshtml"

# Verificar se o arquivo existe
if [ ! -f "$COMPRIMIR_JPEG_FILE" ]; then
    echo "❌ Erro: Arquivo CompressorJpeg.cshtml não encontrado!"
    echo "📍 Localização esperada: $COMPRIMIR_JPEG_FILE"
    exit 1
fi

echo "📄 Arquivo encontrado: CompressorJpeg.cshtml"

# Verificar se o CSS foi adicionado
echo "🎨 Verificando estilos CSS..."

# Verificar benefits-grid
if grep -q "compression-benefits .benefits-grid" "$COMPRIMIR_JPEG_FILE"; then
    echo "✅ CSS para benefits-grid encontrado"
else
    echo "❌ CSS para benefits-grid NÃO encontrado"
    exit 1
fi

# Verificar steps-container
if grep -q "howto-section .steps-container" "$COMPRIMIR_JPEG_FILE"; then
    echo "✅ CSS para steps-container encontrado"
else
    echo "❌ CSS para steps-container NÃO encontrado"
    exit 1
fi

# Verificar preset-buttons
if grep -q "preset-buttons" "$COMPRIMIR_JPEG_FILE"; then
    echo "✅ CSS para preset-buttons encontrado"
else
    echo "❌ CSS para preset-buttons NÃO encontrado"
    exit 1
fi

# Verificar responsividade
if grep -q "@@media" "$COMPRIMIR_JPEG_FILE"; then
    echo "✅ CSS responsivo encontrado"
else
    echo "❌ CSS responsivo NÃO encontrado"
    exit 1
fi

# Verificar flexbox/grid
if grep -q "display: grid" "$COMPRIMIR_JPEG_FILE"; then
    echo "✅ Layout Grid CSS encontrado"
else
    echo "❌ Layout Grid CSS NÃO encontrado"
    exit 1
fi

if grep -q "display: flex" "$COMPRIMIR_JPEG_FILE"; then
    echo "✅ Layout Flexbox CSS encontrado"
else
    echo "❌ Layout Flexbox CSS NÃO encontrado"
    exit 1
fi

# Verificar heights uniformes
if grep -q "height: 100%" "$COMPRIMIR_JPEG_FILE"; then
    echo "✅ Alturas uniformes para cards encontradas"
else
    echo "❌ Alturas uniformes para cards NÃO encontradas"
    exit 1
fi

# Verificar min-heights
if grep -q "min-height:" "$COMPRIMIR_JPEG_FILE"; then
    echo "✅ Alturas mínimas para cards encontradas"
else
    echo "❌ Alturas mínimas para cards NÃO encontradas"
    exit 1
fi

# Contar número de media queries
MEDIA_COUNT=$(grep -c "@@media" "$COMPRIMIR_JPEG_FILE")
if [ "$MEDIA_COUNT" -ge 3 ]; then
    echo "✅ Media queries responsivas encontradas ($MEDIA_COUNT)"
else
    echo "⚠️  Poucas media queries encontradas ($MEDIA_COUNT), esperado pelo menos 3"
fi

echo ""
echo "📊 Resumo das Verificações:"
echo "=========================="

# Verificar estrutura específica
BENEFITS_STRUCTURE=$(grep -c "compression-benefits .benefit-item" "$COMPRIMIR_JPEG_FILE")
STEPS_STRUCTURE=$(grep -c "howto-section .step-item" "$COMPRIMIR_JPEG_FILE")
PRESET_STRUCTURE=$(grep -c "preset-btn" "$COMPRIMIR_JPEG_FILE")

echo "🏗️  Estruturas encontradas:"
echo "   - Benefits items: $BENEFITS_STRUCTURE regras CSS"
echo "   - Steps items: $STEPS_STRUCTURE regras CSS"
echo "   - Preset buttons: $PRESET_STRUCTURE regras CSS"

# Verificar específicos de layout
GRID_RULES=$(grep -c "grid-template-columns" "$COMPRIMIR_JPEG_FILE")
FLEX_RULES=$(grep -c "flex-direction" "$COMPRIMIR_JPEG_FILE")
ALIGN_RULES=$(grep -c "align-items" "$COMPRIMIR_JPEG_FILE")

echo ""
echo "🎯 Regras de alinhamento:"
echo "   - Grid templates: $GRID_RULES"
echo "   - Flex directions: $FLEX_RULES" 
echo "   - Align items: $ALIGN_RULES"

# Verificar responsividade detalhada
echo ""
echo "📱 Breakpoints responsivos:"
if grep -q "1024px" "$COMPRIMIR_JPEG_FILE"; then
    echo "   ✅ Desktop (1024px+)"
else
    echo "   ❌ Desktop (1024px+)"
fi

if grep -q "768px" "$COMPRIMIR_JPEG_FILE"; then
    echo "   ✅ Tablet (768px)"
else
    echo "   ❌ Tablet (768px)"
fi

if grep -q "480px" "$COMPRIMIR_JPEG_FILE"; then
    echo "   ✅ Mobile (480px)"
else
    echo "   ❌ Mobile (480px)"
fi

echo ""
echo "✅ Verificação concluída com sucesso!"
echo "📖 Consulte docs/CORRECOES_LAYOUT_COMPRIMIR_JPEG.md para detalhes"
echo ""
echo "🚀 Para testar:"
echo "   1. Inicie a aplicação: dotnet run"
echo "   2. Acesse: http://localhost:5169/comprimir-jpeg"
echo "   3. Verifique o alinhamento dos cards e responsividade" 