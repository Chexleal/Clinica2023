using ClinicaDomain;
using ClinicaInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public interface ICotizacionService
{
    Cotizacion Crear(Guid? idPaciente, string? clienteNombre, int vigenciaDias = 15, string? observaciones = null);
    bool DescartarSiVacia(Guid idCotizacion);
    Cotizacion? Get(Guid idCotizacion);
    List<Cotizacion> GetActivas(int top = 100);
    List<Cotizacion> GetHistorial(DateTime from, DateTime to, string? texto = null, int top = 200);
    List<CotizacionDetalle> GetDetalles(Guid idCotizacion);
    CotizacionDetalle AddServicio(Guid idCotizacion, Guid motivoCobroId, decimal cantidad, decimal? precioUnitario = null, string? descripcion = null, decimal descuentoMonto = 0, string? motivoDescuento = null);
    CotizacionDetalle AddProducto(Guid idCotizacion, Guid productoId, decimal cantidad, Guid? loteId = null, decimal? precioUnitario = null, string? descripcion = null, bool esSobrePedido = false, decimal descuentoMonto = 0, string? motivoDescuento = null);
    CotizacionDetalle AplicarDescuento(Guid idCotizacionDetalle, decimal descuentoMonto, string? motivoDescuento);
    void RemoveDetalle(Guid idCotizacionDetalle);
    /// <summary>Borrador -&gt; Vigente. Fija FechaVence = Fecha + VigenciaDias.</summary>
    void Emitir(Guid idCotizacion, int? vigenciaDias = null);
    void Rechazar(Guid idCotizacion);
    void Anular(Guid idCotizacion);
    /// <summary>Vigentes vencidas por fecha -&gt; Vencida. Se corre al listar.</summary>
    void MarcarVencidas();
    /// <summary>Alta rápida desde cotización: crea el producto (sin lote) SIN entrada de inventario + línea. La cotización no mueve stock.</summary>
    CotizacionDetalle AgregarProductoExpress(Guid idCotizacion, string nombre, decimal precioVenta, decimal cantidad, decimal? costoUnitario = null, Guid? idCategoria = null, decimal descuentoMonto = 0, string? motivoDescuento = null);
    /// <summary>Servicio no catalogado: crea el MotivoCobro (si no existe) + línea. No toca inventario.</summary>
    CotizacionDetalle AgregarServicioExpress(Guid idCotizacion, string descripcion, decimal precio, decimal cantidad, decimal descuentoMonto = 0, string? motivoDescuento = null);
    /// <summary>Genera la Venta libre Pendiente con las mismas líneas (ahí sí se valida stock). Devuelve la venta.</summary>
    Venta ConvertirAVenta(Guid idCotizacion);
}

public class CotizacionService(ClinicaContext db, ICurrentUser? currentUser, IVentaService ventas) : ICotizacionService
{
    private static readonly string[] Editables = ["Borrador", "Vigente"];

    private string NuevoFolio() =>
        $"COT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

    /// <summary>Suma líneas vigentes: BD + agregadas pendientes − eliminadas pendientes (el SUM en SQL no ve lo aún no guardado).</summary>
    private void RecalcularTotal(Cotizacion cot)
    {
        var id = cot.IdCotizacion;
        var enBd = db.CotizacionDetalles.AsNoTracking()
            .Where(d => d.IdCotizacion == id)
            .ToDictionary(d => d.IdCotizacionDetalle, d => d.Subtotal);
        decimal total = 0;
        foreach (var e in db.ChangeTracker.Entries<CotizacionDetalle>().Where(e => e.Entity.IdCotizacion == id))
        {
            if (e.State == EntityState.Added) total += e.Entity.Subtotal;
            else if (e.State == EntityState.Deleted) enBd.Remove(e.Entity.IdCotizacionDetalle);
            else enBd[e.Entity.IdCotizacionDetalle] = e.Entity.Subtotal; // Modified/Unchanged: valor vigente
        }
        cot.Total = total + enBd.Values.Sum();
    }

