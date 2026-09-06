using ClinicaDomain;
using ClinicaServices;
using ClinicaServices.Storage;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

[SecurityFilter("Inicio")]
public class EstudiosController : Controller
{
    private readonly IEstudioImagenService _estudioService;
    private readonly IOrdenEstudioService _ordenService;
    private readonly IPacienteServices _pacienteServices;
    private readonly IStorageService _storage;
    private readonly ICatalogoIndicacionService _catalogoService;

    public EstudiosController(IEstudioImagenService estudioService, IOrdenEstudioService ordenService, IPacienteServices pacienteServices, IStorageService storage, ICatalogoIndicacionService catalogoService)
    {
        _estudioService = estudioService;
        _ordenService = ordenService;
        _pacienteServices = pacienteServices;
        _storage = storage;
        _catalogoService = catalogoService;
    }

    // Area de carga - usada por asistente/recepcion, no por el doctor en consulta
    public IActionResult Index(Guid? pacienteId, Guid? consultaId, Guid? idOrden)
    {
        ViewBag.PacienteId = pacienteId;
        ViewBag.ConsultaId = consultaId;
        ViewBag.PacienteActual = pacienteId.HasValue ? _pacienteServices.GetPacienteById(pacienteId.Value) : null;
        ViewBag.OrdenesPendientes = pacienteId.HasValue ? _ordenService.GetPendientesByPaciente(pacienteId.Value) : new List<OrdenEstudio>();
        var ordenSel = idOrden.HasValue ? _ordenService.GetById(idOrden.Value) : null;
        if (ordenSel != null && pacienteId.HasValue && ordenSel.IdPaciente != pacienteId.Value) ordenSel = null;
        ViewBag.IdOrdenSeleccionada = ordenSel?.IdOrden;
        ViewBag.TipoOrdenSeleccionada = ordenSel != null ? (int?)ordenSel.Tipo : null;
        ViewBag.Estudios = pacienteId.HasValue ? _estudioService.GetByPaciente(pacienteId.Value) : new List<EstudioImagen>();
        ViewBag.TodasPendientes = _ordenService.GetAllPendientes();
        ViewBag.EsSoloLectura = false;
        return View();
    }

    [HttpGet]
    public IActionResult GetEstudios(Guid idPaciente)
    {
        var estudios = _estudioService.GetByPaciente(idPaciente);
        ViewBag.TodasPendientes = _ordenService.GetAllPendientes();
        ViewBag.EsSoloLectura = false;
        return PartialView("~/Views/ContinuarConsulta/Partials/_estudiosLista.cshtml", estudios);
    }

    [HttpGet]
    public IActionResult GetOrdenes(Guid idPaciente)
    {
        var ordenes = _ordenService.GetByPaciente(idPaciente);
        return PartialView("~/Views/ContinuarConsulta/Partials/_ordenesTabla.cshtml", ordenes);
    }

    [HttpGet]
    public IActionResult GetPacientesSelect(string? q, int page = 1)
    {
        const int take = 20;
        var (datos, hayMas) = _pacienteServices.BuscarSelect(q, page, take);
        return Json(new { results = datos.Select(p => new { id = p.IdPaciente, text = p.Nombre + " " + p.Apellido + " - " + p.Dpi }), more = hayMas });
    }

    [HttpGet]
    public IActionResult GetOrdenesPendientes(Guid idPaciente)
    {
        var ordenes = _ordenService.GetPendientesByPaciente(idPaciente);
        return Json(ordenes.Select(o=> new { o.IdOrden, o.Tipo, o.Indicacion, Fecha=o.FechaOrden.ToString("dd/MM/yyyy") }));
    }

    [HttpGet]
    public IActionResult GetPendientesRefresh(Guid? idPaciente)
    {
        var todas = _ordenService.GetAllPendientes()
            .Select(o => new { idOrden = o.IdOrden, idPaciente = o.IdPaciente, fecha = o.FechaOrden.ToString("dd/MM/yyyy"), paciente = o.Paciente != null ? o.Paciente.Nombre + " " + o.Paciente.Apellido : "-", tipo = o.Tipo.ToString(), indicacion = o.Indicacion }).ToList();
        var pac = idPaciente.HasValue
            ? _ordenService.GetPendientesByPaciente(idPaciente.Value)
                .Select(o => new { idOrden = o.IdOrden, fecha = o.FechaOrden.ToString("dd/MM/yyyy"), tipo = o.Tipo.ToString(), tipoId = (int)o.Tipo, indicacion = o.Indicacion }).ToList<object>()
            : new List<object>();
        return Json(new { todas = todas, paciente = pac });
    }

