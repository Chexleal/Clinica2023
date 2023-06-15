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