    private Cotizacion ExigirEditable(Guid idCotizacion)
    {
        var cot = db.Cotizaciones.FirstOrDefault(c => c.IdCotizacion == idCotizacion)
            ?? throw new InvalidOperationException("Cotización no encontrada.");
        if (!Editables.Contains(cot.Estado))
            throw new InvalidOperationException($"La cotización {cot.Folio} está {cot.Estado}: ya no se puede modificar.");
        return cot;
    }

    public Cotizacion Crear(Guid? idPaciente, string? clienteNombre, int vigenciaDias = 15, string? observaciones = null)
    {
        if (vigenciaDias is < 1 or > 365) vigenciaDias = 15;
        var cot = new Cotizacion
        {
            IdCotizacion = Guid.NewGuid(),
            Folio = NuevoFolio(),
            Fecha = DateTime.Now,
            VigenciaDias = vigenciaDias,
            IdPaciente = idPaciente,
            ClienteNombre = clienteNombre ?? string.Empty,
            Total = 0,
            Estado = "Borrador",
            Observaciones = observaciones ?? string.Empty
        };
        cot.BeforeSaveChanges();
        db.Cotizaciones.Add(cot);
        db.SaveChanges();
        return cot;
    }

    public bool DescartarSiVacia(Guid idCotizacion)
    {
        var c = db.Cotizaciones.FirstOrDefault(x => x.IdCotizacion == idCotizacion);
        if (c is null || c.Estado != "Borrador") return false;
        if (db.CotizacionDetalles.Any(d => d.IdCotizacion == idCotizacion)) return false;
        db.Cotizaciones.Remove(c);
        db.SaveChanges();
        return true;
    }

    public Cotizacion? Get(Guid idCotizacion) =>
        db.Cotizaciones.FirstOrDefault(c => c.IdCotizacion == idCotizacion);

    public List<Cotizacion> GetActivas(int top = 100)
    {
        MarcarVencidas();
        return db.Cotizaciones
            .Where(c => c.Estado == "Borrador" || c.Estado == "Vigente")
            .OrderByDescending(c => c.Fecha).Take(top).ToList();
    }

    public List<Cotizacion> GetHistorial(DateTime from, DateTime to, string? texto = null, int top = 200)
    {
        MarcarVencidas();
        var f = from.Date;
        var t = to.Date.AddDays(1);
        var q = db.Cotizaciones.Where(c => c.Fecha >= f && c.Fecha < t
            && (c.Estado == "Vigente" || c.Estado == "Vencida" || c.Estado == "Aceptada"
                || c.Estado == "Rechazada" || c.Estado == "Anulada"));
        texto = (texto ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(texto))
            q = q.Where(c => EF.Functions.Collate(c.Folio, "Latin1_General_CI_AI").Contains(texto)
                || (c.ClienteNombre != null && EF.Functions.Collate(c.ClienteNombre, "Latin1_General_CI_AI").Contains(texto))
                || (c.Observaciones != null && EF.Functions.Collate(c.Observaciones, "Latin1_General_CI_AI").Contains(texto)));
        return q.OrderByDescending(c => c.Fecha).Take(top).ToList();
    }

    public List<CotizacionDetalle> GetDetalles(Guid idCotizacion)
    {
        var lineas = db.CotizacionDetalles.Where(d => d.IdCotizacion == idCotizacion).ToList();
        try
        {
            var ids = lineas.Where(l => l.DescuentoOtorgadoPor.HasValue)
                .Select(l => l.DescuentoOtorgadoPor!.Value).Distinct().ToList();
            if (ids.Any())
            {
                var nombres = db.Usuarios.Where(u => ids.Contains(u.IdUsuario))
                    .ToDictionary(u => u.IdUsuario, u => (u.Nombre + " " + u.Apellido).Trim());
                foreach (var l in lineas)
                    if (l.DescuentoOtorgadoPor.HasValue && nombres.TryGetValue(l.DescuentoOtorgadoPor.Value, out var n))
                        l.DescuentoOtorgadoPorNombre = n;
            }
        }
        catch { }
        return lineas;
    }

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

