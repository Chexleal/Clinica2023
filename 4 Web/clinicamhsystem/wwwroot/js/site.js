// Normalización global de texto para buscadores: minúsculas, sin tildes/diacríticos
// y con espacios colapsados. "José  Pérez" y "jose perez" quedan igual.
function normTexto(s) {
    return (!s ? '' : String(s).normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase()).replace(/\s+/g, ' ').trim();
}
window.normTexto = normTexto;

// Select2: matcher global normalizado. Cada palabra escrita debe aparecer en la
// opción, en cualquier orden ("perez juan" encuentra "Juan Pérez - 123") y sin
// importar tildes ni mayúsculas. Aplica a todos los select2 (actuales y futuros).
function matchSelect2(params, data) {
    if (!params.term || $.trim(params.term) === '') return data;
    if (data.children && data.children.length > 0) {
        var grupo = $.extend(true, {}, data);
        grupo.children = [];
        for (var c = 0; c < data.children.length; c++) {
            var mh = matchSelect2(params, data.children[c]);
            if (mh) grupo.children.push(mh);
        }
        return grupo.children.length ? grupo : null;
    }
    var texto = normTexto(data.text || '');
    var tokens = normTexto(params.term).split(' ');
    for (var t = 0; t < tokens.length; t++) {
        if (!tokens[t]) continue;
        if (texto.indexOf(tokens[t]) === -1) return null;
    }
    return data;
}
if ($.fn.select2 && $.fn.select2.defaults) {
    $.fn.select2.defaults.set('matcher', matchSelect2);
}

// DataTables (lado cliente): normaliza el contenido a buscar para que escribir
// sin tildes encuentre datos con tildes ("medicamento" encuentra "Medicación").
if ($.fn.dataTable && $.fn.dataTable.ext && $.fn.dataTable.ext.type && $.fn.dataTable.ext.type.search) {
    $.extend($.fn.dataTable.ext.type.search, {
        string: function (data) {
            return !data || typeof data !== 'string' ? (data || '') : normTexto(data.replace(/[\r\n\u2028]/g, ' '));
        },
        html: function (data) {
            return !data || typeof data !== 'string' ? '' : normTexto(data.replace(/[\r\n\u2028]/g, ' ').replace(/<.*?>/g, ''));
        }
    });
}

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
}

function logout() {
        $.ajax({
            url: '/Home/CerrarSesion',
            data: {},
            async: true,
            type: "POST",
            atType: 'html',
            success: function (res) {
                window.location.href = '/Home/Index';
            }
        });
}

const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
popoverTriggerList.forEach(pop => new bootstrap.Popover(pop));

function successWithTimer(url) {
    Swal.fire({
        icon: 'success',
        title: 'Cambios guardados',
        showConfirmButton: false,
        timer: 1500
    }).then((result) => {
        if (result.dismiss === Swal.DismissReason.timer && url) {
            window.location.href = url;
        }
    });
}

function failWithTimer() {
    Swal.fire({
        icon: 'error',
        title: 'Ocurrió un error',
        showConfirmButton: false,
        timer: 1500
    });
}