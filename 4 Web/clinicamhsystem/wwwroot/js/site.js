// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.getElementById("toggle-button").addEventListener("click", function () {
    var sidebar = document.querySelector(".menu");
    var toggleButton = document.querySelector(".toggle-button");
    var container = document.querySelector(".container-larger");
    if (sidebar.classList.contains("hidden")) {
        sidebar.classList.remove("hidden");
        toggleButton.classList.remove("hidden-tgb");
        container.classList.remove("hidden");
    } else {
        sidebar.classList.add("hidden");
        container.classList.add("hidden");
        toggleButton.classList.add("hidden-tgb");
    }
});

function parseDate(stringDate) {

    const fecha = new Date(stringDate); // crea un objeto Date a partir de la cadena de fecha

    const dia = fecha.getDate().toString().padStart(2, '0'); // obtiene el día como un número y lo convierte en una cadena con dos dígitos, rellenando con ceros a la izquierda si es necesario
    const mes = (fecha.getMonth() + 1).toString().padStart(2, '0'); // obtiene el mes como un número (ten en cuenta que en JavaScript, los meses empiezan en 0) y lo convierte en una cadena con dos dígitos, rellenando con ceros a la izquierda si es necesario
    const anio = fecha.getFullYear().toString(); // obtiene el año como un número de cuatro dígitos y lo convierte en una cadena
    const hora = fecha.getHours().toString().padStart(2, '0'); // obtiene la hora como un número y lo convierte en una cadena con dos dígitos, rellenando con ceros a la izquierda si es necesario
    const minutos = fecha.getMinutes().toString().padStart(2, '0'); // obtiene los minutos como un número y los convierte en una cadena con dos dígitos, rellenando con ceros a la izquierda si es necesario
    const segundos = fecha.getSeconds().toString().padStart(2, '0'); // obtiene los segundos como un número y los convierte en una cadena con dos dígitos, rellenando con ceros a la izquierda si es necesario

   return cadenaFechaHora = `${dia}/${mes}/${anio} ${hora}:${minutos}:${segundos}`; // crea la cadena de fecha y hora concatenando los componentes obtenidos

}

function OpenReceta(id) {
    window.open(urlReceta + id, '_blank');
    location.reload(true);
}
