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
    ICategoriaGastoService categoriaGastoService,
    IAseguradoraService aseguradoraService,
    IAseguradoraTarifaService aseguradoraTarifaService) : Controller
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
    [HttpGet]
    public IActionResult Aseguradoras() => View(aseguradoraService.GetAll());
    [HttpGet]
    public IActionResult TarifasAseguradora(Guid? idAseguradora)
    {
        var aseguradoras=aseguradoraService.GetAll();
        var selected=idAseguradora ?? aseguradoras.FirstOrDefault()?.IdAseguradora;
        ViewBag.Aseguradoras=aseguradoras; ViewBag.IdAseguradora=selected;
        ViewBag.ConceptosDisponibles = selected.HasValue ? aseguradoraTarifaService.GetConceptosDisponibles(selected.Value) : new List<AseguradoraTarifaConceptoItem>();
        return View(selected.HasValue?aseguradoraTarifaService.GetCatalogo(selected.Value):new List<AseguradoraTarifaCatalogoItem>());
    }

    [HttpPost]
    public IActionResult CrearTarifaAseguradora(Guid idAseguradora, string concepto, decimal precioConvenido, decimal? precioEmergencia)
    {
        try
        {
            var separador = concepto?.IndexOf(':') ?? -1;
            if (separador <= 0 || !Guid.TryParse(concepto[(separador + 1)..], out var idConcepto))
                throw new ArgumentException("Selecciona un producto o servicio válido.");
            var tipo = concepto[..separador];
            aseguradoraTarifaService.Crear(idAseguradora, tipo == "P" ? idConcepto : null, tipo == "S" ? idConcepto : null, precioConvenido, precioEmergencia);
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("TarifasAseguradora", new { idAseguradora });
    }
    [HttpPost]
    public IActionResult ActualizarTarifaAseguradora(Guid idAseguradora, Guid idTarifa, decimal precioConvenido, decimal? precioEmergencia)
    {
        try { aseguradoraTarifaService.Actualizar(idTarifa,idAseguradora,precioConvenido,precioEmergencia); }
        catch(Exception ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction("TarifasAseguradora",new { idAseguradora });
    }
    [HttpPost]
    public IActionResult ToggleTarifaAseguradora(Guid idAseguradora, Guid idTarifa, bool activa)
    {
        try { aseguradoraTarifaService.CambiarActiva(idTarifa, idAseguradora, activa); }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("TarifasAseguradora", new { idAseguradora });
    }
    [HttpPost]
    public IActionResult GuardarAseguradora(Guid? id, string nombre, string? identificadorFiscal, string? contacto, decimal copagoDefault, decimal coaseguroPorcDefault, bool activa = true)
    {
        try { aseguradoraService.Guardar(new ClinicaDomain.Aseguradora { IdAseguradora=id ?? Guid.Empty,Nombre=nombre,IdentificadorFiscal=identificadorFiscal,Contacto=contacto,CopagoDefault=copagoDefault,CoaseguroPorcDefault=coaseguroPorcDefault,Activa=activa }); }
        catch(Exception ex) { TempData["Error"]=ex.Message; }
        return RedirectToAction("Aseguradoras");
    }
    [HttpPost]
    public IActionResult ToggleAseguradora(Guid id, bool activa) { aseguradoraService.CambiarActiva(id,activa); return RedirectToAction("Aseguradoras"); }

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

    // ===== Categorías de productos y servicios =====
    public IActionResult Categorias()
    {
        categoriaService.EnsureSeed();
        var list = categoriaService.GetAll(false);
        return View(list);
    }

    [HttpPost]
    public IActionResult CrearCategoria(string nombre, string tipo, bool exigeLote, bool exigeVencimiento, int orden = 99)
    {
        try { categoriaService.Crear(nombre, tipo, exigeLote, exigeVencimiento, orden); }
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
    public IActionResult ActualizarCategoria(Guid id, string nombre, string tipo, bool exigeLote, bool exigeVencimiento, int orden = 99)
    {
        try
        {
            var actual = categoriaService.Get(id);
            if (actual is null) return RedirectToAction("Categorias");
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre requerido.");
            if (tipo != "Bien" && tipo != "Servicio") tipo = "Bien";
            actual.Nombre = nombre.Trim();
            actual.Tipo = tipo;
            actual.Orden = orden;
            actual.ExigeLoteDefault = tipo == "Bien" && exigeLote;
            actual.ExigeVencimientoDefault = tipo == "Bien" && exigeLote && exigeVencimiento;
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
