// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Modal de confirmação (Views/Shared/_ModalConfirmacao): preenche a ação e o resumo
// com os data-* do botão que abriu o modal
document.querySelectorAll('.modal-confirmacao').forEach(modal => {
    const form = modal.querySelector('form');
    const botaoConfirmar = form.querySelector('[type=submit]');

    modal.addEventListener('show.bs.modal', (evento) => {
        const dados = evento.relatedTarget.dataset;
        form.action = dados.acao;
        modal.querySelectorAll('[data-campo]').forEach(campo => {
            const valor = dados[campo.dataset.campo] ?? '';
            campo.textContent = valor;
            campo.closest('.modal-confirmacao-linha').hidden = !valor;
        });
        const resumo = modal.querySelector('.modal-confirmacao-resumo');
        if (resumo) resumo.hidden = !resumo.querySelector('.modal-confirmacao-linha:not([hidden])');
        botaoConfirmar.disabled = false;
    });

    // evita enviar duas vezes com clique duplo
    form.addEventListener('submit', () => {
        botaoConfirmar.disabled = true;
    });
});

// Rolagem horizontal com setas (.rolagem-setas): cada seta só aparece se houver itens escondidos daquele lado
document.querySelectorAll('.rolagem-setas').forEach(caixa => {
    const lista = caixa.querySelector('.rolagem-lista');
    const esquerda = caixa.querySelector('.rolagem-seta-esq');
    const direita = caixa.querySelector('.rolagem-seta-dir');

    const atualizar = () => {
        esquerda.hidden = lista.scrollLeft <= 1;
        direita.hidden = lista.scrollLeft + lista.clientWidth >= lista.scrollWidth - 1;
    };

    esquerda.addEventListener('click', () => lista.scrollBy({ left: -lista.clientWidth * 0.7, behavior: 'smooth' }));
    direita.addEventListener('click', () => lista.scrollBy({ left: lista.clientWidth * 0.7, behavior: 'smooth' }));
    lista.addEventListener('scroll', atualizar, { passive: true });
    window.addEventListener('resize', atualizar);

    // deixa o item da página atual visível
    const ativo = lista.querySelector('.active');
    if (ativo) lista.scrollLeft = ativo.offsetLeft - (lista.clientWidth - ativo.offsetWidth) / 2;

    atualizar();
});

// PWA: registra o service worker
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/sw.js').catch(err => console.error('Falha ao registrar o service worker:', err));
    });
}
