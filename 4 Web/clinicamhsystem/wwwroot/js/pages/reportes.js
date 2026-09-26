$(document).ready(function () {
    var dt = CreateTable();
    CreateTotalesPagoTable();
    CreatePendientesTable();

    // Delegado: también funciona en filas micro insertadas dinámicamente.
    $(document).on('click', '.btn-total', function () {
        var consultaId = $(this).data('id');
        $.ajax({
            url: '/Reportes/Detalles',
            type: 'POST',
            data: { idconsulta: consultaId },
            success: function (result) {
                $('#detallesTotalModal').find('.modal-body').html(result);
                $('#detallesTotalModal').modal('show');
            },
            error: function (error) {
                console.log(error);
            }
        });
    });

    $('#idPaciente').select2();

    // Check "Incluir servicios sin ingresos": sincroniza el hidden que se postea (default apagado = "false").
    function syncSinIngresos() {
        var checked = $('#chkSinIngresos').is(':checked');
        $('#hidSinIngresos').val(checked ? 'true' : 'false');
        // Preserva la preferencia al buscar por paciente.
        $('#paciente-search input[name="incluirSinIngresos"]').val(checked ? 'true' : 'false');
    }
    $('#chkSinIngresos').on('change', syncSinIngresos);
    syncSinIngresos();

    // Gráfico de barras APILADAS: ingresos por día divididos por servicio (mismo estilo que Inicio, Chart.js).
    if ($('#graficaIngresosServicios').length && typeof Chart !== 'undefined') {
        var labelsDia = JSON.parse(document.getElementById('labelsIngresosDia').value);
        var datasetsDia = JSON.parse(document.getElementById('datasetsIngresosDia').value);
        var ctxDia = document.getElementById('graficaIngresosServicios').getContext('2d');
        new Chart(ctxDia, {
            type: 'bar',
            data: {
                labels: labelsDia,
                datasets: datasetsDia
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: { stacked: true },
                    y: {
                        stacked: true,
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) { return 'Q ' + value; }
                        }
                    }
                },
                plugins: {
                    legend: { display: true, position: 'bottom' },
                    tooltip: {
                        filter: function (item) { return item.parsed.y > 0; },
                        callbacks: {
                            label: function (ctx) { return ' ' + ctx.dataset.label + ': Q ' + ctx.parsed.y; },
                            footer: function (items) {
                                var total = items.reduce(function (acc, it) { return acc + it.parsed.y; }, 0);
                                return 'Total: Q ' + total;
                            }
                        }
                    }
                }
            }
        });
    }

    // Vista micro (servicios): detalle expandible por fila (macro = resumen).
    // El contenido micro vive en divs ocultos FUERA de la tabla (#micro-xxx).
    function getMicroHtml(microId) {
        var holder = document.getElementById(microId);
        if (!holder) {
            console.warn('Micro no encontrado: ' + microId);
            return null;
        }
        return holder.innerHTML;
    }

    function toggleMicro(btn, row) {
        var tr = btn.closest('tr');
        if (row.child.isShown()) {
            row.child.hide();
            tr.removeClass('shown');
            btn.text('+');
        } else {
            var html = getMicroHtml(btn.data('micro'));
            if (html === null) return;
            row.child(html).show();
            tr.addClass('shown');
            btn.text('−');
        }
    }

    function vistaActivaEsDia() {
        return $('#vistaDia').length > 0 && $('#vistaDia').is(':visible');
    }

    function dtActivo() {
        return vistaActivaEsDia() && dtDia ? dtDia : dt;
    }

    $('#table').on('click', '.btn-micro', function (e) {
        e.preventDefault();
        e.stopPropagation();
        if (!dt) return;
        var row = dt.row($(this).closest('tr'));
        if (!row || !row.node()) return;
        toggleMicro($(this), row);
    });

    // Vista inversa (por día): se inicializa perezosamente al mostrarla por primera vez.
    var dtDia = null;
    function ensureDiaTable() {
        if (dtDia) return dtDia;
        if (!$('#tableDia').length) return null;
        var colsDia = (typeof num_columns_dia !== 'undefined' && num_columns_dia.length) ? num_columns_dia : [1, 2, 3];
        var fileDia = (typeof custom_file_name_dia !== 'undefined') ? custom_file_name_dia : 'Reporte por dia';
        dtDia = $('#tableDia').DataTable({
            "ordering": true,
            "lengthChange": true,
            dom: '<"table-title">Bfrtip',
            "pageLength": 20,
            "language": DataTablesCommon.withLanguage(),
            "columnDefs": [{ "orderable": false, "targets": 0 }],
            buttons: [
                {
                    extend: 'copy',
                    text: '<i class="fas fa-clone"></i><strong>Copiar</strong>',
                    className: "btn btn-outline-dark",
                    title: "Reporte",
                    exportOptions: { columns: colsDia, page: 'all' },
                    footer: true
                },
                {
                    extend: 'excel',
                    text: '<i class="fas fa-file-excel"></i><strong>Excel </strong>',
                    className: "btn btn-outline-dark",
                    title: encabezado,
                    filename: fileDia,
                    exportOptions: { columns: colsDia, modifier: { page: 'all', search: 'none' } },
                    footer: true
                }, {
                    extend: 'pdf',
                    text: '<i class="fas fa-file-pdf"></i><strong>PDF </strong>',
                    customize: estiloPdfReporte,
                    className: "btn btn-outline-dark",
                    title: encabezado,
                    filename: fileDia,
                    exportOptions: { columns: colsDia },
                    footer: true
                }]
        });
        return dtDia;
    }

    $('#vistaDia').on('click', '.btn-micro-dia', function (e) {
        e.preventDefault();
        e.stopPropagation();
        if (!dtDia) return;
        var row = dtDia.row($(this).closest('tr'));
        if (!row || !row.node()) return;
        toggleMicro($(this), row);
    });

    // Nivel 2 del micro (agrupado día/servicio -> movimientos): las tablas micro se
    // insertan dinámicamente como child rows, por eso el handler es delegado.
    $(document).on('click', '.btn-micro-n2', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var btn = $(this);
        var head = btn.closest('tr.micro-n2-head');
        var det = head.next('tr.micro-n2-det');
        if (!det.length) return;
        if (det.is(':visible')) {
            det.hide();
            head.removeClass('shown');
            btn.text('+');
        } else {
            det.show();
            head.addClass('shown');
            btn.text('−');
        }
    });

    // Toggle Por servicio / Por día (se recuerda la última vista).
    function setVista(vista) {
        var esDia = vista === 'dia';
        if (!$('#vistaDia').length) return;
        $('#vistaServicio').toggle(!esDia);
        $('#vistaDia').toggle(esDia);
        $('#vistaServicioBtn').toggleClass('btn-primary', !esDia).toggleClass('btn-outline-primary', esDia);
        $('#vistaDiaBtn').toggleClass('btn-primary', esDia).toggleClass('btn-outline-primary', !esDia);
        if (esDia) {
            var t = ensureDiaTable();
            if (t) t.columns.adjust().draw(false);
        } else if (dt) {
            dt.columns.adjust().draw(false);
        }
        try { localStorage.setItem('reportesVista', vista); } catch (err) {}
    }
    $('#vistaServicioBtn').click(function () { setVista('servicio'); });
    $('#vistaDiaBtn').click(function () { setVista('dia'); });
    var vistaGuardada = 'servicio';
    try { vistaGuardada = localStorage.getItem('reportesVista') || 'servicio'; } catch (err) {}
    if (vistaGuardada === 'dia') setVista('dia');

    $('#expandAllMicro').click(function () {
        var api = dtActivo();
        if (!api) return;
        var scope = vistaActivaEsDia() ? '#vistaDia' : '#table';
        $(scope + ' .btn-micro, ' + scope + ' .btn-micro-dia').each(function () {
            var btn = $(this);
            var row = api.row(btn.closest('tr'));
            if (!row || !row.node() || row.child.isShown()) return;
            var html = getMicroHtml(btn.data('micro'));
            if (html === null) return;
            row.child(html).show();
            btn.closest('tr').addClass('shown');
            btn.text('−');
        });
        // Nivel 2: expandir también los grupos visibles (los holders ocultos no son :visible y quedan colapsados).
        $('.btn-micro-n2:visible').each(function () {
            var head = $(this).closest('tr.micro-n2-head');
            head.next('tr.micro-n2-det').show();
            head.addClass('shown');
            $(this).text('−');
        });
    });

    $('#collapseAllMicro').click(function () {
        var api = dtActivo();
        if (!api) return;
        var scope = vistaActivaEsDia() ? '#vistaDia' : '#table';
        $(scope + ' .btn-micro, ' + scope + ' .btn-micro-dia').each(function () {
            var btn = $(this);
            var row = api.row(btn.closest('tr'));
            if (!row || !row.node() || !row.child.isShown()) return;
            row.child.hide();
            btn.closest('tr').removeClass('shown');
            btn.text('+');
        });
    });
});

