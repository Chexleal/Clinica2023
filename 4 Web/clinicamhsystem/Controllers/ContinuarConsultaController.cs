using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using ClinicaServices.Storage;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using System.Globalization;
using System.Security.Policy;
using System.Text;
//using System.Web;
//using WkHtmlToPdfDotNet;
//using WkHtmlToPdfDotNet.Contracts;

namespace clinicaWeb.Controllers;

[SecurityFilter("ContinuarConsulta")]
public class ContinuarConsulta : Controller
{

    private readonly IConsultaServices _consultaServices;
    private readonly IRecetaServices _recetaServices;
    private readonly IPacienteServices _pacienteServices;
    private readonly ICitaServices _citasServices;
    private readonly IEstudioImagenService _estudioService;
    private readonly IOrdenEstudioService _ordenService;
    private readonly IStorageService _storage;
    private readonly ICatalogoIndicacionService _catalogoService;
    //private readonly IConverter _converter;

    public ContinuarConsulta(IConsultaServices consultaServices, IPacienteServices pacienteServices, IRecetaServices recetaServices,ICitaServices citaServices, IEstudioImagenService estudioService, IOrdenEstudioService ordenService, IStorageService storage, ICatalogoIndicacionService catalogoService/*, IConverter converter*/)
    {
        _consultaServices = consultaServices;
        _pacienteServices = pacienteServices;
        _recetaServices = recetaServices;
        _citasServices = citaServices;
        _estudioService = estudioService;
        _ordenService = ordenService;
        _storage = storage;
        _catalogoService = catalogoService;
        //_converter = converter;
    }


    public ActionResult Index(Guid consultaId)
    {
        var consulta = _consultaServices.GetConsulta(consultaId);
        consulta.PacienteInformacion ??= _pacienteServices.GetPacienteById(consulta.IdPaciente);
        consulta.PacienteInformacion.Consulta = _consultaServices.GetAllByPacienteId(consulta.IdPaciente);
        var receta = _recetaServices.GetByConsulta(consultaId) ?? new();
        var medicamentos = _recetaServices.GetAllMedicamentos() ?? new();
        // Estudios y órdenes para el área de carga
        var estudios = _estudioService.GetByPaciente(consulta.IdPaciente);
        var ordenes = _ordenService.GetByPaciente(consulta.IdPaciente);
        ViewBag.Estudios = estudios;
        ViewBag.Ordenes = ordenes;
        ViewBag.EsSoloLectura = true;
        ViewBag.Catalogo = _catalogoService.GetAll().Where(c=>c.Activo).ToList();
        ViewBag.CatalogoJson = System.Text.Json.JsonSerializer.Serialize(ViewBag.Catalogo);
        ViewBag.TiposEstudio = Enum.GetValues(typeof(TipoEstudio)).Cast<TipoEstudio>().Select(t => new { Id = (int)t, Nombre = t.ToString() }).ToList();
        //var paciente = _pacienteServices.GetPacienteById(consulta.IdPaciente);
        return View(new ConsultaContinuarViewModel { Consulta = consulta, Receta = receta,Medicamentos= medicamentos/*, Paciente = paciente */});
    }



    // POST: ConsultasController/Create
    [HttpPost]
    public ActionResult Guardar(Consulta consulta, string? antecedentes)
    {
        var consultaDb = _consultaServices.GetConsulta(consulta.IdConsulta);

        String duracion = DateTime.Now.Subtract(consultaDb.Fecha).Minutes.ToString();

        consultaDb.Diagnostico = consulta.Diagnostico;
        consultaDb.HistoriaClinica = consulta.HistoriaClinica;
        consultaDb.MotivoConsulta = consulta.MotivoConsulta;
        consultaDb.Observaciones = consulta.Observaciones;
        consultaDb.Peso = consulta.Peso;
        consultaDb.PresionArterial = consulta.PresionArterial;
        consultaDb.Radiografias = consulta.Radiografias;
        consultaDb.Temperatura = consulta.Temperatura;
        consultaDb.SaturacionOxigeno = consulta.SaturacionOxigeno;
        consultaDb.Glucometro = consulta.Glucometro;
        consultaDb.FrecuenciaCardiaca = consulta.FrecuenciaCardiaca;
        consultaDb.Terminada = consulta.Terminada;
        consultaDb.TiempoDuracion = duracion.ToString();

        _consultaServices.UpdateConsulta(consultaDb);
        _pacienteServices.UpdateAntecedentes(consultaDb.IdPaciente, antecedentes);
        if (consulta.Terminada) return RedirectToAction("Index", "Consultas");
        return RedirectToAction("Index", new { consultaId = consulta.IdConsulta });
    }

    [HttpPost]
    public ActionResult GuardarReceta(Receta receta)
    {
        var updReceta = _recetaServices.GetByConsulta(receta.IdConsulta);
        if (updReceta is null)
        {
            var recetaCreate = new Receta()
            {
                Descripcion = receta.Descripcion,
                IdConsulta = receta.IdConsulta
            };
            _recetaServices.Create(recetaCreate);
        }
        else
        {
            updReceta.Descripcion = receta.Descripcion;
            _recetaServices.Update(updReceta);
        }

        return RedirectToAction("Index", new { consultaId = receta.IdConsulta });
    }



