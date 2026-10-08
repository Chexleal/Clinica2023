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
                // Acción frecuente + componente global ⓘ (contenido lo construye audit-info.js)
                "targets": 0,
                "render": function (data, type, row) {
                    var info = '<span class="audit-info ms-2" tabindex="0" role="button" data-bs-toggle="popover" data-bs-trigger="manual-hover" data-bs-placement="top" data-bs-html="true"'
                        + ' data-audit-id="' + escAttr(row.idPaciente) + '"'
                        + ' data-audit-fecha="' + escAttr(row.fechaCreacion) + '"'
                        + ' data-audit-autor="' + escAttr(row.creadoPor) + '"'
                        + ' data-audit-hospital="' + escAttr(row.hospitalCreacion) + '">'
                        + '<i class="fa-solid fa-circle-info"></i></span>';
                    return '<div class="d-flex align-items-center text-nowrap">'
                        + '<button class="btn btn-sm btn-primary" title="Generar consulta" onclick="ShowConsultaModal(\'' + row.idPaciente + '\')"><i class="fa-solid fa-stethoscope me-1"></i>Consulta</button>'
                        + info + '</div>';
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
                    var id = "'" + row.idPaciente + "'";
                    return '<div class="btn-group btn-group-sm" role="group">'
                        + '<button class="btn btn-outline-secondary" title="Historial" onclick="ShowHistorialModal(' + id + ')"><i class="fa-solid fa-clock-rotate-left me-1"></i>Historial</button>'
                        + '<button class="btn btn-outline-info" title="Ver datos" onclick="ShowEditModal(' + id + ')"><i class="fa-solid fa-eye me-1"></i>Datos</button>'
                        + '<button class="btn btn-outline-danger" title="Eliminar" onclick="alertElminarPrevent(' + id + ')"><i class="fa-solid fa-trash"></i></button>'
                        + '</div>';
                }
            }
        ],
        "drawCallback": function () {
            if (typeof initAuditPopovers === 'function') initAuditPopovers();
        },
        "language": DataTablesCommon.withLanguage({ searchPlaceholder: 'Buscar paciente' }),
        buttons: DataTablesCommon.exportButtons('Pacientes', [1, 2, 3, 4, 5, 6, 7], true),
    });
}


function escAttr(s) {
    return String(s == null ? '' : s)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
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