using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public interface IOrdenEstudioService
{
    List<OrdenEstudio> GetByPaciente(Guid idPaciente);
    List<OrdenEstudio> GetPendientesByPaciente(Guid idPaciente);
    List<OrdenEstudio> GetAllPendientes();
    List<OrdenEstudio> GetAllExternasPendientes();
    List<OrdenEstudio> GetExternasByPaciente(Guid idPaciente);
    List<OrdenEstudio> GetExternasByConsulta(Guid idConsulta);
    OrdenEstudio? GetById(Guid id);
    OrdenEstudio? GetByIdDetallado(Guid id);
    OrdenEstudio Crear(OrdenEstudio orden);
    void ActualizarEstado(Guid idOrden, EstadoOrden estado);
    void MarcarImpresa(Guid idOrden);
    void SetEsExterna(Guid idOrden, bool esExterna);
    void Eliminar(Guid idOrden);
}

public class OrdenEstudioService(ClinicaContext db) : IOrdenEstudioService
{

    public List<OrdenEstudio> GetByPaciente(Guid idPaciente)
    {
        try { return db.OrdenesEstudio.Where(o => o.IdPaciente == idPaciente && o.FechaEliminacion == null)
           .OrderByDescending(o => o.FechaOrden).ToList(); }
        catch { return new List<OrdenEstudio>(); }
    }

    public List<OrdenEstudio> GetPendientesByPaciente(Guid idPaciente)
    {
        try { return db.OrdenesEstudio.Where(o => o.IdPaciente == idPaciente && o.Estado == EstadoOrden.Pendiente && !o.EsExterna && o.FechaEliminacion == null).ToList(); }
        catch { return new List<OrdenEstudio>(); }
    }

    public List<OrdenEstudio> GetAllPendientes()
    {
        // Cola de trabajo interno: solo órdenes NO externas en estado Pendiente.
        // Las externas se imprimen y no deben estorbar aquí.
        try { return db.OrdenesEstudio.Include(o => o.Paciente).Where(o => o.Estado == EstadoOrden.Pendiente && !o.EsExterna && o.FechaEliminacion == null).OrderBy(o => o.FechaOrden).ToList(); }
        catch { return new List<OrdenEstudio>(); }
    }

    public List<OrdenEstudio> GetAllExternasPendientes()
    {
        try { return db.OrdenesEstudio.Include(o => o.Paciente).Where(o => o.EsExterna && (o.Estado == EstadoOrden.Pendiente || o.Estado == EstadoOrden.Impresa) && o.FechaEliminacion == null).OrderByDescending(o => o.FechaOrden).ToList(); }
        catch { return new List<OrdenEstudio>(); }
    }

    public List<OrdenEstudio> GetExternasByPaciente(Guid idPaciente)
    {
        // Externas no cerradas: Pendiente (por imprimir) o Impresa (impresa, por entregar).
        // Las Completadas (resultado ya cargado) y eliminadas ya no aparecen.
        try { return db.OrdenesEstudio.Where(o => o.IdPaciente == idPaciente && o.EsExterna && (o.Estado == EstadoOrden.Pendiente || o.Estado == EstadoOrden.Impresa) && o.FechaEliminacion == null).OrderByDescending(o => o.FechaOrden).ToList(); }
        catch { return new List<OrdenEstudio>(); }
    }

    public List<OrdenEstudio> GetExternasByConsulta(Guid idConsulta)
    {
        // Solo las nacidas en esta consulta: no se repiten en otras consultas del mismo paciente.
        try { return db.OrdenesEstudio.Include(o => o.Paciente).Where(o => o.IdConsulta == idConsulta && o.EsExterna && (o.Estado == EstadoOrden.Pendiente || o.Estado == EstadoOrden.Impresa) && o.FechaEliminacion == null).OrderByDescending(o => o.FechaOrden).ToList(); }
        catch { return new List<OrdenEstudio>(); }
    }

    public OrdenEstudio? GetById(Guid id) => db.OrdenesEstudio.Include(o => o.Paciente).FirstOrDefault(o => o.IdOrden == id);

    public OrdenEstudio? GetByIdDetallado(Guid id) => db.OrdenesEstudio.Include(o => o.Paciente).Include(o => o.Consulta).FirstOrDefault(o => o.IdOrden == id);

    public OrdenEstudio Crear(OrdenEstudio orden)
    {
        orden.IdOrden = Guid.NewGuid();
        orden.FechaOrden = DateTime.Now;
        if (orden.Estado == default) orden.Estado = EstadoOrden.Pendiente;
        db.OrdenesEstudio.Add(orden);
        db.SaveChanges();
        return orden;
    }

    public void ActualizarEstado(Guid idOrden, EstadoOrden estado)
    {
        var o = db.OrdenesEstudio.FirstOrDefault(x => x.IdOrden == idOrden);
        if (o == null) return;
        o.Estado = estado;
        db.SaveChanges();
    }

    public void MarcarImpresa(Guid idOrden)
    {
        var o = db.OrdenesEstudio.FirstOrDefault(x => x.IdOrden == idOrden);
        if (o == null) return;
        // Solo tiene sentido para externas; si es interna la dejamos igual (no cambia cola).
        o.Estado = EstadoOrden.Impresa;
        db.SaveChanges();
    }

    public void SetEsExterna(Guid idOrden, bool esExterna)
    {
        var o = db.OrdenesEstudio.FirstOrDefault(x => x.IdOrden == idOrden);
        if (o == null) return;
        o.EsExterna = esExterna;
        // Si vuelve a interna y estaba Impresa, regresa a Pendiente para reingresar a la cola.
        if (!esExterna && o.Estado == EstadoOrden.Impresa)
            o.Estado = EstadoOrden.Pendiente;
        db.SaveChanges();
    }

    public void Eliminar(Guid idOrden)
    {
        // Soft delete: no rompe FK con estudios ya cargados
        var o = db.OrdenesEstudio.FirstOrDefault(x => x.IdOrden == idOrden);
        if (o == null) return;
        o.FechaEliminacion = DateTime.UtcNow;
        db.SaveChanges();
    }
}
