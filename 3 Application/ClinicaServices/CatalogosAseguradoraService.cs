using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public sealed class AseguradoraTarifaPrecios
{
    public Dictionary<Guid, AseguradoraTarifaPrecio> Productos { get; init; } = new();
    public Dictionary<Guid, AseguradoraTarifaPrecio> Servicios { get; init; } = new();
}

public sealed record AseguradoraTarifaPrecio(decimal Convenido, decimal? Emergencia);
public sealed class AseguradoraTarifaCatalogoItem
{
    public Guid IdTarifa { get; init; }
    public Guid IdAseguradora { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string? UnidadArea { get; init; }
    public decimal PrecioConvenido { get; init; }
    public decimal? PrecioEmergencia { get; init; }
    public bool Activa { get; init; }
}

public sealed class AseguradoraTarifaConceptoItem
{
    public string Clave { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string? UnidadArea { get; init; }
}

public interface IAseguradoraTarifaService
{
    AseguradoraTarifaPrecios GetPrecios(Guid? idAseguradora);
    List<AseguradoraTarifaCatalogoItem> GetCatalogo(Guid idAseguradora);
    List<AseguradoraTarifaConceptoItem> GetConceptosDisponibles(Guid idAseguradora);
    void Crear(Guid idAseguradora, Guid? idProducto, Guid? idMotivoCobro, decimal precioConvenido, decimal? precioEmergencia);
    void Actualizar(Guid idTarifa, Guid idAseguradora, decimal precioConvenido, decimal? precioEmergencia);
    void CambiarActiva(Guid idTarifa, Guid idAseguradora, bool activa);
}

public class AseguradoraTarifaService(ClinicaContext db) : IAseguradoraTarifaService
{
    public List<AseguradoraTarifaCatalogoItem> GetCatalogo(Guid idAseguradora)
    {
        var tarifas=db.AseguradoraTarifas.Where(t=>t.IdAseguradora==idAseguradora).OrderBy(t=>t.IdProducto==null).ToList();
        var prodIds=tarifas.Where(t=>t.IdProducto.HasValue).Select(t=>t.IdProducto!.Value).Distinct().ToList();
        var servIds=tarifas.Where(t=>t.IdMotivoCobro.HasValue).Select(t=>t.IdMotivoCobro!.Value).Distinct().ToList();
        var productos=db.Productos.Where(p=>prodIds.Contains(p.IdProducto)).ToDictionary(p=>p.IdProducto);
        var servicios=db.MotivoCobros.Where(s=>servIds.Contains(s.IdMotivoCobro)).ToDictionary(s=>s.IdMotivoCobro);
        return tarifas.Select(t=>
        {
            var producto=t.IdProducto.HasValue?productos.GetValueOrDefault(t.IdProducto.Value):null;
            var servicio=t.IdMotivoCobro.HasValue?servicios.GetValueOrDefault(t.IdMotivoCobro.Value):null;
            return new AseguradoraTarifaCatalogoItem
            {
                IdTarifa=t.IdAseguradoraTarifa,IdAseguradora=t.IdAseguradora,
                Descripcion=producto?.Nombre ?? servicio?.Descripcion ?? "Concepto no disponible",
                Tipo=producto is not null?"Producto":"Servicio",
                UnidadArea=producto?.UnidadMedida ?? servicio?.Area,
                PrecioConvenido=t.PrecioConvenido,PrecioEmergencia=t.PrecioEmergencia,Activa=t.Activa
            };
        }).ToList();
    }
    public void Actualizar(Guid idTarifa, Guid idAseguradora, decimal precioConvenido, decimal? precioEmergencia)
    {
        if(precioConvenido<0 || (precioEmergencia.HasValue && precioEmergencia.Value<0)) throw new ArgumentException("Las tarifas no pueden ser negativas.");
        var tarifa=db.AseguradoraTarifas.FirstOrDefault(t=>t.IdAseguradoraTarifa==idTarifa && t.IdAseguradora==idAseguradora)
            ?? throw new InvalidOperationException("Tarifa no encontrada para esta aseguradora.");
        tarifa.PrecioConvenido=decimal.Round(precioConvenido,2,MidpointRounding.AwayFromZero);
        tarifa.PrecioEmergencia=precioEmergencia.HasValue?decimal.Round(precioEmergencia.Value,2,MidpointRounding.AwayFromZero):null;
        db.SaveChanges();
    }

