using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

/// <summary>Cotizaciones de mostrador: precios estimados sin mover inventario. Comparte UX con Ventas.</summary>
[SecurityFilter("Pagos")]
public class CotizacionesController(
    ICotizacionService cotizaciones,
    IServiciosServices servicios,
    IProductoService productos,
    IPacienteServices pacientes) : ErrorHandlingController
{
    public ActionResult Index(DateTime? from, DateTime? to, string? texto = null)
    {
        var activas = cotizaciones.GetActivas();
        var f = (from ?? DateTime.Today.AddDays(-30)).Date;
        var t = (to ?? DateTime.Today).Date;
        ViewBag.Historial = cotizaciones.GetHistorial(f, t, texto);
        ViewBag.HistorialFrom = f.ToString("yyyy-MM-dd");
        ViewBag.HistorialTo = t.ToString("yyyy-MM-dd");
        ViewBag.HistorialTexto = texto ?? string.Empty;
        try
        {
            ViewBag.PacientesMap = pacientes.GetAll()
                .ToDictionary(p => p.IdPaciente, p => $"{p.Nombre} {p.Apellido}".Trim());
        }
        catch { ViewBag.PacientesMap = new Dictionary<Guid, string>(); }
        return View(activas);
    }

    [HttpPost]
    public ActionResult Crear(Guid? idPaciente, string? clienteNombre, int vigenciaDias = 15, string? observaciones = null)
    {
        try
        {
            var cot = cotizaciones.Crear(idPaciente, clienteNombre, vigenciaDias, observaciones);
            return RedirectToAction("Editar", new { id = cot.IdCotizacion });
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            return RedirectToAction("Index");
        }
    }

    public ActionResult Editar(Guid id)
    {
        var cot = cotizaciones.Get(id);
        if (cot is null) return RedirectToAction("Index");
        var modelo = new clinicamhsystem.Models.CotizacionEditorViewModel
        {
            Cotizacion = cot,
            Detalles = cotizaciones.GetDetalles(id),
            Servicios = servicios.GetAll(),
            Productos = productos.GetAll(),
            Editable = cot.Estado == "Borrador" || cot.Estado == "Vigente",
            PacienteNombre = ResolverPaciente(cot)
        };
        return View(modelo);
    }

    [HttpGet]
    public ActionResult Imprimir(Guid id)
    {
        var cot = cotizaciones.Get(id);
        if (cot is null) return RedirectToAction("Index");
        ViewBag.Detalles = cotizaciones.GetDetalles(id);
        ViewBag.PacienteNombre = ResolverPaciente(cot);
        return View(cot);
    }

    private string ResolverPaciente(Cotizacion cot)
    {
        if (cot.IdPaciente.HasValue)
        {
            var pac = pacientes.GetPacienteById(cot.IdPaciente.Value);
            if (pac is not null) return $"{pac.Nombre} {pac.Apellido}".Trim();
        }
        return string.IsNullOrWhiteSpace(cot.ClienteNombre) ? "Mostrador" : cot.ClienteNombre!;
    }

    [HttpPost]
    public ActionResult AddServicio(Guid id, Guid idMotivoCobro, decimal cantidad, string? precio, string? descripcion, string? descuento, string? motivoDescuento)
    {
        try { cotizaciones.AddServicio(id, idMotivoCobro, cantidad <= 0 ? 1 : cantidad, ParsePrecioFlexible(precio), descripcion, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCot"] = ex.Message; }
        return RedirectToAction("Editar", new { id });
    }

    [HttpPost]
    public ActionResult AddProducto(Guid id, Guid idProducto, decimal cantidad, Guid? loteId, string? precio, string? descripcion, bool esSobrePedido = false, string? descuento = null, string? motivoDescuento = null)
    {
        try { cotizaciones.AddProducto(id, idProducto, cantidad <= 0 ? 1 : cantidad, loteId, ParsePrecioFlexible(precio), descripcion, esSobrePedido, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCot"] = ex.Message; }
        return RedirectToAction("Editar", new { id });
    }

    [HttpPost]
    public ActionResult AplicarDescuento(Guid idDetalle, Guid id, string? descuento, string? motivoDescuento)
    {
        try { cotizaciones.AplicarDescuento(idDetalle, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCot"] = ex.Message; }
        return RedirectToAction("Editar", new { id });
    }

    [HttpPost]
    public ActionResult EliminarDetalle(Guid idDetalle, Guid id)
    {
        try { cotizaciones.RemoveDetalle(idDetalle); }
        catch (Exception ex) { RegistrarError(ex); }
        return RedirectToAction("Editar", new { id });
    }

    [HttpPost]
    public ActionResult Emitir(Guid id, int? vigenciaDias)
    {
        try { cotizaciones.Emitir(id, vigenciaDias); TempData["OkCot"] = "Cotización emitida."; }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCot"] = ex.Message; }
        return RedirectToAction("Editar", new { id });
    }

    [HttpPost]
    public ActionResult Rechazar(Guid id)
    {
        try { cotizaciones.Rechazar(id); }
        catch (Exception ex) { RegistrarError(ex); }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Anular(Guid id)
    {
        try { cotizaciones.Anular(id); }
        catch (Exception ex) { RegistrarError(ex); }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Descartar(Guid id)
    {
        try
        {
            if (cotizaciones.DescartarSiVacia(id)) return RedirectToAction("Index");
        }
        catch (Exception ex) { RegistrarError(ex); }
        return RedirectToAction("Editar", new { id });
    }

    [HttpPost]
    public ActionResult Convertir(Guid id)
    {
        try
        {
            var venta = cotizaciones.ConvertirAVenta(id);
            TempData["GuardadoTemporal"] = $"Cotización convertida: venta {venta.Folio} pendiente de cobro.";
            return RedirectToAction("Cobrar", "Ventas", new { id = venta.IdVenta });
        }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCot"] = ex.Message; }
        return RedirectToAction("Editar", new { id });
    }

    [HttpGet]
    public IActionResult GetPacientesSelect(string? q, int page = 1)
    {
        const int take = 20;
        var (datos, hayMas) = pacientes.BuscarSelect(q, page, take);
        return Json(new { results = datos.Select(p => new { id = p.IdPaciente, text = p.Nombre + " " + p.Apellido + " - " + p.Dpi }), more = hayMas });
    }

    [HttpGet]
    public IActionResult BuscarProducto(string texto)
    {
        var lista = productos.Buscar(texto ?? string.Empty).Select(p => new
        {
            p.IdProducto, p.Nombre, p.PrecioVenta, p.StockActual,
            p.RequiereLote, p.RequiereVencimiento
        });
        return Json(lista);
    }

    [HttpPost]
    public ActionResult AddProductoExpress(Guid id, string nombre, decimal precio, decimal cantidad, decimal? costo, string? descuento = null, string? motivoDescuento = null)
    {
        try { cotizaciones.AgregarProductoExpress(id, nombre, precio, cantidad <= 0 ? 1 : cantidad, costo, null, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCot"] = ex.Message; }
        return RedirectToAction("Editar", new { id });
    }

    [HttpPost]
    public ActionResult AddServicioExpress(Guid id, string descripcion, decimal precio, decimal cantidad, string? descuento = null, string? motivoDescuento = null)
    {
        try { cotizaciones.AgregarServicioExpress(id, descripcion, precio, cantidad <= 0 ? 1 : cantidad, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCot"] = ex.Message; }
        return RedirectToAction("Editar", new { id });
    }

    /// <summary>Acepta 120.50 / 120,50 / Q 120.50. El último separador manda como decimal.</summary>
    private static decimal? ParsePrecioFlexible(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = new string(raw.Trim().Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-').ToArray());
        if (s.Length == 0 || s == "-") return null;
        int lastDot = s.LastIndexOf('.'), lastComma = s.LastIndexOf(',');
        string norm;
        if (lastDot >= 0 && lastComma >= 0)
            norm = lastDot > lastComma ? s.Replace(",", "") : s.Replace(".", "").Replace(',', '.');
        else if (lastComma >= 0)
            norm = s.Replace(',', '.');
        else
            norm = s;
        return decimal.TryParse(norm, System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : (decimal?)null;
    }
}
