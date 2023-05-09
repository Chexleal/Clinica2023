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
/*                agregar();*/
            },
            error: function (error) {
                console.log(error);
            }
        });
    });

  
});

function addDetalle() {
    var detalle = $("#agregarDetalleForm").serialize();
    //console.log("serializado: " + detalle);
    $.ajax({
        url: '/Pagos/AddDetalle',
        type: 'POST',
        data: detalle,
        //contentType: false,
        success: function (result) {
            $('#pagarConsultaModal').find('.modal-body').html(result);
       
/*            $('#pagarConsultaModal').modal('show');*/
        },
        error: function (error) {
            alert("no");
            console.log(error);
        }
    });
}

function deleteDetalle(id) {
    var idConsulta = $("#IdConsulta").val();
    $.ajax({
        url: '/Pagos/Eliminar',
        type: 'POST',
        data: { id, idConsulta },
        success: function (result) {
            $('#pagarConsultaModal').find('.modal-body').html(result);

            /*            $('#pagarConsultaModal').modal('show');*/
        },
        error: function (error) {
            alert("no");
            console.log(error);
        }
    });
}
