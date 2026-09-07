// Inicializa DataTables (con buscador centrado) en las tablas de catálogos de Configuración.
// Uso: <table class="... tabla-config" data-search-placeholder="Buscar..." data-export-name="Nombre">
$(document).ready(function () {
    $('.tabla-config').each(function () {
        var $t = $(this);
        if (!$t.length || $.fn.DataTable.isDataTable($t)) return;
        $t.DataTable({
            "responsive": true,
            "ordering": true,
            "lengthChange": true,
            dom: '<"row align-items-center"<"col-sm-auto"B><"col"f>>rtip',
            "pageLength": 20,
            "language": DataTablesCommon.withLanguage({ searchPlaceholder: $t.data('search-placeholder') || 'Buscar...' }),
            buttons: DataTablesCommon.exportButtons($t.data('export-name') || 'Catalogo', ':not(:last-child)', false),
            "drawCallback": function () { if (typeof initAuditTooltips === 'function') initAuditTooltips(); },
        });
    });
});
