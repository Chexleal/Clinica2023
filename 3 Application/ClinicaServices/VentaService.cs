using ClinicaDomain;
using ClinicaInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public interface IVentaService
{
    Venta GetOrCreatePorConsulta(Guid idConsulta);
    Venta CrearVentaLibre(Guid? idPaciente, string? observaciones = null);
    Venta? GetVenta(Guid idVenta);
    List<Venta> GetPendientes(int top = 100);
    List<Venta> GetPendientesPago(int top = 100);
    List<Venta> GetPorRango(DateTime from, DateTime to);
    List<VentaDetalle> GetDetalles(Guid idVenta);
    VentaDetalle AddServicio(Guid idVenta, Guid motivoCobroId, decimal cantidad, decimal? precioUnitario = null, string? descripcion = null, decimal descuentoMonto = 0, string? motivoDescuento = null);
    VentaDetalle AddProducto(Guid idVenta, Guid productoId, decimal cantidad, Guid? loteId = null, decimal? precioUnitario = null, string? descripcion = null, bool esSobrePedido = false, decimal descuentoMonto = 0, string? motivoDescuento = null);
    /// <summary>Aplica o edita el descuento de una línea pendiente (motivo obligatorio si monto &gt; 0). Registra quién lo otorgó.</summary>
    VentaDetalle AplicarDescuento(Guid idVentaDetalle, decimal descuentoMonto, string? motivoDescuento);
    void RemoveDetalle(Guid idVentaDetalle);
    void Pagar(Guid idVenta);
    void Anular(Guid idVenta);
    // Tipos de pago (efectivo, tarjeta, transferencia...): una venta admite varios pagos.
    List<VentaPago> GetPagos(Guid idVenta);
    VentaPago AgregarPago(Guid idVenta, Guid metodoId, decimal monto, string? referencia);
    void EliminarPago(Guid idPago);
    decimal SaldoPendiente(Guid idVenta);
    void FinalizarPago(Guid idVenta);
    /// <summary>Deja la cuenta pendiente de pago: descuenta stock ahora y deja el saldo por cobrar (responsable requerido).</summary>
    void DejarPendientePago(Guid idVenta, string responsable, DateTime? fechaPromesa);
    VentaDetalle? AgregarDesdeReceta(Guid idVenta, string nombreMedicamento, decimal cantidad);
    /// <summary>Alta rápida desde caja: crea el producto (sin lote) + entrada + línea de venta en una sola operación.</summary>
    VentaDetalle AgregarProductoExpress(Guid idVenta, string nombre, decimal precioVenta, decimal cantidad, decimal? costoUnitario = null, Guid? idCategoria = null, decimal descuentoMonto = 0, string? motivoDescuento = null);
    /// <summary>Servicio no catalogado: crea el MotivoCobro (si no existe) + línea de venta. No toca inventario.</summary>
    VentaDetalle AgregarServicioExpress(Guid idVenta, string descripcion, decimal precio, decimal cantidad, decimal descuentoMonto = 0, string? motivoDescuento = null);
}

public class VentaService : IVentaService
{
    private readonly ClinicaContext _db;
    private readonly ICurrentUser? _currentUser;
    public VentaService(ClinicaContext db, ICurrentUser? currentUser = null) { _db = db; _currentUser = currentUser; }

    private string NuevoFolio()
    {
        return $"V-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
    }

    private void RecalcularTotal(Venta venta)
    {
        venta.Total = _db.VentaDetalles.Where(d => d.IdVenta == venta.IdVenta).Sum(d => (decimal?)d.Subtotal) ?? 0;
    }

    private void SincronizarConsulta(Venta venta)
    {
        if (!venta.IdConsulta.HasValue) return;
        var consulta = _db.Consulta.FirstOrDefault(c => c.IdConsulta == venta.IdConsulta.Value);
        if (consulta is null) return;
        consulta.Total = venta.Total;
        if (venta.Estado == "Pagada") consulta.Pagada = true;
        if (venta.Estado == "Pendiente" && consulta.Pagada)
        {
            // Si se reabre una venta pagada (nueva línea), la consulta vuelve a pendiente de pago.
            consulta.Pagada = false;
        }
    }

