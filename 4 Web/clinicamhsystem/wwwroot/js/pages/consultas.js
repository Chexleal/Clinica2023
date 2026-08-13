// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    CreateTable();
    $('#idPaciente').select2({
        dropdownParent: $('#createModal'),
    });
});

function CreateTable() {
    if (!$('#table').length) return;
    $('#table').DataTable({
        "responsive": true,
        "ordering": true,
        "lengthChange": true,
        "processing": true,
        "serverSide": true,
        dom: 'Bfrtip',
        "pageLength": 20,
        "ajax": {
            "url": "/Consultas/GetConsultasTable",
            "type": "POST"
        },
        "columns": [
            { "data": null, "orderable": false, "defaultContent": "" },
            { "data": "fecha" },
            { "data": "pacienteNombre" },
            { "data": "pacienteApellido" },
            { "data": "motivoConsulta" },
            { "data": null, "orderable": false, "defaultContent": "" }
        ],
        "columnDefs": [
            {
                "targets": 0,
                "render": function (data, type, row) {
                    return '<a class="option btn" href="/ContinuarConsulta?consultaId=' + row.idConsulta + '">Continuar</a>';
                }
            },
            {
                "targets": 4,
                "render": function (data, type, row) {
                    if (type === 'display' && data && data.length > 30) {
                        return '<span title="' + data + '">' + data.substr(0, 30) + '...</span>';
                    }
                    return data;
                }
            },
            {
                "targets": 5,
                "render": function (data, type, row) {
                    var token = $('input[name="__RequestVerificationToken"]').first().val();
                    return '<form method="post" action="/Consultas/Eliminar">' +
                        '<input type="hidden" name="__RequestVerificationToken" value="' + token + '" />' +
                        '<input type="hidden" name="id" value="' + row.idConsulta + '" />' +
                        '<button class="option btn" type="submit">Eliminar</button>' +
                        '</form>';
                }
            }
        ],
        "language": DataTablesCommon.withLanguage({ searchPlaceholder: 'Buscar consulta' }),
        buttons: DataTablesCommon.exportButtons('Consultas', [1, 2, 3, 4], false),
    });
}