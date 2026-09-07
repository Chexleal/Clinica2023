using ClinicaDomain;
using ClinicaInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public interface ICatalogoIndicacionService
{
    List<CatalogoIndicacion> GetAll();
    List<CatalogoIndicacion> GetByTipo(TipoEstudio tipo);
    CatalogoIndicacion? GetById(Guid id);
    CatalogoIndicacion Crear(CatalogoIndicacion item);
    void Actualizar(CatalogoIndicacion item);
    void Eliminar(Guid id);
}

public class CatalogoIndicacionService : ICatalogoIndicacionService
{
    private readonly ClinicaContext _db;
    public CatalogoIndicacionService(ClinicaContext db) => _db = db;

    public List<CatalogoIndicacion> GetAll()
    {
        try
        {
            var lista = _db.CatalogoIndicaciones.Where(c=>c.FechaEliminacion==null).OrderBy(c=>c.Tipo).ThenBy(c=>c.Descripcion).ToList();
            AuditoriaNombres.Completar(_db, lista);
            return lista;
        }
        catch { return new List<CatalogoIndicacion>(); }
    }

    public List<CatalogoIndicacion> GetByTipo(TipoEstudio tipo)
    {
        try
        {
            var lista = _db.CatalogoIndicaciones.Where(c=>c.Tipo==tipo && c.Activo && c.FechaEliminacion==null).OrderBy(c=>c.Descripcion).ToList();
            AuditoriaNombres.Completar(_db, lista);
            return lista;
        }
        catch { return new List<CatalogoIndicacion>(); }
    }

    public CatalogoIndicacion? GetById(Guid id)
    {
        try { return _db.CatalogoIndicaciones.FirstOrDefault(c=>c.IdCatalogo==id); }
        catch { return null; }
    }

    public CatalogoIndicacion Crear(CatalogoIndicacion item)
    {
        item.IdCatalogo = Guid.NewGuid();
        item.Codigo = item.Codigo.TextoCatalogo();
        item.Descripcion = item.Descripcion.TextoCatalogo();
        _db.CatalogoIndicaciones.Add(item);
        _db.SaveChanges();
        return item;
    }

    public void Actualizar(CatalogoIndicacion item)
    {
        var db = _db.CatalogoIndicaciones.FirstOrDefault(c=>c.IdCatalogo==item.IdCatalogo);
        if(db==null) return;
        db.Tipo = item.Tipo;
        db.Codigo = item.Codigo.TextoCatalogo();
        db.Descripcion = item.Descripcion.TextoCatalogo();
        db.Activo = item.Activo;
        _db.SaveChanges();
    }

    public void Eliminar(Guid id)
    {
        var db = _db.CatalogoIndicaciones.FirstOrDefault(c=>c.IdCatalogo==id);
        if(db==null) return;
        _db.CatalogoIndicaciones.Remove(db);
        _db.SaveChanges();
    }
}
