$(document).ready(function () {
    $("#btnGuardar").click(function () {
        $("#terminado").val(false);
        $("#formCST").submit();
    });

    $("#btnTerminar").click(function () {
        $("#terminado").val(true);
        $("#formCST").submit();
    });


    $('#Medicamento').select2({
        dropdownParent: $('#modalReceta'),
        tags: true
    });
});

function addMedicamento() {
    var detalle = $("#agregarMedForm").serialize();
    $.ajax({
        url: '/ContinuarConsulta/AddDetalleReceta',
        type: 'POST',
        data: detalle,
        success: function (result) {
            $('#tablaInfo').html(result);
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function deleteDetalle(id) {
    $.ajax({
        url: '/ContinuarConsulta/DeleteMedicamentos',
        type: 'POST',
        data: { idDetalleReceta: id },
        success: function (result) {
            $('#tablaInfo').html(result);
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function ShowHistorialModal(consultas) {
    $('#loading').show();
    $("#table > tbody").empty();

    consultas.forEach(c => {
        $('#table > tbody:last-child').append(`
            <tr>
                <td>
                    <div class="options d-flex">
                        <a class="option" href="/ContinuarConsulta/Index?consultaId=${c.IdConsulta}">Ver</a>
                    </div>
                </td>
                <td>${parseDate(c.Fecha)}</td>
                <td>${truncateWithPopover(c.MotivoConsulta)}</td>
                <td>${truncateWithPopover(c.HistoriaClinica)}</td>
                <td>${truncateWithPopover(c.Diagnostico)}</td>
                <td>${truncateWithPopover(c.Observaciones)}</td>
            </tr>
        `);
    });

    const modal = document.getElementById('modalConsulta');

    // ⚠️ IMPORTANTE: evita duplicados si abres el modal varias veces
    modal.addEventListener('shown.bs.modal', function initPopoversOnce() {

        const popovers = modal.querySelectorAll('[data-bs-toggle="popover"]');

        popovers.forEach(pop => {
            bootstrap.Popover.getOrCreateInstance(pop, {
                container: 'body',
                trigger: 'hover',
                placement: 'top'
            });
        });

        // Se ejecuta SOLO una vez
        modal.removeEventListener('shown.bs.modal', initPopoversOnce);
    });

    $("#modalConsulta").modal('show');
    $('#loading').hide();
}

function ShowReceta(id) {
    $.ajax({
        url: urlGetMedicamentos,
        type: 'GET',
        data: { idReceta: id },
        success: function (result) {
            $('#tablaInfo').html(result);
            $('#modalReceta').modal('show');
        },
        error: function (error) {
            console.log(error);
        }
    });
    //$('#descripcionReceta').val(detail);
    //$('#idConsulta').val(id);
}

function ShowCalendario(id) {
    $.ajax({
        url: '/Citas/Calendar',
        data: { pacienteId: id },
        type: "POST",
        success: function (res) {
            //$('#modalCalendario').find('.modal-body').html(res);
            //$('#modalCalendario').modal('show');

            //$('#modalCalendario').on('shown.bs.modal', Loadcalendar);
            //Loadcalendar()

            //$('#modalCalendario').modal('show');
            $('#modalCalendario').find('.modal-body').html(res);
            $('#modalCalendario').modal('show');
            $('#modalCalendario').on('shown.bs.modal', Loadcalendar);
            $("#Destiny").val("consultas");


            $('#cita-form').submit(function (e) {
                // Detiene el envío del formulario normal
                e.preventDefault();

                // Obtén los datos del formulario
                var datos = $(this).serialize();

                // Envía la solicitud AJAX
                $.ajax({
                    type: 'POST',
                    url: "Citas/Add",
                    data: datos,
                    success: function (response) {
                        // Maneja la respuesta del servidor
                        $('#addCitaModal').modal('toggle');
                        $('#modalCalendario').modal('toggle');
                        $("#modalCalendario").find('.modal-body').html("");
                    }
                });
            });

        }
    });
}

function SetReadOnly() {
    $('textarea').attr('readonly', true);
    $('input').attr('readonly', true);
}

function truncateWithPopover(text, maxLength = 20) {
    if (!text) return '';

    const escapedText = text.replace(/"/g, '&quot;');
    const shortText = text.length > maxLength
        ? text.substring(0, maxLength) + '...'
        : text;

    return `
        <span 
            data-bs-toggle="popover"
            data-bs-trigger="hover"
            data-bs-placement="top"
            data-bs-content="${escapedText}">
            ${shortText}
        </span>`;
}

$('#recetaForm').submit(function (e) {
    // Detiene el envío del formulario normal
    e.preventDefault();

    // Obtén los datos del formulario
    var datos = $(this).serialize();

    // Envía la solicitud AJAX
    $.ajax({
        type: 'POST',
        url: urlSaveReceta,  // Reemplaza 'archivo.php' con la URL de tu archivo de servidor que procesará los datos
        data: datos,
        success: function (response) {
            // Maneja la respuesta del servidor
            /*       $('#modalReceta,.modal, .modal-overlay').hide();*/
            $('#modalReceta').modal('toggle');
        }
    });
});
