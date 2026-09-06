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
        // validación tamaño (100MB)
        const files = $('#filesEstudio')[0].files;
        if(!files.length){ Swal.fire({icon:'warning', title:'Selecciona archivo'}); return; }
        let total = 0; for(let f of files) total += f.size;
        if(total > 100*1024*1024){ Swal.fire({icon:'warning', title:'Muy grande', text:'Máx 100MB'}); return; }

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

    // Ver DICOM en visor (desde storage)
    $(document).on('click', '#contEstudios .btn-ver-dicom, #contEstudiosModal .btn-ver-dicom', function(){
        const idArch = $(this).data('arch');
        const nombre = $(this).data('nombre');
        const url = `/ContinuarConsulta/GetArchivoBytes?idArchivo=${idArch}`;
        // usar viewer global
        if(window.DicomViewer && window.DicomViewer.loadFromUrl){
            window.DicomViewer.loadFromUrl(url, nombre);
            $('html, body').animate({scrollTop: $('#cardRadiografias').offset().top - 20}, 300);
        } else {
            window.open(`/ContinuarConsulta/VerArchivo?idArchivo=${idArch}`, '_blank');
        }
    });

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
