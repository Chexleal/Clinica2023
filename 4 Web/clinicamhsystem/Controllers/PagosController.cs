using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using ClinicaInfrastructure;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace clinicaWeb.Controllers;
[SecurityFilter("Pagos")]
public class PagosController : ErrorHandlingController
{
    private readonly IConsultaServices _consultaServices;
    private readonly IDetallesServices _detallesServices;
    private readonly IServiciosServices _serviciosServices;
    private readonly IVentaService _ventaService;
    private readonly IProductoService _productoService;
    private readonly IPacienteServices _pacienteServices;
    private readonly IMetodoPagoService _metodos;

    public PagosController(IConsultaServices consultaServices, IDetallesServices detallesServices,
        IServiciosServices serviciosServices, IVentaService ventaService, IProductoService productoService,
        IPacienteServices pacienteServices, IMetodoPagoService metodos)
    {
        _consultaServices = consultaServices;
        _detallesServices = detallesServices;
        _serviciosServices = serviciosServices;
        _ventaService = ventaService;
        _productoService = productoService;
        _pacienteServices = pacienteServices;
        _metodos = metodos;
    }

    // GET: PagosController
    public ActionResult Index()
    {
        var consultas = _consultaServices.GetAllNotPaid();
        var servicios = _serviciosServices.GetAll();
        return View(new PagarConsultaViewModel { Consultas = consultas, Servicios = servicios });
    }

    private DetallesPagarViewModel ArmarModelo(Guid idconsulta)
    {
        // Vía 1 (consulta): una sola venta pendiente por consulta.
        var venta = _ventaService.GetOrCreatePorConsulta(idconsulta);
        MigrarLegadoSiAplica(idconsulta, venta.IdVenta);
        var detallesVenta = _ventaService.GetDetalles(venta.IdVenta);
        var servicios = _serviciosServices.GetAll();
        var consulta = _consultaServices.GetConsulta(idconsulta);
        var productos = _productoService.GetAll();
        var paciente = consulta is null ? null : _pacienteServices.GetPacienteById(consulta.IdPaciente);
        ViewBag.PacienteNombre = paciente is null ? "—" : $"{paciente.Nombre} {paciente.Apellido}".Trim();
        ViewBag.AtencionFecha = consulta is null
            ? "—"
            : DateManager.GetDisplayDate(consulta.FechaCreacion, consulta.Fecha).ToString("dd/MM/yyyy HH:mm");
        // Se mantiene Detalles (legado) vacío para compatibilidad con reportes antiguos.
        return new DetallesPagarViewModel
        {
            Detalles = new List<DetalleCobro>(),
            Servicios = servicios,
            consulta = consulta,
            Venta = venta,
            VentaDetalles = detallesVenta,
            Productos = productos,
            Metodos = _metodos.GetAll(),
            Pagos = _ventaService.GetPagos(venta.IdVenta)
        };
    }

    /// <summary>Migración transparente: viejos DetalleCobro -> VentaDetalle (una sola vez).</summary>
    private void MigrarLegadoSiAplica(Guid idConsulta, Guid idVenta)
    {
        var legados = _detallesServices.GetDetallesByConsulta(idConsulta);
        if (!legados.Any()) return;
        if (_ventaService.GetDetalles(idVenta).Any()) return;
        foreach (var l in legados.ToList())
        {
            try
            {
                _ventaService.AddServicio(idVenta, l.IdMotivoCobro, l.Cantidad <= 0 ? 1 : l.Cantidad, l.Valor,
                    string.IsNullOrWhiteSpace(l.Producto) ? l.NombreServicio : $"{l.NombreServicio} - {l.Producto}");
                _detallesServices.Delete(l.IdDetalleCobro);
            }
            catch
            {
                // Si el servicio ya no existe, se conserva la fila legada y se muestra en reportes viejos.
            }
        }
    }

    [HttpPost]
    public IActionResult Detalles(Guid idconsulta)
    {
        return PartialView("Detalles", ArmarModelo(idconsulta));
    }


