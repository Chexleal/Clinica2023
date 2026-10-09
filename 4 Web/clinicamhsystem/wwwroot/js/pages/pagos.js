$(document).ready(function () {
    CreateTable();
    // Modo página (/Ventas/Cobrar): inicializar selects al cargar, sin modal.
    if (typeof COBRO_MODO !== 'undefined' && COBRO_MODO === 'pagina') {
        initSelectsPagina();
        presetPrecio(false);
    }
});

function cobroEsPagina() {
    return typeof COBRO_MODO !== 'undefined' && COBRO_MODO === 'pagina';
}

// Vista Ver (corrección) reusa el diseño de caja: las acciones vuelven a Ver, no a Cobrar.
function cobroEsCorreccion() {
    return cobroEsPagina() && typeof COBRO_ORIGEN !== 'undefined' && COBRO_ORIGEN === 'ver';
}

function conOrigen(url) {
    return cobroEsCorreccion() ? url + '?origen=ver' : url;
}

function submitCobro(formId, url) {
    var f = document.getElementById(formId);
    f.setAttribute('action', url);
    f.setAttribute('method', 'POST');
    if (['agregarDetalleForm', 'agregarProductoForm', 'productoExpressForm', 'servicioExpressForm'].includes(formId)) {
        copiarCoberturaVentaAlFormulario(f);
    }
    f.submit();
}

function copiarCoberturaVentaAlFormulario(form) {
    if (!form) return;
    form.querySelectorAll('[data-cobertura-copiada="true"]').forEach(input => input.remove());
    const usaAseguradora = document.getElementById('usaAseguradora');
    const fields = [...document.querySelectorAll('[data-coverage-name]')];
    const flag = document.createElement('input');
    flag.type = 'hidden'; flag.name = 'guardarCobertura'; flag.value = 'true'; flag.dataset.coberturaCopiada = 'true'; form.appendChild(flag);
    fields.forEach(field => {
        const hidden = document.createElement('input');
        hidden.type = 'hidden'; hidden.name = field.dataset.coverageName;
        hidden.value = !usaAseguradora?.checked && hidden.name === 'idAseguradora' ? '' : field.value;
        hidden.dataset.coberturaCopiada = 'true'; form.appendChild(hidden);
    });
}

function CreateTable() {
    if (!$('#table').length) return;
    $('#table').DataTable({
        "ordering": true,
        "lengthChange": true,
        dom: 'Bfrtip',
        "pageLength": 20,
        "order": [[2, "asc"]], // Fecha ascendente: la más antigua primero (0=ⓘ, 1=Acciones, 2=Fecha)
        "language": DataTablesCommon.withLanguage({ searchPlaceholder: 'Buscar consulta' }),
        buttons: DataTablesCommon.exportButtons('Consultas', [2, 3, 4, 5], false),
        "drawCallback": function () {
            if (typeof initAuditTooltips === 'function') initAuditTooltips();
            if (typeof initAuditPopovers === 'function') initAuditPopovers();
        },
        columnDefs: [
            {
                targets: [5], // Motivo de consulta: truncar texto largo (0=ⓘ, 1=Acciones, 2=Fecha, 3=Nombre, 4=Apellido, 5=Motivo)
                render: function (data, type, row) {
                    if (type === 'display' && data.length > 30) {
                        return '<span title="' + data + '">' + data.substr(0, 30) + '...</span>';
                    }
                    return data;
                }
            }
        ]
    });
}

$('.btn-detalles').click(function () {
    var consultaId = $(this).data('id');
    $.ajax({
        url: '/Pagos/Detalles',
        type: 'POST',
        data: { idconsulta: consultaId },
        success: function (result) {
            $('#pagarConsultaModal').find('.modal-content').html(result);
            $('#pagarConsultaModal').modal('show');
            /*                agregar();*/
            initSelects();
            presetPrecio(false);

        },
        error: function (error) {
            console.log(error);
        }
    });
});