    public CotizacionDetalle AddServicio(Guid idCotizacion, Guid motivoCobroId, decimal cantidad, decimal? precioUnitario = null, string? descripcion = null, decimal descuentoMonto = 0, string? motivoDescuento = null)
    {
        if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser mayor a cero.");
        var cot = ExigirEditable(idCotizacion);
        var servicio = db.MotivoCobros.FirstOrDefault(m => m.IdMotivoCobro == motivoCobroId)
            ?? throw new InvalidOperationException("Servicio no encontrado.");
        var precio = precioUnitario ?? servicio.PrecioSugerido;
        var bruto = cantidad * precio;
        var (desc, motivo) = ValidarDescuento(bruto, descuentoMonto, motivoDescuento);
        var detalle = new CotizacionDetalle
        {
            IdCotizacionDetalle = Guid.NewGuid(),
            IdCotizacion = idCotizacion,
            TipoLinea = "Servicio",
            IdMotivoCobro = motivoCobroId,
            Descripcion = descripcion.TextoLibre() is { Length: > 0 } d ? d : servicio.Descripcion,
            Cantidad = cantidad,
            PrecioUnitario = precio,
            Subtotal = bruto - desc,
            DescuentoMonto = desc,
            DescuentoMotivo = motivo,
            DescuentoOtorgadoPor = desc > 0 ? currentUser?.Usuario?.IdUsuario : null
        };
        detalle.BeforeSaveChanges();
        db.CotizacionDetalles.Add(detalle);
        RecalcularTotal(cot);
        db.SaveChanges();
        return detalle;
    }

    public CotizacionDetalle AddProducto(Guid idCotizacion, Guid productoId, decimal cantidad, Guid? loteId = null, decimal? precioUnitario = null, string? descripcion = null, bool esSobrePedido = false, decimal descuentoMonto = 0, string? motivoDescuento = null)
    {
        if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser mayor a cero.");
        if (precioUnitario.HasValue && precioUnitario.Value < 0) throw new ArgumentException("Precio no válido.");
        var cot = ExigirEditable(idCotizacion);
        var producto = db.Productos.FirstOrDefault(p => p.IdProducto == productoId && p.Activo)
            ?? throw new InvalidOperationException("Producto no encontrado o inactivo.");
        var precioFinal = precioUnitario ?? producto.PrecioVenta;
        if (precioFinal <= 0)
            throw new ArgumentException($"Indica el valor de '{producto.Nombre}': el catálogo lo tiene en Q0.00.");
        // NOTA: la cotización NO valida stock ni reserva nada. Al convertir a venta sí se valida.
        var bruto = cantidad * precioFinal;
        var (desc, motivo) = ValidarDescuento(bruto, descuentoMonto, motivoDescuento);
        var detalle = new CotizacionDetalle
        {
            IdCotizacionDetalle = Guid.NewGuid(),
            IdCotizacion = idCotizacion,
            TipoLinea = "Producto",
            IdProducto = productoId,
            IdLote = loteId,
            Descripcion = descripcion.TextoLibre() is { Length: > 0 } d ? d : producto.Nombre,
            Cantidad = cantidad,
            PrecioUnitario = precioFinal,
            Subtotal = bruto - desc,
            DescuentoMonto = desc,
            DescuentoMotivo = motivo,
            DescuentoOtorgadoPor = desc > 0 ? currentUser?.Usuario?.IdUsuario : null,
            EsSobrePedido = esSobrePedido
        };
        detalle.BeforeSaveChanges();
        db.CotizacionDetalles.Add(detalle);
        RecalcularTotal(cot);
        db.SaveChanges();
        return detalle;
    }

    public CotizacionDetalle AplicarDescuento(Guid idCotizacionDetalle, decimal descuentoMonto, string? motivoDescuento)
    {
        var detalle = db.CotizacionDetalles.FirstOrDefault(d => d.IdCotizacionDetalle == idCotizacionDetalle)
            ?? throw new InvalidOperationException("Línea no encontrada.");
        var cot = ExigirEditable(detalle.IdCotizacion);
        var bruto = detalle.Cantidad * detalle.PrecioUnitario;
        var (desc, motivo) = ValidarDescuento(bruto, descuentoMonto, motivoDescuento);
        detalle.DescuentoMonto = desc;
        detalle.DescuentoMotivo = motivo;
        detalle.DescuentoOtorgadoPor = desc > 0 ? currentUser?.Usuario?.IdUsuario : null;
        detalle.Subtotal = bruto - desc;
        RecalcularTotal(cot);
        db.SaveChanges();
        return detalle;
    }

