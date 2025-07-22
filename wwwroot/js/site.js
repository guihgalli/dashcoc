// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Proteções de segurança: bloquear DevTools, atalhos e menu de contexto
(function () {
    // Bloquear F12, Ctrl+Shift+I/J/C/U, Ctrl+U, Ctrl+S, Ctrl+Shift+C
    document.addEventListener('keydown', function (e) {
        if (
            e.key === 'F12' ||
            (e.ctrlKey && e.shiftKey && (e.key === 'I' || e.key === 'J' || e.key === 'C')) ||
            (e.ctrlKey && (e.key === 'U' || e.key === 'S'))
        ) {
            e.preventDefault();
            e.stopPropagation();
            return false;
        }
    });
    // Bloquear clique direito (menu de contexto)
    document.addEventListener('contextmenu', function (e) {
        e.preventDefault();
        return false;
    });
    // Detectar abertura do DevTools (técnica básica)
    let devtoolsOpen = false;
    const threshold = 160;
    setInterval(function () {
        const widthThreshold = window.outerWidth - window.innerWidth > threshold;
        const heightThreshold = window.outerHeight - window.innerHeight > threshold;
        if (widthThreshold || heightThreshold) {
            if (!devtoolsOpen) {
                devtoolsOpen = true;
                // Opcional: redirecionar, exibir alerta, etc.
                // alert('DevTools detectado!');
            }
        } else {
            devtoolsOpen = false;
        }
    }, 500);
})();

// Write your JavaScript code.
