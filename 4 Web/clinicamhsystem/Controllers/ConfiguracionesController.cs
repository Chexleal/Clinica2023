using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

[SecurityFilter("Configuraciones")]
public class ConfiguracionesController : Controller
{
    private readonly ICatalogoIndicacionService _catalogoService;
    private readonly IServiciosServices _serviciosService;
    private readonly IRecetaServices _recetaService;

    public ConfiguracionesController(
        ICatalogoIndicacionService catalogoService,
        IServiciosServices serviciosService,
        IRecetaServices recetaService)
    {
        _catalogoService = catalogoService;
        _serviciosService = serviciosService;
        _recetaService = recetaService;
    }

    // Hub con accesos a todos los mantenimientos de catálogos
    public IActionResult Index()
    {
        ViewBag.TotalIndicaciones = _catalogoService.GetAll().Count;
        ViewBag.TotalServicios = _serviciosService.GetAll()?.Count ?? 0;
        ViewBag.TotalMedicamentos = _recetaService.GetAllMedicamentos()?.Count ?? 0;
        return View();
    }

    // ===== Medicamentos (no tenía UI, se auto-creaba desde receta) =====
    public IActionResult Medicamentos()
    {
        var list = _recetaService.GetAllMedicamentos();
        return View(list);
    }

    [HttpPost]
    public IActionResult CrearMedicamento(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return BadRequest("Nombre requerido");
        _recetaService.CrearMedicamento(nombre);
        return RedirectToAction("Medicamentos");
    }

    [HttpPost]
    public IActionResult EliminarMedicamento(Guid id)
    {
        _recetaService.EliminarMedicamento(id);
        return RedirectToAction("Medicamentos");
    }
}