    [HttpPost]
    public IActionResult AddDetalle(Guid idConsulta, Guid idMotivoCobro, decimal cantidad, string? precio, string? descripcion, string? descuento = null, string? motivoDescuento = null)
    {
        try
        {
            // Nuevo: todo cobro de consulta pasa por Venta (descuenta inventario al pagar).
            // No se escribe espejo en DetalleCobro para no duplicar Consulta.Total;
            // las filas legadas se migraron en ArmarModelo y los reportes leen ambas fuentes.
            decimal? precioParsed = null;
            if (!string.IsNullOrWhiteSpace(precio))
            {
                precioParsed = ParsePrecioFlexible(precio);
                if (precioParsed is null)
                {
                    Response.StatusCode = 400;
                    return Content($"Valor no válido: '{precio}'. Usa solo números, ej. 120.50");
                }
            }
            var venta = _ventaService.GetOrCreatePorConsulta(idConsulta);
            _ventaService.AddServicio(venta.IdVenta, idMotivoCobro,
                cantidad <= 0 ? 1 : cantidad, precioParsed, descripcion, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return PartialView("Detalles", ArmarModelo(idConsulta));
    }

    [HttpPost]
    public IActionResult AddProducto(Guid idVenta, Guid idProducto, decimal cantidad, Guid? loteId, string? precio, string? descripcion, bool esSobrePedido = false, string? descuento = null, string? motivoDescuento = null)
    {
        try
        {
            decimal? precioParsed = null;
            if (!string.IsNullOrWhiteSpace(precio))
            {
                precioParsed = ParsePrecioFlexible(precio);
                if (precioParsed is null)
                {
                    Response.StatusCode = 400;
                    return Content($"Valor no válido: '{precio}'. Usa solo números, ej. 120.50");
                }
            }
            _ventaService.AddProducto(idVenta, idProducto, cantidad <= 0 ? 1 : cantidad, loteId, precioParsed, descripcion, esSobrePedido, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento);
            var venta = _ventaService.GetVenta(idVenta);
            return PartialView("Detalles", ArmarModelo(venta!.IdConsulta!.Value));
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            Response.StatusCode = 400;
            return Content(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult AddProductoExpress(Guid idVenta, string nombre, decimal precio, decimal cantidad, decimal? costo, string? descuento = null, string? motivoDescuento = null)
    {
        try
        {
            _ventaService.AgregarProductoExpress(idVenta, nombre, precio, cantidad <= 0 ? 1 : cantidad, costo, null, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento);
            var venta = _ventaService.GetVenta(idVenta);
            return PartialView("Detalles", ArmarModelo(venta!.IdConsulta!.Value));
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            Response.StatusCode = 400;
            return Content(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult AddServicioExpress(Guid idVenta, string descripcion, decimal precio, decimal cantidad, string? descuento = null, string? motivoDescuento = null)
    {
        try
        {
            _ventaService.AgregarServicioExpress(idVenta, descripcion, precio, cantidad <= 0 ? 1 : cantidad, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento);
            var venta = _ventaService.GetVenta(idVenta);
            return PartialView("Detalles", ArmarModelo(venta!.IdConsulta!.Value));
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            Response.StatusCode = 400;
            return Content(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult AplicarDescuento(Guid id, Guid idConsulta, string? descuento, string? motivoDescuento)
    {
        try
        {
            _ventaService.AplicarDescuento(id, ParsePrecioFlexible(descuento) ?? 0, motivoDescuento);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            Response.StatusCode = 400;
            return Content(ex.Message);
        }
        return PartialView("Detalles", ArmarModelo(idConsulta));
    }

    [HttpPost]
    public ActionResult Eliminar(Guid id, Guid idConsulta)
    {
        try
        {
            _detallesServices.Delete(id);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return PartialView("Detalles", ArmarModelo(idConsulta));
    }

    [HttpPost]
    public ActionResult EliminarVentaDetalle(Guid id, Guid idConsulta)
    {
        try
        {
            _ventaService.RemoveDetalle(id);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return PartialView("Detalles", ArmarModelo(idConsulta));
    }

    [HttpPost]
    public IActionResult AgregarPago(Guid idConsulta, Guid metodoId, decimal monto, string? referencia)
    {
        try
        {
            var venta = _ventaService.GetOrCreatePorConsulta(idConsulta);
            _ventaService.AgregarPago(venta.IdVenta, metodoId, monto, referencia);
            return PartialView("Detalles", ArmarModelo(idConsulta));
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            Response.StatusCode = 400;
            return Content(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult EliminarPago(Guid idPago, Guid idConsulta)
    {
        try
        {
            _ventaService.EliminarPago(idPago);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return PartialView("Detalles", ArmarModelo(idConsulta));
    }

    [HttpPost]
    public ActionResult Finalizar(Guid id)
    {
        try
        {
            // id = IdConsulta (compatibilidad con la vista actual).
            var venta = _ventaService.GetOrCreatePorConsulta(id);
            _ventaService.FinalizarPago(venta.IdVenta);
            // Compatibilidad: el flag antiguo también queda marcado vía SincronizarConsulta.
            try { _detallesServices.Pagar(id); } catch { }
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult PendientePago(Guid idConsulta, string responsable, DateTime? fechaPromesa)
    {
        try
        {
            var venta = _ventaService.GetOrCreatePorConsulta(idConsulta);
            _ventaService.DejarPendientePago(venta.IdVenta, responsable, fechaPromesa);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            Response.StatusCode = 400;
            return Content(ex.Message);
        }
        return PartialView("Detalles", ArmarModelo(idConsulta));
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