    public Venta GetOrCreatePorConsulta(Guid idConsulta)
    {
        var venta = _db.Ventas.FirstOrDefault(v => v.IdConsulta == idConsulta && (v.Estado == "Pendiente" || v.Estado == "Pendiente de pago"));
        if (venta is not null) return venta;
        // Anti-duplicado: si ya existe una pagada, se crea una nueva pendiente solo si la consulta se reabrió.
        var consulta = _db.Consulta.FirstOrDefault(c => c.IdConsulta == idConsulta)
            ?? throw new InvalidOperationException("Consulta no encontrada.");
        venta = new Venta
        {
            IdVenta = Guid.NewGuid(),
            Folio = NuevoFolio(),
            Fecha = DateTime.Now,
            IdPaciente = consulta.IdPaciente,
            IdConsulta = idConsulta,
            Total = 0,
            Estado = "Pendiente"
        };
        _db.Ventas.Add(venta);
        _db.SaveChanges();
        return venta;
    }

    public Venta CrearVentaLibre(Guid? idPaciente, string? observaciones = null)
    {
        var venta = new Venta
        {
            IdVenta = Guid.NewGuid(),
            Folio = NuevoFolio(),
            Fecha = DateTime.Now,
            IdPaciente = idPaciente,
            IdConsulta = null,
            Total = 0,
            Estado = "Pendiente",
            Observaciones = observaciones ?? string.Empty
        };
        _db.Ventas.Add(venta);
        _db.SaveChanges();
        return venta;
    }

    public Venta? GetVenta(Guid idVenta) =>
        _db.Ventas.FirstOrDefault(v => v.IdVenta == idVenta);

    public List<Venta> GetPendientes(int top = 100) =>
        _db.Ventas.Where(v => v.Estado == "Pendiente" && v.IdConsulta == null).OrderByDescending(v => v.Fecha).Take(top).ToList();

    public List<Venta> GetPendientesPago(int top = 100) =>
        _db.Ventas.Where(v => v.Estado == "Pendiente de pago").OrderBy(v => v.FechaPromesa == null ? 1 : 0).ThenBy(v => v.FechaPromesa).ThenByDescending(v => v.Fecha).Take(top).ToList();

    public List<Venta> GetPorRango(DateTime from, DateTime to)
    {
        var f = from.Date;
        var t = to.Date.AddDays(1);
        return _db.Ventas.Where(v => v.Fecha >= f && v.Fecha < t && v.Estado != "Anulada").ToList();
    }

    public List<VentaDetalle> GetDetalles(Guid idVenta)
    {
        var lineas = _db.VentaDetalles.Where(d => d.IdVenta == idVenta).ToList();
        CompletarNombresDescuento(lineas);
        return lineas;
    }

    /// <summary>Valida monto/motivo y calcula el subtotal neto. Motivo obligatorio si hay descuento.</summary>
    private static (decimal monto, string? motivo) ValidarDescuento(decimal bruto, decimal descuentoMonto, string? motivoDescuento)
    {
        if (descuentoMonto < 0) throw new ArgumentException("El descuento no puede ser negativo.");
        if (descuentoMonto == 0) return (0, null);
        if (descuentoMonto > bruto)
            throw new ArgumentException($"El descuento (Q{descuentoMonto:0.00}) no puede ser mayor al bruto (Q{bruto:0.00}).");
        motivoDescuento = (motivoDescuento ?? "").Trim();
        if (string.IsNullOrWhiteSpace(motivoDescuento))
            throw new ArgumentException("Indica el motivo del descuento.");
        if (motivoDescuento.Length > 200) motivoDescuento = motivoDescuento[..200];
        return (descuentoMonto, motivoDescuento);
    }

