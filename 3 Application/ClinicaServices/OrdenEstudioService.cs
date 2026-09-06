using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public interface IOrdenEstudioService
{
    List<OrdenEstudio> GetByPaciente(Guid idPaciente);
    List<OrdenEstudio> GetPendientesByPaciente(Guid idPaciente);
    List<OrdenEstudio> GetAllPendientes();
    OrdenEstudio? GetById(Guid id);
    OrdenEstudio Crear(OrdenEstudio orden);
    void ActualizarEstado(Guid idOrden, EstadoOrden estado);
    void Eliminar(Guid idOrden);
}

public class OrdenEstudioService : IOrdenEstudioService
{
    private readonly ClinicaContext _db;
    public OrdenEstudioService(ClinicaContext db) => _db = db;

    public List<OrdenEstudio> GetByPaciente(Guid idPaciente)
    {
        try { return _db.OrdenesEstudio.Where(o => o.IdPaciente == idPaciente && o.FechaEliminacion == null)
           .OrderByDescending(o => o.FechaOrden).ToList(); }
        catch { return new List<OrdenEstudio>(); }
    }

    public List<OrdenEstudio> GetPendientesByPaciente(Guid idPaciente)
    {
        try { return _db.OrdenesEstudio.Where(o => o.IdPaciente == idPaciente && o.Estado == EstadoOrden.Pendiente && o.FechaEliminacion == null).ToList(); }
        catch { return new List<OrdenEstudio>(); }
    }

    public List<OrdenEstudio> GetAllPendientes()
    {
        try { return _db.OrdenesEstudio.Include(o => o.Paciente).Where(o => o.Estado == EstadoOrden.Pendiente && o.FechaEliminacion == null).OrderBy(o => o.FechaOrden).ToList(); }
        catch { return new List<OrdenEstudio>(); }
    }

    public OrdenEstudio? GetById(Guid id) => _db.OrdenesEstudio.FirstOrDefault(o => o.IdOrden == id);

    public OrdenEstudio Crear(OrdenEstudio orden)
    {
        orden.IdOrden = Guid.NewGuid();
        orden.FechaOrden = DateTime.Now;
        if (orden.Estado == default) orden.Estado = EstadoOrden.Pendiente;
        _db.OrdenesEstudio.Add(orden);
        _db.SaveChanges();
        return orden;
    }

    public void ActualizarEstado(Guid idOrden, EstadoOrden estado)
    {
        var o = _db.OrdenesEstudio.FirstOrDefault(x => x.IdOrden == idOrden);
        if (o == null) return;
        o.Estado = estado;
        _db.SaveChanges();
    }

    public void Eliminar(Guid idOrden)
    {
        // Soft delete: no rompe FK con estudios ya cargados
        var o = _db.OrdenesEstudio.FirstOrDefault(x => x.IdOrden == idOrden);
        if (o == null) return;
        o.FechaEliminacion = DateTime.UtcNow;
        _db.SaveChanges();
    }
}
