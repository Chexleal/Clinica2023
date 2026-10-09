using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public sealed class EstadoCuentaCategoria
{
    public Guid? IdCategoriaProducto { get; init; }
    public string Nombre { get; init; } = "OTROS";
    public int Orden { get; init; }
    public List<VentaDetalle> Detalles { get; init; } = new();
    public decimal Subtotal => Detalles.Sum(x => x.Subtotal);
}

public sealed class EstadoCuenta
{
    public Venta Venta { get; init; } = null!;
    public Paciente? Paciente { get; init; }
    public Aseguradora? Aseguradora { get; init; }
    public List<EstadoCuentaCategoria> Categorias { get; init; } = new();
    public decimal Total => Categorias.Sum(x => x.Subtotal);
    public decimal CoaseguroMonto => decimal.Round(Total * Venta.CoaseguroPorc / 100m, 2, MidpointRounding.AwayFromZero);
    public decimal TotalAsegurado => Venta.Copago + CoaseguroMonto;
    public decimal TotalAseguradora => Total - TotalAsegurado;
}

public interface IEstadoCuentaService
{
    EstadoCuenta? GetEstadoCuenta(Guid idVenta);
    List<Venta> Buscar(DateTime? desde, DateTime? hasta, Guid? idPaciente, Guid? idAseguradora, string? autorizacion);
}

public class EstadoCuentaService(ClinicaContext db) : IEstadoCuentaService
{
    public EstadoCuenta? GetEstadoCuenta(Guid idVenta)
    {
        var venta = db.Ventas.FirstOrDefault(v => v.IdVenta == idVenta);
        if (venta is null) return null;
        var paciente = venta.IdPaciente.HasValue ? db.Pacientes.FirstOrDefault(p => p.IdPaciente == venta.IdPaciente) : null;
        var aseguradora = venta.IdAseguradora.HasValue ? db.Aseguradoras.FirstOrDefault(a => a.IdAseguradora == venta.IdAseguradora) : null;
        var lineas = db.VentaDetalles.Where(d => d.IdVenta == idVenta).ToList();
        var productoIds = lineas.Where(x => x.TipoLinea == "Producto" && x.IdProducto.HasValue).Select(x => x.IdProducto!.Value).Distinct().ToList();
        var servicioIds = lineas.Where(x => x.TipoLinea != "Producto" && x.IdMotivoCobro.HasValue).Select(x => x.IdMotivoCobro!.Value).Distinct().ToList();
        var categoriasPorProducto = db.Productos.Where(x => productoIds.Contains(x.IdProducto))
            .ToDictionary(x => x.IdProducto, x => (Guid?)x.IdCategoriaProducto);
        var categoriasPorServicio = db.MotivoCobros.Where(x => servicioIds.Contains(x.IdMotivoCobro))
            .ToDictionary(x => x.IdMotivoCobro, x => x.IdCategoriaProducto);
        var categoriaIds = categoriasPorProducto.Values.Concat(categoriasPorServicio.Values)
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var categorias = db.CategoriasProducto.Where(x => categoriaIds.Contains(x.IdCategoriaProducto))
            .ToDictionary(x => x.IdCategoriaProducto);
        var fallback = db.CategoriasProducto.FirstOrDefault(x => x.Activo && x.Nombre.ToUpper() == "OTROS");
        var grupos = lineas.GroupBy(d =>
        {
            Guid? categoriaId = d.TipoLinea == "Producto" && d.IdProducto.HasValue
                ? categoriasPorProducto.GetValueOrDefault(d.IdProducto.Value)
                : d.IdMotivoCobro.HasValue ? categoriasPorServicio.GetValueOrDefault(d.IdMotivoCobro.Value) : null;
            return categoriaId.HasValue && categorias.TryGetValue(categoriaId.Value, out var categoria) ? categoria : fallback;
        });
        var salida = grupos.Select(g => new EstadoCuentaCategoria
        {
            Nombre = g.Key?.Nombre ?? "OTROS",
            Orden = g.Key?.Orden ?? 99,
            IdCategoriaProducto = g.Key?.IdCategoriaProducto,
            Detalles = g.ToList()
        }).OrderBy(x => x.Orden).ThenBy(x => x.Nombre).ToList();
        return new EstadoCuenta { Venta = venta, Paciente = paciente, Aseguradora = aseguradora, Categorias = salida };
    }

    public List<Venta> Buscar(DateTime? desde, DateTime? hasta, Guid? idPaciente, Guid? idAseguradora, string? autorizacion)
    {
        var q = db.Ventas.AsQueryable();
        if (desde.HasValue) q = q.Where(v => v.Fecha >= desde.Value.Date);
        if (hasta.HasValue) q = q.Where(v => v.Fecha < hasta.Value.Date.AddDays(1));
        if (idPaciente.HasValue) q = q.Where(v => v.IdPaciente == idPaciente);
        if (idAseguradora.HasValue) q = q.Where(v => v.IdAseguradora == idAseguradora);
        if (!string.IsNullOrWhiteSpace(autorizacion)) q = q.Where(v => v.Autorizacion != null && v.Autorizacion.Contains(autorizacion));
        return q.OrderByDescending(v => v.Fecha).Take(500).ToList();
    }
}
