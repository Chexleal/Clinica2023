// Órdenes + Carga Estudios - AJAX - guarda solo ruta, peso, quien, fecha
$(function(){
    const idPaciente = $('input[name="idPaciente"]').first().val();
    // Crear Orden
    $('#formCrearOrden').on('submit', function(e){
        e.preventDefault();
        const data = $(this).serialize();
        $.post('/ContinuarConsulta/CrearOrden', data)
         .done(function(res){
            if(res.ok){
                // recargar lista ordenes
                $.get('/ContinuarConsulta/GetOrdenes', {idPaciente: idPaciente}, function(html){
                    $('#contOrdenes').html(html);
                });
                // agregar opción al select de carga
                const tipoText = $('#formCrearOrden select[name="tipo"] option:selected').text();
                const indic = $('#formCrearOrden input[name="indicacion"]').val();
                $('#selOrden').append(`<option value="${res.idOrden}" selected>${tipoText} - ${indic}</option>`);
                $('#formCrearOrden input[name="indicacion"]').val('');
                Swal.fire({icon:'success', title:'Orden creada', timer:1200, showConfirmButton:false});
            }
         })
         .fail(function(xhr){ Swal.fire({icon:'error', title:'Error', text: xhr.responseText}); });
    });

    // Usar orden -> seleccionar en carga
    $(document).on('click', '.btn-usar-orden', function(){
        const id = $(this).data('id');
        const tipo = $(this).data('tipo');
        $('#selOrden').val(id);
        // sincronizar tipo del form carga
        $('#formUploadEstudio select[name="tipo"]').val(tipo);
        $('html, body').animate({scrollTop: $('#cardCargaEstudios').offset().top - 20}, 300);
        Swal.fire({icon:'info', title:'Orden seleccionada', text:'Ahora carga el archivo para esa orden', timer:1500, showConfirmButton:false});
    });

    // Upload Estudio
    $('#formUploadEstudio').on('submit', function(e){
        e.preventDefault();
        const form = this;
        const fd = new FormData(form);
        // validación tamaño (250MB)
        const files = $('#filesEstudio')[0].files;
        if(!files.length){ Swal.fire({icon:'warning', title:'Selecciona archivo'}); return; }
        let total = 0; for(let f of files) total += f.size;
        if(total > 250*1024*1024){ Swal.fire({icon:'warning', title:'Muy grande', text:'Máx 250MB'}); return; }

        const btn = $(form).find('button[type="submit"]');
        btn.prop('disabled', true).html('<i class="fa-solid fa-spinner fa-spin"></i> Subiendo...');

        $.ajax({
            url: '/ContinuarConsulta/UploadEstudio',
            type: 'POST',
            data: fd,
            processData: false,
            contentType: false,
            success: function(html){
                $('#contEstudios').html(html);
                // limpiar
                $('#filesEstudio').val('');
                $('#formUploadEstudio input[name="titulo"]').val('');
                $('#formUploadEstudio input[name="descripcion"]').val('');
                // refrescar ordenes (marcar completada)
                $.get('/ContinuarConsulta/GetOrdenes', {idPaciente: idPaciente}, function(h){ $('#contOrdenes').html(h); });
                // actualizar select ordenes: quitar la usada (si completada)
                const usedId = $('#selOrden').val();
                if(usedId) $(`#selOrden option[value="${usedId}"]`).remove();
                $('#selOrden').val('');
                Swal.fire({icon:'success', title:'Estudio cargado', text:`${files.length} archivo(s) guardado(s) (ruta + peso + quien)`, timer:1800, showConfirmButton:false});
            },
            error: function(xhr){
                Swal.fire({icon:'error', title:'Error al subir', text: xhr.responseText || 'Verifica tamaño y formato'});
            },
            complete: function(){ btn.prop('disabled', false).html('<i class="fa-solid fa-cloud-arrow-up"></i> Subir estudio'); }
        });
    });

    // Ver DICOM en visor (desde storage) - clic o programático (dock/side/modaI)
    function verDicomEnVisor(idArch, nombre) {
        if (!idArch) return;
        if (window.DicomViewer && window.DicomViewer.loadFromArch) {
            window.DicomViewer.loadFromArch(idArch, nombre);
        } else if (window.DicomViewer && window.DicomViewer.loadFromUrl) {
            const url = `/ContinuarConsulta/GetArchivoBytes?idArchivo=${idArch}`;
            window.DicomViewer.loadFromUrl(url, nombre, idArch);
            const card = document.getElementById('cardRadiografias');
            if (card) card.scrollIntoView({ behavior: 'smooth', block: 'start' });
        } else {
            window.open(`/ContinuarConsulta/VerArchivo?idArchivo=${idArch}`, '_blank');
        }
    }
    $(document).on('click', '#contEstudios .btn-ver-dicom, #contEstudiosModal .btn-ver-dicom, #rxSideList .rx-side-item', function(){
        const idArch = $(this).data('arch');
        const nombre = $(this).data('nombre');
        verDicomEnVisor(idArch, nombre);
    });

    // Drag source: RX hacia el visor (HTML5 DnD)
    $(document).on('dragstart', '.rx-drag-source, .rx-side-item', function(e){
        const idArch = $(this).data('arch');
        const nombre = $(this).data('nombre') || 'estudio.dcm';
        try {
            const payload = JSON.stringify({ idArch: idArch, nombre: nombre });
            const dt = e.originalEvent.dataTransfer;
            dt.setData('text/rx-arch', payload);
            dt.setData('text/plain', payload);
            dt.effectAllowed = 'copy';
        } catch (err) { /* noop */ }
        $(this).addClass('dragging');
    });
    $(document).on('dragend', '.rx-drag-source, .rx-side-item', function(){
        $(this).removeClass('dragging');
    });

    // ===== Lista cuadrada sobre el visor + playlist =====
    function collectRxFromModal() {
        const items = [];
        $('#contEstudiosModal .btn-ver-dicom').each(function(){
            const idArch = $(this).data('arch');
            const nombre = $(this).data('nombre') || 'estudio.dcm';
            if (idArch) items.push({ idArch: String(idArch), nombre: String(nombre) });
        });
        // dedup por idArch manteniendo orden
        const seen = new Set();
        return items.filter(x => !seen.has(x.idArch) && (seen.add(x.idArch), true));
    }
    function shortName(n, max = 26) {
        if (!n) return 'RX';
        // recorta extensión para que quepa en la fila
        const base = n.replace(/\.(dcm|dicom)$/i, '');
        const label = base || n;
        return label.length > max ? label.substring(0, max - 1) + '…' : label;
    }
    function rebuildRxDock() {
        const side = $('#rxSideList');
        if (!side.length) return;
        const items = collectRxFromModal();
        // playlist del visor
        if (window.DicomViewer && window.DicomViewer.setPlaylist) window.DicomViewer.setPlaylist(items);
        if (!items.length) {
            side.html('<span class="text-muted small">Sin RX.</span>');
            const c = $('#dcmCounter'); if (c.length) c.text('0/0');
            return;
        }
        side.html(items.map((x, i) =>
            `<button type="button" class="rx-side-item" draggable="true" data-arch="${x.idArch}" data-nombre="${$('<div>').text(x.nombre).html()}" title="${$('<div>').text(x.nombre).html()}&#10;Clic para ver • Arrastra al visor"><span class="rx-chip-num">${i + 1}</span><i class="fa-solid fa-x-ray text-info"></i><span class="rx-thumb-name">${$('<div>').text(shortName(x.nombre)).html()}</span><i class="fa-solid fa-grip-vertical rx-thumb-grip"></i></button>`
        ).join(''));
    }
    window.RxDock = { rebuild: rebuildRxDock, ver: verDicomEnVisor };
    // build inicial (el modal ya viene renderizado desde servidor)
    rebuildRxDock();
    // re-build si el modal cambia por AJAX posterior
    setTimeout(rebuildRxDock, 800);

    // Botones del visor: prev-next
    $(document).on('click', '#btnDcmPrev', function(){ if (window.DicomViewer) window.DicomViewer.prev(); });
    $(document).on('click', '#btnDcmNext', function(){ if (window.DicomViewer) window.DicomViewer.next(); });

    // Eliminar archivo
    $(document).on('click', '#contEstudios .btn-del-arch, #contEstudiosModal .btn-del-arch', function(){
        const idArch = $(this).data('arch');
        Swal.fire({title:'¿Eliminar archivo?', icon:'warning', showCancelButton:true, confirmButtonText:'Sí'}).then(r=>{
            if(!r.isConfirmed) return;
            $.post('/ContinuarConsulta/DeleteArchivo', {idArchivo: idArch}, function(html){
                $('#contEstudios').html(html);
                Swal.fire({icon:'success', title:'Eliminado', timer:1200, showConfirmButton:false});
            });
        });
    });
    $(document).on('click', '#contEstudios .btn-del-estudio, #contEstudiosModal .btn-del-estudio', function(){
        const idEst = $(this).data('estudio');
        Swal.fire({title:'¿Eliminar estudio completo?', text:'Borra todos los archivos del estudio', icon:'warning', showCancelButton:true, confirmButtonText:'Sí, borrar'}).then(r=>{
            if(!r.isConfirmed) return;
            $.post('/ContinuarConsulta/DeleteEstudio', {idEstudio: idEst}, function(html){
                $('#contEstudios').html(html);
                Swal.fire({icon:'success', title:'Estudio eliminado', timer:1200, showConfirmButton:false});
            });
        });
    });
});