    public void RemoveDetalle(Guid idCotizacionDetalle)
    {
        var detalle = db.CotizacionDetalles.FirstOrDefault(d => d.IdCotizacionDetalle == idCotizacionDetalle);
        if (detalle is null) return;
        var cot = ExigirEditable(detalle.IdCotizacion);
        db.CotizacionDetalles.Remove(detalle);
        RecalcularTotal(cot);
        db.SaveChanges();
    }

    public void Emitir(Guid idCotizacion, int? vigenciaDias = null)
    {
        var cot = db.Cotizaciones.FirstOrDefault(c => c.IdCotizacion == idCotizacion)
            ?? throw new InvalidOperationException("Cotización no encontrada.");
        if (cot.Estado != "Borrador") throw new InvalidOperationException("Solo se puede emitir un borrador.");
        if (!db.CotizacionDetalles.Any(d => d.IdCotizacion == idCotizacion))
            throw new InvalidOperationException("Agrega al menos una línea antes de emitir.");
        if (vigenciaDias is >= 1 and <= 365) cot.VigenciaDias = vigenciaDias.Value;
        cot.FechaVence = cot.Fecha.AddDays(cot.VigenciaDias);
        cot.Estado = "Vigente";
        db.SaveChanges();
    }

    public void Rechazar(Guid idCotizacion)
    {
        var cot = db.Cotizaciones.FirstOrDefault(c => c.IdCotizacion == idCotizacion)
            ?? throw new InvalidOperationException("Cotización no encontrada.");
        if (cot.Estado != "Vigente" && cot.Estado != "Borrador")
            throw new InvalidOperationException("Solo se puede rechazar una cotización vigente o en borrador.");
        cot.Estado = "Rechazada";
        db.SaveChanges();
    }

    public void Anular(Guid idCotizacion)
    {
        var cot = db.Cotizaciones.FirstOrDefault(c => c.IdCotizacion == idCotizacion)
            ?? throw new InvalidOperationException("Cotización no encontrada.");
        if (cot.Estado == "Aceptada")
            throw new InvalidOperationException("Ya fue convertida a venta: anula la venta si aplica.");
        cot.Estado = "Anulada";
        db.SaveChanges();
    }

    public void MarcarVencidas()
    {
        var hoy = DateTime.Today;
        var vencidas = db.Cotizaciones
            .Where(c => c.Estado == "Vigente" && c.FechaVence != null && c.FechaVence.Value.Date < hoy)
            .ToList();
        if (!vencidas.Any()) return;
        foreach (var c in vencidas) c.Estado = "Vencida";
        db.SaveChanges();
    }

