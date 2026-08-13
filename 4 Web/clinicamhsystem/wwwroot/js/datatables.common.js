window.DataTablesCommon = (function () {
    "use strict";

    var language = {
        searchPlaceholder: 'Buscar...',
        sSearch: '',
        lengthMenu: 'Mostrando _MENU_ por página',
        paginate: {
            previous: 'Anterior',
            next: 'Siguiente',
        },
        info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
        infoEmpty: "Mostrando 0 a 0 de 0 registros",
        infoFiltered: "(Filtrado de _TOTAL_ registros)",
        processing: '<div class="progress" style="margin: 0; width: 100%"><div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 100%"></div></div>',
        emptyTable: "No hay datos disponibles en esta tabla",
        buttons: {
            copyTitle: "Copiar al portapapeles",
            copySuccess: {
                1: "Copi&oacute; una fila al portapapeles",
                _: "Se copiaron %d filas al portapapeles"
            },
        }
    };

    function withLanguage(custom) {
        return $.extend(true, {}, language, custom || {});
    }

    function exportButtons(filename, exportColumns, includePdf) {
        var buttons = [
            {
                extend: 'copy',
                text: '<i class="fas fa-clone"></i><strong>Copiar</strong>',
                messageTop: '',
                className: "btn btn-outline-dark",
                title: filename,
                filename: filename,
                exportOptions: {
                    columns: exportColumns,
                    page: 'all'
                },
                orientation: "landscape",
                pageSize: "LEGAL"
            },
            {
                extend: 'excel',
                text: '<i class="fas fa-file-excel"></i><strong>Excel </strong>',
                messageTop: '',
                className: "btn btn-outline-dark",
                title: filename,
                filename: filename,
                exportOptions: {
                    columns: exportColumns,
                    modifier: {
                        page: 'all',
                        search: 'none'
                    }
                },
                orientation: "landscape",
                pageSize: "LEGAL"
            }
        ];
        if (includePdf) {
            buttons.push({
                extend: 'pdf',
                text: '<i class="fas fa-file-excel"></i><strong>PDF </strong>',
                messageTop: '',
                className: "btn btn-outline-dark",
                title: filename,
                filename: filename,
                exportOptions: {
                    columns: exportColumns
                },
                orientation: "landscape",
                pageSize: "LEGAL"
            });
        }
        return buttons;
    }

    return {
        language: language,
        withLanguage: withLanguage,
        exportButtons: exportButtons
    };
})();