// Estilo PDF común: igual que la tabla en pantalla (encabezado oscuro con texto
// blanco, cuerpo centrado y fila de totales en negrita). No usa índices fijos de
// doc.content para no romperse si cambia el título o el mensaje superior.
function estiloPdfReporte(doc) {
    var tabla = null;
    for (var i = 0; i < doc.content.length; i++) {
        if (doc.content[i] && doc.content[i].table) { tabla = doc.content[i].table; break; }
    }
    if (!tabla || !tabla.body || !tabla.body.length) return;
    var ncols = tabla.body[0].length;
    tabla.widths = Array.apply(null, Array(ncols)).map(function () { return '*'; });
    tabla.body.forEach(function (fila, idx) {
        if (!fila || !fila.forEach) return;
        fila.forEach(function (celda) {
            if (!celda || typeof celda !== 'object') return;
            if (idx === 0) {
                celda.fillColor = '#212529';
                celda.color = '#ffffff';
                celda.bold = true;
            }
            celda.alignment = 'center';
        });
    });
    var pie = tabla.body[tabla.body.length - 1];
    if (tabla.body.length > 1 && pie && pie.forEach) {
        pie.forEach(function (celda) {
            if (celda && typeof celda === 'object') celda.bold = true;
        });
    }
    doc.defaultStyle = doc.defaultStyle || {};
    doc.defaultStyle.fontSize = 9;
}