    public ActionResult DescargarPdf(Guid consultaId)
    {
            var receta = _recetaServices.GetByConsulta(consultaId);
            var consulta = _consultaServices.GetConsulta(consultaId);
            var pacienteInfo = _pacienteServices.GetPacienteById(consulta.IdPaciente);
            var proximaCita = _citasServices.GetNextCita(consulta.Fecha, consulta.IdPaciente);

        return View("ConsultaPdf", new GenerarRecetaModel
        {
            Receta = receta,
            Consulta = consulta,
            Paciente = pacienteInfo,
            DetallesReceta = _recetaServices.GetAllDetalles(receta.IdReceta),
            CitaProx = proximaCita
        });
    }


    // GET: ConsultasController/Edit/5
    public ActionResult Editar(Guid id)
    {
        var consultas = _consultaServices.GetConsulta(id);
        return RedirectToAction("Editar", consultas);
    }

    //// POST: ConsultasController/Edit/5
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public ActionResult Edit(Consulta consulta)
    //{
    //    try
    //    {
    //        _consultaServices.UpdateConsulta(consulta);
    //        var consultas = _consultaServices.GetAll();
    //        return RedirectToAction("Index", consultas);
    //    }
    //    catch
    //    {
    //        return View("Error");
    //    }
    //}

    [HttpGet]
    public ActionResult GetMedicamentos(Guid idReceta)
    {
        var detalles=_recetaServices.GetAllDetalles(idReceta);

        return PartialView("Partials/_medicamentosTabla", new ConsultaContinuarViewModel {  DetallesReceta= detalles });
    }

    [HttpPost]
    public ActionResult DeleteMedicamentos(Guid idDetalleReceta)
    { 
        var idReceta = _recetaServices.DeleteDetalle(idDetalleReceta);

        return RedirectToAction("GetMedicamentos", new { idReceta });
    }


    [HttpPost]
    public ActionResult AddDetalleReceta(DetalleReceta detalleReceta)
    {
        _recetaServices.AddDetalleReceta(detalleReceta);

        return RedirectToAction("GetMedicamentos", new { idReceta = detalleReceta.IdReceta });
    }

    // ===== ÓRDENES =====
    [HttpPost]
    public IActionResult CrearOrden(Guid idPaciente, Guid? idConsulta, TipoEstudio tipo, string indicacion)
    {
        var orden = new OrdenEstudio { IdPaciente = idPaciente, IdConsulta = idConsulta, Tipo = tipo, Indicacion = indicacion ?? "", Estado = EstadoOrden.Pendiente };
        _ordenService.Crear(orden);
        return Json(new { ok = true, idOrden = orden.IdOrden });
    }

    [HttpGet]
    public IActionResult GetOrdenes(Guid idPaciente)
    {
        var ordenes = _ordenService.GetByPaciente(idPaciente);
        return PartialView("Partials/_ordenesTabla", ordenes);
    }

    [HttpPost]
    public IActionResult ActualizarEstadoOrden(Guid idOrden, EstadoOrden estado)
    {
        _ordenService.ActualizarEstado(idOrden, estado);
        return Json(new { ok = true });
    }

    [HttpPost]
    public IActionResult DeleteOrden(Guid idOrden, Guid idPaciente)
    {
        _ordenService.Eliminar(idOrden);
        var ordenes = _ordenService.GetByPaciente(idPaciente);
        return PartialView("Partials/_ordenesTabla", ordenes);
    }

    // ===== ESTUDIOS (carga con metadata: ruta, peso, quien, fecha) =====
    [HttpPost]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> UploadEstudio(Guid idPaciente, Guid? idConsulta, Guid? idOrden, TipoEstudio tipo, string titulo, string descripcion, List<IFormFile> files)
    {
        if (files == null || files.Count == 0) return BadRequest("Sin archivos");
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
        var estudios = _estudioService.GetByPaciente(idPaciente);
        ViewBag.EsSoloLectura = true;
        return PartialView("Partials/_estudiosLista", estudios);
    }

    [HttpGet]
    public IActionResult GetEstudios(Guid idPaciente)
    {
        var estudios = _estudioService.GetByPaciente(idPaciente);
        ViewBag.EsSoloLectura = true;
        return PartialView("Partials/_estudiosLista", estudios);
    }

    [HttpGet]
    public async Task<IActionResult> VerArchivo(Guid idArchivo)
    {
        var arch = _estudioService.GetArchivo(idArchivo);
        if (arch == null) return NotFound();
        var bytes = await _storage.ReadAsync(arch.RutaStorage);
        return File(bytes, arch.MimeType); // inline: el navegador muestra PDF/imagen en pestana
    }

    // Para visor DICOM via URL (fase 2 blob/local)
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
        var idPaciente = arch?.Estudio?.IdPaciente;
        await _estudioService.DeleteArchivoAsync(idArchivo, _storage);
        ViewBag.EsSoloLectura = true;
        if (idPaciente.HasValue)
        {
            var estudios = _estudioService.GetByPaciente(idPaciente.Value);
            return PartialView("Partials/_estudiosLista", estudios);
        }
        return Json(new { ok = true });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEstudio(Guid idEstudio)
    {
        var est = _estudioService.GetById(idEstudio);
        var idPaciente = est?.IdPaciente;
        await _estudioService.DeleteEstudioAsync(idEstudio, _storage);
        ViewBag.EsSoloLectura = true;
        if (idPaciente.HasValue)
        {
            var estudios = _estudioService.GetByPaciente(idPaciente.Value);
            return PartialView("Partials/_estudiosLista", estudios);
        }
        return Json(new { ok = true });
    }

}
