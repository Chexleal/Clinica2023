using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

/// <summary>Vía 2: venta libre de mostrador (sin consulta). Comparte IVentaService con Pagos.</summary>
[SecurityFilter("Pagos")]
public class VentasController : ErrorHandlingController
{
    private readonly IVentaService _ventas;
    private readonly IServiciosServices _servicios;
    private readonly IProductoService _productos;
    private readonly IPacienteServices _pacientes;
    private readonly IMetodoPagoService _metodos;

    public VentasController(IVentaService ventas, IServiciosServices servicios,
        IProductoService productos, IPacienteServices pacientes, IMetodoPagoService metodos)
    {
        _ventas = ventas;
        _servicios = servicios;
        _productos = productos;
        _pacientes = pacientes;
        _metodos = metodos;
    }

    public ActionResult Index(DateTime? from, DateTime? to, string? texto = null)
    {
        var pendientes = _ventas.GetPendientes();
        ViewBag.PendientesPago = _ventas.GetPendientesPago();
        ViewBag.SaldosPendientesPago = ((List<ClinicaDomain.Venta>)ViewBag.PendientesPago)
            .ToDictionary(v => v.IdVenta, v => _ventas.SaldoPendiente(v.IdVenta));
        // Historial (Pagadas + Anuladas): por defecto últimos 30 días para corregir errores.
        var f = (from ?? DateTime.Today.AddDays(-30)).Date;
        var t = (to ?? DateTime.Today).Date;
        var historial = _ventas.GetHistorial(f, t, texto);
        ViewBag.HistorialFrom = f.ToString("yyyy-MM-dd");
        ViewBag.HistorialTo = t.ToString("yyyy-MM-dd");
        ViewBag.HistorialTexto = texto ?? string.Empty;
        ViewBag.Historial = historial;
        ViewBag.SaldosHistorial = historial.ToDictionary(v => v.IdVenta, v => _ventas.SaldoPendiente(v.IdVenta));
        try
        {
            ViewBag.PacientesMap = _pacientes.GetAll()
                .ToDictionary(p => p.IdPaciente, p => $"{p.Nombre} {p.Apellido}".Trim());
        }
        catch { ViewBag.PacientesMap = new Dictionary<Guid, string>(); }
        return View(pendientes);
    }

    [HttpPost]
    public ActionResult CrearLibre(Guid? idPaciente, string? observaciones)
    {
        try
        {
            var venta = _ventas.CrearVentaLibre(idPaciente, observaciones);
            return RedirectToAction("Cobrar", new { id = venta.IdVenta });
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            return RedirectToAction("Index");
        }
    }

    public ActionResult Cobrar(Guid id)
    {
        var venta = _ventas.GetVenta(id);
        if (venta is null) return RedirectToAction("Index");
        ViewBag.CobroModo = "pagina";
        if (venta.IdPaciente.HasValue)
        {
            var pac = _pacientes.GetPacienteById(venta.IdPaciente.Value);
            if (pac is not null) ViewBag.PacienteNombre = $"{pac.Nombre} {pac.Apellido}".Trim();
        }
        var modelo = new clinicamhsystem.Models.DetallesPagarViewModel
        {
            consulta = null,
            Servicios = _servicios.GetAll(),
            Detalles = new List<DetalleCobro>(),
            Venta = venta,
            VentaDetalles = _ventas.GetDetalles(id),
            Productos = _productos.GetAll(),
            Metodos = _metodos.GetAll(),
            Pagos = _ventas.GetPagos(id)
        };
        return View(modelo);
    }

