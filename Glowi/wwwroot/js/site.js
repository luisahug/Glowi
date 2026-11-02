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

// PWA: registra o service worker
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/sw.js').catch(err => console.error('Falha ao registrar o service worker:', err));
    });
}
