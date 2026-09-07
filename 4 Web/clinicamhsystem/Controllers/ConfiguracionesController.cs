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
    private readonly ICategoriaProductoService _categoriaService;
    private readonly IProductoService _productoService;

    public ConfiguracionesController(
        ICatalogoIndicacionService catalogoService,
        IServiciosServices serviciosService,
        IRecetaServices recetaService,
        ICategoriaProductoService categoriaService,
        IProductoService productoService)
    {
        _catalogoService = catalogoService;
        _serviciosService = serviciosService;
        _recetaService = recetaService;
        _categoriaService = categoriaService;
        _productoService = productoService;
    }

    // Hub con accesos a todos los mantenimientos de catálogos
    public IActionResult Index()
    {
        ViewBag.TotalIndicaciones = _catalogoService.GetAll().Count;
        ViewBag.TotalServicios = _serviciosService.GetAll()?.Count ?? 0;
        ViewBag.TotalMedicamentos = _recetaService.GetAllMedicamentos()?.Count ?? 0;
        ViewBag.TotalCategorias = _categoriaService.GetAll(false).Count;
        ViewBag.TotalProductos = _productoService.GetAll(false).Count;
        ViewBag.StockBajo = _productoService.GetStockBajo().Count;
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

    // ===== Categorías de inventario (antes se administraban en Inventario/Index) =====
    public IActionResult Categorias()
    {
        _categoriaService.EnsureSeed();
        var list = _categoriaService.GetAll(false);
        return View(list);
    }

    [HttpPost]
    public IActionResult CrearCategoria(string nombre, string tipo, bool exigeLote, bool exigeVencimiento)
    {
        try { _categoriaService.Crear(nombre, tipo, exigeLote, exigeVencimiento); }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("Categorias");
    }

    [HttpPost]
    public IActionResult ToggleCategoria(Guid id)
    {
        var item = _categoriaService.Get(id);
        if (item != null) _categoriaService.CambiarActivo(id, !item.Activo);
        return RedirectToAction("Categorias");
    }

    [HttpPost]
    public IActionResult ActualizarCategoria(Guid id, string nombre, string tipo, bool exigeLote, bool exigeVencimiento)
    {
        try
        {
            var actual = _categoriaService.Get(id);
            if (actual is null) return RedirectToAction("Categorias");
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre requerido.");
            if (tipo != "Bien" && tipo != "Servicio") tipo = "Bien";
            actual.Nombre = nombre.Trim();
            actual.Tipo = tipo;
            actual.ExigeLoteDefault = exigeLote;
            actual.ExigeVencimientoDefault = exigeVencimiento;
            _categoriaService.Actualizar(actual);
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("Categorias");
    }
}
