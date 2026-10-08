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
// Constructor ÚNICO del contenido del popover ⓘ (Id + Fecha + Autor + Hospital origen).
// Lo usan tanto el partial Razor _AuditPopover.cshtml como las tablas DataTables (vía data-audit-*).
// fecha y hospital son opcionales: si no vienen, no se muestran (compatibilidad con usos existentes).
// Recibe valores crudos y los escapa una sola vez (el contenido se asigna por opción JS, sin decodificación de atributo).
function auditPopoverContent(id, autor, fecha, hospital) {
    var e = function (s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    };
    var html = '<div class=\'text-start\' style=\'font-size:11px;line-height:1.5;\'><span style=\'white-space:nowrap;\'><b>Id:</b> '
        + e(id) + '</span>';
    if (fecha) html += '<br><b>Fecha:</b> ' + e(fecha);
    html += '<br><b>Autor:</b> ' + e(autor);
    if (hospital) html += '<br><b>Hospital origen:</b> ' + e(hospital);
    return html + '</div>';
}
function initAuditPopovers() {
    if (typeof bootstrap === 'undefined') return;
    var els = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    els.forEach(function (el) {
        var p = bootstrap.Popover.getInstance(el);
        if (p) p.dispose();
        // Si trae data-audit-id, el contenido se construye aquí (fuente única de diseño)
        var auditId = el.getAttribute('data-audit-id');
        var opts = { html: true };
        if (auditId !== null) {
            opts.content = auditPopoverContent(
                auditId,
                el.getAttribute('data-audit-autor') || '—',
                el.getAttribute('data-audit-fecha') || '',
                el.getAttribute('data-audit-hospital') || '');
        }
        if (el.getAttribute('data-bs-trigger') === 'manual-hover') {
            opts.trigger = 'manual';
            opts.placement = el.getAttribute('data-bs-placement') || 'top';
            new bootstrap.Popover(el, opts);
            bindHoverablePopover(el);
        } else {
            new bootstrap.Popover(el, opts);
        }
    });
}
function bindHoverablePopover(el) {
    if (el._auditHoverBound) return;
    el._auditHoverBound = true;
    var showTimer = null, hideTimer = null;
    function tipEl() {
        var id = el.getAttribute('aria-describedby');
        return id ? document.getElementById(id) : null;
    }
    function show() {
        clearTimeout(hideTimer);
        clearTimeout(showTimer);
        showTimer = setTimeout(function () {
            var inst = bootstrap.Popover.getInstance(el);
            if (inst) inst.show();
            var tip = tipEl();
            if (tip && !tip._auditHoverBound) {
                tip._auditHoverBound = true;
                tip.addEventListener('mouseenter', function () { clearTimeout(hideTimer); });
                tip.addEventListener('mouseleave', function () { hide(); });
            }
        }, 120);
    }
    function hide() {
        clearTimeout(showTimer);
        clearTimeout(hideTimer);
        hideTimer = setTimeout(function () {
            var tip = tipEl();
            if (tip && tip.matches(':hover')) return;
            if (el.matches(':hover') || el === document.activeElement) return;
            var inst = bootstrap.Popover.getInstance(el);
            if (inst) inst.hide();
        }, 250);
    }
    el.addEventListener('mouseenter', show);
    el.addEventListener('mouseleave', hide);
    el.addEventListener('focus', show);
    el.addEventListener('blur', hide);
}
document.addEventListener('DOMContentLoaded', initAuditTooltips);
document.addEventListener('DOMContentLoaded', initAuditPopovers);
