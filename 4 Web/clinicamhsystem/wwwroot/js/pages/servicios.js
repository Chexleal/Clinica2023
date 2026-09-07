$(document).ready(function () {
    CreateTable();
});

function CreateTable() {
    if (!$('#table').length) return;
    $('#table').DataTable({

        "responsive": true,
        "ordering": true,
        "lengthChange": true,
        dom: '<"row align-items-center"<"col-sm-auto"B><"col"f>>rtip',
        "pageLength": 20,
        "language": DataTablesCommon.withLanguage({ searchPlaceholder: 'Buscar servicio' }),
        buttons: DataTablesCommon.exportButtons('Servicios', [0, 1], true),
        "drawCallback": function () { if (typeof initAuditTooltips === 'function') initAuditTooltips(); },
    });
}