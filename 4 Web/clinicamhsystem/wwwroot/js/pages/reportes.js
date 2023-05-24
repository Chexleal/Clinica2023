$(document).ready(function () {
    CreateTable();

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

    $('#idPaciente').select2({
        tags: true
    });
});

function CreateTable() {
    $('#table').DataTable({
        "ordering": true,
        "lengthChange": true,
        searching: false,
        dom: '<"table-title">Bfrtip',
        buttons: ['copy', 'excel', 'pdf', 'print'],
        "pageLength": 20,
        "language": {
            searchPlaceholder: 'Buscar consultas',
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
                title: "Reporte",
                filename: "Reporte",
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7],
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
                title: "Reporte",
                filename: "Reporte",
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7],
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
                messageTop: '',
                className: "btn btn-outline-dark",
                title: "Reporte",
                filename: "Reporte",
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7],
                },
                orientation: "landscape",
                pageSize: "LEGAL"
            }]
    });
}