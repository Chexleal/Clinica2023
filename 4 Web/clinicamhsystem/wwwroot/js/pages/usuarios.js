// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    CreateTable();
    $('#permissionsList').select2({
        dropdownParent: $('#createModal'),
    });
});

function CreateTable() {
    if (!$('#tableUser').length) return;
    $('#tableUser').DataTable({
        "autoWidth": true,
        "ordering": true,
        "lengthChange": true,
        dom: 'Bfrtip',
        "pageLength": 20,
        "language": DataTablesCommon.withLanguage({ searchPlaceholder: 'Buscar Usuario' }),
        buttons: DataTablesCommon.exportButtons('Usuarios', [0, 1, 2, 3, 4], false),
    });
}

function ShowEditModal(id) {
    $('#modalEdit').modal('show');
    $.ajax({
        url: '/Usuarios/GetUsuario',
        data: { usuarioId: id },
        async: true,
        type: "GET",
        atType: 'html',
        success: function (res) {
            $('#modalEditBody').html(res);
            $('#permissionsListEdit').select2({
                dropdownParent: $('#modalEdit'),
            });
        }
    });
}