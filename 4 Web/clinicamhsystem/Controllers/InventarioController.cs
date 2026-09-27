using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

[SecurityFilter("Configuraciones")]
public class InventarioController(ICategoriaProductoService categorias, IProductoService productos,
    IMovimientoInventarioService movimientos) : ErrorHandlingController
{

    public ActionResult Index()
    {
        categorias.EnsureSeed();
        ViewBag.Categorias = categorias.GetAll();
        ViewBag.StockBajo = productos.GetStockBajo();
        ViewBag.PorVencer = productos.GetProximosAVencer(30);
        return View(productos.GetAll());
    }

    [HttpPost]
    public ActionResult CrearProducto(Producto p)
    {
        try { productos.Crear(p); }
        catch (Exception ex) { RegistrarError(ex); TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult ActualizarProducto(Producto p)
    {
        try
        {
            var actual = productos.Get(p.IdProducto);
            if (actual is null) return RedirectToAction("Index");
            if (string.IsNullOrWhiteSpace(p.Nombre)) throw new ArgumentException("Nombre requerido.");
            actual.Sku = p.Sku;
            actual.Nombre = p.Nombre.Trim();
            actual.IdCategoriaProducto = p.IdCategoriaProducto;
            actual.UnidadMedida = p.UnidadMedida;
            // NOTA: el form de edición no envía CostoUltimo/StockActual/Activo:
            // no sobrescribirlos (antes se reseteaba CostoUltimo a 0 en cada edición).
            // Parseo robusto de decimales: el binder depende de la cultura del servidor
            // (punto vs coma). Si el binder dejó 0 pero el form traía otro valor,
            // re-parsear el crudo aceptando ambos separadores.
            actual.PrecioVenta = ParseDecimalForm("PrecioVenta", p.PrecioVenta);
            actual.StockMinimo = ParseDecimalForm("StockMinimo", p.StockMinimo);
            actual.RequiereLote = p.RequiereLote;
            actual.RequiereVencimiento = p.RequiereVencimiento;
            actual.EsSobrePedido = false; // LEGADO: ya sin uso, se normaliza
            productos.Actualizar(actual);
            TempData["Ok"] = $"Precio de '{actual.Nombre}' actualizado a Q{actual.PrecioVenta:0.00}.";
        }
        catch (Exception ex) { RegistrarError(ex); TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Re-parsea un decimal del form aceptando punto o coma como separador,
    /// para no depender de la cultura del servidor. Si el campo viene vacío,
    /// conserva el valor ya bindeado; si trae texto inválido, lanza error visible.
    /// </summary>
    private decimal ParseDecimalForm(string fieldName, decimal boundValue)
    {
        var raw = Request.Form[fieldName].ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(raw)) return boundValue;
        if (TryParseDecimalFlexible(raw, out var parsed)) return parsed;
        // Si el binder ya había parseado algo distinto de cero, respetarlo;
        // si no, el valor es realmente inválido: avisar en vez de guardar 0 silencioso.
        if (boundValue != 0) return boundValue;
        throw new ArgumentException($"Valor inválido en '{fieldName}': '{raw}'.");
    }

    private static bool TryParseDecimalFlexible(string raw, out decimal value)
    {
        // Intento 1: cultura invariante (punto decimal, lo que envía input type=number).
        if (decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out value))
            return true;
        // Intento 2: cultura actual del servidor.
        if (decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.CurrentCulture, out value))
            return true;
        // Intento 3: normalizar coma a punto (usuarios es-GT que escriben "12,50").
        var normalized = raw.Replace(',', '.');
        if (decimal.TryParse(normalized, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out value))
            return true;
        value = 0;
        return false;
    }

    [HttpPost]
    public ActionResult CambiarActivoProducto(Guid id, bool activo)
    {
        try { productos.CambiarActivo(id, activo); }
        catch (Exception ex) { RegistrarError(ex); TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Entrada(Guid productoId, decimal cantidad, decimal costo,
        string? codigoLote, DateTime? vencimiento, string? motivo)
    {
        try { movimientos.RegistrarEntrada(productoId, cantidad, costo, codigoLote, vencimiento, motivo); }
        catch (Exception ex) { RegistrarError(ex); TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Ajuste(Guid productoId, decimal cantidad, string motivo)
    {
        try { movimientos.RegistrarAjuste(productoId, cantidad, motivo); }
        catch (Exception ex) { RegistrarError(ex); TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    public ActionResult Kardex(Guid id)
    {
        var producto = productos.Get(id);
        if (producto is null) return RedirectToAction("Index");
        ViewBag.Producto = producto;
        ViewBag.Lotes = productos.GetLotesDisponibles(id);
        return View(movimientos.GetKardex(id));
    }
}
