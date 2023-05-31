$(document).ready(function () {
    Loadcalendar()
    $('#selectPaciente').select2({
        dropdownParent: $('#addCitaModal'),
    });
    $("#Destiny").val("citas");

    $('#cita-form').submit(function (e) {
        // Detiene el envío del formulario normal
        e.preventDefault();

        // Obtén los datos del formulario
        var datos = $(this).serialize();

        // Envía la solicitud AJAX
        $.ajax({
            type: 'POST',
            url: "Citas/Add",  // Reemplaza 'archivo.php' con la URL de tu archivo de servidor que procesará los datos
            data: datos,
            success: function (response) {
                // Maneja la respuesta del servidor
                    location.reload();
            }
        });
    });
})