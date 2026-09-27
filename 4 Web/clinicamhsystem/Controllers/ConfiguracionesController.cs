using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

[SecurityFilter("Configuraciones")]
public class ConfiguracionesController(
    ICatalogoIndicacionService catalogoService,
    IServiciosServices serviciosService,
    IRecetaServices recetaService,
    ICategoriaProductoService categoriaService,
    IProductoService productoService,
    ICategoriaGastoService categoriaGastoService) : Controller
{

    // Hub con accesos a todos los mantenimientos de catálogos
    public IActionResult Index()
    {
        ViewBag.TotalIndicaciones = catalogoService.GetAll().Count;
        ViewBag.TotalServicios = serviciosService.GetAll()?.Count ?? 0;
        ViewBag.TotalMedicamentos = recetaService.GetAllMedicamentos()?.Count ?? 0;
        ViewBag.TotalCategorias = categoriaService.GetAll(false).Count;
        ViewBag.TotalProductos = productoService.GetAll(false).Count;
        ViewBag.StockBajo = productoService.GetStockBajo().Count;
        ViewBag.TotalCategoriasGasto = categoriaGastoService.GetAll(false).Count;
        return View();
    }

    // ===== Medicamentos (no tenía UI, se auto-creaba desde receta) =====
    public IActionResult Medicamentos()
    {
        var list = recetaService.GetAllMedicamentos();
        return View(list);
    }

    [HttpPost]
    public IActionResult CrearMedicamento(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return BadRequest("Nombre requerido");
        recetaService.CrearMedicamento(nombre);
        return RedirectToAction("Medicamentos");
    }

    [HttpPost]
    public IActionResult EliminarMedicamento(Guid id)
    {
        recetaService.EliminarMedicamento(id);
        return RedirectToAction("Medicamentos");
    }

    // ===== Categorías de inventario (antes se administraban en Inventario/Index) =====
    public IActionResult Categorias()
    {
        categoriaService.EnsureSeed();
        var list = categoriaService.GetAll(false);
        return View(list);
    }

    [HttpPost]
    public IActionResult CrearCategoria(string nombre, string tipo, bool exigeLote, bool exigeVencimiento)
    {
        try { categoriaService.Crear(nombre, tipo, exigeLote, exigeVencimiento); }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("Categorias");
    }

    [HttpPost]
    public IActionResult ToggleCategoria(Guid id)
    {
        var item = categoriaService.Get(id);
        if (item != null) categoriaService.CambiarActivo(id, !item.Activo);
        return RedirectToAction("Categorias");
    }

    [HttpPost]
    public IActionResult ActualizarCategoria(Guid id, string nombre, string tipo, bool exigeLote, bool exigeVencimiento)
    {
        try
        {
            var actual = categoriaService.Get(id);
            if (actual is null) return RedirectToAction("Categorias");
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre requerido.");
            if (tipo != "Bien" && tipo != "Servicio") tipo = "Bien";
            actual.Nombre = nombre.Trim();
            actual.Tipo = tipo;
            actual.ExigeLoteDefault = exigeLote;
            actual.ExigeVencimientoDefault = exigeVencimiento;
            categoriaService.Actualizar(actual);
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("Categorias");
    }

    // ===== Categorías de gasto =====
    public IActionResult CategoriasGasto()
    {
        categoriaGastoService.EnsureSeed();
        var list = categoriaGastoService.GetAll(false);
        return View(list);
    }

    [HttpPost]
    public IActionResult CrearCategoriaGasto(string nombre)
    {
        try { categoriaGastoService.Crear(nombre); }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("CategoriasGasto");
    }

    [HttpPost]
    public IActionResult ToggleCategoriaGasto(Guid id)
    {
        var item = categoriaGastoService.Get(id);
        if (item != null) categoriaGastoService.CambiarActivo(id, !item.Activo);
        return RedirectToAction("CategoriasGasto");
    }

    [HttpPost]
    public IActionResult ActualizarCategoriaGasto(Guid id, string nombre)
    {
        try
        {
            var actual = categoriaGastoService.Get(id);
            if (actual is null) return RedirectToAction("CategoriasGasto");
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre requerido.");
            actual.Nombre = nombre.Trim();
            categoriaGastoService.Actualizar(actual);
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("CategoriasGasto");
    }
}
