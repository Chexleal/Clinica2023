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