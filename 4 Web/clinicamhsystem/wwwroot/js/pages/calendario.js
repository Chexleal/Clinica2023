$(document).ready(function () {
    Loadcalendar()
    $('#selectPaciente').select2({
        dropdownParent: $('#addCitaModal'),
    });
    $("#Destiny").val("citas");

    $('#cita-form').submit(function (e) {
        // Detiene el env�o del formulario normal
        e.preventDefault();

        // Obt�n los datos del formulario
        var datos = $(this).serialize();

        // Env�a la solicitud AJAX
        $.ajax({
            type: 'POST',
            url: "Citas/Add",  // Reemplaza 'archivo.php' con la URL de tu archivo de servidor que procesar� los datos
            data: datos,
            success: function (response) {
                // Maneja la respuesta del servidor
                    location.reload();
            }
        });
    });
})