    /// <summary>Vuelve a Cobrar o a Ver según el origen (la vista Ver reabierta usa el mismo diseño de caja).</summary>
    private ActionResult RedirectCobro(Guid idVenta, string? origen) =>
        string.Equals(origen, "ver", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Ver", new { id = idVenta })
            : RedirectToAction("Cobrar", new { id = idVenta });

    [HttpPost]
    public ActionResult AddServicio(Guid idVenta, Guid idMotivoCobro, decimal cantidad, string? precio, string? descripcion, string? descuento, string? motivoDescuento, string? origen = null)
    {
        try
        {
            decimal? precioParsed = null;
            if (!string.IsNullOrWhiteSpace(precio))
            {
                precioParsed = ParsePrecioFlexible(precio);
                if (precioParsed is null) throw new ArgumentException($"Valor no válido: '{precio}'.");
            }
            var descParsed = ParsePrecioFlexible(descuento) ?? 0;
            _ventas.AddServicio(idVenta, idMotivoCobro, cantidad <= 0 ? 1 : cantidad, precioParsed, descripcion, descParsed, motivoDescuento);
        }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectCobro(idVenta, origen);
    }

    [HttpPost]
    public ActionResult AddProducto(Guid idVenta, Guid idProducto, decimal cantidad, Guid? loteId, string? precio, string? descripcion, bool esSobrePedido = false, string? descuento = null, string? motivoDescuento = null, string? origen = null)
    {
        try
        {
            decimal? precioParsed = null;
            if (!string.IsNullOrWhiteSpace(precio))
            {
                precioParsed = ParsePrecioFlexible(precio);
                if (precioParsed is null) throw new ArgumentException($"Valor no válido: '{precio}'.");
            }
            var descParsed = ParsePrecioFlexible(descuento) ?? 0;
            _ventas.AddProducto(idVenta, idProducto, cantidad <= 0 ? 1 : cantidad, loteId, precioParsed, descripcion, esSobrePedido, descParsed, motivoDescuento);
        }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectCobro(idVenta, origen);
    }

    [HttpPost]
    public ActionResult AddProductoExpress(Guid idVenta, string nombre, decimal precio, decimal cantidad, decimal? costo, string? descuento = null, string? motivoDescuento = null, string? origen = null)
    {
        try { _ventas.AgregarProductoExpress(idVenta, nombre, precio, cantidad <= 0 ? 1 : cantidad, costo, null, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectCobro(idVenta, origen);
    }

    [HttpPost]
    public ActionResult AddServicioExpress(Guid idVenta, string descripcion, decimal precio, decimal cantidad, string? descuento = null, string? motivoDescuento = null, string? origen = null)
    {
        try { _ventas.AgregarServicioExpress(idVenta, descripcion, precio, cantidad <= 0 ? 1 : cantidad, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectCobro(idVenta, origen);
    }

    [HttpPost]
    public ActionResult AplicarDescuento(Guid id, Guid idVenta, string? descuento, string? motivoDescuento, string? origen = null)
    {
        try { _ventas.AplicarDescuento(id, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectCobro(idVenta, origen);
    }

    [HttpPost]
    public ActionResult EliminarDetalle(Guid id, Guid idVenta, string? origen = null)
    {
        try { _ventas.RemoveDetalle(id); }
        catch (Exception ex) { RegistrarError(ex); }
        return RedirectCobro(idVenta, origen);
    }

    [HttpPost]
    public ActionResult AgregarPago(Guid idVenta, decimal monto, Guid metodoId, string? referencia, string? origen = null)
    {
        try { _ventas.AgregarPago(idVenta, metodoId, monto, referencia); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectCobro(idVenta, origen);
    }

    [HttpPost]
    public ActionResult EliminarPago(Guid idPago, Guid idVenta, string? origen = null)
    {
        try { _ventas.EliminarPago(idPago); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectCobro(idVenta, origen);
    }

    [HttpPost]
    public ActionResult Finalizar(Guid id, string? origen = null)
    {
        try { _ventas.FinalizarPago(id); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; return RedirectCobro(id, origen); }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult PendientePago(Guid id, string responsable, DateTime? fechaPromesa, string? origen = null)
    {
        try { _ventas.DejarPendientePago(id, responsable, fechaPromesa); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; return RedirectCobro(id, origen); }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult GuardarTemporal(Guid id)
    {
        // Guardado temporal: las líneas, descuentos y pagos ya persisten con cada
        // acción como venta "Pendiente". Solo se confirma y se vuelve al listado.
        var venta = _ventas.GetVenta(id);
        TempData["GuardadoTemporal"] = venta is null
            ? "Cuenta guardada — queda pendiente de cobro."
            : $"Cuenta {venta.Folio} guardada — queda pendiente de cobro.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Anular(Guid id)
    {
        try { _ventas.Anular(id); }
        catch (Exception ex) { RegistrarError(ex); }
        return RedirectToAction("Index");
    }

    /// <summary>Historial + corrección: ver una venta pagada/anulada con sus líneas, pagos y motivo.</summary>
    [HttpGet]
    public ActionResult Ver(Guid id)
    {
        var venta = _ventas.GetVenta(id);
        if (venta is null) return RedirectToAction("Index");
        ViewBag.CobroModo = "pagina";
        ViewBag.CobroOrigen = "ver";
        string? nombrePaciente = null;
        if (venta.IdPaciente.HasValue)
        {
            var pac = _pacientes.GetPacienteById(venta.IdPaciente.Value);
            if (pac is not null) nombrePaciente = $"{pac.Nombre} {pac.Apellido}".Trim();
        }
        ViewBag.PacienteNombre = nombrePaciente ?? "—";
        var modelo = new clinicamhsystem.Models.DetallesPagarViewModel
        {
            consulta = null,
            Servicios = _servicios.GetAll(),
            Detalles = new List<DetalleCobro>(),
            Venta = venta,
            VentaDetalles = _ventas.GetDetalles(id),
            Productos = _productos.GetAll(),
            Metodos = _metodos.GetAll(),
            Pagos = _ventas.GetPagos(id)
        };
        return View(modelo);
    }

    [HttpPost]
    public ActionResult Reabrir(Guid id, string motivo)
    {
        try { _ventas.Reabrir(id, motivo); TempData["OkCobro"] = "Venta reabierta: corrige líneas y vuelve a finalizar."; }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectToAction("Ver", new { id });
    }

    [HttpPost]
    public ActionResult Devolver(Guid id, string motivo)
    {
        try { _ventas.DevolverAnular(id, motivo); return RedirectToAction("Index"); }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; return RedirectToAction("Ver", new { id }); }
    }

    [HttpPost]
    public ActionResult CorregirDetalle(Guid id, Guid idVenta, decimal cantidad, decimal precio, string motivo)
    {
        try { _ventas.CorregirDetalle(id, cantidad, precio, motivo); TempData["OkCobro"] = "Línea corregida."; }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectToAction("Ver", new { id = idVenta });
    }

    [HttpPost]
    public ActionResult QuitarDetalle(Guid id, Guid idVenta)
    {
        try { _ventas.RemoveDetalle(id); TempData["OkCobro"] = "Línea eliminada."; }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectToAction("Ver", new { id = idVenta });
    }

    [HttpPost]
    public ActionResult DescuentoVer(Guid id, Guid idVenta, string? descuento, string? motivoDescuento)
    {
        try { _ventas.AplicarDescuento(id, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento); TempData["OkCobro"] = "Descuento actualizado."; }
        catch (Exception ex) { RegistrarError(ex); TempData["ErrorCobro"] = ex.Message; }
        return RedirectToAction("Ver", new { id = idVenta });
    }

    [HttpGet]
    public IActionResult BuscarProducto(string texto)
    {
        var lista = _productos.Buscar(texto ?? string.Empty).Select(p => new
        {
            p.IdProducto, p.Nombre, p.PrecioVenta, p.StockActual,
            p.RequiereLote, p.RequiereVencimiento
        });
        return Json(lista);
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
