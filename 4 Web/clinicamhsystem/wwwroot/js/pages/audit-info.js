// Inicializa los tooltips de Bootstrap para los iconos de auditoría (ⓘ).
// Llamar de nuevo después de cada redibujado de DataTables (drawCallback).
function initAuditTooltips() {
    if (typeof bootstrap === 'undefined') return;
    var els = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    els.forEach(function (el) {
        var t = bootstrap.Tooltip.getInstance(el);
        if (t) t.dispose();
        new bootstrap.Tooltip(el);
    });
}
document.addEventListener('DOMContentLoaded', initAuditTooltips);
