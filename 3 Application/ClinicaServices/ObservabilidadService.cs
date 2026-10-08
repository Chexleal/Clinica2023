using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

/// <summary>Evento de auditoría unificado (columnas FechaCreacion/Modificacion/Eliminacion + autor).</summary>
public sealed class EventoAuditoria
{
    public string Entidad { get; set; } = string.Empty;
    public string Registro { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public DateTime? Fecha { get; set; }
    public string Usuario { get; set; } = "—";
    public string Ambito { get; set; } = string.Empty;
    public Guid? UsuarioId { get; set; }
    public Guid? ClinicaId { get; set; }
    public Guid? HospitalId { get; set; }
}

public interface IObservabilidadService
{
    PagedResult<ErrorLog> GetErrores(int start, int length, string? search, string? nivel, bool? resuelto, string sortDir);
    ErrorLog? GetError(Guid id);
    void CambiarResuelto(Guid id, bool resuelto, string? observaciones);
    List<EventoAuditoria> GetAuditoriaReciente(int topPorEntidad = 50);
}

public sealed class ObservabilidadService(ClinicaContext db) : IObservabilidadService
{
    public PagedResult<ErrorLog> GetErrores(int start, int length, string? search, string? nivel, bool? resuelto, string sortDir)
    {
        var query = db.ErrorLogs.AsNoTracking().AsQueryable();
        var total = query.Count();

        if (!string.IsNullOrWhiteSpace(nivel))
            query = query.Where(x => x.Nivel == nivel);
        if (resuelto.HasValue)
            query = query.Where(x => x.Resuelto == resuelto.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Mensaje.Contains(term) || x.TipoError.Contains(term)
                || (x.Ruta != null && x.Ruta.Contains(term)));
        }

        var totalFiltered = query.Count();
        var asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        query = asc ? query.OrderBy(x => x.FechaCreacion) : query.OrderByDescending(x => x.FechaCreacion);

        var data = length > 0 ? query.Skip(start).Take(length).ToList() : query.ToList();
        return new PagedResult<ErrorLog> { Total = total, TotalFiltered = totalFiltered, Data = data };
    }

    public ErrorLog? GetError(Guid id) => db.ErrorLogs.FirstOrDefault(x => x.IdErrorLog == id);

    public void CambiarResuelto(Guid id, bool resuelto, string? observaciones)
    {
        var e = db.ErrorLogs.FirstOrDefault(x => x.IdErrorLog == id);
        if (e is null) return;
        e.Resuelto = resuelto;
        e.Observaciones = observaciones ?? string.Empty;
        db.SaveChanges();
    }

