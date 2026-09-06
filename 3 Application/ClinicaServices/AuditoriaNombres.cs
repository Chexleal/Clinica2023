using ClinicaDomain;

namespace ClinicaServices;

/// <summary>
/// Resuelve los Guid de auditoría (CreadoPor/ModificadoPor) a nombres de usuario en memoria.
/// No desnormaliza la BD: 1 sola query a Usuarios por lote, sin N+1.
/// </summary>
public static class AuditoriaNombres
{
    public static void Completar(ClinicaContext db, IEnumerable<Base> items)
    {
        try
        {
            var lista = items?.ToList() ?? new List<Base>();
            if (!lista.Any()) return;
            var ids = lista.SelectMany(e => new[] { e.CreadoPor, e.ModificadoPor })
                .Where(g => g.HasValue).Select(g => g!.Value).Distinct().ToList();
            if (!ids.Any()) return;
            var nombres = db.Usuarios
                .Where(u => ids.Contains(u.IdUsuario))
                .ToDictionary(u => u.IdUsuario, u => (u.Nombre + " " + u.Apellido).Trim());
            foreach (var e in lista)
            {
                if (e.CreadoPor.HasValue && nombres.TryGetValue(e.CreadoPor.Value, out var n1)) e.CreadoPorNombre = n1;
                if (e.ModificadoPor.HasValue && nombres.TryGetValue(e.ModificadoPor.Value, out var n2)) e.ModificadoPorNombre = n2;
            }
        }
        catch { }
    }
}
