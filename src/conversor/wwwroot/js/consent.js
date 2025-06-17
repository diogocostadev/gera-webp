/**
 * Sistema de Consentimento de Cookies - LGPD/GDPR
 * Compatível com Google Analytics, Google Tag Manager e outras ferramentas
 */
class CookieConsentManager {
    constructor() {
        this.consentKey = 'gera_webp_cookie_consent';
        this.consentVersion = '1.0';

        // Detectar idioma atual (pt, en, es)
        this.lang = (document.documentElement.lang || 'pt').substring(0, 2);

        // Textos traduzidos
        this.translations = {
            pt: {
                bannerTitle: 'Respeito à sua Privacidade',
                bannerMessage: 'Usamos cookies para melhorar sua experiência, analisar o tráfego e personalizar conteúdo. Você pode escolher quais cookies aceitar. Consulte nossa',
                policyText: 'Política de Privacidade',
                policyUrl: '/privacidade',
                rejectBtn: 'Rejeitar',
                settingsBtn: 'Configurar',
                acceptBtn: 'Aceitar Todos',
                modalTitle: 'Configurações de Privacidade',
                modalIntro: 'Controle quais cookies você permite. Os cookies essenciais são necessários para o funcionamento básico do site e não podem ser desabilitados.',
                rejectAllBtn: 'Rejeitar Tudo',
                cancelBtn: 'Cancelar',
                saveBtn: 'Salvar Preferências',
                requiredTag: '(Obrigatório)',
                cookiesUsedLabel: 'Cookies utilizados:',
                statusLabel: 'Cookies'
            },
            en: {
                bannerTitle: 'Respecting Your Privacy',
                bannerMessage: 'We use cookies to improve your experience, analyze traffic and personalize content. You can choose which cookies to accept. See our',
                policyText: 'Privacy Policy',
                policyUrl: '/en/privacy',
                rejectBtn: 'Reject',
                settingsBtn: 'Settings',
                acceptBtn: 'Accept All',
                modalTitle: 'Privacy Settings',
                modalIntro: 'Control which cookies you allow. Essential cookies are required for basic site functionality and cannot be disabled.',
                rejectAllBtn: 'Reject All',
                cancelBtn: 'Cancel',
                saveBtn: 'Save Preferences',
                requiredTag: '(Required)',
                cookiesUsedLabel: 'Used cookies:',
                statusLabel: 'Cookies'
            },
            es: {
                bannerTitle: 'Respeto a tu Privacidad',
                bannerMessage: 'Usamos cookies para mejorar tu experiencia, analizar el tráfico y personalizar el contenido. Puedes elegir qué cookies aceptar. Consulta nuestra',
                policyText: 'Política de Privacidad',
                policyUrl: '/es/privacidad',
                rejectBtn: 'Rechazar',
                settingsBtn: 'Configurar',
                acceptBtn: 'Aceptar Todo',
                modalTitle: 'Configuración de Privacidad',
                modalIntro: 'Controla qué cookies permites. Las cookies esenciales son necesarias para el funcionamiento básico del sitio y no pueden deshabilitarse.',
                rejectAllBtn: 'Rechazar Todo',
                cancelBtn: 'Cancelar',
                saveBtn: 'Guardar Preferencias',
                requiredTag: '(Obligatorio)',
                cookiesUsedLabel: 'Cookies utilizados:',
                statusLabel: 'Cookies'
            }
        };

        // Categorias base
        const baseCategories = {
            essential: {
                required: true,
                enabled: true,
                cookies: ['sessão', 'preferências de idioma', 'segurança']
            },
            analytics: {
                required: false,
                enabled: false,
                cookies: ['Google Analytics', '_ga', '_ga_*', '_gid']
            },
            marketing: {
                required: false,
                enabled: false,
                cookies: ['Google Ads', 'Facebook Pixel', 'remarketing']
            },
            performance: {
                required: false,
                enabled: false,
                cookies: ['CDN', 'cache', 'otimização']
            }
        };

        // Textos das categorias por idioma
        const categoryTexts = {
            pt: {
                essential: { name: 'Essenciais', description: 'Cookies necessários para o funcionamento básico do site' },
                analytics: { name: 'Analíticos', description: 'Nos ajudam a entender como você usa nosso site' },
                marketing: { name: 'Marketing', description: 'Usados para mostrar anúncios relevantes' },
                performance: { name: 'Performance', description: 'Melhoram a velocidade e funcionalidade do site' }
            },
            en: {
                essential: { name: 'Essential', description: 'Cookies necessary for basic site functionality' },
                analytics: { name: 'Analytics', description: 'Help us understand how you use our site' },
                marketing: { name: 'Marketing', description: 'Used to display relevant ads' },
                performance: { name: 'Performance', description: 'Improve site speed and functionality' }
            },
            es: {
                essential: { name: 'Esenciales', description: 'Cookies necesarios para el funcionamiento básico del sitio' },
                analytics: { name: 'Analíticos', description: 'Nos ayudan a entender cómo utilizas nuestro sitio' },
                marketing: { name: 'Marketing', description: 'Usados para mostrar anuncios relevantes' },
                performance: { name: 'Rendimiento', description: 'Mejoran la velocidad y funcionalidad del sitio' }
            }
        };

        // Montar categorias combinando base e textos traduzidos
        this.categories = {};
        Object.keys(baseCategories).forEach(key => {
            this.categories[key] = Object.assign({}, baseCategories[key], categoryTexts[this.lang][key]);
        });
        
        this.init();
    }

