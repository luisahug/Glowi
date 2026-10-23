// Service worker do Glowi.
// As páginas são dinâmicas e dependem do usuário logado, então nunca são cacheadas:
// navegações vão sempre para a rede e, se estiver offline, mostram /offline.html.
// Arquivos estáticos (css, js, lib, ícones) usam stale-while-revalidate.

// Altere a versão para forçar a limpeza do cache antigo.
const CACHE = 'glowi-v1';
const OFFLINE_URL = '/offline.html';

const PRECACHE = [
    OFFLINE_URL,
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/icons/icon-192.png'
];

const STATIC_PATHS = ['/lib/', '/css/', '/js/', '/icons/', '/favicon.ico', '/Glowi.styles.css'];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE)
            .then(cache => cache.addAll(PRECACHE))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys()
            .then(keys => Promise.all(keys.filter(k => k !== CACHE).map(k => caches.delete(k))))
            .then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', event => {
    const request = event.request;
    if (request.method !== 'GET') return;

    const url = new URL(request.url);
    if (url.origin !== self.location.origin) return;

    if (request.mode === 'navigate') {
        event.respondWith(
            fetch(request).catch(() => caches.match(OFFLINE_URL))
        );
        return;
    }

    if (STATIC_PATHS.some(p => url.pathname.startsWith(p))) {
        event.respondWith(
            caches.open(CACHE).then(cache =>
                cache.match(request).then(cached => {
                    const network = fetch(request)
                        .then(response => {
                            if (response.ok) cache.put(request, response.clone());
                            return response;
                        })
                        .catch(() => cached);
                    return cached || network;
                })
            )
        );
    }
});
