/**
 * Sistema de Consentimento de Cookies - LGPD/GDPR
 * Compatível com Google Analytics, Google Tag Manager e outras ferramentas
 */
class CookieConsentManager {
    constructor() {
        this.consentKey = 'gera_webp_cookie_consent';
        this.consentVersion = '1.0';
        this.categories = {
            essential: {
                name: 'Essenciais',
                description: 'Cookies necessários para o funcionamento básico do site',
                required: true,
                enabled: true,
                cookies: ['sessão', 'preferências de idioma', 'segurança']
            },
            analytics: {
                name: 'Analíticos',
                description: 'Nos ajudam a entender como você usa nosso site',
                required: false,
                enabled: false,
                cookies: ['Google Analytics', '_ga', '_ga_*', '_gid']
            },
            marketing: {
                name: 'Marketing',
                description: 'Usados para mostrar anúncios relevantes',
                required: false,
                enabled: false,
                cookies: ['Google Ads', 'Facebook Pixel', 'remarketing']
            },
            performance: {
                name: 'Performance',
                description: 'Melhoram a velocidade e funcionalidade do site',
                required: false,
                enabled: false,
                cookies: ['CDN', 'cache', 'otimização']
            }
        };
        
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
        const banner = document.createElement('div');
        banner.id = 'cookieConsentBanner';
        banner.className = 'cookie-consent-banner';
        banner.innerHTML = `
            <div class="cookie-consent-content">
                <div class="cookie-consent-text">
                    <h4>
                        <i data-feather="shield"></i>
                        Respeito à sua Privacidade
                    </h4>
                    <p>
                        Usamos cookies para melhorar sua experiência, analisar o tráfego e personalizar conteúdo. 
                        Você pode escolher quais cookies aceitar. Consulte nossa 
                        <a href="/privacidade" target="_blank" style="color: #4CAF50; text-decoration: underline;">Política de Privacidade</a>.
                    </p>
                </div>
                <div class="cookie-consent-actions">
                    <button class="consent-btn consent-btn-reject" onclick="cookieConsent.rejectAll()">
                        <i data-feather="x"></i>
                        Rejeitar
                    </button>
                    <button class="consent-btn consent-btn-settings" onclick="cookieConsent.openSettings()">
                        <i data-feather="settings"></i>
                        Configurar
                    </button>
                    <button class="consent-btn consent-btn-accept" onclick="cookieConsent.acceptAll()">
                        <i data-feather="check"></i>
                        Aceitar Todos
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(banner);
    }

    createModalHTML() {
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
                    <div class="cookie-category-header" onclick="cookieConsent.toggleCategory('${key}')">
                        <div class="cookie-category-title">
                            <i data-feather="${this.getCategoryIcon(key)}"></i>
                            <h4>${category.name}</h4>
                            ${category.required ? '<span style="color: #4CAF50; font-size: 12px;">(Obrigatório)</span>' : ''}
                        </div>
                        <div class="cookie-toggle ${toggleClass} ${disabledClass}" data-category="${key}">
                        </div>
                    </div>
                    <div class="cookie-category-content">
                        <p>${category.description}</p>
                        <p><strong>Cookies utilizados:</strong></p>
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
                        Configurações de Privacidade
                    </h3>
                    <p>
                        Controle quais cookies você permite. Os cookies essenciais são necessários 
                        para o funcionamento básico do site e não podem ser desabilitados.
                    </p>
                </div>
                <div class="cookie-settings-body">
                    ${categoriesHTML}
                </div>
                <div class="cookie-settings-footer">
                    <button class="consent-btn consent-btn-reject" onclick="cookieConsent.rejectAll()">
                        <i data-feather="x"></i>
                        Rejeitar Tudo
                    </button>
                    <button class="consent-btn consent-btn-settings" onclick="cookieConsent.closeSettings()">
                        <i data-feather="x-circle"></i>
                        Cancelar
                    </button>
                    <button class="consent-btn consent-btn-accept" onclick="cookieConsent.saveSettings()">
                        <i data-feather="save"></i>
                        Salvar Preferências
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    }

    createStatusHTML() {
        const status = document.createElement('div');
        status.id = 'consentStatus';
        status.className = 'consent-status';
        status.innerHTML = `
            <i data-feather="settings"></i>
            Cookies
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
            if (typeof feather !== 'undefined') {
                feather.replace();
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
        if (typeof feather !== 'undefined') {
            feather.replace();
        }
    }

    openSettings() {
        const modal = document.getElementById('cookieSettingsModal');
        modal.classList.add('show');
        this.updateToggleStates();
        if (typeof feather !== 'undefined') {
            feather.replace();
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
        
        console.log('Analytics habilitado');
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
        
        console.log('Analytics desabilitado');
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
        
        console.log('Marketing habilitado');
    }

    disableMarketing() {
        if (typeof gtag !== 'undefined') {
            gtag('consent', 'update', {
                'ad_storage': 'denied'
            });
        }
        
        // Limpar cookies de marketing
        this.deleteCookies(['_fbp', '_fbc', 'fr']);
        
        console.log('Marketing desabilitado');
    }

    enablePerformance() {
        console.log('Performance habilitado');
    }

    disablePerformance() {
        console.log('Performance desabilitado');
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
    
    console.log('Sistema de Consentimento inicializado');
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