    init() {
        this.createBannerHTML();
        this.createModalHTML();
        this.createStatusHTML();
        this.attachEventListeners();
        this.checkConsentStatus();
    }

    createBannerHTML() {
        const t = this.translations[this.lang] || this.translations.pt;
        const banner = document.createElement('div');
        banner.id = 'cookieConsentBanner';
        banner.className = 'cookie-consent-banner';
        banner.innerHTML = `
            <div class="cookie-consent-content">
                <div class="cookie-consent-text">
                    <h4>
                        <i data-feather="shield"></i>
                        ${t.bannerTitle}
                    </h4>
                    <p>
                        ${t.bannerMessage}
                        <a href="${t.policyUrl}" target="_blank" style="color: #4CAF50; text-decoration: underline;">${t.policyText}</a>.
                    </p>
                </div>
                <div class="cookie-consent-actions">
                    <button class="consent-btn consent-btn-reject" data-action="rejectAll">
                        <i data-feather="x"></i>
                        ${t.rejectBtn}
                    </button>
                    <button class="consent-btn consent-btn-settings" data-action="openSettings">
                        <i data-feather="settings"></i>
                        ${t.settingsBtn}
                    </button>
                    <button class="consent-btn consent-btn-accept" data-action="acceptAll">
                        <i data-feather="check"></i>
                        ${t.acceptBtn}
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(banner);
    }

    createModalHTML() {
        const t = this.translations[this.lang] || this.translations.pt;
        const modal = document.createElement('div');
        modal.id = 'cookieSettingsModal';
        modal.className = 'cookie-settings-modal';
        
        let categoriesHTML = '';
        Object.keys(this.categories).forEach(key => {
            const category = this.categories[key];
            const toggleClass = category.enabled ? 'active' : '';
            const disabledClass = category.required ? 'disabled' : '';
            
            categoriesHTML += `
                <div class="cookie-category">
                    <div class="cookie-category-header" data-action="toggleCategory" data-key="${key}">
                        <div class="cookie-category-title">
                            <i data-feather="${this.getCategoryIcon(key)}"></i>
                            <h4>${category.name}</h4>
                            ${category.required ? `<span style="color: #4CAF50; font-size: 12px;">${t.requiredTag}</span>` : ''}
                        </div>
                        <div class="cookie-toggle ${toggleClass} ${disabledClass}" data-category="${key}">
                        </div>
                    </div>
                    <div class="cookie-category-content">
                        <p>${category.description}</p>
                        <p><strong>${t.cookiesUsedLabel}</strong></p>
                        <ul>
                            ${category.cookies.map(cookie => `<li>${cookie}</li>`).join('')}
                        </ul>
                    </div>
                </div>
            `;
        });

        modal.innerHTML = `
            <div class="cookie-settings-content">
                <div class="cookie-settings-header">
                    <h3>
                        <i data-feather="shield"></i>
                        ${t.modalTitle}
                    </h3>
                    <p>
                        ${t.modalIntro}
                    </p>
                </div>
                <div class="cookie-settings-body">
                    ${categoriesHTML}
                </div>
                <div class="cookie-settings-footer">
                    <button class="consent-btn consent-btn-reject" data-action="rejectAll">
                        <i data-feather="x"></i>
                        ${t.rejectAllBtn}
                    </button>
                    <button class="consent-btn consent-btn-settings" data-action="closeSettings">
                        <i data-feather="x-circle"></i>
                        ${t.cancelBtn}
                    </button>
                    <button class="consent-btn consent-btn-accept" data-action="saveSettings">
                        <i data-feather="save"></i>
                        ${t.saveBtn}
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    }

