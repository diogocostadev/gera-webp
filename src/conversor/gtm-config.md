# Configuração do Google Tag Manager para Sistema de Consentimento

## 1. Código Base do GTM (HEAD)
Adicione antes do `</head>`:

```html
<!-- Google Tag Manager -->
<script>(function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({'gtm.start':
new Date().getTime(),event:'gtm.js'});var f=d.getElementsByTagName(s)[0],
j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';j.async=true;j.src=
'https://www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j,f);
})(window,document,'script','dataLayer','GTM-XXXXXXX');</script>
<!-- End Google Tag Manager -->
```

## 2. Código Base do GTM (BODY)
Adicione logo após `<body>`:

```html
<!-- Google Tag Manager (noscript) -->
<noscript><iframe src="https://www.googletagmanager.com/ns.html?id=GTM-XXXXXXX"
height="0" width="0" style="display:none;visibility:hidden"></iframe></noscript>
<!-- End Google Tag Manager (noscript) -->
```

## 3. Configuração de Consentimento no GTM

### A. Criar Template de Consentimento
1. Vá para **Templates** > **Modelos de Tag**
2. Clique em **Novo**
3. Selecione **Consent Mode**

### B. Configurar Triggers Personalizados

#### Trigger 1: Consentimento Analytics Concedido
```javascript
// Trigger Type: Custom Event
// Event name: consent_analytics_granted
// Use este trigger para ativar tags do Google Analytics
```

#### Trigger 2: Consentimento Marketing Concedido
```javascript
// Trigger Type: Custom Event  
// Event name: consent_marketing_granted
// Use este trigger para ativar tags de marketing/remarketing
```

#### Trigger 3: Atualização de Consentimento
```javascript
// Trigger Type: Custom Event
// Event name: consent_update
// Use para rastrear mudanças nas preferências
```

### C. Configurar Tags

#### Tag do Google Analytics
```javascript
// Tag Type: Google Analytics 4
// Configuration Tag: Sua tag de configuração GA4
// Triggering: consent_analytics_granted + All Pages
// Consent Settings:
//   - Analytics Storage: Required
//   - Ad Storage: Not Required
```

#### Tag de Remarketing
```javascript
// Tag Type: Google Ads Remarketing
// Triggering: consent_marketing_granted + All Pages
// Consent Settings:
//   - Analytics Storage: Not Required
//   - Ad Storage: Required
```

## 4. Variáveis de DataLayer

O sistema de consentimento envia automaticamente estas variáveis:

```javascript
{
  'event': 'consent_update',
  'consent_analytics': true/false,
  'consent_marketing': true/false, 
  'consent_performance': true/false
}
```

### Usar nas Tags:
- `{{consent_analytics}}` - Boolean para analytics
- `{{consent_marketing}}` - Boolean para marketing
- `{{consent_performance}}` - Boolean para performance

## 5. Teste do Consentimento

### Verificar no Preview Mode:
1. Ative o Preview Mode no GTM
2. Acesse seu site
3. Verifique se aparecem os eventos:
   - `consent_analytics_granted`
   - `consent_marketing_granted`
   - `consent_update`

### Debug no Console:
```javascript
// Ver estado atual do consentimento
cookieConsent.hasConsent('analytics')
cookieConsent.hasConsent('marketing')

// Resetar consentimento (para teste)
cookieConsent.resetConsent()
```

## 6. Configuração Avançada

### A. Consent Mode para GA4
```javascript
gtag('consent', 'default', {
  'analytics_storage': 'denied',
  'ad_storage': 'denied',
  'wait_for_update': 500
});

gtag('consent', 'update', {
  'analytics_storage': 'granted'
});
```

### B. Triggers Condicionais
Crie triggers que só disparam com consentimento:

```javascript
// Custom JavaScript Variable
function() {
  return cookieConsent && cookieConsent.hasConsent('analytics');
}
```

## 7. Compliance LGPD/GDPR

### Funcionalidades Implementadas:
- ✅ Consentimento granular por categoria
- ✅ Opt-in para cookies não essenciais
- ✅ Remoção automática de cookies rejeitados
- ✅ Interface para alterar preferências
- ✅ Armazenamento local das escolhas
- ✅ Integração com Google Consent Mode

### Categorias de Cookies:
1. **Essenciais** - Sempre ativos (não requer consentimento)
2. **Analíticos** - Google Analytics, métricas
3. **Marketing** - Remarketing, anúncios personalizados  
4. **Performance** - CDN, otimizações

## 8. Eventos para Rastreamento

O sistema dispara automaticamente:

```javascript
// Quando aceita todos os cookies
dataLayer.push({
  'event': 'consent_all_accepted',
  'consent_method': 'accept_all'
});

// Quando rejeita todos
dataLayer.push({
  'event': 'consent_all_rejected', 
  'consent_method': 'reject_all'
});

// Quando configura manualmente
dataLayer.push({
  'event': 'consent_configured',
  'consent_method': 'custom_settings'
});
```

## 9. Implementação no Seu Site

O sistema já está implementado e funcionando automaticamente:

1. **Banner de Consentimento** - Aparece na primeira visita
2. **Modal de Configurações** - Controle granular
3. **Botão de Status** - Sempre visível para reconfigurar
4. **Integração GTM** - Eventos automáticos
5. **Compliance Total** - LGPD e GDPR compliant 