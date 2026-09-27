using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public interface IEstudioImagenService
{
    List<EstudioImagen> GetByPaciente(Guid idPaciente);
    List<EstudioImagen> GetByConsulta(Guid idConsulta);
    EstudioImagen? GetById(Guid idEstudio);
    ArchivoEstudio? GetArchivo(Guid idArchivo);
    List<EstudioImagen> GetByOrden(Guid idOrden);
    Task<EstudioImagen> CrearEstudioAsync(EstudioImagen estudio, List<Microsoft.AspNetCore.Http.IFormFile> files, Storage.IStorageService storage);
    Task<int> AgregarArchivosAsync(Guid idEstudio, List<Microsoft.AspNetCore.Http.IFormFile> files, Storage.IStorageService storage);
    Task DeleteArchivoAsync(Guid idArchivo, Storage.IStorageService storage);
    Task DeleteEstudioAsync(Guid idEstudio, Storage.IStorageService storage);
}

public class EstudioImagenService(ClinicaContext db) : IEstudioImagenService
{

    public List<EstudioImagen> GetByPaciente(Guid idPaciente)
    {
        try
        {
            var lista = db.EstudiosImagen.Include(e => e.Archivos).Include(e => e.Orden)
               .Where(e => e.IdPaciente == idPaciente && e.FechaEliminacion == null)
               .OrderByDescending(e => e.FechaEstudio).ToList();
            CompletarNombres(lista);
            return lista;
        }
        catch { return new List<EstudioImagen>(); } // tabla aún no migrada -> vacío
    }

    public List<EstudioImagen> GetByConsulta(Guid idConsulta)
    {
        try
        {
            var lista = db.EstudiosImagen.Include(e => e.Archivos).Include(e => e.Orden)
               .Where(e => e.IdConsulta == idConsulta && e.FechaEliminacion == null)
               .OrderByDescending(e => e.FechaEstudio).ToList();
            CompletarNombres(lista);
            return lista;
        }
        catch { return new List<EstudioImagen>(); }
    }

    private void CompletarNombres(List<EstudioImagen> lista)
    {
        AuditoriaNombres.Completar(db, lista);
    }

    public EstudioImagen? GetById(Guid id) =>
        db.EstudiosImagen.Include(e => e.Archivos).FirstOrDefault(e => e.IdEstudio == id);

    public List<EstudioImagen> GetByOrden(Guid idOrden)
    {
        try
        {
            var lista = db.EstudiosImagen.Include(e => e.Archivos).Include(e => e.Orden)
               .Where(e => e.IdOrden == idOrden && e.FechaEliminacion == null)
               .OrderByDescending(e => e.FechaEstudio).ToList();
            CompletarNombres(lista);
            return lista;
        }
        catch { return new List<EstudioImagen>(); }
    }

    public ArchivoEstudio? GetArchivo(Guid id) => db.ArchivosEstudio.FirstOrDefault(a => a.IdArchivo == id);

    public async Task<EstudioImagen> CrearEstudioAsync(EstudioImagen estudio, List<Microsoft.AspNetCore.Http.IFormFile> files, Storage.IStorageService storage)
    {
        estudio.IdEstudio = Guid.NewGuid();
        estudio.FechaEstudio = estudio.FechaEstudio == default ? DateTime.Now : estudio.FechaEstudio;
        db.EstudiosImagen.Add(estudio);
        await db.SaveChangesAsync();

        foreach (var file in files)
        {
            var safeName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var relPath = $"estudios/{estudio.IdPaciente}/{estudio.IdEstudio}/{safeName}";
            await storage.SaveAsync(file, relPath);

            var archivo = new ArchivoEstudio
            {
                IdArchivo = Guid.NewGuid(),
                IdEstudio = estudio.IdEstudio,
                NombreOriginal = file.FileName,
                RutaStorage = relPath,
                MimeType = storage.GetContentType(file.FileName),
                TamanoBytes = file.Length
            };
            db.ArchivosEstudio.Add(archivo);
        }
        await db.SaveChangesAsync();
        return estudio;
    }

    public async Task<int> AgregarArchivosAsync(Guid idEstudio, List<Microsoft.AspNetCore.Http.IFormFile> files, Storage.IStorageService storage)
    {
        var estudio = db.EstudiosImagen.FirstOrDefault(e => e.IdEstudio == idEstudio);
        if (estudio == null) return 0;

        foreach (var file in files)
        {
            var safeName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var relPath = $"estudios/{estudio.IdPaciente}/{estudio.IdEstudio}/{safeName}";
            await storage.SaveAsync(file, relPath);

            db.ArchivosEstudio.Add(new ArchivoEstudio
            {
                IdArchivo = Guid.NewGuid(),
                IdEstudio = estudio.IdEstudio,
                NombreOriginal = file.FileName,
                RutaStorage = relPath,
                MimeType = storage.GetContentType(file.FileName),
                TamanoBytes = file.Length
            });
        }
        await db.SaveChangesAsync();
        return files.Count;
    }

    public async Task DeleteArchivoAsync(Guid idArchivo, Storage.IStorageService storage)
    {
        var arch = db.ArchivosEstudio.FirstOrDefault(a => a.IdArchivo == idArchivo);
        if (arch == null) return;
        try { await storage.DeleteAsync(arch.RutaStorage); } catch { }
        db.ArchivosEstudio.Remove(arch);
        await db.SaveChangesAsync();
        // Si era último archivo del estudio, opcionalmente borrar estudio vacío
        var count = db.ArchivosEstudio.Count(a => a.IdEstudio == arch.IdEstudio);
        if (count == 0)
        {
            var est = db.EstudiosImagen.FirstOrDefault(e => e.IdEstudio == arch.IdEstudio);
            if (est != null) { db.EstudiosImagen.Remove(est); await db.SaveChangesAsync(); }
        }
    }

    public async Task DeleteEstudioAsync(Guid idEstudio, Storage.IStorageService storage)
    {
        var est = db.EstudiosImagen.Include(e => e.Archivos).FirstOrDefault(e => e.IdEstudio == idEstudio);
        if (est == null) return;
        foreach (var a in est.Archivos) { try { await storage.DeleteAsync(a.RutaStorage); } catch { } }
        db.ArchivosEstudio.RemoveRange(est.Archivos);
        db.EstudiosImagen.Remove(est);
        await db.SaveChangesAsync();
    }
}
