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

function ShowHistorialModal(consultas) {
    $('#loading').show();
    $("#table > tbody").empty();

    consultas.forEach(c => {
        $('#table > tbody:last-child').append(`<tr>
                                                <td>${parseDate(c.Fecha) }</td>
                                                <td>${c.MotivoConsulta}</td>
                                                <td>${c.HistoriaClinica}</td>
                                                <td>${c.Diagnostico}</td>
                                                <td>${c.Observaciones}</td>
                                                <td>
                                                    <div class="options d-flex ">
                                                    <a class="option" href="/ContinuarConsulta/Index?consultaId=${c.IdConsulta}">Ver</a>
                                                    </div>
                                                </td>
                                               </tr>`);
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

function SetReadOnly() {
    $('textarea').attr('readonly', true);
    $('input').attr('readonly', true);
}

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
            alert("no");
            console.log(error);
        }
    });
}

function deleteDetalle(id) {
    $.ajax({
        url: '/ContinuarConsulta/DeleteMedicamentos',
        type: 'POST',
        data: { idDetalleReceta:id },
        success: function (result) {
            $('#tablaInfo').html(result);
        },
        error: function (error) {
            alert("no");
            console.log(error);
        }
    });
}


