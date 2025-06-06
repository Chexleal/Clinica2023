$(document).ready(function () {
    CreateTable();
});

function CreateTable() {
    $('#table').DataTable({
        "ordering": true,
        "lengthChange": true,
        dom: 'Bfrtip',
        "pageLength": 20,
        "language": {
            searchPlaceholder: 'Buscar consulta',
            sSearch: '',
            lengthMenu: 'MENU items/page',
            paginate: {
                previous: 'Anterior',
                next: 'Siguiente',
            },
            info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
            infoEmpty: "Mostrando 0 a 0 de 0 registros",
            infoFiltered: "(Filtrado de _TOTAL_ registros)",
            processing: `<div class="progress" style="margin: 0; width: 100%">
                                      <div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 100%"></div>
                                    </div>`,
            emptyTable: "No hay datos disponibles en esta tabla",
            buttons: {
                copyTitle: "Copiar al portapapeles",
                copySuccess: {
                    1: "Copi&oacute; una fila al portapapeles",
                    _: "Se copiaron %d filas al portapapeles"
                },
            }
        },
        buttons: [
            {
                extend: 'copy',
                text: '<i class="fas fa-clone"></i><strong>Copiar</strong>',
                messageTop: '',
                className: "btn btn-outline-dark",
                title: "Consultas",
                filename: "Consultas",
                exportOptions: {
                    columns: [1, 2, 3, 4],
                    page: 'all'
                },
                orientation: "landscape",
                pageSize: "LEGAL"
            },
            {
                extend: 'excel',
                text: '<i class="fas fa-file-excel"></i><strong>Excel </strong>',
                messageTop: '',
                className: "btn btn-outline-dark",
                title: "Consultas",
                filename: "Consultas",
                exportOptions: {
                    columns: [1, 2, 3, 4],
                    modifier: {
                        page: 'all',
                        search: 'none'
                    }
                },
                orientation: "landscape",
                pageSize: "LEGAL"
            }],
        columnDefs: [
            {
                targets: [4, 5], // Índice de la columna que deseas truncar
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
