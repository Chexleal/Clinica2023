using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public interface INotaMedicaService
{
    NotaMedica? GetByConsulta(Guid idConsulta);
    bool ExistePorConsulta(Guid idConsulta);
    List<NotaMedica> GetByPaciente(Guid idPaciente);
    NotaMedica? GetById(Guid id);
    NotaMedica? GetByIdDetallado(Guid id);
    NotaMedica Guardar(NotaMedica nota);
    void Eliminar(Guid id);
}

public class NotaMedicaService(ClinicaContext db) : INotaMedicaService
{

    public NotaMedica? GetByConsulta(Guid idConsulta)
    {
        try
        {
            return db.NotasMedicas
                .Where(n => n.IdConsulta == idConsulta && n.FechaEliminacion == null)
                .OrderByDescending(n => n.FechaNota)
                .FirstOrDefault();
        }
        catch { return null; }
    }

    public bool ExistePorConsulta(Guid idConsulta)
    {
        try { return db.NotasMedicas.Any(n => n.IdConsulta == idConsulta && n.FechaEliminacion == null); }
        catch { return false; }
    }

    public List<NotaMedica> GetByPaciente(Guid idPaciente)
    {
        try
        {
            return db.NotasMedicas
                .Where(n => n.IdPaciente == idPaciente && n.FechaEliminacion == null)
                .OrderByDescending(n => n.FechaNota)
                .ToList();
        }
        catch { return new List<NotaMedica>(); }
    }

    public NotaMedica? GetById(Guid id) =>
        db.NotasMedicas.FirstOrDefault(n => n.IdNotaMedica == id && n.FechaEliminacion == null);

    public NotaMedica? GetByIdDetallado(Guid id) =>
        db.NotasMedicas.Include(n => n.Paciente).Include(n => n.Consulta)
            .FirstOrDefault(n => n.IdNotaMedica == id && n.FechaEliminacion == null);

    public NotaMedica Guardar(NotaMedica nota)
    {
        nota.Motivo = (nota.Motivo ?? "").Trim();
        nota.Contenido = (nota.Contenido ?? "").Trim();

        if (nota.IdNotaMedica == Guid.Empty)
        {
            nota.IdNotaMedica = Guid.NewGuid();
            nota.FechaNota = DateTime.Now;
            db.NotasMedicas.Add(nota);
        }
        else
        {
            var actual = db.NotasMedicas.FirstOrDefault(n => n.IdNotaMedica == nota.IdNotaMedica);
            if (actual == null)
            {
                nota.FechaNota = DateTime.Now;
                db.NotasMedicas.Add(nota);
            }
            else
            {
                actual.Motivo = nota.Motivo;
                actual.Contenido = nota.Contenido;
                actual.IncluirMotivo = nota.IncluirMotivo;
                actual.IncluirDiagnostico = nota.IncluirDiagnostico;
                nota = actual;
            }
        }
        db.SaveChanges();
        return nota;
    }

    public void Eliminar(Guid id)
    {
        var n = db.NotasMedicas.FirstOrDefault(x => x.IdNotaMedica == id);
        if (n == null) return;
        n.FechaEliminacion = DateTime.UtcNow;
        db.SaveChanges();
    }
}