function CreateTotalesPagoTable() {
    if (!$('#tablaTotalesPago').length) return null;
    var tituloPago = 'Totales por tipo de pago';
    try {
        if (typeof encabezado !== 'undefined' && encabezado) tituloPago = encabezado + ' - Totales por tipo de pago';
    } catch (e) {}
    var filePago = 'Totales por tipo de pago';
    try {
        if (typeof custom_file_name !== 'undefined' && custom_file_name) filePago = custom_file_name + ' - por tipo de pago';
    } catch (e) {}
    return $('#tablaTotalesPago').DataTable({
        "ordering": true,
        "paging": false,
        "searching": false,
        "info": false,
        "lengthChange": false,
        dom: 'Bfrtip',
        "language": DataTablesCommon.withLanguage(),
        buttons: [
            {
                extend: 'copy',
                text: '<i class="fas fa-clone"></i><strong>Copiar</strong>',
                className: "btn btn-outline-dark btn-sm",
                title: tituloPago,
                exportOptions: { columns: [0, 1, 2] },
                footer: true
            },
            {
                extend: 'excel',
                text: '<i class="fas fa-file-excel"></i><strong>Excel </strong>',
                className: "btn btn-outline-dark btn-sm",
                title: tituloPago,
                filename: filePago,
                exportOptions: { columns: [0, 1, 2] },
                footer: true
            },
            {
                extend: 'pdf',
                text: '<i class="fas fa-file-pdf"></i><strong>PDF </strong>',
                customize: estiloPdfReporte,
                className: "btn btn-outline-dark btn-sm",
                title: tituloPago,
                filename: filePago,
                exportOptions: { columns: [0, 1, 2] },
                footer: true,
                orientation: "portrait",
                pageSize: "LETTER"
            }]
    });
}

