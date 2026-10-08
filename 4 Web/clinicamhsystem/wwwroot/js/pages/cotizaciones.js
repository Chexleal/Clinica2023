// Sistema de cotización: mismo UX que cobro de venta (pagos.js), posteando a /Cotizaciones.
// La página siempre recarga tras cada acción (modo página, sin modal).
function submitCot(formId, url) {
    var f = document.getElementById(formId);
    f.setAttribute('action', url);
    f.setAttribute('method', 'POST');
    f.submit();
}

function initSelectsCot() {
    ['#idMotivoCobro', '#idProducto'].forEach(function (s) {
        var el = $(s);
        if (el.length && !el.hasClass('select2-hidden-accessible')) {
            el.select2({ dropdownParent: $('.cot-root').first() });
        }
    });
    presetPrecioCot(false);
    presetPrecioProductoCot(false);
}

function presetPrecioCot(force) {
    var sel = $('#idMotivoCobro option:selected');
    var precio = sel.data('precio');
    if (precio !== undefined && (force || !$('#ValorServicio').val())) {
        $('#ValorServicio').val(precio);
    }
}

$(document).on('change', '#idMotivoCobro', function () { presetPrecioCot(true); });

function presetPrecioProductoCot(force) {
    var sel = $('#idProducto option:selected');
    var precio = parseFloat(sel.data('precio'));
    // No prellenar ceros: si el catálogo está en 0, se deja vacío para obligar a digitarlo.
    if (!isNaN(precio) && precio > 0 && (force || !$('#ValorProducto').val())) {
        $('#ValorProducto').val(sel.data('precio'));
    }
}

$(document).on('change', '#idProducto', function () { presetPrecioProductoCot(true); });

function addDetalle() { submitCot('agregarDetalleForm', '/Cotizaciones/AddServicio'); }
function addProducto() { submitCot('agregarProductoForm', '/Cotizaciones/AddProducto'); }
function addProductoExpress() { submitCot('productoExpressForm', '/Cotizaciones/AddProductoExpress'); }
function addServicioExpress() { submitCot('servicioExpressForm', '/Cotizaciones/AddServicioExpress'); }

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
    initSelectsCot();
}

function setTipoExpress(t) {
    var esProd = t === 'producto';
    $('#panel-expres-producto').toggle(esProd);
    $('#panel-expres-servicio').toggle(!esProd);
    $('#seg-expres-producto').toggleClass('active', esProd);
    $('#seg-expres-servicio').toggleClass('active', !esProd);
}

function aplicarDescuento(idCotizacionDetalle) {
    var monto = ($('#descMonto-' + idCotizacionDetalle).val() || '0').trim();
    var motivo = ($('#descMotivo-' + idCotizacionDetalle).val() || '').trim();
    var f = document.createElement('form');
    f.method = 'POST';
    f.action = '/Cotizaciones/AplicarDescuento';
    f.innerHTML = '<input hidden name="idDetalle" value="' + idCotizacionDetalle + '">'
        + '<input hidden name="id" value="' + $("#IdCotizacionRef").val() + '">'
        + '<input hidden name="descuento">'
        + '<input hidden name="motivoDescuento">';
    f.querySelector('[name=descuento]').value = monto;
    f.querySelector('[name=motivoDescuento]').value = motivo;
    document.body.appendChild(f);
    f.submit();
}

function deleteCotizacionDetalle(id) {
    $.post('/Cotizaciones/EliminarDetalle',
        { idDetalle: id, id: $("#IdCotizacionRef").val() },
        function () { location.reload(); });
}

$(document).ready(function () {
    if (!$('.cot-root').length) return;
    initSelectsCot();
});
