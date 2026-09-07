using ClinicaDomain;
using ClinicaInfrastructure;

namespace ClinicaServices;

public interface IMetodoPagoService
{
    List<MetodoPago> GetAll(bool soloActivos = true);
    MetodoPago? Get(Guid id);
    MetodoPago Crear(string nombre, bool requiereReferencia);
    void CambiarActivo(Guid id, bool activo);
    void EnsureSeed();
}

public class MetodoPagoService : IMetodoPagoService
{
    private readonly ClinicaContext _db;
    public MetodoPagoService(ClinicaContext db) { _db = db; }

    public List<MetodoPago> GetAll(bool soloActivos = true)
    {
        if (!_db.MetodosPago.Any()) EnsureSeed();
        var q = _db.MetodosPago.AsQueryable();
        if (soloActivos) q = q.Where(x => x.Activo);
        return q.OrderBy(x => x.Nombre).ToList();
    }

    public MetodoPago? Get(Guid id) =>
        _db.MetodosPago.FirstOrDefault(x => x.IdMetodoPago == id);

    public MetodoPago Crear(string nombre, bool requiereReferencia)
    {
        var limpio = nombre.TextoCatalogo();
        if (string.IsNullOrWhiteSpace(limpio)) throw new ArgumentException("Nombre requerido.");
        var existente = _db.MetodosPago.FirstOrDefault(x => x.Nombre.ToLower() == limpio.ToLower());
        if (existente is not null) return existente;
        var nuevo = new MetodoPago
        {
            IdMetodoPago = Guid.NewGuid(),
            Nombre = limpio,
            RequiereReferencia = requiereReferencia,
            Activo = true
        };
        _db.MetodosPago.Add(nuevo);
        _db.SaveChanges();
        return nuevo;
    }

    public void CambiarActivo(Guid id, bool activo)
    {
        var m = Get(id);
        if (m is null) return;
        m.Activo = activo;
        _db.SaveChanges();
    }

    public void EnsureSeed()
    {
        if (_db.MetodosPago.Any()) return;
        var seeds = new (string Nombre, bool Ref)[]
        {
            ("EFECTIVO", false),
            ("TARJETA DE CRÉDITO", true),
            ("TARJETA DE DÉBITO", true),
            ("TRANSFERENCIA BANCARIA", true),
            ("DEPÓSITO BANCARIO", true),
            ("CHEQUE", true),
        };
        foreach (var s in seeds)
        {
            _db.MetodosPago.Add(new MetodoPago
            {
                IdMetodoPago = Guid.NewGuid(),
                Nombre = s.Nombre,
                RequiereReferencia = s.Ref,
                Activo = true
            });
        }
        _db.SaveChanges();
    }
}