function CreatePendientesTable() {
    if (!$('#tablaPendientes').length) return null;
    var tituloPend = 'Cobros pendientes';
    try {
        if (typeof encabezado !== 'undefined' && encabezado) tituloPend = encabezado + ' - Cobros pendientes';
    } catch (e) {}
    var filePend = 'Cobros pendientes';
    try {
        if (typeof custom_file_name !== 'undefined' && custom_file_name) filePend = custom_file_name + ' - pendientes';
    } catch (e) {}
    return $('#tablaPendientes').DataTable({
        "ordering": true,
        "pageLength": 20,
        "lengthChange": true,
        dom: 'Bfrtip',
        "language": DataTablesCommon.withLanguage(),
        buttons: [
            {
                extend: 'copy',
                text: '<i class="fas fa-clone"></i><strong>Copiar</strong>',
                className: "btn btn-outline-dark btn-sm",
                title: tituloPend,
                exportOptions: { columns: [0, 1, 2, 3, 4, 5, 6] },
                footer: true
            },
            {
                extend: 'excel',
                text: '<i class="fas fa-file-excel"></i><strong>Excel </strong>',
                className: "btn btn-outline-dark btn-sm",
                title: tituloPend,
                filename: filePend,
                exportOptions: { columns: [0, 1, 2, 3, 4, 5, 6] },
                footer: true
            },
            {
                extend: 'pdf',
                text: '<i class="fas fa-file-pdf"></i><strong>PDF </strong>',
                customize: estiloPdfReporte,
                className: "btn btn-outline-dark btn-sm",
                title: tituloPend,
                filename: filePend,
                exportOptions: { columns: [0, 1, 2, 3, 4, 5, 6] },
                footer: true,
                orientation: "landscape",
                pageSize: "LEGAL"
            }]
    });
}

function CreateTable() {
    if (!$('#table').length) return null;
    var hasMicro = $('#table .btn-micro').length > 0;
    var table = $('#table').DataTable({
        "ordering": true,
        "lengthChange": true,
        dom: '<"table-title">Bfrtip',
        "pageLength": 20,
        "language": DataTablesCommon.withLanguage(),
        "columnDefs": hasMicro ? [{ "orderable": false, "targets": 0 }] : [],
        buttons: [
            {
                extend: 'copy',
                text: '<i class="fas fa-clone"></i><strong>Copiar</strong>',
                messageTop: '',
                className: "btn btn-outline-dark",
                title: "Reporte",
                filename: "Reporte por paciente",
                exportOptions: {
                    columns: (typeof num_columns !== 'undefined' && num_columns.length) ? num_columns : [0, 1, 2, 3, 4],
                    page: 'all'
                },
                footer: true,
                orientation: "landscape",
                pageSize: "LEGAL"
            },
            {
                extend: 'excel',
                text: '<i class="fas fa-file-excel"></i><strong>Excel </strong>',
                messageTop: '',
                className: "btn btn-outline-dark",
                title: encabezado,
                filename: custom_file_name,
                exportOptions: {
                    columns: num_columns,
                    modifier: {
                        page: 'all',
                        search: 'none'
                    }
                },
                footer: true,
                orientation: "landscape",
                pageSize: "LEGAL"
            }, {
                extend: 'pdf',
                text: '<i class="fas fa-file-pdf"></i><strong>PDF </strong>',
                customize: estiloPdfReporte,
                messageTop: '',
                className: "btn btn-outline-dark",
                title: encabezado,
                filename: custom_file_name,
                exportOptions: {
                    columns: num_columns,
                },
                footer: true,
                orientation: "landscape",
                pageSize: "LEGAL"
            }]
    });
    return table;
}