using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

[SecurityFilter("Configuraciones")]
public class InventarioController : ErrorHandlingController
{
    private readonly ICategoriaProductoService _categorias;
    private readonly IProductoService _productos;
    private readonly IMovimientoInventarioService _movimientos;

    public InventarioController(ICategoriaProductoService categorias, IProductoService productos,
        IMovimientoInventarioService movimientos)
    {
        _categorias = categorias;
        _productos = productos;
        _movimientos = movimientos;
    }

    public ActionResult Index()
    {
        _categorias.EnsureSeed();
        ViewBag.Categorias = _categorias.GetAll();
        ViewBag.StockBajo = _productos.GetStockBajo();
        ViewBag.PorVencer = _productos.GetProximosAVencer(30);
        return View(_productos.GetAll());
    }

    [HttpPost]
    public ActionResult CrearProducto(Producto p)
    {
        try { _productos.Crear(p); }
        catch (Exception ex) { RegistrarError(ex); TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult ActualizarProducto(Producto p)
    {
        try
        {
            var actual = _productos.Get(p.IdProducto);
            if (actual is null) return RedirectToAction("Index");
            if (string.IsNullOrWhiteSpace(p.Nombre)) throw new ArgumentException("Nombre requerido.");
            actual.Sku = p.Sku;
            actual.Nombre = p.Nombre.Trim();
            actual.IdCategoriaProducto = p.IdCategoriaProducto;
            actual.UnidadMedida = p.UnidadMedida;
            actual.PrecioVenta = p.PrecioVenta;
            actual.CostoUltimo = p.CostoUltimo;
            actual.StockMinimo = p.StockMinimo;
            actual.RequiereLote = p.RequiereLote;
            actual.RequiereVencimiento = p.RequiereVencimiento;
            actual.EsSobrePedido = false; // LEGADO: ya sin uso, se normaliza
            _productos.Actualizar(actual);
        }
        catch (Exception ex) { RegistrarError(ex); TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult CambiarActivoProducto(Guid id, bool activo)
    {
        try { _productos.CambiarActivo(id, activo); }
        catch (Exception ex) { RegistrarError(ex); }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Entrada(Guid productoId, decimal cantidad, decimal costo,
        string? codigoLote, DateTime? vencimiento, string? motivo)
    {
        try { _movimientos.RegistrarEntrada(productoId, cantidad, costo, codigoLote, vencimiento, motivo); }
        catch (Exception ex) { RegistrarError(ex); }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Ajuste(Guid productoId, decimal cantidad, string motivo)
    {
        try { _movimientos.RegistrarAjuste(productoId, cantidad, motivo); }
        catch (Exception ex) { RegistrarError(ex); }
        return RedirectToAction("Index");
    }

    public ActionResult Kardex(Guid id)
    {
        var producto = _productos.Get(id);
        if (producto is null) return RedirectToAction("Index");
        ViewBag.Producto = producto;
        ViewBag.Lotes = _productos.GetLotesDisponibles(id);
        return View(_movimientos.GetKardex(id));
    }
}
