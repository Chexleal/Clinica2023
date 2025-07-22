// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    CreateTable();
});

function alertElminarPrevent(id) {
    swal({
        title: '¿Está seguro de eliminar a este paciente?',
        icon: 'warning',
        buttons: ["No", "Si"]
    }).then((result) => {
        if (result) {
            $.ajax({
                url: '/Pacientes/Eliminar',
                type: 'POST',
                data: { id: id },
                success: function (result) {
                    successWithTimer();
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
    $('#tablePaciente').DataTable({

        "responsive": true,
        "ordering": true,
        "lengthChange": true,
        dom: 'Bfrtip',
        "pageLength": 20,
        "language": {
            searchPlaceholder: 'Buscar paciente',
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
                title: "Pacientes",
                filename: "Pacientes",
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6, 7],
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
                title: "Pacientes",
                filename: "Pacientes",
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6, 7],
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
                title: "Pacientes",
                filename: "Pacientes",
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6, 7],
                },
                orientation: "landscape",
                pageSize: "LEGAL"
            }]
    });
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