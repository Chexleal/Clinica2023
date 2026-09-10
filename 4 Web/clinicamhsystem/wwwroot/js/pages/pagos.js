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

function submitCobro(formId, url) {
    var f = document.getElementById(formId);
    f.setAttribute('action', url);
    f.setAttribute('method', 'POST');
    f.submit();
}

function CreateTable() {
    if (!$('#table').length) return;
    $('#table').DataTable({
        "ordering": true,
        "lengthChange": true,
        dom: 'Bfrtip',
        "pageLength": 20,
        "language": DataTablesCommon.withLanguage({ searchPlaceholder: 'Buscar consulta' }),
        buttons: DataTablesCommon.exportButtons('Consultas', [1, 2, 3, 4], false),
        columnDefs: [
            {
                targets: [4, 5], // �ndice de la columna que deseas truncar
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
    if (cobroEsPagina()) { submitCobro('agregarDetalleForm', '/Ventas/AddServicio'); return; }
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
    if (cobroEsPagina()) { submitCobro('agregarProductoForm', '/Ventas/AddProducto'); return; }
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
    if (cobroEsPagina()) { submitCobro('productoExpressForm', '/Ventas/AddProductoExpress'); return; }
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
    if (cobroEsPagina()) { submitCobro('servicioExpressForm', '/Ventas/AddServicioExpress'); return; }
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
    if (cobroEsPagina()) { submitCobro('agregarPagoForm', '/Ventas/AgregarPago'); return; }
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
        $.post('/Ventas/EliminarPago', { idPago: idPago, idVenta: $("#IdVentaRef").val() }, function () { location.reload(); });
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
        f.action = '/Ventas/AplicarDescuento';
        f.innerHTML = '<input hidden name="id" value="' + idVentaDetalle + '">'
            + '<input hidden name="idVenta" value="' + $("#IdVentaRef").val() + '">'
            + '<input hidden name="descuento">'
            + '<input hidden name="motivoDescuento">';
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
        $.post('/Ventas/EliminarDetalle', { id: id, idVenta: $("#IdVentaRef").val() }, function () { location.reload(); });
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
    // Las líneas, descuentos y pagos ya se guardan en BD con cada acción
    // (la venta queda "Pendiente"); este botón solo cierra y confirma.
    // El toast lo muestra el evento hidden de la modal.
}

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
    $.ajax({
        url: '/Pagos/PendientePago',
        type: 'POST',
        data: $("#pendientePagoFormModal").serialize(),
        success: refreshModal,
        error: function (error) {
            Swal.fire('Pendiente de pago', error.responseText || 'No se pudo dejar pendiente de pago.', 'warning');
        }
    });
}
