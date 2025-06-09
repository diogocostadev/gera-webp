// Service Worker para Wepper.vip4.link
const CACHE_NAME = 'wepper-v1.0.0';
const STATIC_CACHE = 'wepper-static-v1';
const DYNAMIC_CACHE = 'wepper-dynamic-v1';

// Recursos para cache imediato
const STATIC_FILES = [
    '/',
    '/css/site.css',
    '/css/consent.css',
    '/js/site.js',
    '/js/consent.js',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    '/lib/jquery/dist/jquery.min.js',
    '/img/favicon.ico',
    '/img/favicon.svg',
    '/img/apple-touch-icon.png',
    'https://unpkg.com/feather-icons@latest/dist/feather.min.js'
];

// Instalar Service Worker
self.addEventListener('install', event => {
    console.log('Service Worker: Instalando...');
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then(cache => {
                console.log('Service Worker: Cache aberto');
                return cache.addAll(STATIC_FILES);
            })
            .then(() => {
                console.log('Service Worker: Recursos em cache');
                return self.skipWaiting();
            })
            .catch(err => {
                console.log('Service Worker: Erro no cache', err);
            })
    );
});

// Ativar Service Worker
self.addEventListener('activate', event => {
    console.log('Service Worker: Ativando...');
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    // Remove caches antigos
                    if (cacheName !== STATIC_CACHE && cacheName !== DYNAMIC_CACHE) {
                        console.log('Service Worker: Removendo cache antigo', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        }).then(() => {
            console.log('Service Worker: Ativo');
            return self.clients.claim();
        })
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
                // Retorna do cache se disponível
                if (response) {
                    console.log('Service Worker: Servindo do cache', request.url);
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
            console.log('Service Worker: Background sync para conversões');
            // Implementar lógica de conversão em background se necessário
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