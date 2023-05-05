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
                agregar();
            },
            error: function (error) {
                console.log(error);
            }
        });
    });

    function agregar() {
        $('#pagarConsulta').submit(function (event) {
            event.preventDefault();
            console.log("objeto this: " + $(this));
            console.log("--------------------------------------")
            var detalle = $(this).serialize();
            //console.log("serializado: " + detalle);
            $.ajax({
                url: '/Pagos/AddDetalle',
                type: 'POST',
                data: detalle,
                dataType: 'json',
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                //contentType: false,
                success: function (result) {
                    $('#pagarConsultaModal').find('.modal-body').html(result);
                    $('#pagarConsultaModal').modal('show');
                },
                error: function (error) {
                    console.log(error);
                }
            });
        });
    }
});