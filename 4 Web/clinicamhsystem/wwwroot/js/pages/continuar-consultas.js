$(document).ready(function () {
    $("#btnGuardar").click(function () {
        $("#terminado").val(false);
        $("#formCST").submit();
    });

    $("#btnTerminar").click(function () {
        $("#terminado").val(true);
        $("#formCST").submit();
    });
});

function ShowHistorialModal(consultas) {
    $('#loading').show();
    $("#table > tbody").empty();

    consultas.forEach(c => {
        $('#table > tbody:last-child').append(`<tr>
                                                <td>${c.Fecha}</td>
                                                <td>${c.MotivoConsulta}</td>
                                                <td>${c.Diagnostico}</td>
                                                <td>${c.Observaciones}</td>
                                                <td>
                                                    <div class="options d-flex ">
                                                    <a class="option" href="/Usuarios/Detalles/@item.IdConsulta">Ver</a>
                                                    </div>
                                                </td>
                                               </tr>`);
    });
    $("#modalConsulta").modal('show');
    $('#loading').hide();
}

function ShowReceta(/*detail,id*/) {

    $("#modalReceta").modal('show');
    //$('#descripcionReceta').val(detail);
    //$('#idConsulta').val(id);
}