    public Venta ConvertirAVenta(Guid idCotizacion)
    {
        var cot = db.Cotizaciones.FirstOrDefault(c => c.IdCotizacion == idCotizacion)
            ?? throw new InvalidOperationException("Cotización no encontrada.");
        if (cot.Estado != "Vigente" && cot.Estado != "Borrador")
            throw new InvalidOperationException("Solo se puede convertir una cotización vigente o en borrador.");
        if (cot.IdVentaConvertida.HasValue)
            throw new InvalidOperationException("Ya fue convertida a venta.");
        var lineas = db.CotizacionDetalles.Where(d => d.IdCotizacion == idCotizacion).ToList();
        if (!lineas.Any()) throw new InvalidOperationException("La cotización no tiene líneas.");

        // Pre-validar stock de productos (la venta sí descuenta al cobrar).
        foreach (var l in lineas.Where(x => x.TipoLinea == "Producto" && !x.EsSobrePedido && x.IdProducto.HasValue))
        {
            var producto = db.Productos.FirstOrDefault(p => p.IdProducto == l.IdProducto!.Value && p.Activo)
                ?? throw new InvalidOperationException($"Producto no disponible: {l.Descripcion}.");
            var disponible = producto.RequiereLote
                ? db.LotesProducto.Where(x => x.IdProducto == producto.IdProducto && x.Activo && x.Stock > 0).Sum(x => (decimal?)x.Stock) ?? 0
                : producto.StockActual;
            if (disponible < l.Cantidad)
                throw new InvalidOperationException($"Stock insuficiente de {producto.Nombre} (hay {disponible}, cotizado {l.Cantidad}). Ajusta la cotización o márcalo sobre pedido.");
        }

        var venta = ventas.CrearVentaLibre(cot.IdPaciente, $"Convertida de {cot.Folio}"
            + (string.IsNullOrWhiteSpace(cot.ClienteNombre) ? "" : $" · {cot.ClienteNombre}"));
        try
        {
            foreach (var l in lineas)
            {
                if (l.TipoLinea == "Servicio" && l.IdMotivoCobro.HasValue)
                    ventas.AddServicio(venta.IdVenta, l.IdMotivoCobro.Value, l.Cantidad, l.PrecioUnitario, l.Descripcion, l.DescuentoMonto, l.DescuentoMotivo);
                else if (l.TipoLinea == "Producto" && l.IdProducto.HasValue)
                    ventas.AddProducto(venta.IdVenta, l.IdProducto.Value, l.Cantidad, l.IdLote, l.PrecioUnitario, l.Descripcion, l.EsSobrePedido, l.DescuentoMonto, l.DescuentoMotivo);
            }
        }
        catch
        {
            // Sin líneas a medias: se elimina la venta creada y se reporta el motivo.
            foreach (var d in ventas.GetDetalles(venta.IdVenta)) ventas.RemoveDetalle(d.IdVentaDetalle);
            db.Ventas.Remove(ventas.GetVenta(venta.IdVenta)!);
            db.SaveChanges();
            throw;
        }

        cot.Estado = "Aceptada";
        cot.IdVentaConvertida = venta.IdVenta;
        db.SaveChanges();
        return ventas.GetVenta(venta.IdVenta)!;
    }

    public CotizacionDetalle AgregarProductoExpress(Guid idCotizacion, string nombre, decimal precioVenta, decimal cantidad, decimal? costoUnitario = null, Guid? idCategoria = null, decimal descuentoMonto = 0, string? motivoDescuento = null)
    {
        nombre = nombre.TextoCatalogo();
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre del producto requerido.");
        if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser mayor a cero.");
        if (precioVenta < 0) throw new ArgumentException("Precio no válido.");
        ExigirEditable(idCotizacion);

        // Reutilizar si ya existe (evita duplicados por alta rápida repetida).
        var existente = db.Productos.FirstOrDefault(p => p.Activo && p.Nombre.ToLower() == nombre.ToLower());
        Producto producto;
        if (existente is not null)
        {
            producto = existente;
        }
        else
        {
            Guid catId;
            if (idCategoria.HasValue && db.CategoriasProducto.Any(c => c.IdCategoriaProducto == idCategoria.Value && c.Tipo == "Bien"))
                catId = idCategoria.Value;
            else
                catId = db.CategoriasProducto.Where(c => c.Tipo == "Bien" && c.Activo)
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
            db.Productos.Add(producto);
            db.SaveChanges();
        }

        // SIN entrada de inventario: la cotización no mueve stock.
        if (precioVenta > 0) producto.PrecioVenta = precioVenta;
        return AddProducto(idCotizacion, producto.IdProducto, cantidad, null, null, null, false, descuentoMonto, motivoDescuento);
    }

    public CotizacionDetalle AgregarServicioExpress(Guid idCotizacion, string descripcion, decimal precio, decimal cantidad, decimal descuentoMonto = 0, string? motivoDescuento = null)
    {
        descripcion = descripcion.TextoCatalogo();
        if (string.IsNullOrWhiteSpace(descripcion)) throw new ArgumentException("Describe el servicio.");
        if (precio <= 0) throw new ArgumentException("Indica el valor del servicio.");
        if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser mayor a cero.");
        ExigirEditable(idCotizacion);

        var servicio = db.MotivoCobros.FirstOrDefault(m => !m.EstadoEliminado && m.Descripcion.ToLower() == descripcion.ToLower());
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
            db.MotivoCobros.Add(servicio);
            db.SaveChanges();
        }
        return AddServicio(idCotizacion, servicio.IdMotivoCobro, cantidad, precio, descripcion, descuentoMonto, motivoDescuento);
    }
}
