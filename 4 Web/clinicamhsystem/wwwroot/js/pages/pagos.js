//function ShowDetallesModal(id) {
//    $('#pagarConsultaModal').modal('show');

//    $.ajax({
//        url: urls.getDetalles,
//        data: { idconsulta: id },
//        async: true,
//        type: "GET",
//        atType: 'html',
//        success: function (res) {
//            $('#modalDetallesBody').html(res);
//        }
//    });
//}

//function ShowDetallesModal(id) {
//    $('#modalEdit').modal('show');

//    $.ajax({
//        url: urls.getPaciente,
//        data: { pacienteId: id },
//        async: true,
//        type: "GET",
//        atType: 'html',
//        success: function (res) {
//            $('#modalEditBody').html(res);
//        }
//    });
//}

//$(document).ready(function () {
//    $('.btn-detalles').click(function () {
//        var consultaId = $(this).data('id');
//        $('#pagarConsultaModal').find('.modal-body').load('/Consultas/Index?id=' + pacienteId);
//        $('#pagarConsultaModal').modal('show');
//    });
//});

$(document).ready(function () {
    $('.btn-detalles').click(function () {
        var consultaId = $(this).data('id');
        $.ajax({
            url: '/Pagos/Detalles',
            type: 'POST',
            data: { idconsulta: consultaId },
            success: function (result) {
                $('#pagarConsultaModal').find('.modal-body').html(result);
                $('#pagarConsultaModal').modal('show');
            },
            error: function (error) {
                console.log(error);
            }
        });
    });
});