    createStatusHTML() {
        const t = this.translations[this.lang] || this.translations.pt;
        const status = document.createElement('div');
        status.id = 'consentStatus';
        status.className = 'consent-status';
        status.innerHTML = `
            <i data-feather="settings"></i>
            ${t.statusLabel}
        `;
        status.onclick = () => this.openSettings();
        document.body.appendChild(status);
    }

    getCategoryIcon(category) {
        const icons = {
            essential: 'shield',
            analytics: 'bar-chart-2',
            marketing: 'target',
            performance: 'zap'
        };
        return icons[category] || 'circle';
    }

    attachEventListeners() {
        // Event delegation para evitar problemas de sintaxe com onclick
        document.addEventListener('click', (e) => {
            const target = e.target.closest('[data-action]');
            if (!target) return;
            
            const action = target.dataset.action;
            
            try {
                switch(action) {
                    case 'acceptAll':
                        this.acceptAll();
                        break;
                    case 'rejectAll':
                        this.rejectAll();
                        break;
                    case 'openSettings':
                        this.openSettings();
                        break;
                    case 'closeSettings':
                        this.closeSettings();
                        break;
                    case 'saveSettings':
                        this.saveSettings();
                        break;
                    case 'toggleCategory':
                        const key = target.dataset.key;
                        if (key) this.toggleCategory(key);
                        break;
                }
            } catch(err) {
                // Silent fail - prevent console errors
            }
        });

        // Fechar modal clicando no fundo
        document.getElementById('cookieSettingsModal').addEventListener('click', (e) => {
            if (e.target.id === 'cookieSettingsModal') {
                this.closeSettings();
            }
        });

        // ESC para fechar modal
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.closeSettings();
            }
        });
    }

    checkConsentStatus() {
        const consent = this.getConsent();
        
        if (!consent || consent.version !== this.consentVersion) {
            this.showBanner();
        } else {
            this.applyConsent(consent);
            this.showStatus();
        }
    }

    showBanner() {
        setTimeout(() => {
            const banner = document.getElementById('cookieConsentBanner');
            banner.classList.add('show');
            // Use safe feather replacement
            if (window.feather && typeof window.feather.replace === 'function') {
                window.feather.replace();
            }
        }, 1000);
    }

    hideBanner() {
        const banner = document.getElementById('cookieConsentBanner');
        banner.classList.remove('show');
    }

    showStatus() {
        const status = document.getElementById('consentStatus');
        status.classList.add('show');
        // Use safe feather replacement
        if (window.feather && typeof window.feather.replace === 'function') {
            window.feather.replace();
        }
    }

    openSettings() {
        const modal = document.getElementById('cookieSettingsModal');
        modal.classList.add('show');
        this.updateToggleStates();
        // Use safe feather replacement
        if (window.feather && typeof window.feather.replace === 'function') {
            window.feather.replace();
        }
    }

    closeSettings() {
        const modal = document.getElementById('cookieSettingsModal');
        modal.classList.remove('show');
    }

    toggleCategory(categoryKey) {
        const category = this.categories[categoryKey];
        if (category.required) return;
        
        category.enabled = !category.enabled;
        this.updateToggleStates();
    }

    updateToggleStates() {
        Object.keys(this.categories).forEach(key => {
            const toggle = document.querySelector(`[data-category="${key}"]`);
            const category = this.categories[key];
            
            if (category.enabled) {
                toggle.classList.add('active');
            } else {
                toggle.classList.remove('active');
            }
        });
    }

    acceptAll() {
        Object.keys(this.categories).forEach(key => {
            this.categories[key].enabled = true;
        });
        this.saveConsent();
    }

    rejectAll() {
        Object.keys(this.categories).forEach(key => {
            const category = this.categories[key];
            category.enabled = category.required;
        });
        this.saveConsent();
    }

    saveSettings() {
        this.saveConsent();
        this.closeSettings();
    }

    saveConsent() {
        const consent = {
            version: this.consentVersion,
            timestamp: new Date().toISOString(),
            categories: {}
        };

        Object.keys(this.categories).forEach(key => {
            consent.categories[key] = this.categories[key].enabled;
        });

        localStorage.setItem(this.consentKey, JSON.stringify(consent));
        this.applyConsent(consent);
        this.hideBanner();
        this.showStatus();
        
        // Trigger consent event
        this.triggerConsentEvent(consent);
    }

    getConsent() {
        try {
            const stored = localStorage.getItem(this.consentKey);
            return stored ? JSON.parse(stored) : null;
        } catch (e) {
            console.error('Erro ao ler consentimento:', e);
            return null;
        }
    }

    applyConsent(consent) {
        // Aplicar configurações de cookies baseadas no consentimento
        if (consent.categories.analytics) {
            this.enableAnalytics();
        } else {
            this.disableAnalytics();
        }

        if (consent.categories.marketing) {
            this.enableMarketing();
        } else {
            this.disableMarketing();
        }

        if (consent.categories.performance) {
            this.enablePerformance();
        } else {
            this.disablePerformance();
        }

        // Atualizar estado interno
        Object.keys(consent.categories).forEach(key => {
            if (this.categories[key]) {
                this.categories[key].enabled = consent.categories[key];
            }
        });
    }

    enableAnalytics() {
        // Google Analytics
        if (typeof gtag !== 'undefined') {
            gtag('consent', 'update', {
                'analytics_storage': 'granted'
            });
        }
        
        // Google Tag Manager
        if (typeof dataLayer !== 'undefined') {
            dataLayer.push({
                'event': 'consent_analytics_granted'
            });
        }
        
        // Analytics enabled - silent
    }

    disableAnalytics() {
        // Google Analytics
        if (typeof gtag !== 'undefined') {
            gtag('consent', 'update', {
                'analytics_storage': 'denied'
            });
        }
        
        // Limpar cookies do Google Analytics
        this.deleteCookies(['_ga', '_ga_*', '_gid', '_gat']);
        
        // Analytics disabled - silent
    }

    enableMarketing() {
        if (typeof gtag !== 'undefined') {
            gtag('consent', 'update', {
                'ad_storage': 'granted'
            });
        }
        
        if (typeof dataLayer !== 'undefined') {
            dataLayer.push({
                'event': 'consent_marketing_granted'
            });
        }
        
        // Marketing enabled - silent
    }

    disableMarketing() {
        if (typeof gtag !== 'undefined') {
            gtag('consent', 'update', {
                'ad_storage': 'denied'
            });
        }
        
        // Limpar cookies de marketing
        this.deleteCookies(['_fbp', '_fbc', 'fr']);
        
        // Marketing disabled - silent
    }

    enablePerformance() {
        // Performance enabled - silent
    }

    disablePerformance() {
        // Performance disabled - silent
    }

    deleteCookies(cookieNames) {
        cookieNames.forEach(name => {
            // Deletar do domínio atual
            document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`;
            // Deletar do domínio pai
            document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/; domain=.${window.location.hostname};`;
        });
    }

    triggerConsentEvent(consent) {
        // Disparar evento customizado para integração com outras ferramentas
        const event = new CustomEvent('cookieConsentChanged', {
            detail: consent
        });
        document.dispatchEvent(event);
    }

    // Método público para verificar se uma categoria está habilitada
    hasConsent(category) {
        const consent = this.getConsent();
        return consent && consent.categories && consent.categories[category];
    }

    // Método para resetar consentimento (útil para desenvolvimento)
    resetConsent() {
        localStorage.removeItem(this.consentKey);
        location.reload();
    }
}

// Inicializar quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function() {
    // Criar instância global
    window.cookieConsent = new CookieConsentManager();
    
    // Configurar Google Analytics com consentimento
    if (typeof gtag !== 'undefined') {
        gtag('consent', 'default', {
            'analytics_storage': 'denied',
            'ad_storage': 'denied',
            'wait_for_update': 500
        });
    }
    
    // Configurar Google Tag Manager com consentimento
    if (typeof dataLayer === 'undefined') {
        window.dataLayer = window.dataLayer || [];
    }
    
    // Sistema de Consentimento inicializado - silent
});

// Função para integração com Google Tag Manager
function updateGTMConsent(consentObject) {
    if (typeof dataLayer !== 'undefined') {
        dataLayer.push({
            'event': 'consent_update',
            'consent_analytics': consentObject.categories.analytics,
            'consent_marketing': consentObject.categories.marketing,
            'consent_performance': consentObject.categories.performance
        });
    }
}

// Listener para mudanças de consentimento
document.addEventListener('cookieConsentChanged', function(e) {
    updateGTMConsent(e.detail);
});
