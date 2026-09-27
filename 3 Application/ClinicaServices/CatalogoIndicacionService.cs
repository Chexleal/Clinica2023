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

public class CatalogoIndicacionService(ClinicaContext db) : ICatalogoIndicacionService
{

    public List<CatalogoIndicacion> GetAll()
    {
        try
        {
            var lista = db.CatalogoIndicaciones.Where(c=>c.FechaEliminacion==null).OrderBy(c=>c.Tipo).ThenBy(c=>c.Descripcion).ToList();
            AuditoriaNombres.Completar(db, lista);
            return lista;
        }
        catch { return new List<CatalogoIndicacion>(); }
    }

    public List<CatalogoIndicacion> GetByTipo(TipoEstudio tipo)
    {
        try
        {
            var lista = db.CatalogoIndicaciones.Where(c=>c.Tipo==tipo && c.Activo && c.FechaEliminacion==null).OrderBy(c=>c.Descripcion).ToList();
            AuditoriaNombres.Completar(db, lista);
            return lista;
        }
        catch { return new List<CatalogoIndicacion>(); }
    }

    public CatalogoIndicacion? GetById(Guid id)
    {
        try { return db.CatalogoIndicaciones.FirstOrDefault(c=>c.IdCatalogo==id); }
        catch { return null; }
    }

    public CatalogoIndicacion Crear(CatalogoIndicacion item)
    {
        item.IdCatalogo = Guid.NewGuid();
        item.Codigo = item.Codigo.TextoCatalogo();
        item.Descripcion = item.Descripcion.TextoCatalogo();
        db.CatalogoIndicaciones.Add(item);
        db.SaveChanges();
        return item;
    }

    public void Actualizar(CatalogoIndicacion item)
    {
        var actual = db.CatalogoIndicaciones.FirstOrDefault(c=>c.IdCatalogo==item.IdCatalogo);
        if(actual==null) return;
        actual.Tipo = item.Tipo;
        actual.Codigo = item.Codigo.TextoCatalogo();
        actual.Descripcion = item.Descripcion.TextoCatalogo();
        actual.Activo = item.Activo;
        db.SaveChanges();
    }

    public void Eliminar(Guid id)
    {
        var actual = db.CatalogoIndicaciones.FirstOrDefault(c=>c.IdCatalogo==id);
        if(actual==null) return;
        db.CatalogoIndicaciones.Remove(actual);
        db.SaveChanges();
    }
}
