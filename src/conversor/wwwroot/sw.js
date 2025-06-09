// Service Worker para Wepper.vip4.link - SILENT MODE
const CACHE_NAME = 'wepper-v1.0.1';
const STATIC_CACHE = 'wepper-static-v1.1';
const DYNAMIC_CACHE = 'wepper-dynamic-v1.1';

// Recursos para cache imediato - Bootstrap removido, PageSpeed otimizado
const STATIC_FILES = [
    '/',
    '/css/site.css',
    '/css/consent.css',
    '/js/site.js',
    '/js/consent.js',
    '/img/favicon.ico',
    '/img/favicon.svg',
    '/img/apple-touch-icon.png'
    // Bootstrap removido para otimização PageSpeed
    // jQuery e Feather Icons são carregados sob demanda
];

// Instalar Service Worker - SILENT
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then(cache => cache.addAll(STATIC_FILES))
            .then(() => self.skipWaiting())
            .catch(() => {}) // Silent error handling
    );
});

// Ativar Service Worker - SILENT
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    // Remove caches antigos silenciosamente
                    if (cacheName !== STATIC_CACHE && cacheName !== DYNAMIC_CACHE) {
                        return caches.delete(cacheName);
                    }
                })
            );
        }).then(() => self.clients.claim())
    );
});

// Interceptar requisições
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);
    
    // Não fazer cache de:
    // - APIs internas
    // - Uploads de arquivos
    // - SignalR
    if (url.pathname.startsWith('/api/') || 
        url.pathname.startsWith('/progressHub') ||
        url.pathname.includes('/uploads/') ||
        url.pathname.includes('/convertidos/') ||
        request.method !== 'GET') {
        return;
    }
    
    event.respondWith(
        caches.match(request)
            .then(response => {
                // Retorna do cache se disponível (silent)
                if (response) {
                    return response;
                }
                
                // Busca na rede e adiciona ao cache dinâmico
                return fetch(request)
                    .then(fetchResponse => {
                        // Verifica se a resposta é válida
                        if (!fetchResponse || fetchResponse.status !== 200 || fetchResponse.type !== 'basic') {
                            return fetchResponse;
                        }
                        
                        // Clona a resposta para o cache
                        const responseToCache = fetchResponse.clone();
                        
                        caches.open(DYNAMIC_CACHE)
                            .then(cache => {
                                // Limita o cache dinâmico
                                cache.keys().then(keys => {
                                    if (keys.length > 50) {
                                        cache.delete(keys[0]);
                                    }
                                });
                                cache.put(request, responseToCache);
                            });
                        
                        return fetchResponse;
                    })
                    .catch(() => {
                        // Fallback para páginas offline
                        if (request.headers.get('accept').includes('text/html')) {
                            return caches.match('/');
                        }
                    });
            })
    );
});

// Limpar cache quando necessário
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'CLEAR_CACHE') {
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => caches.delete(cacheName))
            );
        }).then(() => {
            event.ports[0].postMessage({ success: true });
        });
    }
});

// Notification para updates
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

// Background sync para melhor UX (quando disponível)
if ('sync' in self.registration) {
    self.addEventListener('sync', event => {
        if (event.tag === 'background-conversion') {
            // Background sync - silent processing
        }
    });
}

// Push notifications (se implementado no futuro)
self.addEventListener('push', event => {
    if (event.data) {
        const data = event.data.json();
        const options = {
            body: data.body,
            icon: '/img/favicon-96x96.png',
            badge: '/img/favicon-96x96.png',
            data: data.url
        };
        
        event.waitUntil(
            self.registration.showNotification(data.title, options)
        );
    }
});

// Manipular cliques em notificações
self.addEventListener('notificationclick', event => {
    event.notification.close();
    
    if (event.notification.data) {
        event.waitUntil(
            clients.openWindow(event.notification.data)
        );
    }
}); 