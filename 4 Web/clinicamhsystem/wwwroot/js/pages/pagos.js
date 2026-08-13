$(document).ready(function () {
    CreateTable();
});

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
            $('#pagarConsultaModal').find('.modal-body').html(result);
            $('#pagarConsultaModal').modal('show');
            /*                agregar();*/
            $('#idMotivoCobro').select2({
                dropdownParent: $('#pagarConsultaModal')
            });

        },
        error: function (error) {
            console.log(error);
        }
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
            $('#idMotivoCobro').select2({
                dropdownParent: $('#pagarConsultaModal')
            });

/*            $('#pagarConsultaModal').modal('show');*/
        },
        error: function (error) {
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
            $('#idMotivoCobro').select2({
                dropdownParent: $('#pagarConsultaModal')
            });

            /*            $('#pagarConsultaModal').modal('show');*/
        },
        error: function (error) {
            console.log(error);
        }
    });
}
