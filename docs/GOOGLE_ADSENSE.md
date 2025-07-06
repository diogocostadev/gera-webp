# Implementação do Google AdSense

## Visão Geral

O template foi desenvolvido com espaços específicos para anúncios do Google AdSense, otimizados para diferentes dispositivos e tamanhos de tela.

## Espaços de Anúncios Disponíveis

### 1. **Desktop (1024px+)**
- **2 Anúncios Laterais**: 300x250 ou 300x600 (um de cada lado do conteúdo principal)
- **1 Anúncio Horizontal**: 728x90 (banner leaderboard)

### 2. **Mobile/Tablet (768px-)**
- **1 Anúncio Horizontal**: 320x50 ou 320x100 (banner mobile)

## Implementação Passo a Passo

### Passo 1: Configurar Google AdSense

1. Acesse [Google AdSense](https://www.google.com/adsense/)
2. Crie uma conta ou faça login
3. Adicione seu site e aguarde aprovação
4. Obtenha seu código de anúncio

### Passo 2: Substituir Placeholders

Localize os elementos com classe `.ad-placeholder` nos arquivos:
- `src/conversor/Views/Home/Index.cshtml`
- `src/conversor/Views/Home/Privacidade.cshtml`

### Passo 3: Código de Exemplo

#### Anúncio Lateral (300x250)
```html
<!-- Substituir .ad-placeholder por: -->
<script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-XXXXXXXXXX"
     crossorigin="anonymous"></script>
<ins class="adsbygoogle"
     style="display:inline-block;width:300px;height:250px"
     data-ad-client="ca-pub-XXXXXXXXXX"
     data-ad-slot="XXXXXXXXXX"></ins>
<script>
     (adsbygoogle = window.adsbygoogle || []).push({});
</script>
```

#### Anúncio Leaderboard (728x90)
```html
<!-- Para desktop -->
<script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-XXXXXXXXXX"
     crossorigin="anonymous"></script>
<ins class="adsbygoogle"
     style="display:inline-block;width:728px;height:90px"
     data-ad-client="ca-pub-XXXXXXXXXX"
     data-ad-slot="XXXXXXXXXX"></ins>
<script>
     (adsbygoogle = window.adsbygoogle || []).push({});
</script>
```

#### Anúncio Mobile (320x50)
```html
<!-- Para mobile -->
<script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-XXXXXXXXXX"
     crossorigin="anonymous"></script>
<ins class="adsbygoogle"
     style="display:inline-block;width:320px;height:50px"
     data-ad-client="ca-pub-XXXXXXXXXX"
     data-ad-slot="XXXXXXXXXX"></ins>
<script>
     (adsbygoogle = window.adsbygoogle || []).push({});
</script>
```

### Passo 4: Anúncios Responsivos (Recomendado)

Para uma implementação mais flexível, use anúncios responsivos:

```html
<script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-XXXXXXXXXX"
     crossorigin="anonymous"></script>
<ins class="adsbygoogle"
     style="display:block"
     data-ad-client="ca-pub-XXXXXXXXXX"
     data-ad-slot="XXXXXXXXXX"
     data-ad-format="auto"
     data-full-width-responsive="true"></ins>
<script>
     (adsbygoogle = window.adsbygoogle || []).push({});
</script>
```

## Localizações dos Anúncios

### Index.cshtml (Página Principal)

1. **Linha ~92**: Anúncio lateral esquerdo (desktop)
2. **Linha ~123**: Anúncio horizontal (mobile/tablet)
3. **Linha ~211**: Anúncio lateral direito (desktop)

### Privacidade.cshtml

1. **Linha ~24**: Anúncio lateral esquerdo (desktop)
2. **Linha ~156**: Anúncio lateral direito (desktop)

## Otimizações de Performance

### 1. Carregamento Assíncrono
Todos os anúncios já estão configurados para carregamento assíncrono.

### 2. Lazy Loading
Adicione lazy loading para anúncios abaixo da dobra:

```javascript
// Adicionar ao final do script
const observerOptions = {
    rootMargin: '100px'
};

const adObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            // Carregar anúncio
            (adsbygoogle = window.adsbygoogle || []).push({});
            adObserver.unobserve(entry.target);
        }
    });
}, observerOptions);

// Observar anúncios
document.querySelectorAll('.adsbygoogle').forEach(ad => {
    adObserver.observe(ad);
});
```

### 3. Fallback para Bloqueadores
Adicione detecção de bloqueadores de anúncios:

```javascript
// Detectar bloqueador de anúncios
function detectAdBlock() {
    const adElement = document.createElement('div');
    adElement.innerHTML = '&nbsp;';
    adElement.className = 'adsbox';
    adElement.style.position = 'absolute';
    adElement.style.left = '-10000px';
    document.body.appendChild(adElement);
    
    setTimeout(() => {
        if (adElement.offsetHeight === 0) {
            // AdBlock detectado - mostrar mensagem amigável
            console.log('AdBlock detectado');
        }
        document.body.removeChild(adElement);
    }, 100);
}
```

## Configurações de CSS

Os espaços de anúncios já estão estilizados com:

- **Bordas suaves**: `border-radius: var(--radius-lg)`
- **Sombras elegantes**: `box-shadow: var(--shadow-md)`
- **Responsividade**: Grid layout automático
- **Placeholders visuais**: Facilitam desenvolvimento

## Monetização Estimada

### Fatores que Influenciam:
- **Tráfico**: Mais visitas = mais receita
- **Nicho**: Tecnologia/ferramentas tem bom CPM
- **Localização**: Usuários do Brasil, EUA, Europa
- **Posicionamento**: Anúncios "above the fold" rendem mais

### Estimativas (aproximadas):
- **1.000 visitas/mês**: $5-15 USD
- **10.000 visitas/mês**: $50-150 USD
- **100.000 visitas/mês**: $500-1500 USD

## Políticas do AdSense

### ✅ Permitido:
- Ferramenta de conversão de imagens
- Processamento de arquivos
- Conteúdo original e útil

### ❌ Evitar:
- Cliques próprios nos anúncios
- Incentivar cliques
- Conteúdo adulto ou violento

## Monitoramento

### Métricas Importantes:
- **CTR (Click-Through Rate)**: 1-3% é normal
- **CPC (Cost Per Click)**: Varia por região
- **RPM (Revenue Per Mille)**: Receita por 1000 visualizações
- **Viewability**: % de anúncios vistos

### Ferramentas:
- **Google AdSense Console**: Relatórios detalhados
- **Google Analytics**: Integração com AdSense
- **PageSpeed Insights**: Monitorar performance

## Configuração do Arquivo ads.txt

### Importância
O arquivo `ads.txt` é obrigatório para evitar problemas de monetização no Google AdSense. Sem ele, você pode receber avisos como "Ganhos em risco" no console do AdSense.

### Localização
O arquivo foi criado em: `src/conversor/wwwroot/ads.txt`

### Configuração
✅ **CONCLUÍDO**: O arquivo já foi configurado com seu Publisher ID real
1. **Publisher ID**: `pub-4599273059062403`
2. **Configuração atual**: `google.com, pub-4599273059062403, DIRECT, f08c47fec0942fa0`
3. **Verificação**: Após deploy, acesse `https://wepper.app/ads.txt`

### Documentação Completa
Consulte `docs/CONFIGURACAO_ADS_TXT.md` para instruções detalhadas.

## Próximos Passos

1. ✅ Solicitar aprovação do Google AdSense
2. ✅ **Configurar arquivo ads.txt** (CONCLUÍDO)
3. 🚀 **Fazer deploy da aplicação** (PRÓXIMO)
4. ✅ Substituir placeholders pelos códigos reais
5. ✅ Testar em diferentes dispositivos
6. ✅ Monitorar performance e ajustar posições
7. ✅ Otimizar baseado nos relatórios

## Suporte

Para dúvidas específicas sobre implementação:
- **Google AdSense Help**: [support.google.com/adsense](https://support.google.com/adsense)
- **Políticas**: [support.google.com/adsense/answer/48182](https://support.google.com/adsense/answer/48182)

---

**Nota**: Substitua `ca-pub-XXXXXXXXXX` e `XXXXXXXXXX` pelos seus códigos reais do AdSense. 