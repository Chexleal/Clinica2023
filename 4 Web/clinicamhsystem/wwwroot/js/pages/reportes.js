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

    $('#idPaciente').select2();
});

function CreateTable() {
    if (!$('#table').length) return;
    $('#table').DataTable({
        "ordering": true,
        "lengthChange": true,
        dom: '<"table-title">Bfrtip',
        "pageLength": 20,
        "language": DataTablesCommon.withLanguage(),
        buttons: [
            {
                extend: 'copy',
                text: '<i class="fas fa-clone"></i><strong>Copiar</strong>',
                messageTop: '',
                className: "btn btn-outline-dark",
                title: "Reporte",
                filename: "Reporte por paciente",
                exportOptions: {
                    columns: [0, 1, 2, 3, 4],
                    page: 'all'
                },
                orientation: "landscape",
                pageSize: "LEGAL"
            },
            {
                extend: 'excel',
                text: '<i class="fas fa-file-excel"></i><strong>Excel </strong>',
                customize: function (xlsx) {
                    var sheet = xlsx.xl.worksheets['sheet1.xml'];
                    $('row:first c', sheet).attr('s', '25');
                    $('row:first c:nth-child(1) t', sheet).text('Contenido adicional 1');
                    $('row:first c:nth-child(2) ', sheet).text('Contenido adicional 2');
                },
                messageTop: '',
                className: "btn btn-outline-dark",
                title: encabezado,
                filename: custom_file_name,
                exportOptions: {
                    columns: num_columns,
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
                customize: function (doc) {
                    doc.content[1].table.widths = Array(doc.content[1].table.body[0].length + 1).join('*').split('');
                    doc.styles.tableBodyEven.alignment = 'center';
                    doc.styles.tableBodyOdd.alignment = 'center';    
                },
                messageTop: '',
                className: "btn btn-outline-dark",
                title: encabezado,
                filename: custom_file_name,
                exportOptions: {
                    columns: num_columns,
                },
                orientation: "landscape",
                pageSize: "LEGAL"
            }]
    });
}9