    private void CompletarNombresDescuento(List<VentaDetalle> lineas)
    {
        try
        {
            var ids = lineas.Where(l => l.DescuentoOtorgadoPor.HasValue)
                .Select(l => l.DescuentoOtorgadoPor!.Value).Distinct().ToList();
            if (!ids.Any()) return;
            var nombres = _db.Usuarios.Where(u => ids.Contains(u.IdUsuario))
                .ToDictionary(u => u.IdUsuario, u => (u.Nombre + " " + u.Apellido).Trim());
            foreach (var l in lineas)
                if (l.DescuentoOtorgadoPor.HasValue && nombres.TryGetValue(l.DescuentoOtorgadoPor.Value, out var n))
                    l.DescuentoOtorgadoPorNombre = n;
        }
        catch { }
    }

    public VentaDetalle AddServicio(Guid idVenta, Guid motivoCobroId, decimal cantidad, decimal? precioUnitario = null, string? descripcion = null, decimal descuentoMonto = 0, string? motivoDescuento = null)
    {
        if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser mayor a cero.");
        var venta = GetVenta(idVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        if (venta.Estado != "Pendiente") throw new InvalidOperationException("Solo se puede modificar una venta pendiente.");
        var servicio = _db.MotivoCobros.FirstOrDefault(m => m.IdMotivoCobro == motivoCobroId)
            ?? throw new InvalidOperationException("Servicio no encontrado.");
        var precio = precioUnitario ?? servicio.PrecioSugerido;
        var bruto = cantidad * precio;
        var (desc, motivo) = ValidarDescuento(bruto, descuentoMonto, motivoDescuento);
        var detalle = new VentaDetalle
        {
            IdVentaDetalle = Guid.NewGuid(),
            IdVenta = idVenta,
            TipoLinea = "Servicio",
            IdMotivoCobro = motivoCobroId,
            Descripcion = descripcion.TextoLibre() is { Length: > 0 } d ? d : servicio.Descripcion,
            Cantidad = cantidad,
            PrecioUnitario = precio,
            Subtotal = bruto - desc,
            DescuentoMonto = desc,
            DescuentoMotivo = motivo,
            DescuentoOtorgadoPor = desc > 0 ? _currentUser?.Usuario?.IdUsuario : null
        };
        _db.VentaDetalles.Add(detalle);
        RecalcularTotal(venta);
        SincronizarConsulta(venta);
        _db.SaveChanges();
        return detalle;
    }

    public VentaDetalle AddProducto(Guid idVenta, Guid productoId, decimal cantidad, Guid? loteId = null, decimal? precioUnitario = null, string? descripcion = null, bool esSobrePedido = false, decimal descuentoMonto = 0, string? motivoDescuento = null)
    {
        if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser mayor a cero.");
        if (precioUnitario.HasValue && precioUnitario.Value < 0) throw new ArgumentException("Precio no válido.");
        var venta = GetVenta(idVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        if (venta.Estado != "Pendiente") throw new InvalidOperationException("Solo se puede modificar una venta pendiente.");
        var producto = _db.Productos.FirstOrDefault(p => p.IdProducto == productoId && p.Activo)
            ?? throw new InvalidOperationException("Producto no encontrado o inactivo.");
        var precioFinal = precioUnitario ?? producto.PrecioVenta;
        if (precioFinal <= 0)
            throw new ArgumentException($"Indica el valor de '{producto.Nombre}': el catálogo lo tiene en Q0.00.");

        // Sobre pedido por línea (ej. plantilla mandada a hacer): no valida ni descuenta stock.
        if (!esSobrePedido)
        {
            // Validar stock disponible (sin descontar aún; el descuento real es al Pagar).
            var disponible = producto.RequiereLote
                ? _db.LotesProducto.Where(l => l.IdProducto == productoId && l.Activo && l.Stock > 0).Sum(l => (decimal?)l.Stock) ?? 0
                : producto.StockActual;
            var yaReservado = _db.VentaDetalles.Where(d => d.IdProducto == productoId && d.IdVenta == idVenta).Sum(d => (decimal?)d.Cantidad) ?? 0;
            if (disponible < yaReservado + cantidad)
                throw new InvalidOperationException($"Stock insuficiente de {producto.Nombre}. Disponible: {disponible}. Si es sobre pedido, marca el check S/pedido al agregarlo.");
        }

        if (producto.RequiereLote && loteId.HasValue)
        {
            var lote = _db.LotesProducto.FirstOrDefault(l => l.IdLote == loteId.Value && l.IdProducto == productoId)
                ?? throw new InvalidOperationException("Lote no válido para este producto.");
            if (lote.Stock < cantidad) throw new InvalidOperationException($"El lote {lote.CodigoLote} solo tiene {lote.Stock}.");
        }

        var brutoProd = cantidad * precioFinal;
        var (descProd, motivoProd) = ValidarDescuento(brutoProd, descuentoMonto, motivoDescuento);
        var detalle = new VentaDetalle
        {
            IdVentaDetalle = Guid.NewGuid(),
            IdVenta = idVenta,
            TipoLinea = "Producto",
            IdProducto = productoId,
            IdLote = producto.RequiereLote ? loteId : null, // null = FEFO automático al pagar
            Descripcion = descripcion.TextoLibre() is { Length: > 0 } d ? d : producto.Nombre,
            Cantidad = cantidad,
            PrecioUnitario = precioFinal,
            Subtotal = brutoProd - descProd,
            DescuentoMonto = descProd,
            DescuentoMotivo = motivoProd,
            DescuentoOtorgadoPor = descProd > 0 ? _currentUser?.Usuario?.IdUsuario : null,
            EsSobrePedido = esSobrePedido
        };
        _db.VentaDetalles.Add(detalle);
        RecalcularTotal(venta);
        SincronizarConsulta(venta);
        _db.SaveChanges();
        return detalle;
    }

    public VentaDetalle? AgregarDesdeReceta(Guid idVenta, string nombreMedicamento, decimal cantidad)
    {
        var nombre = (nombreMedicamento ?? "").Trim().ToLower();
        if (string.IsNullOrWhiteSpace(nombre)) return null;
        var producto = _db.Productos.FirstOrDefault(p => p.Activo && p.Nombre.ToLower() == nombre)
            ?? _db.Productos.FirstOrDefault(p => p.Activo && p.Nombre.ToLower().Contains(nombre));
        if (producto is null) return null; // no hay match: la compra no es obligatoria en clínica
        return AddProducto(idVenta, producto.IdProducto, cantidad);
    }

    public VentaDetalle AplicarDescuento(Guid idVentaDetalle, decimal descuentoMonto, string? motivoDescuento)
    {
        var detalle = _db.VentaDetalles.FirstOrDefault(d => d.IdVentaDetalle == idVentaDetalle)
            ?? throw new InvalidOperationException("Línea no encontrada.");
        var venta = GetVenta(detalle.IdVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        if (venta.Estado != "Pendiente") throw new InvalidOperationException("Solo se puede modificar una venta pendiente.");
        var bruto = detalle.Cantidad * detalle.PrecioUnitario;
        var (desc, motivo) = ValidarDescuento(bruto, descuentoMonto, motivoDescuento);
        detalle.DescuentoMonto = desc;
        detalle.DescuentoMotivo = motivo;
        detalle.DescuentoOtorgadoPor = desc > 0 ? _currentUser?.Usuario?.IdUsuario : null;
        detalle.Subtotal = bruto - desc;
        RecalcularTotal(venta);
        SincronizarConsulta(venta);
        _db.SaveChanges();
        return detalle;
    }

    public void RemoveDetalle(Guid idVentaDetalle)
    {
        var detalle = _db.VentaDetalles.FirstOrDefault(d => d.IdVentaDetalle == idVentaDetalle);
        if (detalle is null) return;
        var venta = GetVenta(detalle.IdVenta);
        if (venta is not null && venta.Estado != "Pendiente")
            throw new InvalidOperationException("Solo se puede modificar una venta pendiente.");
        _db.VentaDetalles.Remove(detalle);
        if (venta is not null)
        {
            RecalcularTotal(venta);
            SincronizarConsulta(venta);
        }
        _db.SaveChanges();
    }

    public void Pagar(Guid idVenta)
    {
        // Compatibilidad: pago total en efectivo (método por defecto) + cobro.
        var venta = GetVenta(idVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        if (venta.Estado != "Pendiente") throw new InvalidOperationException("La venta ya fue procesada.");
        RecalcularTotal(venta);
        var efectivo = _db.MetodosPago.FirstOrDefault(m => m.Activo && m.Nombre.ToLower().Contains("efectivo"))
            ?? _db.MetodosPago.FirstOrDefault(m => m.Activo);
        if (efectivo is null)
            throw new InvalidOperationException("No hay tipos de pago configurados.");
        if (!GetPagos(idVenta).Any() && venta.Total > 0)
            AgregarPago(idVenta, efectivo.IdMetodoPago, venta.Total, null);
        AplicarCobro(venta, "Pagada");
    }

    public List<VentaPago> GetPagos(Guid idVenta) =>
        _db.VentaPagos.Where(p => p.IdVenta == idVenta).OrderBy(p => p.Fecha).ToList();

    public VentaPago AgregarPago(Guid idVenta, Guid metodoId, decimal monto, string? referencia)
    {
        var venta = GetVenta(idVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        if (venta.Estado != "Pendiente" && venta.Estado != "Pendiente de pago") throw new InvalidOperationException("Solo se puede pagar una venta pendiente o pendiente de pago.");
        var metodo = _db.MetodosPago.FirstOrDefault(m => m.IdMetodoPago == metodoId && m.Activo)
            ?? throw new InvalidOperationException("Tipo de pago no válido o inactivo.");
        if (monto <= 0) throw new ArgumentException("El monto debe ser mayor a cero.");
        referencia = (referencia ?? "").Trim();
        if (metodo.RequiereReferencia && string.IsNullOrWhiteSpace(referencia))
            throw new ArgumentException($"'{metodo.Nombre}' exige número de referencia/autorización.");
        RecalcularTotal(venta);
        if (GetPagos(idVenta).Sum(p => p.Monto) + monto > venta.Total + 0.001m && venta.Total > 0)
        {
            // Se permite excedente solo como cambio en efectivo; en otros métodos se bloquea.
            if (!metodo.Nombre.ToLower().Contains("efectivo"))
                throw new InvalidOperationException($"El monto excede el saldo (Q{(venta.Total - GetPagos(idVenta).Sum(p => p.Monto)):0.00}).");
        }
        var pago = new VentaPago
        {
            IdVentaPago = Guid.NewGuid(),
            IdVenta = idVenta,
            IdMetodoPago = metodoId,
            Monto = monto,
            Referencia = referencia,
            Fecha = DateTime.Now
        };
        _db.VentaPagos.Add(pago);
        _db.SaveChanges();
        if (venta.Estado == "Pendiente de pago" && SaldoPendiente(idVenta) <= 0.001m)
        {
            // Pendiente de pago saldado con abonos: se cierra (el stock ya se descontó).
            venta.Estado = "Pagada";
            SincronizarConsulta(venta);
            _db.SaveChanges();
        }
        return pago;
    }

    public void EliminarPago(Guid idPago)
    {
        var pago = _db.VentaPagos.FirstOrDefault(p => p.IdVentaPago == idPago);
        if (pago is null) return;
        var venta = GetVenta(pago.IdVenta);
        if (venta is not null && venta.Estado != "Pendiente" && venta.Estado != "Pendiente de pago")
            throw new InvalidOperationException("Solo se puede modificar una venta pendiente o pendiente de pago.");
        _db.VentaPagos.Remove(pago);
        _db.SaveChanges();
    }

    public decimal SaldoPendiente(Guid idVenta)
    {
        var venta = GetVenta(idVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        RecalcularTotal(venta);
        return venta.Total - GetPagos(idVenta).Sum(p => (decimal?)p.Monto ?? 0);
    }

    public void FinalizarPago(Guid idVenta)
    {
        var venta = GetVenta(idVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        if (venta.Estado != "Pendiente" && venta.Estado != "Pendiente de pago") throw new InvalidOperationException("La venta ya fue procesada.");
        if (!GetDetalles(idVenta).Any()) throw new InvalidOperationException("La venta no tiene líneas para cobrar.");
        var saldo = SaldoPendiente(idVenta);
        if (saldo > 0.001m)
            throw new InvalidOperationException($"Falta por cubrir Q{saldo:0.00}. Registra el pago antes de finalizar.");
        if (venta.Estado == "Pendiente de pago")
        {
            // El stock ya se descontó al dejar pendiente de pago: solo se cierra.
            RecalcularTotal(venta);
            venta.Estado = "Pagada";
            SincronizarConsulta(venta);
            _db.SaveChanges();
            return;
        }
        AplicarCobro(venta, "Pagada");
    }

    public void DejarPendientePago(Guid idVenta, string responsable, DateTime? fechaPromesa)
    {
        var venta = GetVenta(idVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        if (venta.Estado != "Pendiente") throw new InvalidOperationException("Solo se puede dejar pendiente de pago una venta en caja.");
        if (!GetDetalles(idVenta).Any()) throw new InvalidOperationException("La venta no tiene líneas para dejar pendiente de pago.");
        responsable = (responsable ?? "").Trim();
        if (string.IsNullOrWhiteSpace(responsable)) throw new ArgumentException("Indica quién queda debiendo (responsable).");
        var saldo = SaldoPendiente(idVenta);
        if (saldo <= 0.001m)
            throw new InvalidOperationException("La cuenta ya está cubierta; usa Completar pago.");
        venta.FiadoResponsable = responsable;
        venta.FechaPromesa = fechaPromesa;
        // Descuenta inventario ahora (el cliente se lleva el producto) y deja el saldo por cobrar.
        AplicarCobro(venta, "Pendiente de pago");
    }

    /// <summary>Descuenta inventario (transaccional), fija el estado y sincroniza la consulta.</summary>
    private void AplicarCobro(Venta venta, string estadoFinal)
    {
        var idVenta = venta.IdVenta;
        var detalles = GetDetalles(idVenta);

        using var tx = _db.Database.BeginTransaction();
        try
        {
            foreach (var d in detalles.Where(x => x.TipoLinea == "Producto" && x.IdProducto.HasValue))
            {
                var producto = _db.Productos.FirstOrDefault(p => p.IdProducto == d.IdProducto!.Value)
                    ?? throw new InvalidOperationException($"Producto no encontrado: {d.Descripcion}");
                var cantidad = d.Cantidad;

                // Sobre pedido por línea: no toca stock; deja rastro para el pendiente con el proveedor.
                if (d.EsSobrePedido)
                {
                    _db.MovimientosInventario.Add(new MovimientoInventario
                    {
                        IdMovimiento = Guid.NewGuid(),
                        Fecha = DateTime.Now,
                        IdProducto = producto.IdProducto,
                        Tipo = "SalidaVentaPedido",
                        Cantidad = -cantidad,
                        CostoUnitario = producto.CostoUltimo,
                        IdVenta = venta.IdVenta,
                        IdConsulta = venta.IdConsulta,
                        Motivo = $"Venta {venta.Folio} (sobre pedido, sin stock)"
                    });
                    continue;
                }

                if (!producto.RequiereLote)
                {
                    if (producto.StockActual < cantidad)
                        throw new InvalidOperationException($"Stock insuficiente de {producto.Nombre}.");
                    producto.StockActual -= cantidad;
                    _db.MovimientosInventario.Add(new MovimientoInventario
                    {
                        IdMovimiento = Guid.NewGuid(),
                        Fecha = DateTime.Now,
                        IdProducto = producto.IdProducto,
                        Tipo = "SalidaVenta",
                        Cantidad = -cantidad,
                        CostoUnitario = producto.CostoUltimo,
                        IdVenta = venta.IdVenta,
                        IdConsulta = venta.IdConsulta,
                        Motivo = $"Venta {venta.Folio}"
                    });
                }
                else
                {
                    // Con lote: lote específico o FEFO automático (vence primero).
                    List<LoteProducto> lotes;
                    if (d.IdLote.HasValue)
                    {
                        var unico = _db.LotesProducto.FirstOrDefault(l => l.IdLote == d.IdLote.Value)
                            ?? throw new InvalidOperationException("Lote no encontrado.");
                        lotes = new List<LoteProducto> { unico };
                    }
                    else
                    {
                        lotes = _db.LotesProducto
                            .Where(l => l.IdProducto == producto.IdProducto && l.Activo && l.Stock > 0)
                            .OrderBy(l => l.FechaVencimiento == null ? 1 : 0)
                            .ThenBy(l => l.FechaVencimiento)
                            .ToList();
                    }
                    var restante = cantidad;
                    foreach (var lote in lotes)
                    {
                        if (restante <= 0) break;
                        if (lote.Stock <= 0) continue;
                        var toma = Math.Min(lote.Stock, restante);
                        lote.Stock -= toma;
                        restante -= toma;
                        _db.MovimientosInventario.Add(new MovimientoInventario
                        {
                            IdMovimiento = Guid.NewGuid(),
                            Fecha = DateTime.Now,
                            IdProducto = producto.IdProducto,
                            IdLote = lote.IdLote,
                            Tipo = "SalidaVenta",
                            Cantidad = -toma,
                            CostoUnitario = lote.CostoUnitario,
                            IdVenta = venta.IdVenta,
                            IdConsulta = venta.IdConsulta,
                            Motivo = $"Venta {venta.Folio} lote {lote.CodigoLote}"
                        });
                        if (d.IdLote == null && d.IdLote != lote.IdLote)
                        {
                            // Dejar constancia del lote usado en la primera toma (auditoría simple).
                            if (d.IdLote == null) d.IdLote = lote.IdLote;
                        }
                    }
                    if (restante > 0)
                        throw new InvalidOperationException($"Stock insuficiente en lotes de {producto.Nombre}.");
                    producto.StockActual = _db.LotesProducto
                        .Where(l => l.IdProducto == producto.IdProducto && l.Activo).Sum(l => (decimal?)l.Stock) ?? 0;
                }
            }

            RecalcularTotal(venta);
            venta.Estado = estadoFinal;
            SincronizarConsulta(venta);
            _db.SaveChanges();
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public void Anular(Guid idVenta)
    {
        var venta = GetVenta(idVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        if (venta.Estado == "Anulada") return;
        if (venta.Estado == "Pagada")
            throw new InvalidOperationException("Una venta pagada no se puede anular (registrar devolución en inventario).");
        if (venta.Estado == "Pendiente de pago")
            throw new InvalidOperationException("Una venta pendiente de pago no se puede anular por el momento.");
        venta.Estado = "Anulada";
        SincronizarConsulta(venta);
        _db.SaveChanges();
    }

    public VentaDetalle AgregarProductoExpress(Guid idVenta, string nombre, decimal precioVenta, decimal cantidad, decimal? costoUnitario = null, Guid? idCategoria = null, decimal descuentoMonto = 0, string? motivoDescuento = null)
    {
        nombre = nombre.TextoCatalogo();
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre del producto requerido.");
        if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser mayor a cero.");
        if (precioVenta < 0) throw new ArgumentException("Precio no válido.");
        var venta = GetVenta(idVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        if (venta.Estado != "Pendiente") throw new InvalidOperationException("Solo se puede modificar una venta pendiente.");

        using var tx = _db.Database.BeginTransaction();
        try
        {
            // Reutilizar si ya existe (evita duplicados por alta rápida repetida).
            var existente = _db.Productos.FirstOrDefault(p => p.Activo && p.Nombre.ToLower() == nombre.ToLower());
            Producto producto;
            if (existente is not null)
            {
                producto = existente;
            }
            else
            {
                Guid catId;
                if (idCategoria.HasValue && _db.CategoriasProducto.Any(c => c.IdCategoriaProducto == idCategoria.Value && c.Tipo == "Bien"))
                    catId = idCategoria.Value;
                else
                    catId = _db.CategoriasProducto.Where(c => c.Tipo == "Bien" && c.Activo)
                        .OrderBy(c => c.Nombre).Select(c => c.IdCategoriaProducto).FirstOrDefault();
                if (catId == Guid.Empty)
                    throw new InvalidOperationException("No hay categorías de producto. Crea una en Configuraciones > Inventario > Categorías.");
                producto = new Producto
                {
                    IdProducto = Guid.NewGuid(),
                    Nombre = nombre,
                    Sku = string.Empty,
                    IdCategoriaProducto = catId,
                    UnidadMedida = "PIEZA",
                    PrecioVenta = precioVenta,
                    CostoUltimo = costoUnitario ?? precioVenta,
                    StockActual = 0,
                    StockMinimo = 0,
                    RequiereLote = false, // express nunca usa lote/vencimiento
                    RequiereVencimiento = false,
                    Activo = true
                };
                producto.BeforeSaveChanges();
                _db.Productos.Add(producto);
                _db.SaveChanges();
            }

            // Entrada express por la cantidad a vender (queda costo registrado, sin negativos).
            var costo = costoUnitario ?? producto.CostoUltimo;
            producto.StockActual += cantidad;
            producto.CostoUltimo = costo;
            if (precioVenta > 0) producto.PrecioVenta = precioVenta;
            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                IdMovimiento = Guid.NewGuid(),
                Fecha = DateTime.Now,
                IdProducto = producto.IdProducto,
                Tipo = "EntradaCompra",
                Cantidad = cantidad,
                CostoUnitario = costo,
                IdVenta = venta.IdVenta,
                IdConsulta = venta.IdConsulta,
                Motivo = "Alta rápida desde caja"
            });
            _db.SaveChanges();

            tx.Commit();
            // Fuera de la tx interna: agrega la línea (valida stock, que ya existe por la entrada).
            return AddProducto(idVenta, producto.IdProducto, cantidad, null, null, null, false, descuentoMonto, motivoDescuento);
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public VentaDetalle AgregarServicioExpress(Guid idVenta, string descripcion, decimal precio, decimal cantidad, decimal descuentoMonto = 0, string? motivoDescuento = null)
    {
        descripcion = descripcion.TextoCatalogo();
        if (string.IsNullOrWhiteSpace(descripcion)) throw new ArgumentException("Describe el servicio.");
        if (precio <= 0) throw new ArgumentException("Indica el valor del servicio.");
        if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser mayor a cero.");
        var venta = GetVenta(idVenta) ?? throw new InvalidOperationException("Venta no encontrada.");
        if (venta.Estado != "Pendiente") throw new InvalidOperationException("Solo se puede modificar una venta pendiente.");

        var servicio = _db.MotivoCobros.FirstOrDefault(m => !m.EstadoEliminado && m.Descripcion.ToLower() == descripcion.ToLower());
        if (servicio is null)
        {
            servicio = new MotivoCobro
            {
                IdMotivoCobro = Guid.NewGuid(),
                Descripcion = descripcion,
                PrecioSugerido = precio,
                EstadoEliminado = false
            };
            servicio.BeforeSaveChanges();
            _db.MotivoCobros.Add(servicio);
            _db.SaveChanges();
        }
        return AddServicio(idVenta, servicio.IdMotivoCobro, cantidad, precio, descripcion, descuentoMonto, motivoDescuento);
    }
}