    public List<AseguradoraTarifaConceptoItem> GetConceptosDisponibles(Guid idAseguradora)
    {
        var aseguradora = db.Aseguradoras.FirstOrDefault(a => a.IdAseguradora == idAseguradora);
        if (aseguradora is null) return new();
        var tarifas = db.AseguradoraTarifas.Where(t => t.IdAseguradora == idAseguradora).ToList();
        var productosTarifados = tarifas.Where(t => t.IdProducto.HasValue).Select(t => t.IdProducto!.Value).ToHashSet();
        var serviciosTarifados = tarifas.Where(t => t.IdMotivoCobro.HasValue).Select(t => t.IdMotivoCobro!.Value).ToHashSet();
        var productos = db.Productos.Where(p => (p.IdHospital == aseguradora.IdHospital || p.IdHospital == Guid.Empty) && p.Activo && !productosTarifados.Contains(p.IdProducto))
            .OrderBy(p => p.Nombre)
            .Select(p => new AseguradoraTarifaConceptoItem { Clave = $"P:{p.IdProducto}", Descripcion = p.Nombre, Tipo = "Producto", UnidadArea = p.UnidadMedida })
            .ToList();
        var servicios = db.MotivoCobros.Where(s => (s.IdHospital == aseguradora.IdHospital || s.IdHospital == Guid.Empty) && !s.EstadoEliminado && !serviciosTarifados.Contains(s.IdMotivoCobro))
            .OrderBy(s => s.Descripcion)
            .Select(s => new AseguradoraTarifaConceptoItem { Clave = $"S:{s.IdMotivoCobro}", Descripcion = s.Descripcion, Tipo = "Servicio", UnidadArea = s.Area })
            .ToList();
        return productos.Concat(servicios).OrderBy(x => x.Tipo).ThenBy(x => x.Descripcion).ToList();
    }

    public void Crear(Guid idAseguradora, Guid? idProducto, Guid? idMotivoCobro, decimal precioConvenido, decimal? precioEmergencia)
    {
        if (idProducto.HasValue == idMotivoCobro.HasValue) throw new ArgumentException("Selecciona un producto o un servicio.");
        if (precioConvenido < 0 || (precioEmergencia.HasValue && precioEmergencia.Value < 0)) throw new ArgumentException("Las tarifas no pueden ser negativas.");
        var aseguradora = db.Aseguradoras.FirstOrDefault(a => a.IdAseguradora == idAseguradora)
            ?? throw new InvalidOperationException("Aseguradora no encontrada.");
        if (idProducto.HasValue && !db.Productos.Any(p => p.IdProducto == idProducto && (p.IdHospital == aseguradora.IdHospital || p.IdHospital == Guid.Empty) && p.Activo))
            throw new ArgumentException("Producto no válido para el hospital de la aseguradora.");
        if (idMotivoCobro.HasValue && !db.MotivoCobros.Any(s => s.IdMotivoCobro == idMotivoCobro && (s.IdHospital == aseguradora.IdHospital || s.IdHospital == Guid.Empty) && !s.EstadoEliminado))
            throw new ArgumentException("Servicio no válido para el hospital de la aseguradora.");
        var existente = db.AseguradoraTarifas.FirstOrDefault(t => t.IdAseguradora == idAseguradora && t.IdProducto == idProducto && t.IdMotivoCobro == idMotivoCobro);
        if (existente is not null)
        {
            existente.PrecioConvenido = decimal.Round(precioConvenido, 2, MidpointRounding.AwayFromZero);
            existente.PrecioEmergencia = precioEmergencia.HasValue ? decimal.Round(precioEmergencia.Value, 2, MidpointRounding.AwayFromZero) : null;
            existente.Activa = true;
        }
        else
        {
            db.AseguradoraTarifas.Add(new AseguradoraTarifa
            {
                IdAseguradoraTarifa = Guid.NewGuid(),
                IdHospital = aseguradora.IdHospital,
                IdAseguradora = idAseguradora,
                IdProducto = idProducto,
                IdMotivoCobro = idMotivoCobro,
                PrecioConvenido = decimal.Round(precioConvenido, 2, MidpointRounding.AwayFromZero),
                PrecioEmergencia = precioEmergencia.HasValue ? decimal.Round(precioEmergencia.Value, 2, MidpointRounding.AwayFromZero) : null,
                Activa = true
            });
        }
        db.SaveChanges();
    }

