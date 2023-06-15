function testAlert() {
    Swal.fire({
        title: 'Sweet Alert',
        text: '¡Hola desde Sweet Alert!',
        icon: 'success',
        confirmButtonText: 'Aceptar'
    });
}

document.addEventListener("DOMContentLoaded", function () {
    var errorMessage = document.getElementById("errorMessage").innerText;
    if (errorMessage) {
        Swal.fire({
            title: "Error",
            text: errorMessage,
            icon: "error",
            confirmButtonText: "Aceptar"
        });
    }
});

document.addEventListener("DOMContentLoaded", function () {
    var successMessageElement = document.getElementById("successMessage");
    if (successMessageElement) {
        var successMessage = successMessageElement.innerText;
        Swal.fire({
            title: "Éxito",
            text: successMessage,
            icon: "success",
            confirmButtonText: "Aceptar"
        });
    }
});


function successTimer(id) {
    Swal.fire({
        icon: 'success',
        title: 'Cambios guardados',
        showConfirmButton: false,
        timer: 1500
    }).then((result) => {
        if (result.dismiss === Swal.DismissReason.timer) {
            const form = document.getElementById(id);
            form.submit();
        }
    })
}