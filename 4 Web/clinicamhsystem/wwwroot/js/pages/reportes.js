$(document).ready(function () {
    var dt = CreateTable();

    $('.btn-total').click(function () {
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

    // Vista micro: detalle expandible por fila (macro = resumen).
    // El contenido micro vive en divs ocultos FUERA de la tabla (#micro-xxx).
    function getMicroHtml(microId) {
        var holder = document.getElementById(microId);
        if (!holder) {
            console.warn('Micro no encontrado: ' + microId);
            return null;
        }
        return holder.innerHTML;
    }

    $('#table').on('click', '.btn-micro', function (e) {
        e.preventDefault();
        e.stopPropagation();
        if (!dt) return;
        var btn = $(this);
        var tr = btn.closest('tr');
        var row = dt.row(tr);
        if (!row || !row.node()) return;
        var microId = btn.data('micro');
        if (row.child.isShown()) {
            row.child.hide();
            tr.removeClass('shown');
            btn.text('+');
        } else {
            var html = getMicroHtml(microId);
            if (html === null) return;
            row.child(html).show();
            tr.addClass('shown');
            btn.text('−');
        }
    });

    $('#expandAllMicro').click(function () {
        if (!dt) return;
        $('#table .btn-micro').each(function () {
            var btn = $(this);
            var tr = btn.closest('tr');
            var row = dt.row(tr);
            if (!row || !row.node() || row.child.isShown()) return;
            var html = getMicroHtml(btn.data('micro'));
            if (html === null) return;
            row.child(html).show();
            tr.addClass('shown');
            btn.text('−');
        });
    });

    $('#collapseAllMicro').click(function () {
        if (!dt) return;
        $('#table .btn-micro').each(function () {
            var btn = $(this);
            var tr = btn.closest('tr');
            var row = dt.row(tr);
            if (!row || !row.node() || !row.child.isShown()) return;
            row.child.hide();
            tr.removeClass('shown');
            btn.text('+');
        });
    });
});

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
                orientation: "landscape",
                pageSize: "LEGAL"
            },
            {
                extend: 'excel',
                text: '<i class="fas fa-file-excel"></i><strong>Excel </strong>',
                customize: function (xlsx) {
                    var sheet = xlsx.xl.worksheets['sheet1.xml'];
                    $('row:first c', sheet).attr('s', '25');
                    $('row:first c:nth-child(1) t', sheet).text('Contenido adicional 1');
                    $('row:first c:nth-child(2) ', sheet).text('Contenido adicional 2');
                },
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
                orientation: "landscape",
                pageSize: "LEGAL"
            }, {
                extend: 'pdf',
                text: '<i class="fas fa-file-excel"></i><strong>PDf </strong>',
                customize: function (doc) {
                    doc.content[1].table.widths = Array(doc.content[1].table.body[0].length + 1).join('*').split('');
                    doc.styles.tableBodyEven.alignment = 'center';
                    doc.styles.tableBodyOdd.alignment = 'center';    
                },
                messageTop: '',
                className: "btn btn-outline-dark",
                title: encabezado,
                filename: custom_file_name,
                exportOptions: {
                    columns: num_columns,
                },
                orientation: "landscape",
                pageSize: "LEGAL"
            }]
    });
    return table;
}