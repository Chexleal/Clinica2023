// DICOM Viewer - Fase 1: visualización local .dcm sin backend
// Stack: cornerstone-core 2.6.1 + dicomParser 1.8.7 + cornerstoneWADOImageLoader 4.13.2
// Fase 2: se agregará upload a Azure Blob (solo cambiar load de file a URL)
(function () {
    let isCornerstoneReady = false;
    let currentImageId = null;
    let currentTool = 'WindowLevel'; // WindowLevel | Zoom | Pan
    let activeMeasure = null; // null | 'Length' | 'Angle'
    let toolsReady = false;
    let pixelSpacingNote = '';
    let initialVoi = null;
    let lastStats = null;
    let isDragging = false;
    let lastX = 0, lastY = 0;
    let loadedFileName = '';
    let dicomDataSet = null;

    const el = {
        fileInput: null,
        viewerContainer: null,
        dicomImage: null,
        placeholder: null,
        toolbar: null,
        status: null,
        overlay: null,
        info: null,
        tags: null,
        tagsContent: null
    };

    function init() {
        el.fileInput = document.getElementById('dcmFileInput');
        el.viewerContainer = document.getElementById('dicomViewerContainer');
        el.dicomImage = document.getElementById('dicomImage');
        el.placeholder = document.getElementById('dcmPlaceholder');
        el.toolbar = document.getElementById('dcmToolbar');
        el.status = document.getElementById('dcmStatus');
        el.overlay = document.getElementById('dicomOverlay');
        el.info = document.getElementById('dicomInfo');
        el.tags = document.getElementById('dcmTags');
        el.tagsContent = document.getElementById('dcmTagsContent');

        if (!el.fileInput) return; // no está en esta vista

        initCornerstone();
        bindEvents();
        console.log('[DICOM] Viewer inicializado - fase 1 local');
    }

    let retryCount = 0;
    const MAX_RETRIES = 20;
    function initCornerstone() {
        try {
            if (typeof cornerstone === 'undefined' || typeof dicomParser === 'undefined' || typeof cornerstoneWADOImageLoader === 'undefined') {
                retryCount++;
                console.warn(`[DICOM] Librerías aún no cargadas (${retryCount}/${MAX_RETRIES}) - cornerstone:${typeof cornerstone} dicomParser:${typeof dicomParser} WADO:${typeof cornerstoneWADOImageLoader}`);
                if (retryCount >= MAX_RETRIES) {
                    console.error('[DICOM] Timeout CDN - verifica pestaña Network (F12) si hay 404/bloqueo. Posible firewall o adblock bloqueando cdn.jsdelivr.net');
                    if (el.status) { el.status.textContent = 'Error CDN - sin internet o bloqueado'; el.status.className = 'badge bg-danger'; }
                    if (el.placeholder) {
                        el.placeholder.innerHTML = `<div class="alert alert-danger mb-0"><i class="fa-solid fa-triangle-exclamation"></i> <b>No cargaron librerías DICOM</b><br><small>Abre F12 -> Network y verifica que estos 3 URLs carguen 200 OK:<br>
                        - cdn.jsdelivr.net/npm/cornerstone-core<br>
                        - cdn.jsdelivr.net/npm/dicom-parser<br>
                        - cdn.jsdelivr.net/npm/cornerstone-wado-image-loader<br>
                        Si salen 404/bloqueados, desactiva AdBlock o prueba con <code>unpkg.com</code> como fallback.</small></div>`;
                    }
                    return;
                }
                setTimeout(initCornerstone, 500);
                return;
            }
            cornerstoneWADOImageLoader.external.cornerstone = cornerstone;
            cornerstoneWADOImageLoader.external.dicomParser = dicomParser;

            // Desactivar workers para evitar problemas de CORS/path en fase 1
            // Fase 2 con Blob podremos activar workers para performance
            try {
                cornerstoneWADOImageLoader.webWorkerManager.initialize({
                    maxWebWorkers: navigator.hardwareConcurrency || 1,
                    startWebWorkersOnDemand: true,
                    taskConfiguration: {
                        decodeTask: {
                            initializeCodecsOnStartup: false,
                            usePDFJS: false,
                            strict: false
                        }
                    }
                });
            } catch (e) {
                console.log('[DICOM] WebWorkers no inicializados, fallback a single thread', e);
                cornerstoneWADOImageLoader.configure({ useWebWorkers: false });
            }

            initMeasureTools();
            cornerstone.enable(el.dicomImage);
            isCornerstoneReady = true;

            // Resize handler
            window.addEventListener('resize', () => {
                if (currentImageId) cornerstone.resize(el.dicomImage, true);
            });

            // Viewer interactions (WW/WL, Zoom, Pan)
            setupViewportInteractions();

            console.log('[DICOM] Cornerstone listo');
        } catch (err) {
            console.error('[DICOM] Error init cornerstone', err);
            if (el.status) { el.status.textContent = 'Error visor'; el.status.className = 'badge bg-danger'; }
        }
    }

    function bindEvents() {
        // File input
        el.fileInput.addEventListener('change', e => {
            const file = e.target.files[0];
            if (file) loadDicomFile(file);
        });

        // Drag & drop
        ['dragenter', 'dragover'].forEach(evt => {
            el.placeholder.addEventListener(evt, e => {
                e.preventDefault(); e.stopPropagation();
                el.placeholder.classList.add('dragover');
            });
            if (el.viewerContainer) {
                el.viewerContainer.addEventListener(evt, e => {
                    e.preventDefault(); e.stopPropagation();
                });
            }
        });
        ['dragleave', 'drop'].forEach(evt => {
            el.placeholder.addEventListener(evt, e => {
                e.preventDefault(); e.stopPropagation();
                el.placeholder.classList.remove('dragover');
            });
        });
        el.placeholder.addEventListener('drop', e => {
            const file = e.dataTransfer.files[0];
            if (file && file.name.toLowerCase().endsWith('.dcm')) {
                // sincronizar input
                const dt = new DataTransfer(); dt.items.add(file); el.fileInput.files = dt.files;
                loadDicomFile(file);
            } else if (file) {
                Swal.fire({ icon: 'warning', title: 'Solo .dcm', text: 'Selecciona un archivo DICOM (.dcm)' });
            }
        });
        // También drop sobre viewer
        el.viewerContainer.addEventListener('drop', e => {
            const file = e.dataTransfer.files[0];
            if (file && file.name.toLowerCase().endsWith('.dcm')) {
                const dt = new DataTransfer(); dt.items.add(file); el.fileInput.files = dt.files;
                loadDicomFile(file);
            }
        });

        // Toolbar
        document.querySelectorAll('#dcmToolbar [data-tool]').forEach(btn => {
            btn.addEventListener('click', () => {
                setMeasure(null);
                document.querySelectorAll('#dcmToolbar [data-tool]').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                currentTool = btn.getAttribute('data-tool');
                updateInfo();
            });
        });
        // Set default active
        const defaultBtn = document.querySelector('#dcmToolbar [data-tool="WindowLevel"]');
        if (defaultBtn) defaultBtn.classList.add('active');
        try {
            if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
                document.querySelectorAll('#dcmToolbar [title]').forEach(function (b) {
                    b.setAttribute('data-bs-toggle', 'tooltip');
                    b.setAttribute('data-bs-placement', 'bottom');
                    bootstrap.Tooltip.getOrCreateInstance(b);
                });
            }
        } catch (err) { console.warn('[DICOM] tooltips', err); }

        document.getElementById('btnDcmReset')?.addEventListener('click', resetViewport);
        document.getElementById('btnDcmInvert')?.addEventListener('click', invertImage);
        document.getElementById('btnDcmFullscreen')?.addEventListener('click', toggleFullscreen);
        document.getElementById('dcmWwRange')?.addEventListener('input', function () {
            if (!currentImageId) return;
            var viewport = cornerstone.getViewport(el.dicomImage);
            viewport.voi.windowWidth = Math.max(1, parseFloat(this.value));
            cornerstone.setViewport(el.dicomImage, viewport);
            updateOverlay();
        });
        document.getElementById('dcmWlRange')?.addEventListener('input', function () {
            if (!currentImageId) return;
            var viewport = cornerstone.getViewport(el.dicomImage);
            viewport.voi.windowCenter = parseFloat(this.value);
            cornerstone.setViewport(el.dicomImage, viewport);
            updateOverlay();
        });
        document.getElementById('btnDcmLength')?.addEventListener('click', () => setMeasure(activeMeasure === 'Length' ? null : 'Length'));
        document.getElementById('btnDcmRotL')?.addEventListener('click', () => rotateImage(-90));
        document.getElementById('btnDcmRotR')?.addEventListener('click', () => rotateImage(90));
        document.getElementById('btnDcmClear')?.addEventListener('click', clearMeasures);

        // Wheel zoom
        el.dicomImage.addEventListener('wheel', e => {
            if (!currentImageId) return;
            e.preventDefault();
            const viewport = cornerstone.getViewport(el.dicomImage);
            const delta = e.deltaY > 0 ? 0.9 : 1.1;
            viewport.scale = Math.max(0.1, Math.min(10, viewport.scale * delta));
            cornerstone.setViewport(el.dicomImage, viewport);
            updateOverlay();
        }, { passive: false });
    }

    function setupViewportInteractions() {
        el.dicomImage.addEventListener('mousedown', e => {
            if (!currentImageId) return;
            if (activeMeasure) return;
            isDragging = true;
            lastX = e.clientX; lastY = e.clientY;
            e.preventDefault();
        });
        window.addEventListener('mouseup', () => { isDragging = false; });
        window.addEventListener('mousemove', e => {
            if (!isDragging || !currentImageId) return;
            const deltaX = e.clientX - lastX;
            const deltaY = e.clientY - lastY;
            lastX = e.clientX; lastY = e.clientY;

            const viewport = cornerstone.getViewport(el.dicomImage);
            if (currentTool === 'WindowLevel') {
                // WW = ancho ventana, WL = centro ventana
                // deltaX -> WW, deltaY -> WL
                viewport.voi.windowWidth = Math.max(1, viewport.voi.windowWidth + deltaX * 2);
                viewport.voi.windowCenter = viewport.voi.windowCenter + deltaY * 2;
                cornerstone.setViewport(el.dicomImage, viewport);
                updateOverlay();
            } else if (currentTool === 'Pan') {
                viewport.translation.x += deltaX / viewport.scale;
                viewport.translation.y += deltaY / viewport.scale;
                cornerstone.setViewport(el.dicomImage, viewport);
            } else if (currentTool === 'Zoom') {
                const zoomFactor = 1 + deltaY * -0.01;
                viewport.scale = Math.max(0.1, Math.min(10, viewport.scale * zoomFactor));
                cornerstone.setViewport(el.dicomImage, viewport);
                updateOverlay();
            }
        });

        // Touch support
        let lastTouchDist = null;
        el.dicomImage.addEventListener('touchstart', e => {
            if (activeMeasure) return;
            if (e.touches.length === 1) {
                isDragging = true;
                lastX = e.touches[0].clientX; lastY = e.touches[0].clientY;
            } else if (e.touches.length === 2) {
                isDragging = false;
                lastTouchDist = Math.hypot(e.touches[0].clientX - e.touches[1].clientX, e.touches[0].clientY - e.touches[1].clientY);
            }
        }, { passive: false });
        el.dicomImage.addEventListener('touchmove', e => {
            if (!currentImageId) return;
            e.preventDefault();
            if (e.touches.length === 1 && isDragging) {
                const deltaX = e.touches[0].clientX - lastX;
                const deltaY = e.touches[0].clientY - lastY;
                lastX = e.touches[0].clientX; lastY = e.touches[0].clientY;
                const viewport = cornerstone.getViewport(el.dicomImage);
                if (currentTool === 'WindowLevel') {
                    viewport.voi.windowWidth = Math.max(1, viewport.voi.windowWidth + deltaX * 2);
                    viewport.voi.windowCenter = viewport.voi.windowCenter + deltaY * 2;
                } else if (currentTool === 'Pan') {
                    viewport.translation.x += deltaX / viewport.scale;
                    viewport.translation.y += deltaY / viewport.scale;
                }
                cornerstone.setViewport(el.dicomImage, viewport);
                updateOverlay();
            } else if (e.touches.length === 2) {
                const dist = Math.hypot(e.touches[0].clientX - e.touches[1].clientX, e.touches[0].clientY - e.touches[1].clientY);
                if (lastTouchDist) {
                    const viewport = cornerstone.getViewport(el.dicomImage);
                    viewport.scale = Math.max(0.1, Math.min(10, viewport.scale * (dist / lastTouchDist)));
                    cornerstone.setViewport(el.dicomImage, viewport);
                    updateOverlay();
                }
                lastTouchDist = dist;
            }
        }, { passive: false });
        el.dicomImage.addEventListener('touchend', () => { isDragging = false; lastTouchDist = null; });
    }

    function loadDicomFile(file) {
        if (!isCornerstoneReady) {
            Swal.fire({ icon: 'error', title: 'Visor no listo', text: 'Espera un segundo y vuelve a intentar.' });
            return;
        }
        if (!file.name.toLowerCase().endsWith('.dcm')) {
            Swal.fire({ icon: 'warning', title: 'Formato no válido', text: 'Solo se aceptan archivos .dcm' });
            return;
        }
        loadedFileName = file.name;
        el.status.textContent = 'Cargando...'; el.status.className = 'badge bg-warning text-dark';
        el.tags.style.display = 'none';

        // Leer como ArrayBuffer para extraer tags también
        const reader = new FileReader();
        reader.onload = function (e) {
            const arrayBuffer = e.target.result;
            try {
                // Parse DICOM tags con dicomParser para info
                try {
                    const byteArray = new Uint8Array(arrayBuffer);
                    dicomDataSet = dicomParser.parseDicom(byteArray);
                    showDicomTags(dicomDataSet);
                } catch (tagErr) {
                    console.warn('[DICOM] No se pudieron leer tags', tagErr);
                }

                // Usar WADO loader fileManager
                const imageId = cornerstoneWADOImageLoader.wadouri.fileManager.add(file);
                currentImageId = imageId;

                cornerstone.loadImage(imageId).then(image => {
                    // Mostrar viewer
                    el.placeholder.style.display = 'none';
                    el.viewerContainer.style.display = 'block';
                    el.toolbar.style.display = 'flex';

                    const viewport = cornerstone.getDefaultViewportForImage(el.dicomImage, image);
                    // Ajuste inicial para ver bien RX
                    // Auto WW/WC si no viene
                    cornerstone.displayImage(el.dicomImage, image, viewport);
                    enableMeasureTools(image);
                    // Fit to window
                    cornerstone.resize(el.dicomImage, true);

                    el.status.textContent = 'Cargado: ' + file.name;
                    el.status.className = 'badge bg-success';
                    updateOverlay(image);
                    updateInfo();

                    // Hook para futuro: aquí se llamaría upload a Blob
                    // uploadToAzureBlob(file, arrayBuffer) // Fase 2

                    if (window.Swal) {
                        // toast sutil
                        // Swal.fire({ icon:'success', title:'DICOM cargado', timer:1200, showConfirmButton:false })
                    }
                }).catch(err => {
                    console.error('[DICOM] loadImage error', err);
                    el.status.textContent = 'Error al renderizar';
                    el.status.className = 'badge bg-danger';
                    Swal.fire({ icon: 'error', title: 'No se pudo renderizar', text: (err && err.message) || 'Archivo DICOM corrupto o no soportado (JPEG2000 sin codec). Prueba con otro .dcm' });
                });
            } catch (err) {
                console.error('[DICOM] Error general', err);
                el.status.textContent = 'Error';
                el.status.className = 'badge bg-danger';
                Swal.fire({ icon: 'error', title: 'Error al leer .dcm', text: err.message });
            }
        };
        reader.onerror = () => {
            Swal.fire({ icon: 'error', title: 'Error lectura', text: 'No se pudo leer el archivo' });
            el.status.textContent = 'Error lectura'; el.status.className = 'badge bg-danger';
        };
        reader.readAsArrayBuffer(file);
    }

    function showDicomTags(dataSet) {
        try {
            const get = (tag, def = '') => {
                try { const e = dataSet.elements[tag]; if (!e) return def; return dataSet.string(tag) || def; } catch { return def; }
            };
            // Tags comunes RX
            const patientName = get('x00100010');
            const patientId = get('x00100020');
            const studyDate = get('x00080020');
            const modality = get('x00080060');
            const bodyPart = get('x00180015');
            const imageSize = `${get('x00280011')} x ${get('x00280010')}`;
            const bits = get('x00280100');

            const tagsStr = [
                patientName ? `Paciente: ${patientName}` : null,
                modality ? `Mod: ${modality}` : null,
                bodyPart ? `Parte: ${bodyPart}` : null,
                studyDate ? `Fecha: ${studyDate}` : null,
                imageSize.trim() !== 'x' ? `Tamaño: ${imageSize}` : null,
                bits ? `${bits} bits` : null
            ].filter(Boolean).join(' | ');

            if (tagsStr) {
                el.tagsContent.textContent = tagsStr;
                el.tags.style.display = 'block';
            }
            // Overlay detallado
            el.overlay.innerHTML = `
                ${patientName ? patientName : loadedFileName}<br>
                ${modality || ''} ${bodyPart || ''} ${studyDate || ''}<br>
                <span style="color:#fff; opacity:0.8">${fileSizeToStr(dicomDataSet?.byteArray?.length || 0)}</span>
            `;
        } catch (e) { console.warn('[DICOM] tags error', e); }
    }

    function updateOverlay(image) {
        if (!currentImageId) return;
        const viewport = cornerstone.getViewport(el.dicomImage);
        const ww = Math.round(viewport.voi.windowWidth);
        const wc = Math.round(viewport.voi.windowCenter);
        const scale = (viewport.scale * 100).toFixed(0);
        const invert = viewport.invert ? 'Sí' : 'No';
        el.info.textContent = `WW: ${ww}  WL: ${wc}  Zoom: ${scale}%  Invert: ${invert}  Tool: ${currentTool}` + (activeMeasure ? `  Medida: ${activeMeasure} (${pixelSpacingNote})` : '');
        syncWlPanel(ww, wc);
        if (image) {
            if (window.__dcmLastId !== currentImageId) { window.__dcmLastId = currentImageId; initialVoi = { ww: ww, wc: wc }; lastStats = { min: image.minPixelValue, max: image.maxPixelValue }; }
            el.overlay.innerHTML = `${loadedFileName}<br> ${image.width} x ${image.height} · ${image.minPixelValue} / ${image.maxPixelValue}`;
        }
    }

    function syncWlPanel(ww, wc) {
        var panel = document.getElementById('dcmWlPanel');
        if (!panel) return;
        var show = currentTool === 'WindowLevel' && !!currentImageId && !activeMeasure;
        panel.style.display = show ? 'block' : 'none';
        if (!show) return;
        var wwR = document.getElementById('dcmWwRange');
        var wlR = document.getElementById('dcmWlRange');
        if (wwR) {
            if (lastStats) wwR.max = Math.max(100, lastStats.max - lastStats.min);
            if (document.activeElement !== wwR) wwR.value = ww;
        }
        if (wlR) {
            if (lastStats) { wlR.min = lastStats.min; wlR.max = lastStats.max; }
            if (document.activeElement !== wlR) wlR.value = wc;
        }
        var set = function (id, txt) { var n = document.getElementById(id); if (n) n.textContent = txt; };
        set('dcmWwVal', ww);
        set('dcmWlVal', wc);
        if (initialVoi) {
            set('dcmWwPct', '(' + Math.round(ww / initialVoi.ww * 100) + '%)');
            set('dcmWlPct', '(' + Math.round(wc / initialVoi.wc * 100) + '%)');
        }
    }
    function updateInfo() { updateOverlay(); }

    function resetViewport() {
        if (!currentImageId) return;
        cornerstone.reset(el.dicomImage);
        updateOverlay();
    }

    function invertImage() {
        if (!currentImageId) return;
        const viewport = cornerstone.getViewport(el.dicomImage);
        viewport.invert = !viewport.invert;
        cornerstone.setViewport(el.dicomImage, viewport);
        updateOverlay();
    }

    function toggleFullscreen() {
        if (!el.viewerContainer) return;
        if (!document.fullscreenElement) {
            el.viewerContainer.requestFullscreen().then(() => {
                setTimeout(() => cornerstone.resize(el.dicomImage, true), 200);
            }).catch(err => console.error(err));
        } else {
            document.exitFullscreen();
        }
    }

    function fileSizeToStr(bytes) {
        if (!bytes) return '';
        const kb = bytes / 1024;
        if (kb < 1024) return kb.toFixed(1) + ' KB';
        return (kb / 1024).toFixed(2) + ' MB';
    }

    // Exponer para Fase 2 (Blob / Local Storage)
    function initMeasureTools() {
        try {
            if (typeof cornerstoneTools === 'undefined' || typeof cornerstoneMath === 'undefined') {
                console.warn('[DICOM] tools lib no cargada');
                if (el.status) { el.status.textContent = 'Medicion no disponible (CDN bloqueado)'; el.status.className = 'badge bg-warning text-dark'; }
                return;
            }
            cornerstoneTools.external.cornerstone = cornerstone;
            cornerstoneTools.external.cornerstoneMath = cornerstoneMath;
            if (typeof Hammer !== 'undefined') cornerstoneTools.external.Hammer = Hammer;
            cornerstoneTools.init({ showSVGCursors: true, mouseEnabled: true, touchEnabled: true, globalToolSyncEnabled: true });
            cornerstoneTools.addTool(cornerstoneTools.LengthTool);
            cornerstoneTools.addTool(cornerstoneTools.AngleTool);
            cornerstoneTools.addTool(cornerstoneTools.EraserTool);
            toolsReady = true;
            console.log('[DICOM] Herramientas de medicion listas');
        } catch (err) { console.warn('[DICOM] No se pudo iniciar cornerstoneTools', err); }
    }
    function ensureStackState() {
        try {
            var st = cornerstoneTools.getToolState(el.dicomImage, 'stack');
            if (!st || !st.data || !st.data.length) {
                cornerstoneTools.addStackStateManager(el.dicomImage, ['stack']);
                cornerstoneTools.addToolState(el.dicomImage, 'stack', { currentImageIdIndex: 0, imageIds: [currentImageId] });
            }
            return true;
        } catch (err) { console.warn('[DICOM] stack state', err); return false; }
    }
    function enableMeasureTools(image) {
        if (!toolsReady) return;
        ensureStackState();
        try {
            cornerstoneTools.setToolDisabled('Length');
            cornerstoneTools.setToolDisabled('Angle');
        } catch (err) { console.warn('[DICOM] disable tools', err); }
        var cal = image && image.rowPixelSpacing && image.columnPixelSpacing;
        pixelSpacingNote = cal ? 'mm' : 'px (sin calibrar)';
        updateOverlay(image);
    }
    function setMeasure(name) {
        console.log('[DICOM] setMeasure click:', name, 'toolsReady=', toolsReady, 'image=', !!currentImageId);
        activeMeasure = null;
        document.querySelectorAll('#dcmToolbar [data-tool]').forEach(b => b.classList.remove('active'));
        ['btnDcmLength', 'btnDcmAngle'].forEach(id => { var b = document.getElementById(id); if (b) b.classList.remove('active'); });
        if (toolsReady && currentImageId) {
            try {
                cornerstoneTools.setToolPassive('Length');
                cornerstoneTools.setToolPassive('Angle');
            } catch (err) { console.warn('[DICOM] passive tools', err); }
        }
        if (!name) { el.dicomImage.style.cursor = 'default'; updateOverlay(); return; }
        if (typeof cornerstoneTools === 'undefined') {
            Swal.fire({ icon: 'error', title: 'Libreria de medicion no cargo', text: 'Abre F12-Network y verifica que cornerstoneTools.min.js y hammer.min.js den 200.' });
            return;
        }
        if (!currentImageId) {
            Swal.fire({ icon: 'info', title: 'Carga primero un .dcm', timer: 1500, showConfirmButton: false });
            return;
        }
        if (!toolsReady) { initMeasureTools(); }
        if (!toolsReady) {
            Swal.fire({ icon: 'error', title: 'No se pudo iniciar medicion', text: 'Abre F12-Consola y pasame el error de [DICOM].' });
            return;
        }
        try {
            if (name === 'Length') {
                cornerstoneTools.setToolActive('Length', { mouseButtonMask: 1 });
                cornerstoneTools.setToolPassive('Angle');
                var bl = document.getElementById('btnDcmLength'); if (bl) bl.classList.add('active');
            } else if (name === 'Angle') {
                cornerstoneTools.setToolActive('Angle', { mouseButtonMask: 1 });
                cornerstoneTools.setToolPassive('Length');
                var ba = document.getElementById('btnDcmAngle'); if (ba) ba.classList.add('active');
            }
            activeMeasure = name;
            ensureStackState();
        } catch (err) {
            console.warn('[DICOM] setMeasure', err);
            Swal.fire({ icon: 'error', title: 'Fallo al activar medicion', text: (err && err.message) || String(err) });
            return;
        }
        updateOverlay();
    }
    function rotateImage(deg) {
        if (!currentImageId) return;
        try {
            var viewport = cornerstone.getViewport(el.dicomImage);
            viewport.rotation = ((viewport.rotation || 0) + deg) % 360;
            cornerstone.setViewport(el.dicomImage, viewport);
            updateOverlay();
        } catch (err) { console.warn('[DICOM] rotate', err); }
    }
    function clearMeasures() {
        if (!toolsReady || !currentImageId) return;
        try {
            cornerstoneTools.clearToolState(el.dicomImage, 'Length');
            cornerstoneTools.clearToolState(el.dicomImage, 'Angle');
            cornerstone.updateImage(el.dicomImage);
        } catch (err) { console.warn('[DICOM] clearMeasures', err); }
    }
    window.DicomViewer = {
        loadFile: loadDicomFile,
        loadFromUrl: function (url, filenameHint) {
            if(!isCornerstoneReady){
                Swal.fire({icon:'error', title:'Visor no listo', text:'Espera 1s y reintenta'});
                return;
            }
            el.status.textContent = 'Cargando desde storage...'; el.status.className='badge bg-warning text-dark';
            loadedFileName = filenameHint || 'estudio.dcm';
            fetch(url).then(r=>{
                if(!r.ok) throw new Error('HTTP '+r.status);
                return r.arrayBuffer();
            }).then(buffer=>{
                try{
                    const byteArray = new Uint8Array(buffer);
                    try{ dicomDataSet = dicomParser.parseDicom(byteArray); showDicomTags(dicomDataSet); }catch(e){ console.warn('[DICOM] tags parse fail', e); }
                    const blob = new Blob([buffer], {type:'application/dicom'});
                    const file = new File([blob], loadedFileName, {type:'application/dicom'});
                    const imageId = cornerstoneWADOImageLoader.wadouri.fileManager.add(file);
                    currentImageId = imageId;
                    cornerstone.loadImage(imageId).then(image=>{
                        el.placeholder.style.display='none';
                        el.viewerContainer.style.display='block';
                        el.toolbar.style.display='flex';
                        const viewport = cornerstone.getDefaultViewportForImage(el.dicomImage, image);
                        cornerstone.displayImage(el.dicomImage, image, viewport);
                    enableMeasureTools(image);
                        cornerstone.resize(el.dicomImage, true);
                        el.status.textContent='Cargado: '+loadedFileName; el.status.className='badge bg-success';
                        updateOverlay(image); updateInfo();
                    }).catch(err=>{
                        console.error('[DICOM] loadImage url error', err);
                        el.status.textContent='Error render'; el.status.className='badge bg-danger';
                        Swal.fire({icon:'error', title:'No se pudo renderizar', text: err.message || 'DICOM corrupto/JPEG2000'});
                    });
                }catch(err){
                    console.error('[DICOM] loadFromUrl parse error', err);
                    el.status.textContent='Error'; el.status.className='badge bg-danger';
                    Swal.fire({icon:'error', title:'Error', text: err.message});
                }
            }).catch(err=>{
                console.error('[DICOM] fetch error', err);
                el.status.textContent='Error descarga'; el.status.className='badge bg-danger';
                Swal.fire({icon:'error', title:'No se pudo descargar', text: err.message});
            });
        },
        debug: function () { var ct = (typeof cornerstoneTools === 'undefined') ? null : cornerstoneTools; var dbgStore = { lenMode: null, lenMask: null, elRegistered: null, nTools: null };
            try {
                var tools = (ct.store && ct.store.state && ct.store.state.tools) || [];
                dbgStore.nTools = tools.length;
                dbgStore.elRegistered = (ct.store.state.enabledElements || []).indexOf(el.dicomImage) !== -1;
                for (var i = 0; i < tools.length; i++) {
                    var t = tools[i];
                    if (t && t.name === 'Length' && t.element === el.dicomImage) { dbgStore.lenMode = t.mode; try { dbgStore.lenMask = JSON.stringify(t.options && t.options.mouseButtonMask); } catch (e) { st.lenMask = '?'; } }
                }
            } catch (e) { dbgStore.err = String((e && e.message) || e); } var len = 0, stk = false; try { var s = ct ? (ct.getToolState(el.dicomImage, 'Length') || ct.getToolState(el.dicomImage, 'length')) : null; len = (s && s.data) ? s.data.length : 0; var st = ct ? ct.getToolState(el.dicomImage, 'stack') : null; stk = !!(st && st.data && st.data.length); } catch (e) { len = -1; } return { toolsReady: toolsReady, activeMeasure: activeMeasure, lengthMeasures: len, stackOk: stk, store: dbgStore, libs: { cornerstone: typeof cornerstone, math: typeof cornerstoneMath, hammer: typeof Hammer, tools: typeof cornerstoneTools }, api: ct ? { init: typeof ct.init, addTool: typeof ct.addTool, setActive: typeof ct.setToolActive, LengthTool: typeof ct.LengthTool, AngleTool: typeof ct.AngleTool } : null }; },
        reset: resetViewport,
        invert: invertImage
    };

    // Init on DOM ready
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();

    // Integrar con SetReadOnly() existente de continuar-consultas.js
    const origSetReadOnly = window.SetReadOnly;
    window.SetReadOnly = function () {
        if (typeof origSetReadOnly === 'function') origSetReadOnly();
        const inp = document.getElementById('dcmFileInput');
        if (inp) { inp.disabled = true; inp.title = 'Consulta terminada - solo lectura'; }
        const ph = document.getElementById('dcmPlaceholder');
        if (ph) ph.style.opacity = '0.6';
    };

})();