    public void CambiarActiva(Guid idTarifa, Guid idAseguradora, bool activa)
    {
        var tarifa = db.AseguradoraTarifas.FirstOrDefault(t => t.IdAseguradoraTarifa == idTarifa && t.IdAseguradora == idAseguradora)
            ?? throw new InvalidOperationException("Tarifa no encontrada para esta aseguradora.");
        tarifa.Activa = activa;
        db.SaveChanges();
    }
    public AseguradoraTarifaPrecios GetPrecios(Guid? idAseguradora)
    {
        if (!idAseguradora.HasValue) return new();
        var tarifas=db.AseguradoraTarifas.Where(t=>t.IdAseguradora==idAseguradora.Value && t.Activa).ToList();
        return new AseguradoraTarifaPrecios
        {
            Productos=tarifas.Where(t=>t.IdProducto.HasValue).ToDictionary(t=>t.IdProducto!.Value,t=>new AseguradoraTarifaPrecio(t.PrecioConvenido,t.PrecioEmergencia)),
            Servicios=tarifas.Where(t=>t.IdMotivoCobro.HasValue).ToDictionary(t=>t.IdMotivoCobro!.Value,t=>new AseguradoraTarifaPrecio(t.PrecioConvenido,t.PrecioEmergencia))
        };
    }
}

public interface IAseguradoraService
{
    List<Aseguradora> GetAll(bool soloActivas = false);
    List<Aseguradora> GetActivas();
    Aseguradora? Get(Guid id);
    Aseguradora Guardar(Aseguradora item);
    void CambiarActiva(Guid id, bool activa);
}

public class AseguradoraService(ClinicaContext db, ICurrentUser? currentUser = null) : IAseguradoraService
{
    public List<Aseguradora> GetAll(bool soloActivas = false) => (soloActivas ? db.Aseguradoras.Where(x => x.Activa) : db.Aseguradoras).OrderBy(x => x.Nombre).ToList();
    public List<Aseguradora> GetActivas() => GetAll(true);
    public Aseguradora? Get(Guid id) => db.Aseguradoras.FirstOrDefault(x => x.IdAseguradora == id);
    public Aseguradora Guardar(Aseguradora item)
    {
        item.BeforeSaveChanges();
        if (string.IsNullOrWhiteSpace(item.Nombre)) throw new ArgumentException("Nombre requerido.");
        if (item.CoaseguroPorcDefault is < 0 or > 100) throw new ArgumentException("El coaseguro debe estar entre 0 y 100.");
        if (item.IdAseguradora == Guid.Empty) { item.IdAseguradora = Guid.NewGuid(); item.IdHospital = currentUser?.Usuario?.HospitalId ?? Guid.Empty; db.Aseguradoras.Add(item); }
        else { var actual = Get(item.IdAseguradora) ?? throw new InvalidOperationException("Aseguradora no encontrada."); actual.Nombre=item.Nombre; actual.IdentificadorFiscal=item.IdentificadorFiscal; actual.Contacto=item.Contacto; actual.CopagoDefault=item.CopagoDefault; actual.CoaseguroPorcDefault=item.CoaseguroPorcDefault; actual.Activa=item.Activa; item=actual; }
        db.SaveChanges(); return item;
    }
    public void CambiarActiva(Guid id, bool activa) { var a=Get(id); if(a is null)return; a.Activa=activa; db.SaveChanges(); }
}

