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

    // Gráfico de barras APILADAS: gastos por día divididos por categoría (mismo estilo).
    if ($('#graficaGastosDia').length && typeof Chart !== 'undefined') {
        var labelsGastos = JSON.parse(document.getElementById('labelsGastosDia').value);
        var datasetsGastos = JSON.parse(document.getElementById('datasetsGastosDia').value);
        var ctxGastos = document.getElementById('graficaGastosDia').getContext('2d');
        new Chart(ctxGastos, {
            type: 'bar',
            data: {
                labels: labelsGastos,
                datasets: datasetsGastos
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
                    title: titulo + ' por fecha',
                    messageTop: fechas,
                    filename: fileDia,
                    exportOptions: { columns: colsDia, modifier: { page: 'all', search: 'none' } },
                    customizeData: function (data) { agregarDetalleExcel(data, dtDia, '.btn-micro-dia'); },
                    customize: estilizarExcelReporte,
                    createEmptyCells: true,
                    autoFilter: true,
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

// Extiende el Excel del resumen con las filas de detalle que la vista muestra
// al desplegar cada fila. Los paneles viven fuera de la tabla principal.
function agregarDetalleExcel(data, api, selector) {
    estilosFilasExcel = [];
    if (!data || !data.body || !api) return;
    var nodos = api.rows({ order: 'applied', search: 'none' }).nodes().toArray();
    if (!nodos.some(function (nodo) { return nodo.querySelector(selector); })) return;

    var encabezadosOriginales = data.header.map(limpiarTextoExcel);
    var agrupadoPorFecha = selector === '.btn-micro-dia';
    var body = [];
    var estilos = [];
    var indiceNodo = 0;
    data.body.forEach(function (fila) {
        var nodo = nodos[indiceNodo++];
        var resumen = mapearResumenExcel(fila, encabezadosOriginales, null, agrupadoPorFecha);
        body.push(resumen);
        estilos.push(agrupadoPorFecha ? 'resumenFecha' : 'resumen');
        if (!nodo) return;
        var boton = nodo.querySelector(selector);
        var holder = boton && document.getElementById(boton.getAttribute('data-micro'));
        if (!holder) return;

        var filasNivelUno = holder.querySelectorAll('.micro-table > tbody > tr');
        filasNivelUno.forEach(function (tr) {
            // La fila de detalle anidada ya se exporta desde su encabezado de grupo.
            if (tr.classList.contains('micro-n2-det')) return;
            var headers = encabezadosTabla(tr.closest('.micro-table'));
            var celdas = Array.from(tr.children).filter(function (td) { return !td.querySelector('button'); });
            var valores = celdas.map(function (td) { return limpiarTextoExcel(td.innerText); });
            var tieneDetalleMovimientos = tr.classList.contains('micro-n2-head');
            var filaNivelUno = crearFilaDesglose(resumen[0], headers, valores);
            if (filaNivelUno) {
                body.push(filaNivelUno);
                estilos.push('grupo');
            }

            if (tieneDetalleMovimientos) {
                var filaDetalle = tr.nextElementSibling;
                var tablaDetalle = filaDetalle && filaDetalle.querySelector('table');
                if (!tablaDetalle) return;
                var headersMovimiento = encabezadosTabla(tablaDetalle);
                tablaDetalle.querySelectorAll('tbody > tr').forEach(function (trMovimiento) {
                    var valoresMovimiento = Array.from(trMovimiento.children).map(function (td) { return limpiarTextoExcel(td.innerText); });
                    var filaMovimiento = crearFilaMovimiento(headersMovimiento, valoresMovimiento);
                    if (!filaMovimiento) return;
                    body.push(filaMovimiento);
                    estilos.push('movimiento');
                });
            }
        });
        body.push(['', '', '', '', '']);
        estilos.push('espacio');
    });

    data.header = ['Concepto', 'Fecha / origen', 'Tipo de pago', 'Cantidad', 'Total'];
    data.body = body;
    if (data.footer && data.footer.length) {
        data.footer = Array.isArray(data.footer[0])
            ? data.footer.map(function (fila) { return mapearResumenExcel(fila, encabezadosOriginales, 'TOTAL'); })
            : mapearResumenExcel(data.footer, encabezadosOriginales, 'TOTAL');
    }
    estilosFilasExcel = estilos;
}

var estilosFilasExcel = [];

function limpiarTextoExcel(valor) {
    return String(valor || '').replace(/\s+/g, ' ').trim();
}

function encabezadosTabla(tabla) {
    return tabla ? Array.from(tabla.querySelectorAll(':scope > thead > tr > th'))
        .map(function (th) { return limpiarTextoExcel(th.innerText); }).filter(Boolean) : [];
}

function mapearResumenExcel(fila, headers, nivel, agrupadoPorFecha) {
    var valores = (fila || []).map(limpiarTextoExcel);
    var indiceImporte = headers.findIndex(function (h) { return /total|importe|subtotal|monto|balance/i.test(h); });
    var indiceCantidad = headers.findIndex(function (h) { return /cantidad|movimiento|consulta/i.test(h); });
    var indiceConcepto = headers.findIndex(function (_, i) { return i !== indiceImporte && i !== indiceCantidad; });
    if (indiceConcepto < 0) indiceConcepto = 0;
    var concepto = valores[indiceConcepto] || '';
    var referencia = headers.map(function (h, i) {
        return i !== indiceConcepto && i !== indiceCantidad && i !== indiceImporte && valores[i]
            ? h + ': ' + valores[i] : '';
    }).filter(Boolean).join(' · ');
    if (agrupadoPorFecha && !nivel) {
        return [concepto, '', '', valores[indiceCantidad] || '', indiceImporte >= 0 ? valores[indiceImporte] : ''];
    }
    return [nivel || concepto, referencia, '', valores[indiceCantidad] || '', indiceImporte >= 0 ? valores[indiceImporte] : ''];
}

function crearFilaDesglose(conceptoPadre, headers, valores) {
    var indiceFecha = headers.findIndex(function (h) { return /fecha/i.test(h); });
    var indiceConcepto = headers.findIndex(function (h) { return /servicio|producto/i.test(h); });
    var indiceCantidad = headers.findIndex(function (h) { return /cantidad/i.test(h); });
    var indiceMovimientos = headers.findIndex(function (h) { return /movimiento/i.test(h); });
    var indiceImporte = headers.findIndex(function (h) { return /subtotal|total|importe/i.test(h); });
    if (!headers.length || !valores.some(Boolean)) return null;

    if (headers.some(function (h) { return /motivo de consulta/i.test(h); })) {
        var indiceMotivo = headers.findIndex(function (h) { return /motivo de consulta/i.test(h); });
        var referenciaConsulta = headers.map(function (h, i) {
            return i !== indiceMotivo && valores[i] ? h + ': ' + valores[i] : '';
        }).filter(Boolean).join(' · ');
        return [valores[indiceMotivo] || '', referenciaConsulta, '', '', indiceImporte >= 0 ? valores[indiceImporte] : ''];
    }

    var cantidad = combinarCantidad(valores[indiceMovimientos], valores[indiceCantidad]);
    if (!cantidad && indiceCantidad >= 0) cantidad = valores[indiceCantidad];
    var importe = indiceImporte >= 0 ? valores[indiceImporte] : '';
    if (indiceFecha >= 0) return ['', valores[indiceFecha], '', cantidad, importe];
    if (indiceConcepto >= 0) return [valores[indiceConcepto], '', '', cantidad, importe];
    return [conceptoPadre, '', '', cantidad, importe];
}

function crearFilaMovimiento(headers, valores) {
    if (!headers.length || !valores.some(Boolean)) return null;
    var indicePaciente = headers.findIndex(function (h) { return /paciente/i.test(h); });
    var indiceOrigen = headers.findIndex(function (h) { return /origen/i.test(h); });
    var indicePago = headers.findIndex(function (h) { return /pago/i.test(h); });
    var indiceCantidad = headers.findIndex(function (h) { return /cantidad/i.test(h); });
    var indiceImporte = headers.findIndex(function (h) { return /subtotal|total|importe/i.test(h); });
    var origen = indiceOrigen >= 0 ? valores[indiceOrigen] : '';
    origen = origen.replace(/^Venta\s+/i, '').replace(/\s+\((consulta|mostrador)\)$/i, '');
    return [indicePaciente >= 0 ? valores[indicePaciente] : '', origen,
        indicePago >= 0 ? valores[indicePago] : '',
        indiceCantidad >= 0 ? valores[indiceCantidad] : '', indiceImporte >= 0 ? valores[indiceImporte] : ''];
}

function combinarCantidad(movimientos, cantidad) {
    var partes = [];
    if (movimientos) partes.push(movimientos + ' mov.');
    if (cantidad) partes.push(cantidad + ' unid.');
    return partes.join(' · ');
}

function estilizarExcelReporte(xlsx) {
    var sheet = $(xlsx.xl.worksheets['sheet1.xml']);
    var styles = $(xlsx.xl['styles.xml']);
    var cellXfs = styles.find('cellXfs');
    var styleMovimiento = cellXfs.children('xf').length;
    var xfMovimiento = cellXfs.children('xf').eq(25).clone();
    xfMovimiento.attr('applyAlignment', '1');
    var alignment = styles[0].createElementNS(styles[0].documentElement.namespaceURI, 'alignment');
    alignment.setAttribute('wrapText', '1');
    alignment.setAttribute('vertical', 'top');
    alignment.setAttribute('indent', '1');
    xfMovimiento[0].appendChild(alignment);
    cellXfs.append(xfMovimiento).attr('count', String(styleMovimiento + 1));
    var styleCantidad = cellXfs.children('xf').length;
    var xfCantidad = cellXfs.children('xf').eq(32).clone();
    xfCantidad.attr('numFmtId', '1').attr('applyNumberFormat', '1');
    cellXfs.append(xfCantidad).attr('count', String(styleCantidad + 1));
    var fonts = styles.find('fonts');
    var fontFecha = fonts.children('font').eq(Number(cellXfs.children('xf').eq(32).attr('fontId') || 0)).clone();
    var tamanoFecha = fontFecha.children('sz');
    if (tamanoFecha.length) tamanoFecha.attr('val', '14');
    else {
        var size = styles[0].createElementNS(styles[0].documentElement.namespaceURI, 'sz');
        size.setAttribute('val', '14');
        fontFecha[0].appendChild(size);
    }
    var fontIdFecha = fonts.children('font').length;
    fonts.append(fontFecha).attr('count', String(fontIdFecha + 1));
    var styleFecha = cellXfs.children('xf').length;
    var xfFecha = cellXfs.children('xf').eq(32).clone();
    xfFecha.attr('fontId', String(fontIdFecha)).attr('applyFont', '1').attr('applyAlignment', '1');
    xfFecha.children('alignment').remove();
    var alineacionFecha = styles[0].createElementNS(styles[0].documentElement.namespaceURI, 'alignment');
    alineacionFecha.setAttribute('horizontal', 'center');
    alineacionFecha.setAttribute('vertical', 'center');
    xfFecha[0].appendChild(alineacionFecha);
    cellXfs.append(xfFecha).attr('count', String(styleFecha + 1));
    var sheetData = sheet.find('sheetData');
    var cols = sheet.find('cols');
    if (cols.length) {
        while (cols[0].firstChild) cols[0].removeChild(cols[0].firstChild);
        var sheetNamespace = sheet[0].documentElement.namespaceURI;
        [[1, 32], [2, 42], [3, 42], [4, 17], [5, 18]].forEach(function (col) {
            var column = sheet[0].createElementNS(sheetNamespace, 'col');
            column.setAttribute('min', col[0]);
            column.setAttribute('max', col[0]);
            column.setAttribute('width', col[1]);
            column.setAttribute('customWidth', '1');
            cols[0].appendChild(column);
        });
    }

    // Botones DataTables Excel: fila 1 título, fila 2 período, fila 3 encabezados.
    sheetData.find('row[r="1"] c').attr('s', '22');
    sheetData.find('row[r="3"] c').attr('s', '32');
    var merges = sheet.find('mergeCells');
    estilosFilasExcel.forEach(function (tipo, indice) {
        var numeroFila = indice + 4;
        var fila = sheetData.find('row[r="' + numeroFila + '"]');
        var estilo = tipo.indexOf('resumen') === 0 ? '32' : tipo === 'grupo' ? '42' : tipo === 'movimiento' ? String(styleMovimiento) : '0';
        fila.find('c').each(function () {
            var celda = $(this);
            var esNumero = celda.attr('t') !== 'inlineStr' && celda.find('v').length > 0;
            var esFecha = tipo === 'resumenFecha' && /^A\d+$/.test(celda.attr('r') || '');
            celda.attr('s', esFecha ? String(styleFecha) : tipo.indexOf('resumen') === 0 && esNumero ? String(styleCantidad) : estilo);
        });
        if (tipo === 'movimiento') fila.attr('ht', '30').attr('customHeight', '1');
        if (tipo === 'espacio') fila.attr('ht', '18').attr('customHeight', '1');
        if (tipo === 'resumenFecha' && merges.length) {
            var merge = sheet[0].createElementNS(sheet[0].documentElement.namespaceURI, 'mergeCell');
            merge.setAttribute('ref', 'A' + numeroFila + ':C' + numeroFila);
            merges[0].appendChild(merge);
            merges.attr('count', String(merges.children('mergeCell').length));
        }
    });
}

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
                className: "btn btn-outline-dark",
                title: titulo,
                messageTop: fechas,
                filename: custom_file_name,
                exportOptions: {
                    columns: num_columns,
                    modifier: {
                        page: 'all',
                        search: 'none'
                    }
                },
                customizeData: function (data) { agregarDetalleExcel(data, table, '.btn-micro'); },
                customize: estilizarExcelReporte,
                createEmptyCells: true,
                autoFilter: true,
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