    public List<EventoAuditoria> GetAuditoriaReciente(int topPorEntidad = 50)
    {
        var eventos = new List<EventoAuditoria>();
        eventos.AddRange(EventosDe(db.Pacientes, p => $"{p.Nombre} {p.Apellido}".Trim(), null, topPorEntidad));
        eventos.AddRange(EventosDe(db.Consulta, c => $"Consulta {c.Fecha:dd/MM/yyyy}", c => (Guid?)c.IdClinica, topPorEntidad));
        eventos.AddRange(EventosDe(db.Cita, c => $"{c.Titulo} {c.FechaHora:dd/MM/yyyy HH:mm}", c => (Guid?)c.IdClinica, topPorEntidad));
        eventos.AddRange(EventosDe(db.Ventas, v => $"Folio {v.Folio}", v => (Guid?)v.IdClinica, topPorEntidad));
        eventos.AddRange(EventosDe(db.Gastos, g => g.Concepto, g => (Guid?)g.IdClinica, topPorEntidad));
        eventos.AddRange(EventosDe(db.Usuarios, u => u.NombreUsuario, null, topPorEntidad));
        eventos.AddRange(EventosDe(db.Productos, p => p.Nombre, null, topPorEntidad, p => (Guid?)p.IdHospital));
        eventos.AddRange(EventosDe(db.OrdenesEstudio, o => $"{o.Tipo} {o.FechaOrden:dd/MM/yyyy}", o => (Guid?)o.IdClinica, topPorEntidad));
        eventos.AddRange(EventosDe(db.EstudiosImagen, e => e.Titulo, e => (Guid?)e.IdClinica, topPorEntidad));
        eventos.AddRange(EventosDe(db.NotasMedicas, n => $"Nota {n.FechaNota:dd/MM/yyyy}", n => (Guid?)n.IdClinica, topPorEntidad));

        // Nombres de usuarios y ámbitos en 3 queries (sin N+1).
        var usuarios = db.Usuarios.AsNoTracking()
            .ToDictionary(u => u.IdUsuario, u => (u.Nombre + " " + u.Apellido).Trim());
        var clinicas = db.Clinicas.AsNoTracking().Include(c => c.Hospital)
            .ToDictionary(c => c.IdClinica, c => $"{c.Hospital!.Nombre} · {c.Nombre}");
        var hospitales = db.Hospitales.AsNoTracking()
            .ToDictionary(h => h.IdHospital, h => h.Nombre);

        foreach (var ev in eventos)
        {
            if (ev.UsuarioId.HasValue && usuarios.TryGetValue(ev.UsuarioId.Value, out var nom) && !string.IsNullOrWhiteSpace(nom))
                ev.Usuario = nom;
            else if (ev.UsuarioId.HasValue)
                ev.Usuario = ev.UsuarioId.Value.ToString().Substring(0, 8);

            if (ev.ClinicaId.HasValue && ev.ClinicaId.Value != Guid.Empty && clinicas.TryGetValue(ev.ClinicaId.Value, out var c))
                ev.Ambito = c;
            else if (ev.HospitalId.HasValue && ev.HospitalId.Value != Guid.Empty && hospitales.TryGetValue(ev.HospitalId.Value, out var h))
                ev.Ambito = h;
        }

        return eventos.OrderByDescending(e => e.Fecha).Take(200).ToList();
    }

    private static List<EventoAuditoria> EventosDe<T>(
        IQueryable<T> query, Func<T, string> describir, Func<T, Guid?>? clinicaDe,
        int top, Func<T, Guid?>? hospitalDe = null) where T : Base
    {
        var lista = new List<EventoAuditoria>();
        var nombre = typeof(T).Name;

        foreach (var e in query.AsNoTracking().OrderByDescending(x => x.FechaCreacion).Take(top).ToList())
            lista.Add(Nuevo(nombre, describir(e), "Creado", e.FechaCreacion, e.CreadoPor, clinicaDe?.Invoke(e), hospitalDe?.Invoke(e)));
        foreach (var e in query.AsNoTracking().Where(x => x.FechaModificacion != null).OrderByDescending(x => x.FechaModificacion).Take(top).ToList())
            lista.Add(Nuevo(nombre, describir(e), "Modificado", e.FechaModificacion, e.ModificadoPor, clinicaDe?.Invoke(e), hospitalDe?.Invoke(e)));
        foreach (var e in query.AsNoTracking().Where(x => x.FechaEliminacion != null).OrderByDescending(x => x.FechaEliminacion).Take(top).ToList())
            lista.Add(Nuevo(nombre, describir(e), "Eliminado", e.FechaEliminacion, e.EliminadoPor, clinicaDe?.Invoke(e), hospitalDe?.Invoke(e)));

        return lista;
    }

    private static EventoAuditoria Nuevo(
        string entidad, string registro, string accion, DateTime? fecha, Guid? usuarioId,
        Guid? clinicaId, Guid? hospitalId) => new()
        {
            Entidad = entidad,
            Registro = registro,
            Accion = accion,
            Fecha = fecha?.ToLocalTime(),
            UsuarioId = usuarioId,
            ClinicaId = clinicaId,
            HospitalId = hospitalId
        };
}
