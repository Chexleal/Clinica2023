// DICOM Viewer - Fase 1: visualización local .dcm sin backend
// Stack: cornerstone-core 2.6.1 + dicomParser 1.8.7 + cornerstoneWADOImageLoader 4.13.2
// Fase 2: se agregará upload a Azure Blob (solo cambiar load de file a URL)
(function () {
    let isCornerstoneReady = false;
    let currentTool = 'WindowLevel'; // WindowLevel | Zoom | Pan
    let activeMeasure = null; // null | 'Length' | 'Angle'
    let toolsReady = false;
    let pixelSpacingNote = '';
    let isDragging = false;
    let dragView = null;
    let lastX = 0, lastY = 0;
    let loadedFileName = '';
    let dicomDataSet = null;
    let rxPlaylist = []; // [{idArch, nombre}]
    let panes = null; // {A:{...}, B:{...}} se crea al habilitar cornerstone
    let activePaneKey = 'A';
    let compareMode = false;

    // pane: {key, view, overlay, wrap, imageId, archId, fileName, idx, initVoi, stats}
    function P(key) { return panes ? panes[key || activePaneKey] : null; }
    function paneOfView(viewEl) {
        if (!panes || !viewEl) return null;
        if (viewEl === panes.A.view) return panes.A;
        if (viewEl === panes.B.view) return panes.B;
        return null;
    }

    const el = {
        fileInput: null,
        viewerContainer: null,
        dicomImage: null,
        dicomImageB: null,
        placeholder: null,
        toolbar: null,
        status: null,
        overlay: null,
        overlayB: null,
        paneA: null,
        paneB: null,
        btnCompare: null,
        info: null,
        tags: null,
        tagsContent: null
    };

    function init() {
        el.fileInput = document.getElementById('dcmFileInput');
        el.viewerContainer = document.getElementById('dicomViewerContainer');
        el.dicomImage = document.getElementById('dicomImage');
        el.dicomImageB = document.getElementById('dicomImageB');
        el.placeholder = document.getElementById('dcmPlaceholder');
        el.toolbar = document.getElementById('dcmToolbar');
        el.status = document.getElementById('dcmStatus');
        el.overlay = document.getElementById('dicomOverlay');
        el.overlayB = document.getElementById('dicomOverlayB');
        el.paneA = document.getElementById('dcmPaneA');
        el.paneB = document.getElementById('dcmPaneB');
        el.btnCompare = document.getElementById('btnDcmCompare');
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
            if (el.dicomImageB) cornerstone.enable(el.dicomImageB);
            panes = {
                A: { key: 'A', view: el.dicomImage, overlay: el.overlay, wrap: el.paneA, imageId: null, archId: null, fileName: '', idx: -1, initVoi: null, stats: null },
                B: { key: 'B', view: el.dicomImageB, overlay: el.overlayB, wrap: el.paneB, imageId: null, archId: null, fileName: '', idx: -1, initVoi: null, stats: null }
            };
            isCornerstoneReady = true;

            // Resize handler
            window.addEventListener('resize', () => {
                if (panes.A.imageId) cornerstone.resize(panes.A.view, true);
                if (panes.B.imageId) cornerstone.resize(panes.B.view, true);
            });

            // Viewer interactions (WW/WL, Zoom, Pan) por vista
            setupViewportInteractions(panes.A.view);
            if (panes.B.view) setupViewportInteractions(panes.B.view);

            // Clic en panel = activar esa vista
            [panes.A, panes.B].forEach(p => {
                if (p.wrap) {
                    p.wrap.addEventListener('mousedown', () => setActivePane(p.key));
                    p.wrap.addEventListener('touchstart', () => setActivePane(p.key), { passive: true });
                }
            });
            el.btnCompare?.addEventListener('click', () => toggleCompare());
            setActivePane('A');
            if (panes.B.overlay) panes.B.overlay.innerHTML = 'B<br><span style="color:#fff;opacity:.7">Clic aquí y luego una RX</span>';

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

        // Drag & drop: archivo .dcm local + RX arrastrada desde dock/lista (text/rx-arch)
        const dropTargets = [el.placeholder, el.viewerContainer].filter(Boolean);
        function hasRxArchPayload(e) {
            try {
                const types = e.dataTransfer ? Array.from(e.dataTransfer.types || []) : [];
                return types.includes('text/rx-arch') || types.includes('text/plain');
            } catch { return false; }
        }
        function extractRxArch(e) {
            try {
                let raw = e.dataTransfer.getData('text/rx-arch') || e.dataTransfer.getData('text/plain');
                if (!raw) return null;
                const parsed = JSON.parse(raw);
                if (parsed && parsed.idArch) return parsed;
                return null;
            } catch { return null; }
        }
        dropTargets.forEach(target => {
            ['dragenter', 'dragover'].forEach(evt => {
                target.addEventListener(evt, e => {
                    e.preventDefault(); e.stopPropagation();
                    if (el.placeholder) el.placeholder.classList.add('dragover');
                    if (el.viewerContainer) el.viewerContainer.classList.add('dcm-dragover');
                    if (e.dataTransfer) e.dataTransfer.dropEffect = 'copy';
                });
            });
            ['dragleave', 'drop'].forEach(evt => {
                target.addEventListener(evt, e => {
                    e.preventDefault(); e.stopPropagation();
                    if (evt === 'dragleave' && e.relatedTarget && target.contains(e.relatedTarget)) return;
                    if (el.placeholder) el.placeholder.classList.remove('dragover');
                    if (el.viewerContainer) el.viewerContainer.classList.remove('dcm-dragover');
                });
            });
        });
        el.placeholder.addEventListener('drop', e => {
            const rx = hasRxArchPayload(e) ? extractRxArch(e) : null;
            if (rx) { loadFromArch(rx.idArch, rx.nombre); return; }
            const file = e.dataTransfer.files[0];
            if (file && file.name.toLowerCase().endsWith('.dcm')) {
                // sincronizar input
                const dt = new DataTransfer(); dt.items.add(file); el.fileInput.files = dt.files;
                loadDicomFile(file);
            } else if (file) {
                Swal.fire({ icon: 'warning', title: 'Solo .dcm', text: 'Para ver una RX del paciente arrastra su tarjeta; o selecciona un archivo DICOM (.dcm)' });
            }
        });
        // Drop sobre viewer: si cae en panel B, carga ahí
        el.viewerContainer.addEventListener('drop', e => {
            const paneEl = e.target && e.target.closest ? e.target.closest('.dcm-pane') : null;
            if (paneEl && paneEl.getAttribute('data-pane') === 'B') setActivePane('B');
            else if (paneEl) setActivePane('A');
            const rx = hasRxArchPayload(e) ? extractRxArch(e) : null;
            if (rx) { loadFromArch(rx.idArch, rx.nombre); return; }
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
        // Al entrar/salir de fullscreen el layout cambia: re-ajustar ambas vistas
        document.addEventListener('fullscreenchange', () => setTimeout(resizeAllViews, 250));
        document.getElementById('dcmWwRange')?.addEventListener('input', function () {
            const pane = P();
            if (!pane || !pane.imageId) return;
            var viewport = cornerstone.getViewport(pane.view);
            viewport.voi.windowWidth = Math.max(1, parseFloat(this.value));
            cornerstone.setViewport(pane.view, viewport);
            updateOverlay(null, pane.key);
        });
        document.getElementById('dcmWlRange')?.addEventListener('input', function () {
            const pane = P();
            if (!pane || !pane.imageId) return;
            var viewport = cornerstone.getViewport(pane.view);
            viewport.voi.windowCenter = parseFloat(this.value);
            cornerstone.setViewport(pane.view, viewport);
            updateOverlay(null, pane.key);
        });
        document.getElementById('btnDcmLength')?.addEventListener('click', () => setMeasure(activeMeasure === 'Length' ? null : 'Length'));
        document.getElementById('btnDcmRotL')?.addEventListener('click', () => rotateImage(-90));
        document.getElementById('btnDcmRotR')?.addEventListener('click', () => rotateImage(90));
        document.getElementById('btnDcmClear')?.addEventListener('click', clearMeasures);

        // Prev / Next RX (header del visor, actúan sobre la vista activa A/B)
        document.getElementById('btnDcmPrev')?.addEventListener('click', () => stepPlaylist(-1));
        document.getElementById('btnDcmNext')?.addEventListener('click', () => stepPlaylist(1));
        document.addEventListener('keydown', e => {
            const tag = (document.activeElement && document.activeElement.tagName) || '';
            if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;
            const pane = P();
            if (!pane || !pane.imageId || rxPlaylist.length < 2) return;
            if (e.key === 'ArrowRight') { e.preventDefault(); stepPlaylist(1); }
            else if (e.key === 'ArrowLeft') { e.preventDefault(); stepPlaylist(-1); }
        });
    }

    function setupViewportInteractions(viewEl) {
        if (!viewEl) return;
        viewEl.addEventListener('mousedown', e => {
            const pane = paneOfView(viewEl);
            if (!pane || !pane.imageId) return;
            if (activeMeasure) return;
            isDragging = true;
            dragView = viewEl;
            lastX = e.clientX; lastY = e.clientY;
            e.preventDefault();
        });
        window.addEventListener('mouseup', () => { isDragging = false; dragView = null; });
        window.addEventListener('mousemove', e => {
            if (!isDragging || !dragView) return;
            const pane = paneOfView(dragView);
            if (!pane || !pane.imageId) return;
            const deltaX = e.clientX - lastX;
            const deltaY = e.clientY - lastY;
            lastX = e.clientX; lastY = e.clientY;

            const viewport = cornerstone.getViewport(dragView);
            if (currentTool === 'WindowLevel') {
                // WW = ancho ventana, WL = centro ventana
                // deltaX -> WW, deltaY -> WL
                viewport.voi.windowWidth = Math.max(1, viewport.voi.windowWidth + deltaX * 2);
                viewport.voi.windowCenter = viewport.voi.windowCenter + deltaY * 2;
                cornerstone.setViewport(dragView, viewport);
                updateOverlay(null, pane.key);
            } else if (currentTool === 'Pan') {
                viewport.translation.x += deltaX / viewport.scale;
                viewport.translation.y += deltaY / viewport.scale;
                cornerstone.setViewport(dragView, viewport);
            } else if (currentTool === 'Zoom') {
                const zoomFactor = 1 + deltaY * -0.01;
                viewport.scale = Math.max(0.1, Math.min(10, viewport.scale * zoomFactor));
                cornerstone.setViewport(dragView, viewport);
                updateOverlay(null, pane.key);
            }
        });

        // Wheel zoom sobre esta vista
        viewEl.addEventListener('wheel', e => {
            const pane = paneOfView(viewEl);
            if (!pane || !pane.imageId) return;
            e.preventDefault();
            const viewport = cornerstone.getViewport(viewEl);
            const delta = e.deltaY > 0 ? 0.9 : 1.1;
            viewport.scale = Math.max(0.1, Math.min(10, viewport.scale * delta));
            cornerstone.setViewport(viewEl, viewport);
            updateOverlay(null, pane.key);
        }, { passive: false });

        // Touch support
        let lastTouchDist = null;
        viewEl.addEventListener('touchstart', e => {
            if (activeMeasure) return;
            if (e.touches.length === 1) {
                isDragging = true;
                dragView = viewEl;
                lastX = e.touches[0].clientX; lastY = e.touches[0].clientY;
            } else if (e.touches.length === 2) {
                isDragging = false;
                dragView = null;
                lastTouchDist = Math.hypot(e.touches[0].clientX - e.touches[1].clientX, e.touches[0].clientY - e.touches[1].clientY);
            }
        }, { passive: false });
        viewEl.addEventListener('touchmove', e => {
            const pane = paneOfView(viewEl);
            if (!pane || !pane.imageId) return;
            e.preventDefault();
            if (e.touches.length === 1 && isDragging && dragView === viewEl) {
                const deltaX = e.touches[0].clientX - lastX;
                const deltaY = e.touches[0].clientY - lastY;
                lastX = e.touches[0].clientX; lastY = e.touches[0].clientY;
                const viewport = cornerstone.getViewport(viewEl);
                if (currentTool === 'WindowLevel') {
                    viewport.voi.windowWidth = Math.max(1, viewport.voi.windowWidth + deltaX * 2);
                    viewport.voi.windowCenter = viewport.voi.windowCenter + deltaY * 2;
                } else if (currentTool === 'Pan') {
                    viewport.translation.x += deltaX / viewport.scale;
                    viewport.translation.y += deltaY / viewport.scale;
                }
                cornerstone.setViewport(viewEl, viewport);
                updateOverlay(null, pane.key);
            } else if (e.touches.length === 2) {
                const dist = Math.hypot(e.touches[0].clientX - e.touches[1].clientX, e.touches[0].clientY - e.touches[1].clientY);
                if (lastTouchDist) {
                    const viewport = cornerstone.getViewport(viewEl);
                    viewport.scale = Math.max(0.1, Math.min(10, viewport.scale * (dist / lastTouchDist)));
                    cornerstone.setViewport(viewEl, viewport);
                    updateOverlay(null, pane.key);
                }
                lastTouchDist = dist;
            }
        }, { passive: false });
        viewEl.addEventListener('touchend', () => { isDragging = false; dragView = null; lastTouchDist = null; });
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
        const pane = P();
        if (!pane) return;
        loadedFileName = file.name;
        pane.archId = null;
        pane.fileName = file.name;
        el.status.textContent = 'Cargando...'; el.status.className = 'badge bg-warning text-dark';
        el.tags.style.display = 'none';

        // Leer como ArrayBuffer para extraer tags también
        const reader = new FileReader();
        reader.onload = function (e) {
            afterBufferLoaded(pane, e.target.result, file, file.name);
        };
        reader.onerror = () => {
            Swal.fire({ icon: 'error', title: 'Error lectura', text: 'No se pudo leer el archivo' });
            el.status.textContent = 'Error lectura'; el.status.className = 'badge bg-danger';
        };
        reader.readAsArrayBuffer(file);
    }

    // Punto común: buffer DICOM ya en memoria -> tags + render en el panel dado
    function afterBufferLoaded(pane, arrayBuffer, loaderFile, displayName) {
        try {
            const byteArray = new Uint8Array(arrayBuffer);
            try {
                dicomDataSet = dicomParser.parseDicom(byteArray);
                showDicomTags(dicomDataSet, pane);
            } catch (tagErr) {
                console.warn('[DICOM] No se pudieron leer tags', tagErr);
            }

            const imageId = cornerstoneWADOImageLoader.wadouri.fileManager.add(loaderFile);
            pane.imageId = imageId;
            if (pane.archId) {
                const i = rxPlaylist.findIndex(x => String(x.idArch) === String(pane.archId));
                if (i >= 0) pane.idx = i;
            }

            cornerstone.loadImage(imageId).then(image => {
                pane.fileName = displayName;
                showImageInPane(pane, image, displayName);
                markActiveRx();
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
    }

    function showImageInPane(pane, image, displayName) {
        // Mostrar viewer
        el.placeholder.style.display = 'none';
        el.viewerContainer.style.display = 'block';
        el.toolbar.style.display = 'flex';

        const viewport = cornerstone.getDefaultViewportForImage(pane.view, image);
        cornerstone.displayImage(pane.view, image, viewport);
        enableMeasureTools(image, pane.view);
        // Fit to window (ambas vistas si comparar está activo)
        cornerstone.resize(pane.view, true);
        if (compareMode) {
            if (panes.A.imageId) cornerstone.resize(panes.A.view, true);
            if (panes.B.imageId) cornerstone.resize(panes.B.view, true);
        }

        el.status.textContent = 'Cargado: ' + displayName;
        el.status.className = 'badge bg-success';
        updateOverlay(image, pane.key);
        updateInfo();
    }

    function showDicomTags(dataSet, pane) {
        const target = pane || P();
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
            // Overlay detallado (en el panel que cargó)
            const ov = target && target.overlay ? target.overlay : el.overlay;
            const label = target && target.fileName ? target.fileName : loadedFileName;
            ov.innerHTML = `
                ${patientName ? patientName : label}<br>
                ${modality || ''} ${bodyPart || ''} ${studyDate || ''}<br>
                <span style="color:#fff; opacity:0.8">${fileSizeToStr(dicomDataSet?.byteArray?.length || 0)}</span>
            `;
        } catch (e) { console.warn('[DICOM] tags error', e); }
    }

    function updateOverlay(image, paneKey) {
        const pane = panes ? panes[paneKey || activePaneKey] : null;
        if (!pane || !pane.imageId) return;
        const viewport = cornerstone.getViewport(pane.view);
        const ww = Math.round(viewport.voi.windowWidth);
        const wc = Math.round(viewport.voi.windowCenter);
        const scale = (viewport.scale * 100).toFixed(0);
        const invert = viewport.invert ? 'Sí' : 'No';
        const tag = compareMode ? `Vista ${pane.key} · ` : '';
        el.info.textContent = `${tag}WW: ${ww}  WL: ${wc}  Zoom: ${scale}%  Invert: ${invert}  Tool: ${currentTool}` + (activeMeasure ? `  Medida: ${activeMeasure} (${pixelSpacingNote})` : '');
        if (pane.key === activePaneKey) syncWlPanel(ww, wc);
        if (image) {
            if (!pane.initVoi) { pane.initVoi = { ww: ww, wc: wc }; pane.stats = { min: image.minPixelValue, max: image.maxPixelValue }; }
            if (pane.overlay) pane.overlay.innerHTML = `${pane.fileName}<br> ${image.width} x ${image.height} · ${image.minPixelValue} / ${image.maxPixelValue}`;
        }
    }

    function syncWlPanel(ww, wc) {
        var panel = document.getElementById('dcmWlPanel');
        if (!panel) return;
        const pane = P();
        var show = currentTool === 'WindowLevel' && !!pane && !!pane.imageId && !activeMeasure;
        panel.style.display = show ? 'block' : 'none';
        if (!show) return;
        var wwR = document.getElementById('dcmWwRange');
        var wlR = document.getElementById('dcmWlRange');
        if (wwR) {
            if (pane.stats) wwR.max = Math.max(100, pane.stats.max - pane.stats.min);
            if (document.activeElement !== wwR) wwR.value = ww;
        }
        if (wlR) {
            if (pane.stats) { wlR.min = pane.stats.min; wlR.max = pane.stats.max; }
            if (document.activeElement !== wlR) wlR.value = wc;
        }
        var set = function (id, txt) { var n = document.getElementById(id); if (n) n.textContent = txt; };
        set('dcmWwVal', ww);
        set('dcmWlVal', wc);
        if (pane.initVoi) {
            set('dcmWwPct', '(' + Math.round(ww / pane.initVoi.ww * 100) + '%)');
            set('dcmWlPct', '(' + Math.round(wc / pane.initVoi.wc * 100) + '%)');
        }
    }
    function updateInfo() { updateOverlay(); }

    function resetViewport() {
        const pane = P();
        if (!pane || !pane.imageId) return;
        cornerstone.reset(pane.view);
        updateOverlay(null, pane.key);
    }

    function invertImage() {
        const pane = P();
        if (!pane || !pane.imageId) return;
        const viewport = cornerstone.getViewport(pane.view);
        viewport.invert = !viewport.invert;
        cornerstone.setViewport(pane.view, viewport);
        updateOverlay(null, pane.key);
    }

    function toggleFullscreen() {
        if (!el.viewerContainer) return;
        if (!document.fullscreenElement) {
            el.viewerContainer.requestFullscreen().then(() => {
                setTimeout(resizeAllViews, 200);
            }).catch(err => console.error(err));
        } else {
            document.exitFullscreen();
        }
    }

    function resizeAllViews() {
        if (!panes) return;
        try {
            if (panes.A.imageId) cornerstone.resize(panes.A.view, true);
            if (panes.B.imageId && compareMode) cornerstone.resize(panes.B.view, true);
        } catch (err) { console.warn('[DICOM] resize', err); }
    }

    function fileSizeToStr(bytes) {
        if (!bytes) return '';
        const kb = bytes / 1024;
        if (kb < 1024) return kb.toFixed(1) + ' KB';
        return (kb / 1024).toFixed(2) + ' MB';
    }

    // ===== Playlist RX + vistas A/B =====
    function setPlaylist(list) {
        rxPlaylist = Array.isArray(list) ? list : [];
        if (panes) {
            [panes.A, panes.B].forEach(pane => {
                if (pane.archId) {
                    const i = rxPlaylist.findIndex(x => String(x.idArch) === String(pane.archId));
                    pane.idx = i >= 0 ? i : -1;
                } else if (pane.imageId && pane.idx < 0 && rxPlaylist.length) {
                    pane.idx = 0;
                }
                if (!rxPlaylist.length) pane.idx = -1;
            });
        }
        updateCounter();
        markActiveRx();
    }
    function updateCounter() {
        const c = document.getElementById('dcmCounter');
        if (!c) return;
        if (!rxPlaylist.length) { c.textContent = '0/0'; return; }
        if (compareMode && panes) {
            const a = panes.A.idx >= 0 ? panes.A.idx + 1 : '–';
            const b = panes.B.idx >= 0 ? panes.B.idx + 1 : '–';
            c.textContent = `A:${a} B:${b}/${rxPlaylist.length}`;
        } else if (panes) {
            c.textContent = panes[activePaneKey].idx >= 0 ? `${panes[activePaneKey].idx + 1}/${rxPlaylist.length}` : `0/${rxPlaylist.length}`;
        }
    }
    function markActiveRx() {
        if (!panes) return;
        const ids = new Set([panes.A.archId, panes.B.archId].filter(Boolean).map(String));
        document.querySelectorAll('.rx-side-item').forEach(n => {
            const id = n.getAttribute('data-arch');
            n.classList.toggle('active', !!id && ids.has(String(id)));
        });
    }
    function setActivePane(key) {
        if (!panes || !panes[key]) return;
        activePaneKey = key;
        [panes.A, panes.B].forEach(p => { if (p.wrap) p.wrap.classList.toggle('active', p.key === key); });
        updateInfo();
        updateCounter();
    }
    function toggleCompare(force) {
        if (!panes) return;
        compareMode = typeof force === 'boolean' ? force : !compareMode;
        if (el.btnCompare) el.btnCompare.classList.toggle('active', compareMode);
        if (el.viewerContainer) el.viewerContainer.classList.toggle('compare', compareMode);
        if (panes.B.wrap) panes.B.wrap.style.display = compareMode ? 'block' : 'none';
        if (compareMode) {
            // Al abrir comparar, B muestra la siguiente RX si A ya tiene una
            if (!panes.B.imageId && rxPlaylist.length > 1) {
                const nextIdx = panes.A.idx >= 0 ? (panes.A.idx + 1) % rxPlaylist.length : 0;
                const item = rxPlaylist[nextIdx];
                setActivePane('B');
                if (item) loadFromArch(item.idArch, item.nombre);
            } else if (panes.B.imageId) {
                setActivePane('B');
            }
            if (panes.A.imageId) cornerstone.resize(panes.A.view, true);
            if (panes.B.imageId) setTimeout(() => cornerstone.resize(panes.B.view, true), 50);
        } else {
            setActivePane('A');
            if (panes.A.imageId) cornerstone.resize(panes.A.view, true);
        }
        updateCounter();
        updateInfo();
    }
    function scrollToViewer(flash = true) {
        const card = document.getElementById('cardRadiografias');
        if (!card) return;
        card.scrollIntoView({ behavior: 'smooth', block: 'start' });
        if (flash) {
            card.classList.remove('dcm-highlight');
            void card.offsetWidth;
            card.classList.add('dcm-highlight');
            setTimeout(() => card.classList.remove('dcm-highlight'), 1400);
        }
    }
    function loadFromArch(idArch, nombre) {
        if (!idArch || !panes) return;
        const pane = P();
        const i = rxPlaylist.findIndex(x => String(x.idArch) === String(idArch));
        if (i >= 0) pane.idx = i;
        pane.archId = idArch;
        pane.fileName = nombre || 'estudio.dcm';
        updateCounter(); markActiveRx();
        const url = `/ContinuarConsulta/GetArchivoBytes?idArchivo=${idArch}`;
        if (window.DicomViewer && window.DicomViewer.loadFromUrl) {
            window.DicomViewer.loadFromUrl(url, nombre || 'estudio.dcm', idArch);
        }
        scrollToViewer(true);
    }
    function stepPlaylist(dir) {
        if (!panes) return;
        if (!rxPlaylist.length) {
            Swal.fire({ icon: 'info', title: 'Sin RX', text: 'Este paciente aún no tiene radiografías.', timer: 1500, showConfirmButton: false });
            return;
        }
        const pane = P();
        const start = pane.idx >= 0 ? pane.idx : (dir > 0 ? -1 : 0);
        pane.idx = ((start + dir) % rxPlaylist.length + rxPlaylist.length) % rxPlaylist.length;
        const item = rxPlaylist[pane.idx];
        if (item) loadFromArch(item.idArch, item.nombre);
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
    function ensureStackState(viewEl) {
        const target = viewEl || (panes ? panes[activePaneKey].view : null);
        const pane = paneOfView(target) || P();
        try {
            var st = cornerstoneTools.getToolState(target, 'stack');
            if (!st || !st.data || !st.data.length) {
                cornerstoneTools.addStackStateManager(target, ['stack']);
                cornerstoneTools.addToolState(target, 'stack', { currentImageIdIndex: 0, imageIds: [pane ? pane.imageId : null] });
            }
            return true;
        } catch (err) { console.warn('[DICOM] stack state', err); return false; }
    }
    function enableMeasureTools(image, viewEl) {
        if (!toolsReady) return;
        ensureStackState(viewEl);
        try {
            cornerstoneTools.setToolDisabled('Length');
            cornerstoneTools.setToolDisabled('Angle');
        } catch (err) { console.warn('[DICOM] disable tools', err); }
        var cal = image && image.rowPixelSpacing && image.columnPixelSpacing;
        pixelSpacingNote = cal ? 'mm' : 'px (sin calibrar)';
    }
    function setMeasure(name) {
        const pane = P();
        const hasImage = !!(pane && pane.imageId);
        console.log('[DICOM] setMeasure click:', name, 'toolsReady=', toolsReady, 'image=', hasImage);
        activeMeasure = null;
        document.querySelectorAll('#dcmToolbar [data-tool]').forEach(b => b.classList.remove('active'));
        ['btnDcmLength', 'btnDcmAngle'].forEach(id => { var b = document.getElementById(id); if (b) b.classList.remove('active'); });
        if (toolsReady && hasImage) {
            try {
                cornerstoneTools.setToolPassive('Length');
                cornerstoneTools.setToolPassive('Angle');
            } catch (err) { console.warn('[DICOM] passive tools', err); }
        }
        const setCursor = v => { if (panes && panes[v].view) panes[v].view.style.cursor = 'default'; };
        if (!name) { setCursor('A'); setCursor('B'); updateOverlay(); return; }
        if (typeof cornerstoneTools === 'undefined') {
            Swal.fire({ icon: 'error', title: 'Libreria de medicion no cargo', text: 'Abre F12-Network y verifica que cornerstoneTools.min.js y hammer.min.js den 200.' });
            return;
        }
        if (!hasImage) {
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
        const pane = P();
        if (!pane || !pane.imageId) return;
        try {
            var viewport = cornerstone.getViewport(pane.view);
            viewport.rotation = ((viewport.rotation || 0) + deg) % 360;
            cornerstone.setViewport(pane.view, viewport);
            updateOverlay(null, pane.key);
        } catch (err) { console.warn('[DICOM] rotate', err); }
    }
    function clearMeasures() {
        const pane = P();
        if (!toolsReady || !pane || !pane.imageId) return;
        try {
            cornerstoneTools.clearToolState(pane.view, 'Length');
            cornerstoneTools.clearToolState(pane.view, 'Angle');
            cornerstone.updateImage(pane.view);
        } catch (err) { console.warn('[DICOM] clearMeasures', err); }
    }
    window.DicomViewer = {
        loadFile: loadDicomFile,
        loadFromUrl: function (url, filenameHint, idArch) {
            if(!isCornerstoneReady){
                Swal.fire({icon:'error', title:'Visor no listo', text:'Espera 1s y reintenta'});
                return;
            }
            const pane = P();
            if (!pane) return;
            if (idArch) {
                pane.archId = idArch;
                pane.fileName = filenameHint || 'estudio.dcm';
                const i = rxPlaylist.findIndex(x => String(x.idArch) === String(idArch));
                if (i >= 0) pane.idx = i;
                updateCounter(); markActiveRx();
            }
            el.status.textContent = 'Cargando desde storage...'; el.status.className='badge bg-warning text-dark';
            loadedFileName = filenameHint || 'estudio.dcm';
            fetch(url).then(r=>{
                if(!r.ok) throw new Error('HTTP '+r.status);
                return r.arrayBuffer();
            }).then(buffer=>{
                try{
                    const blob = new Blob([buffer], {type:'application/dicom'});
                    const file = new File([blob], loadedFileName, {type:'application/dicom'});
                    afterBufferLoaded(pane, buffer, file, loadedFileName);
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
        debug: function () { return { toolsReady: toolsReady, activeMeasure: activeMeasure, activePane: activePaneKey, compare: compareMode, panes: panes ? { A: { hasImage: !!panes.A.imageId, idx: panes.A.idx, arch: panes.A.archId }, B: { hasImage: !!panes.B.imageId, idx: panes.B.idx, arch: panes.B.archId } } : null, playlist: rxPlaylist.length }; },
        reset: resetViewport,
        invert: invertImage,
        setPlaylist: setPlaylist,
        loadFromArch: loadFromArch,
        next: function(){ stepPlaylist(1); },
        prev: function(){ stepPlaylist(-1); },
        scrollToViewer: scrollToViewer,
        getPlaylist: function(){ return rxPlaylist; },
        toggleCompare: toggleCompare,
        setActivePane: setActivePane,
        isCompare: function(){ return compareMode; }
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
