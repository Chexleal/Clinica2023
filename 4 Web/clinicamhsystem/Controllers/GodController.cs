using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

/// <summary>God Mode: solo SuperAdmin. Administra hospitales, clínicas y accesos globales.</summary>
[SecurityFilter("SuperAdmin")]
public class GodController(
    IClinicaAdminService clinicaAdminService,
    IUserServices userServices,
    IObservabilidadService observabilidad,
    ICurrentUser currentUser) : Controller
{
    public IActionResult Index()
    {
        var hospitales = clinicaAdminService.GetHospitales();
        var clinicas = clinicaAdminService.GetClinicas();
        ViewBag.Hospitales = hospitales;
        ViewBag.TotalUsuarios = userServices.GetAll().Count;
        return View(clinicas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GuardarHospital(Hospital hospital)
    {
        if (string.IsNullOrWhiteSpace(hospital.Nombre))
        {
            TempData["Error"] = "El nombre del hospital es requerido.";
            return RedirectToAction("Index");
        }
        clinicaAdminService.GuardarHospital(hospital);
        TempData["Success"] = "Hospital guardado.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GuardarClinica(Clinica clinica)
    {
        if (string.IsNullOrWhiteSpace(clinica.Nombre) || clinica.IdHospital == Guid.Empty)
        {
            TempData["Error"] = "Nombre y hospital son requeridos.";
            return RedirectToAction("Index");
        }
        clinicaAdminService.GuardarClinica(clinica);
        TempData["Success"] = "Clínica guardada.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Usuarios()
    {
        ViewBag.Usuarios = userServices.GetAll();
        ViewBag.Clinicas = clinicaAdminService.GetClinicas(soloActivas: true);
        return View();
    }

    [HttpGet]
    public IActionResult Accesos(Guid id)
    {
        var usuario = userServices.GetUser(id);
        if (usuario is null) return NotFound();
        ViewBag.Usuario = usuario;
        ViewBag.Asignadas = userServices.GetClinicasDeUsuario(id);
        ViewBag.Clinicas = clinicaAdminService.GetClinicas(soloActivas: true);
        ViewBag.Permisos = userServices.GetPermissions(id).Select(p => p.Permiso).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Accesos(Guid idUsuario, List<Guid> clinicaIds, Guid? defaultClinicaId)
    {
        userServices.AsignarClinicas(idUsuario, clinicaIds ?? new List<Guid>(), defaultClinicaId);
        TempData["Success"] = "Accesos actualizados. Aplican en el próximo inicio de sesión del usuario.";
        return RedirectToAction("Accesos", new { id = idUsuario });
    }

    // ===== Logs y auditoría =====
    public IActionResult Logs()
    {
        ViewBag.Auditoria = observabilidad.GetAuditoriaReciente();
        return View();
    }

    [HttpPost]
    public IActionResult GetErroresTable(clinicaWeb.Models.DataTableRequest request, string? nivel, bool? resuelto)
    {
        var result = observabilidad.GetErrores(request.Start, request.Length, request.SearchValue, nivel, resuelto, request.SortDir);
        var data = result.Data.Select(e => new
        {
            e.IdErrorLog,
            fecha = e.FechaCreacion.HasValue ? e.FechaCreacion.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : "—",
            e.Nivel,
            e.TipoError,
            e.Mensaje,
            e.Ruta,
            e.Resuelto
        });
        return Json(new clinicaWeb.Models.DataTableResponse<object>
        {
            Draw = request.Draw,
            RecordsTotal = result.Total,
            RecordsFiltered = result.TotalFiltered,
            Data = data
        });
    }

    [HttpGet]
    public IActionResult GetError(Guid id)
    {
        var e = observabilidad.GetError(id);
        if (e is null) return NotFound();
        return Json(new
        {
            e.IdErrorLog,
            fecha = e.FechaCreacion.HasValue ? e.FechaCreacion.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : "—",
            e.Nivel,
            e.TipoError,
            e.Mensaje,
            e.Detalle,
            e.StackTrace,
            e.Ruta,
            e.MetodoHttp,
            e.TraceIdentifier,
            e.Resuelto,
            e.Observaciones
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResolverError(Guid id, bool resuelto, string? observaciones)
    {
        observabilidad.CambiarResuelto(id, resuelto, observaciones);
        return RedirectToAction("Logs");
    }
}
