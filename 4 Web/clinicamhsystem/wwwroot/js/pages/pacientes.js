// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    CreateTable();
});

function alertElminarPrevent(id) {
    Swal.fire({
        title: '¿Está seguro de eliminar a este paciente?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Si',
        cancelButtonText: 'No'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Pacientes/Eliminar',
                type: 'POST',
                data: { id: id },
                success: function (result) {
                    successWithTimer('/Pacientes/Index');
                },
                error: function (error) {
                    failWithTimer();
                    console.log(error);
                }
            });
        }

    })
}

function CreateTable() {
    if (!$('#tablePaciente').length) return;
    $('#tablePaciente').DataTable({
        "responsive": true,
        "ordering": true,
        "lengthChange": true,
        "processing": true,
        "serverSide": true,
        dom: 'Bfrtip',
        "pageLength": 20,
        "ajax": {
            "url": "/Pacientes/GetPacientesTable",
            "type": "POST"
        },
        "columns": [
            { "data": null, "orderable": false, "defaultContent": "" },
            { "data": "noRegistro" },
            { "data": "nombre" },
            { "data": "apellido" },
            { "data": "dpi" },
            { "data": "fechaNacimiento" },
            { "data": "telefono" },
            { "data": "correo" },
            { "data": null, "orderable": false, "defaultContent": "" }
        ],
        "columnDefs": [
            {
                "targets": 0,
                "render": function (data, type, row) {
                    return '<button class="option btn btn-outline-info" onclick="ShowConsultaModal(\'' + row.idPaciente + '\')">Generar consulta</button>';
                }
            },
            {
                "targets": 5,
                "render": function (data, type, row) {
                    return getAge(row.fechaNacimiento);
                }
            },
            {
                "targets": 8,
                "render": function (data, type, row) {
                    return '<div class="options d-flex ">' +
                        '<button class="option btn" onclick="ShowHistorialModal(\'' + row.idPaciente + '\')">Historial</button>' +
                        '<button class="option btn" onclick="ShowEditModal(\'' + row.idPaciente + '\')">Ver Datos</button>' +
                        '<button class="option btn" type="button" onclick="alertElminarPrevent(\'' + row.idPaciente + '\')">Eliminar</button>' +
                        '</div>';
                }
            }
        ],
        "language": DataTablesCommon.withLanguage({ searchPlaceholder: 'Buscar paciente' }),
        buttons: DataTablesCommon.exportButtons('Pacientes', [1, 2, 3, 4, 5, 6, 7], true),
    });
}


function getAge(dateString) {
    var fecha = new Date(dateString);
    var hoy = new Date();
    var edad = hoy.getFullYear() - fecha.getFullYear();
    var m = hoy.getMonth() - fecha.getMonth();
    if (m < 0 || (m === 0 && hoy.getDate() < fecha.getDate())) {
        edad--;
    }
    return edad;
}

function ShowEditModal(id) {
    $('#modalEdit').modal('show');

    $.ajax({
        url: urls.getPaciente,
        data: { pacienteId: id },
        async: true,
        type: "GET",
        atType: 'html',
        success: function (res) {
            $('#modalEditBody').html(res);
        }
    });
}

function ShowConsultaModal(id) {
    $('#createConsultaModal').modal('show');
    $('#IdPaciente').val(id);
}

function ShowHistorialModal(pacienteId) {
    $('#loading').show();
    $("#table > tbody").empty();

    $.ajax({
        url: urls.getHistorial,
        data: { pacienteId: pacienteId },
        async: true,
        type: "GET",
        atType: 'html',
        success: function (res) {
            $('#modalConsultaBody').html(res);
            $("#modalConsulta").modal('show');
            $('#loading').hide();
            const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
            popoverTriggerList.forEach(pop => new bootstrap.Popover(pop));
        }
    });

   
}