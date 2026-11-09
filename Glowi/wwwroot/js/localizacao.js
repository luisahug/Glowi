// Localização da cliente (busca por raio) e endereço da profissional (cadastro/perfil).
// Os cookies glowi_lat/glowi_lng/glowi_local/glowi_raio são lidos no servidor por LocalizacaoCliente.
window.GlowiLocal = (() => {
    const COOKIE = { lat: 'glowi_lat', lng: 'glowi_lng', rotulo: 'glowi_local', raio: 'glowi_raio', auto: 'glowi_geo_auto' };
    const UM_ANO = 60 * 60 * 24 * 365;

    const lerCookie = nome => {
        const item = document.cookie.split('; ').find(c => c.startsWith(nome + '='));
        return item ? decodeURIComponent(item.slice(nome.length + 1)) : null;
    };
    const gravarCookie = (nome, valor, maxAge = UM_ANO) => {
        const seguro = location.protocol === 'https:' ? '; Secure' : '';
        document.cookie = `${nome}=${encodeURIComponent(valor)}; Max-Age=${maxAge}; Path=/; SameSite=Lax${seguro}`;
    };
    const apagarCookie = nome => gravarCookie(nome, '', 0);

    // 4 casas decimais ≈ 11 m: suficiente para a busca e não guarda a posição exata
    const arredondar = n => Math.round(n * 10000) / 10000;

    const obterJson = async url => {
        const resposta = await fetch(url, { headers: { Accept: 'application/json' } });
        const corpo = await resposta.json().catch(() => ({}));
        if (!resposta.ok) throw new Error(corpo.mensagem || 'Não foi possível consultar a localização agora.');
        return corpo;
    };

    const posicaoAtual = () => new Promise((ok, falha) => {
        if (!navigator.geolocation) return falha(new Error('Seu navegador não permite obter a localização.'));
        navigator.geolocation.getCurrentPosition(
            p => ok({ lat: arredondar(p.coords.latitude), lng: arredondar(p.coords.longitude) }),
            e => falha(new Error(e.code === 1
                ? 'Permissão de localização negada. Libere nas configurações do navegador ou digite o CEP.'
                : 'Não conseguimos obter sua localização. Tente de novo ou digite o CEP.')),
            { enableHighAccuracy: false, timeout: 10000, maximumAge: 5 * 60 * 1000 });
    });

    const rotuloDe = async (lat, lng) => {
        try {
            return (await obterJson(`/Localizacao/Reverso?lat=${lat}&lng=${lng}`)).rotulo || 'Sua localização atual';
        } catch {
            return 'Sua localização atual';
        }
    };

    const salvarLocal = ({ lat, lng, rotulo }) => {
        gravarCookie(COOKIE.lat, String(lat));
        gravarCookie(COOKIE.lng, String(lng));
        gravarCookie(COOKIE.rotulo, rotulo);
        apagarCookie(COOKIE.auto);
    };

    // ---------- tela de busca (cliente) ----------
    function iniciarBusca(modal, { automatico = false } = {}) {
        if (!modal) return;

        const status = modal.querySelector('#localStatus');
        const campoBusca = modal.querySelector('#localBusca');
        const campoRaio = modal.querySelector('#localRaio');
        const botaoGps = modal.querySelector('[data-local-gps]');
        const botaoBuscar = modal.querySelector('[data-local-buscar]');

        let escolhido = modal.dataset.localLat
            ? { lat: modal.dataset.localLat, lng: modal.dataset.localLng, rotulo: modal.dataset.localRotulo }
            : null;

        const mostrar = (texto, tipo = 'muted') => {
            status.className = `small mb-3 text-${tipo}`;
            status.textContent = texto;
        };

        const ocupado = (botao, sim) => {
            botao.disabled = sim;
            botao.classList.toggle('opacity-75', sim);
        };

        botaoGps.addEventListener('click', async () => {
            ocupado(botaoGps, true);
            mostrar('Obtendo sua localização…');
            try {
                const { lat, lng } = await posicaoAtual();
                escolhido = { lat, lng, rotulo: await rotuloDe(lat, lng) };
                mostrar(`Local: ${escolhido.rotulo}. Clique em Aplicar.`, 'success');
            } catch (e) {
                mostrar(e.message, 'danger');
            } finally {
                ocupado(botaoGps, false);
            }
        });

        const buscar = async () => {
            const texto = campoBusca.value.trim();
            if (!texto) return mostrar('Digite um CEP ou "bairro, cidade".', 'danger');
            ocupado(botaoBuscar, true);
            mostrar('Procurando…');
            try {
                const local = await obterJson(`/Localizacao/Buscar?q=${encodeURIComponent(texto)}`);
                escolhido = { lat: arredondar(local.latitude), lng: arredondar(local.longitude), rotulo: local.rotulo };
                mostrar(`Local: ${escolhido.rotulo}. Clique em Aplicar.`, 'success');
            } catch (e) {
                mostrar(e.message, 'danger');
            } finally {
                ocupado(botaoBuscar, false);
            }
        };
        botaoBuscar.addEventListener('click', buscar);
        campoBusca.addEventListener('keydown', e => {
            if (e.key === 'Enter') { e.preventDefault(); buscar(); }
        });

        modal.querySelector('[data-local-aplicar]').addEventListener('click', () => {
            gravarCookie(COOKIE.raio, campoRaio.value);
            if (escolhido) salvarLocal(escolhido);
            else if (!lerCookie(COOKIE.lat)) return mostrar('Use sua localização atual ou busque um CEP antes de aplicar.', 'danger');
            location.reload();
        });

        modal.querySelector('[data-local-limpar]')?.addEventListener('click', () => {
            [COOKIE.lat, COOKIE.lng, COOKIE.rotulo].forEach(apagarCookie);
            gravarCookie(COOKIE.auto, '0'); // não pede o GPS de novo sozinho
            location.reload();
        });

        // primeira visita: tenta o GPS sozinho (uma vez). Se negar ou falhar, a cliente usa o botão "Definir local".
        if (automatico && !lerCookie(COOKIE.lat) && lerCookie(COOKIE.auto) !== '0' && navigator.geolocation) {
            gravarCookie(COOKIE.auto, '0', 60 * 60 * 24);
            const barra = document.getElementById('barraLocal');
            const texto = barra?.querySelector('span');
            const tentar = async () => {
                if (texto) texto.textContent = 'Obtendo sua localização…';
                try {
                    const { lat, lng } = await posicaoAtual();
                    salvarLocal({ lat, lng, rotulo: await rotuloDe(lat, lng) });
                    location.reload();
                } catch {
                    if (texto) texto.textContent = 'Informe onde você está para ver quem atende perto de você.';
                }
            };
            if (navigator.permissions?.query) {
                navigator.permissions.query({ name: 'geolocation' })
                    .then(p => { if (p.state !== 'denied') tentar(); })
                    .catch(tentar);
            } else {
                tentar();
            }
        }
    }

    // ---------- endereço da profissional (cadastro e perfil) ----------
    function iniciarEndereco(container) {
        const campo = nome => container.querySelector(`[data-endereco-campo="${nome}"]`);
        const cep = container.querySelector('[data-endereco-cep]');
        const lat = container.querySelector('[data-endereco-lat]');
        const lng = container.querySelector('[data-endereco-lng]');
        const status = container.querySelector('[data-endereco-status]');
        const botaoGps = container.querySelector('[data-endereco-gps]');
        let ultimoCep = (cep.value || '').replace(/\D/g, '');

        const mostrar = (texto, tipo = 'muted', icone = null) => {
            status.className = `small mt-2 text-${tipo}`;
            status.innerHTML = '';
            if (icone) {
                const i = document.createElement('i');
                i.className = `bi ${icone} me-1`;
                status.append(i);
            }
            status.append(document.createTextNode(texto));
        };

        // editou o endereço à mão: as coordenadas do GPS deixam de valer e o servidor recalcula pelo endereço
        const limparCoordenadas = () => { lat.value = ''; lng.value = ''; };
        container.querySelectorAll('[data-endereco-campo], [data-endereco-cep]')
            .forEach(c => c.addEventListener('input', limparCoordenadas));

        // número: só algarismos
        const numero = campo('numero');
        numero?.addEventListener('input', () => {
            const digitos = numero.value.replace(/\D/g, '').slice(0, 6);
            if (numero.value !== digitos) numero.value = digitos;
        });

        const preencher = (dados, sobrescrever) => {
            for (const nome of ['logradouro', 'numero', 'bairro', 'cidade', 'uf']) {
                const el = campo(nome);
                if (el && dados[nome] && (sobrescrever || !el.value)) el.value = dados[nome];
            }
        };

        cep.addEventListener('input', async () => {
            let digitos = cep.value.replace(/\D/g, '').slice(0, 8);
            cep.value = digitos.length > 5 ? `${digitos.slice(0, 5)}-${digitos.slice(5)}` : digitos;
            if (digitos.length !== 8 || digitos === ultimoCep) return;
            ultimoCep = digitos;

            mostrar('Buscando CEP…');
            try {
                const dados = await obterJson(`/Localizacao/Cep?cep=${digitos}`);
                preencher(dados, true);
                mostrar('Endereço preenchido pelo CEP. Confira e informe o número.', 'success', 'bi-check-circle');
                campo('numero')?.focus();
            } catch (e) {
                mostrar(e.message, 'danger');
            }
        });

        botaoGps?.addEventListener('click', async () => {
            botaoGps.disabled = true;
            mostrar('Obtendo sua localização…');
            try {
                const posicao = await posicaoAtual();
                try {
                    const dados = await obterJson(`/Localizacao/Reverso?lat=${posicao.lat}&lng=${posicao.lng}`);
                    if (dados.cep) {
                        cep.value = `${dados.cep.slice(0, 5)}-${dados.cep.slice(5)}`;
                        ultimoCep = dados.cep;
                    }
                    preencher(dados, true);
                } catch { /* sem endereço legível: as coordenadas já bastam para a busca */ }
                lat.value = posicao.lat;
                lng.value = posicao.lng;
                mostrar('Localização definida. Confira o endereço e salve.', 'success', 'bi-check-circle');
            } catch (e) {
                mostrar(e.message, 'danger');
            } finally {
                botaoGps.disabled = false;
            }
        });
    }

    return { iniciarBusca, iniciarEndereco };
})();