    [HttpPost]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> UploadEstudio(Guid idPaciente, Guid? idConsulta, Guid? idOrden, TipoEstudio tipo, string titulo, string descripcion, List<IFormFile> files)
    {
        if (files == null || !files.Any()) return BadRequest("Sin archivos");
        var estudio = new EstudioImagen
        {
            IdPaciente = idPaciente,
            IdConsulta = idConsulta,
            IdOrden = idOrden,
            Tipo = tipo,
            Titulo = titulo ?? $"{tipo} {DateTime.Now:yyyy-MM-dd}",
            Descripcion = descripcion ?? "",
            Modalidad = tipo == TipoEstudio.Resonancia ? ModalidadDicom.MR : tipo == TipoEstudio.Tomografia ? ModalidadDicom.CT : ModalidadDicom.DX
        };
        await _estudioService.CrearEstudioAsync(estudio, files, _storage);
        if (idOrden.HasValue) _ordenService.ActualizarEstado(idOrden.Value, EstadoOrden.Completada);
        var estudios = idOrden.HasValue ? _estudioService.GetByOrden(idOrden.Value) : _estudioService.GetByPaciente(idPaciente);
        ViewBag.TodasPendientes = _ordenService.GetAllPendientes();
        ViewBag.EsSoloLectura = false;
        return PartialView("~/Views/ContinuarConsulta/Partials/_estudiosLista.cshtml", estudios);
    }

    [HttpGet]
    public IActionResult GetEstudiosPorOrden(Guid idOrden)
    {
        var estudios = _estudioService.GetByOrden(idOrden);
        ViewBag.EsSoloLectura = false;
        return PartialView("~/Views/ContinuarConsulta/Partials/_estudiosLista.cshtml", estudios);
    }

    [HttpGet]
    public async Task<IActionResult> VerArchivo(Guid idArchivo)
    {
        var arch = _estudioService.GetArchivo(idArchivo);
        if (arch == null) return NotFound();
        var bytes = await _storage.ReadAsync(arch.RutaStorage);
        return File(bytes, arch.MimeType); // inline: el navegador muestra PDF/imagen en pestana
    }

    [HttpGet]
    public async Task<IActionResult> GetArchivoBytes(Guid idArchivo)
    {
        var arch = _estudioService.GetArchivo(idArchivo);
        if (arch == null) return NotFound();
        var bytes = await _storage.ReadAsync(arch.RutaStorage);
        return File(bytes, arch.MimeType);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteArchivo(Guid idArchivo)
    {
        var arch = _estudioService.GetArchivo(idArchivo);
        Guid? idPaciente = arch?.Estudio?.IdPaciente;
        if (!idPaciente.HasValue && arch != null)
        {
            idPaciente = _estudioService.GetById(arch.IdEstudio)?.IdPaciente;
        }
        await _estudioService.DeleteArchivoAsync(idArchivo, _storage);
        if (idPaciente.HasValue)
        {
            var estudios = _estudioService.GetByPaciente(idPaciente.Value);
            ViewBag.TodasPendientes = _ordenService.GetAllPendientes();
        ViewBag.EsSoloLectura = false;
            return PartialView("~/Views/ContinuarConsulta/Partials/_estudiosLista.cshtml", estudios);
        }
        return Json(new { ok=true });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEstudio(Guid idEstudio)
    {
        var est = _estudioService.GetById(idEstudio);
        var idPaciente = est?.IdPaciente;
        await _estudioService.DeleteEstudioAsync(idEstudio, _storage);
        if (idPaciente.HasValue)
        {
            var estudios = _estudioService.GetByPaciente(idPaciente.Value);
            ViewBag.TodasPendientes = _ordenService.GetAllPendientes();
        ViewBag.EsSoloLectura = false;
            return PartialView("~/Views/ContinuarConsulta/Partials/_estudiosLista.cshtml", estudios);
        }
        return Json(new { ok=true });
    }

    // ===== CATALOGO =====
    public IActionResult Catalogo()
    {
        var list = _catalogoService.GetAll();
        return View(list);
    }

    [HttpPost]
    public IActionResult CrearCatalogo(TipoEstudio tipo, string codigo, string descripcion)
    {
        if(string.IsNullOrWhiteSpace(descripcion)) return BadRequest("Descripcion requerida");
        _catalogoService.Crear(new CatalogoIndicacion{ Tipo=tipo, Codigo=codigo??"", Descripcion=descripcion.Trim(), Activo=true });
        return RedirectToAction("Catalogo");
    }

    [HttpPost]
    public IActionResult ToggleCatalogo(Guid id)
    {
        var item = _catalogoService.GetById(id);
        if(item!=null){ item.Activo=!item.Activo; _catalogoService.Actualizar(item); }
        return RedirectToAction("Catalogo");
    }

    [HttpPost]
    public IActionResult EliminarCatalogo(Guid id)
    {
        _catalogoService.Eliminar(id);
        return RedirectToAction("Catalogo");
    }
}
