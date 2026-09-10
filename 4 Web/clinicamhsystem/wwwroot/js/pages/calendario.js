$(document).ready(function () {
    if (typeof Loadcalendar === 'function') {
        Loadcalendar();
    }

    var $select = $('#selectPaciente');
    if ($select.length && $.fn.select2) {
        $select.select2({ dropdownParent: $('#addCitaModal') });
    }
    $("#Destiny").val("citas");

    $('#cita-form').off('submit.calendario').on('submit.calendario', function (e) {
        e.preventDefault();
        var datos = $(this).serialize();

        $.ajax({
            type: 'POST',
            url: '/Citas/Add',
            data: datos,
            success: function (response) {
                $('#addCitaModal').modal('hide');
                $('#cita-form')[0].reset();
                if ($select.length && $.fn.select2) {
                    $select.val(null).trigger('change');
                }
                // Alta parcial sin reload: el servidor devuelve { id, titulo, fechaHora }
                try {
                    var id = response && (response.id ?? response.Id ?? response.idCita ?? response.IdCita);
                    var titulo = response && (response.titulo ?? response.Titulo);
                    var inicioRaw = response && (response.fechaHora ?? response.FechaHora);
                    if (id && inicioRaw && typeof calendar !== 'undefined' && calendar) {
                        var start = new Date(inicioRaw);
                        var end = new Date(start.getTime() + 30 * 60000);
                        calendar.addEvent({ id: String(id), title: titulo || 'Cita', start: start, end: end });
                    } else {
                        location.reload();
                        return;
                    }
                } catch (err) {
                    console.log(err);
                    location.reload();
                    return;
                }
                if (typeof successWithTimer === 'function') successWithTimer();
            },
            error: function (xhr) {
                console.log(xhr);
                if (typeof failWithTimer === 'function') failWithTimer();
                else alert('No se pudo agendar la cita.');
            }
        });
    });
});
