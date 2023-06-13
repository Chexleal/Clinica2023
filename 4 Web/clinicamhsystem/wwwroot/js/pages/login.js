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
    var sucessMessage = document.getElementById("successMessage").innerText;
    if (sucessMessage) {
        Swal.fire({
            title: "Exito",
            text: sucessMessage,
            icon: "Sucess",
            confirmButtonText: "Aceptar"
        });
    }
});