function addDetalle() {
    if (window.cargandoTarifasAseguradora) { Swal.fire('Actualizando tarifa', 'Espera un momento antes de agregar la línea.', 'info'); return; }
    if (cobroEsPagina()) { submitCobro('agregarDetalleForm', conOrigen('/Ventas/AddServicio')); return; }
    var detalle = $("#agregarDetalleForm").serialize();
    //console.log("serializado: " + detalle);
    $.ajax({
        url: '/Pagos/AddDetalle',
        type: 'POST',
        data: detalle,
        //contentType: false,
        success: function (result) {
            $('#pagarConsultaModal').find('.modal-content').html(result);
            initSelects();

/*            $('#pagarConsultaModal').modal('show');*/
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function initSelects() {
    if (cobroEsPagina()) { initSelectsPagina(); return; }
    sincronizarCoberturaConsulta();
    ['#idMotivoCobro', '#idProducto'].forEach(function (s) {
        var el = $(s);
        if (el.length && !el.hasClass('select2-hidden-accessible')) {
            el.select2({ dropdownParent: $('#pagarConsultaModal') });
        }
    });
    presetPrecioProducto(false);
    actualizarReferencia();
}

 // En página no hay modal: el dropdown de select2 se ancla al contenedor de cobro.
function initSelectsPagina() {
    ['#idMotivoCobro', '#idProducto'].forEach(function (s) {
        var el = $(s);
        if (el.length && !el.hasClass('select2-hidden-accessible')) {
            el.select2({ dropdownParent: $('.cobro-root').first() });
        }
    });
    presetPrecioProducto(false);
    actualizarReferencia();
}

function refreshModal(result) {
    $('#pagarConsultaModal').find('.modal-content').html(result);
    initSelects();
    presetPrecio();
}

function presetPrecio(force) {
    var sel = $('#idMotivoCobro option:selected');
    var precio = sel.data('precio');
    if (precio !== undefined && (force || !$('#ValorServicio').val())) {
        $('#ValorServicio').val(precio);
    }
}

$(document).on('change', '#idMotivoCobro', function () { presetPrecio(true); });

function presetPrecioProducto(force) {
    var sel = $('#idProducto option:selected');
    var precio = parseFloat(sel.data('precio'));
    // No prellenar ceros: si el catálogo está en 0, se deja vacío para obligar a digitarlo.
    if (!isNaN(precio) && precio > 0 && (force || !$('#ValorProducto').val())) {
        $('#ValorProducto').val(sel.data('precio'));
    }
}

$(document).on('change', '#idProducto', function () { presetPrecioProducto(true); });

function addProducto() {
    if (window.cargandoTarifasAseguradora) { Swal.fire('Actualizando tarifa', 'Espera un momento antes de agregar la línea.', 'info'); return; }
    if (cobroEsPagina()) { submitCobro('agregarProductoForm', conOrigen('/Ventas/AddProducto')); return; }
    var detalle = $("#agregarProductoForm").serialize();
    $.ajax({
        url: '/Pagos/AddProducto',
        type: 'POST',
        data: detalle,
        success: refreshModal,
        error: function (error) {
            Swal.fire('Stock', error.responseText || 'No se pudo agregar el producto.', 'warning');
        }
    });
}

function addProductoExpress() {
    if (cobroEsPagina()) { submitCobro('productoExpressForm', conOrigen('/Ventas/AddProductoExpress')); return; }
    var detalle = $("#productoExpressForm").serialize();
    $.ajax({
        url: '/Pagos/AddProductoExpress',
        type: 'POST',
        data: detalle,
        success: refreshModal,
        error: function (error) {
            Swal.fire('Alta rápida', error.responseText || 'No se pudo crear el producto.', 'warning');
        }
    });
}

function setTipoAgregar(t) {
    ['servicio', 'producto', 'express'].forEach(function (k) {
        $('#panel-agregar-' + k).toggle(k === t);
        var btn = $('#seg-agregar-' + k);
        if (k === t) {
            btn.removeClass('text-muted').addClass('bg-light text-dark fw-bold');
        } else {
            btn.removeClass('bg-light text-dark fw-bold').addClass('text-muted');
        }
    });
    initSelects();
}

function setTipoExpress(t) {
    var esProd = t === 'producto';
    $('#panel-expres-producto').toggle(esProd);
    $('#panel-expres-servicio').toggle(!esProd);
    $('#seg-expres-producto').toggleClass('active', esProd);
    $('#seg-expres-servicio').toggleClass('active', !esProd);
}

function addServicioExpress() {
    if (cobroEsPagina()) { submitCobro('servicioExpressForm', conOrigen('/Ventas/AddServicioExpress')); return; }
    $.ajax({
        url: '/Pagos/AddServicioExpress',
        type: 'POST',
        data: $("#servicioExpressForm").serialize(),
        success: refreshModal,
        error: function (error) {
            Swal.fire('Servicio nuevo', error.responseText || 'No se pudo crear el servicio.', 'warning');
        }
    });
}

function actualizarReferencia() {
    var exige = $('#idMetodoPago option:selected').data('ref') == 1;
    $('#grupoReferencia').toggle(exige);
    if (!exige) { $('#ReferenciaPago').val(''); }
}

$(document).on('change', '#idMetodoPago', actualizarReferencia);

function addPago() {
    if (cobroEsPagina()) { submitCobro('agregarPagoForm', conOrigen('/Ventas/AgregarPago')); return; }
    $.ajax({
        url: '/Pagos/AgregarPago',
        type: 'POST',
        data: $("#agregarPagoForm").serialize(),
        success: function (result) {
            refreshModal(result);
            actualizarReferencia();
        },
        error: function (error) {
            Swal.fire('Pago', error.responseText || 'No se pudo registrar el pago.', 'warning');
        }
    });
}

function deletePago(idPago) {
    if (cobroEsPagina()) {
        var data = { idPago: idPago, idVenta: $("#IdVentaRef").val() };
        if (cobroEsCorreccion()) { data.origen = 'ver'; }
        $.post('/Ventas/EliminarPago', data, function () { location.reload(); });
        return;
    }
    $.ajax({
        url: '/Pagos/EliminarPago',
        type: 'POST',
        data: { idPago: idPago, idConsulta: $("#IdConsulta").val() },
        success: refreshModal,
        error: function (error) {
            console.log(error);
        }
    });
}

function deleteDetalle(id) { deleteVentaDetalle(id); }

function aplicarDescuento(idVentaDetalle) {
    var monto = ($('#descMonto-' + idVentaDetalle).val() || '0').trim();
    var motivo = ($('#descMotivo-' + idVentaDetalle).val() || '').trim();
    if (cobroEsPagina()) {
        var f = document.createElement('form');
        f.method = 'POST';
        f.action = conOrigen('/Ventas/AplicarDescuento');
        f.innerHTML = '<input hidden name="id" value="' + idVentaDetalle + '">'
            + '<input hidden name="idVenta" value="' + $("#IdVentaRef").val() + '">'
            + '<input hidden name="descuento">'
            + '<input hidden name="motivoDescuento">'
            + (cobroEsCorreccion() ? '<input hidden name="origen" value="ver">' : '');
        f.querySelector('[name=descuento]').value = monto;
        f.querySelector('[name=motivoDescuento]').value = motivo;
        document.body.appendChild(f);
        f.submit();
        return;
    }
    $.ajax({
        url: '/Pagos/AplicarDescuento',
        type: 'POST',
        data: { id: idVentaDetalle, idConsulta: $("#IdConsulta").val(), descuento: monto, motivoDescuento: motivo },
        success: refreshModal,
        error: function (error) {
            Swal.fire('Descuento', error.responseText || 'No se pudo aplicar el descuento.', 'warning');
        }
    });
}

function deleteVentaDetalle(id) {
    if (cobroEsPagina()) {
        var data = { id: id, idVenta: $("#IdVentaRef").val() };
        if (cobroEsCorreccion()) { data.origen = 'ver'; }
        $.post('/Ventas/EliminarDetalle', data, function () { location.reload(); });
        return;
    }
    var idConsulta = $("#IdConsulta").val();
    $.ajax({
        url: '/Pagos/EliminarVentaDetalle',
        type: 'POST',
        data: { id, idConsulta },
        success: refreshModal,
        error: function (error) {
            console.log(error);
        }
    });
}

function guardarTemporal() {
    // La cuenta se guarda por acción; al salir guardamos también tipo de atención/cobertura.
    var form = $('#coberturaFormConsulta');
    if (!form.length) return;
    $.post('/Pagos/GuardarCobertura', form.serialize())
        .done(function () { bootstrap.Modal.getOrCreateInstance(document.getElementById('pagarConsultaModal')).hide(); })
        .fail(function (error) { Swal.fire('No se pudo guardar', error.responseText || 'No fue posible guardar la atención y la cobertura.', 'warning'); });
}

function sincronizarCoberturaConsulta() {
    var checkbox = document.getElementById('usaAseguradoraConsulta');
    var fields = document.getElementById('coberturaConsulta');
    if (!checkbox || !fields) return;
    fields.hidden = !checkbox.checked;
    if (!checkbox.checked) {
        var insurer = document.getElementById('aseguradoraConsulta');
        if (insurer && insurer.value) insurer.value = '';
    }
}

function actualizarPrecioMostrado(tipoAtencion) {
    $('#idMotivoCobro option[data-precio-base], #idProducto option[data-precio-base]').each(function () {
        this.dataset.precio = tipoAtencion === 'Emergencia' ? this.dataset.precioEmergencia : this.dataset.precioBase;
    });
    $('#idMotivoCobro, #idProducto').trigger('change');
}

var solicitudTarifasAseguradora = null;
function cargarTarifasAseguradora() {
    var insurer = $('#idAseguradora, #aseguradoraConsulta').first().val() || '';
    var tipo = $('#tipoAtencion, #tipoAtencionConsulta').first().val() || 'Normal';
    var endpoint = '/Ventas/PreciosAseguradora';
    if (solicitudTarifasAseguradora) solicitudTarifasAseguradora.abort();
    window.cargandoTarifasAseguradora = true;
    solicitudTarifasAseguradora = $.getJSON(endpoint, { idAseguradora: insurer || null, tipoAtencion: tipo }).done(function (data) {
        $('#idMotivoCobro option[data-precio-general]').each(function () {
            var p = data.servicios[this.value];
            this.dataset.precioBase = p === undefined ? this.dataset.precioGeneral : p.convenido;
            this.dataset.precioEmergencia = p === undefined ? (this.dataset.emergenciaGeneral || this.dataset.precioGeneral) : (p.emergencia != null ? p.emergencia : p.convenido);
        });
        $('#idProducto option[data-precio-general]').each(function () {
            var p = data.productos[this.value];
            this.dataset.precioBase = p === undefined ? this.dataset.precioGeneral : p.convenido;
            this.dataset.precioEmergencia = p === undefined ? (this.dataset.emergenciaGeneral || this.dataset.precioGeneral) : (p.emergencia != null ? p.emergencia : p.convenido);
        });
        actualizarPrecioMostrado(tipo);
    }).always(function () { window.cargandoTarifasAseguradora = false; });
}

function agregarCoberturaAlFormulario(form) {
    var cobertura = $('#coberturaFormConsulta');
    if (!cobertura.length) return;
    $(form).find('[data-cobertura-copiada="true"]').remove();
    $('<input>', { type: 'hidden', name: 'guardarCobertura', value: 'true', 'data-cobertura-copiada': 'true' }).appendTo(form);
    cobertura.serializeArray().forEach(function (x) {
        if (x.name === 'idVenta' || x.name === 'idConsulta') return;
        $('<input>', { type: 'hidden', name: x.name, value: x.value, 'data-cobertura-copiada': 'true' }).appendTo(form);
    });
}

function camposCoberturaConsulta() {
    var form = $('#coberturaFormConsulta');
    if (!form.length) return [];
    var fields = form.serializeArray().filter(function (x) { return x.name !== 'idVenta' && x.name !== 'idConsulta'; });
    if (!$('#usaAseguradoraConsulta').is(':checked')) {
        var aseguradora = fields.find(function (x) { return x.name === 'idAseguradora'; });
        if (aseguradora) aseguradora.value = '';
        else fields.push({ name: 'idAseguradora', value: '' });
    }
    fields.push({ name: 'guardarCobertura', value: 'true' });
    return fields;
}

// Las acciones AJAX que reconstruyen la modal también guardan la cobertura actual,
// para que agregar una línea o un pago no descarte datos aún no guardados.
$.ajaxPrefilter(function (options) {
    var method = (options.type || options.method || 'GET').toUpperCase();
    var url = options.url || '';
    if (method !== 'POST' || !/\/Pagos\/(AddDetalle|AddProducto|AddProductoExpress|AddServicioExpress|AgregarPago|EliminarPago|AplicarDescuento|EliminarVentaDetalle)$/i.test(url)) return;
    var coverage = camposCoberturaConsulta();
    if (!coverage.length) return;
    var existing = typeof options.data === 'string' ? options.data : $.param(options.data || {});
    options.data = [existing, $.param(coverage)].filter(Boolean).join('&');
});

$(document).on('submit', 'form[action*="/Pagos/Finalizar"]', function () { agregarCoberturaAlFormulario(this); });

$(document).on('change', '#tipoAtencion, #tipoAtencionConsulta', function () {
    $('.js-tipo-atencion').val(this.value);
    cargarTarifasAseguradora();
});
$(document).on('change', '#idAseguradora, #aseguradoraConsulta', cargarTarifasAseguradora);
$(document).on('change', '#usaAseguradoraConsulta', function () {
    var fields = $('#coberturaConsulta'), insurer = $('#aseguradoraConsulta');
    fields.prop('hidden', !this.checked);
    if (!this.checked) insurer.val('').trigger('change');
});
$(document).on('change', '#aseguradoraConsulta', function () {
    var option = this.selectedOptions[0];
    if (option && option.dataset.copago !== undefined) {
        $('#copagoConsulta').val(option.dataset.copago);
        $('#coaseguroConsulta').val(option.dataset.coaseguro);
    }
});

$(document).on('hidden.bs.modal', '#pagarConsultaModal', function () {
    // Cerrar la modal = guardado temporal: todo lo agregado queda en BD
    // como venta "Pendiente" (sin marcar pagada). Solo se confirma en UI.
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            toast: true, position: 'top-end', icon: 'success',
            title: 'Cuenta guardada — queda pendiente de cobro',
            showConfirmButton: false, timer: 2500
        });
    }
});

$(document).on('shown.bs.collapse', '#panelPendientePago, #panelPendientePagoModal', function () {
    // Llevar al usuario hasta el formulario de pendiente de pago y enfocar el responsable.
    var panel = this;
    if (panel.scrollIntoView) {
        panel.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
    var input = panel.querySelector("input[name='responsable']");
    if (input) { setTimeout(function () { input.focus(); }, 350); }
});

function marcarPendientePago() {
    var resp = ($("#pendientePagoFormModal [name='responsable']").val() || '').trim();
    if (!resp) {
        Swal.fire('Pendiente de pago', 'Indica quién queda debiendo (responsable).', 'warning');
        return;
    }
    var datos = $('#pendientePagoFormModal').serializeArray();
    datos.push({ name: 'guardarCobertura', value: 'true' });
    $('#coberturaFormConsulta').serializeArray().forEach(function (x) {
        if (x.name !== 'idVenta' && x.name !== 'idConsulta') datos.push(x);
    });
    $.ajax({
        url: '/Pagos/PendientePago',
        type: 'POST',
        data: $.param(datos),
        success: refreshModal,
        error: function (error) {
            Swal.fire('Pendiente de pago', error.responseText || 'No se pudo dejar pendiente de pago.', 'warning');
        }
    });
}
