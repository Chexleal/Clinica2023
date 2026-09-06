using ClinicaDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ClinicaServices;

public interface IRecetaServices
{
    void AddDetalleReceta(DetalleReceta detalleReceta);
    Receta Get(Guid id);
    Receta GetByConsulta(Guid id);
    void Update(Receta receta);
    void Create(Receta receta);
    List<DetalleReceta> GetAllDetalles(Guid id);

    List<Medicamento> GetAllMedicamentos ();

    Medicamento CrearMedicamento(string nombre);
    void EliminarMedicamento(Guid id);

    Guid DeleteDetalle(Guid idDetalleReceta);
}
public class RecetaServices : IRecetaServices
{
    private readonly ClinicaContext _dbContext;
    public RecetaServices(ClinicaContext dbContext)
    {
        _dbContext = dbContext;
       
    }

    public void AddDetalleReceta(DetalleReceta detalleReceta)
    {
        detalleReceta.IdDetalleReceta=Guid.NewGuid();
        detalleReceta.BeforeSaveChanges();
        _dbContext.DetalleReceta.Add(detalleReceta);
        _dbContext.SaveChanges();

        InsertarMedicamento(detalleReceta.Medicamento);
    }

    private void InsertarMedicamento(string medicamento)
    {
        var existingMed = _dbContext.Medicamento.FirstOrDefault(x => x.Nombre == medicamento);
        if (existingMed is null)
        {
            _dbContext.Medicamento.Add(new Medicamento { IdMedicamento = Guid.NewGuid(), Nombre = medicamento });
            _dbContext.SaveChanges();
        }
    }

    public Receta Get(Guid id)
    {
        return _dbContext.Receta.FirstOrDefault(p => p.IdReceta == id);
    }

    public Receta GetByConsulta(Guid id)
    {
        return _dbContext.Receta.FirstOrDefault(p => p.IdConsulta == id);
    }

    public void Update(Receta receta)
    {
        receta.Descripcion ??= string.Empty;
        _dbContext.SaveChanges();
    }

    public void recetaConverter()
    {
        var builder = WebApplication.CreateBuilder();
       // builder.Services.AddSingleton(typeof(Converter(), new Synchrini));
    }

    public void Create(Receta receta)
    {
            receta.IdReceta = Guid.NewGuid();
            receta.Fecha = DateTime.Now;
            receta.Descripcion ??= string.Empty;
           _dbContext.Receta.Add(receta);
            _dbContext.SaveChanges();
    }

    public List<DetalleReceta> GetAllDetalles(Guid id)
    {
        return _dbContext.DetalleReceta.Where(x => x.IdReceta == id).ToList();
    }

    public List<Medicamento> GetAllMedicamentos()
    {
        var lista = _dbContext.Medicamento.OrderBy(x => x.Nombre).ToList();
        AuditoriaNombres.Completar(_dbContext, lista);
        return lista;
    }

    public Medicamento CrearMedicamento(string nombre)
    {
        var limpio = (nombre ?? "").Trim();
        var existente = _dbContext.Medicamento.FirstOrDefault(x => x.Nombre.ToLower() == limpio.ToLower());
        if (existente is not null) return existente;
        var nuevo = new Medicamento { IdMedicamento = Guid.NewGuid(), Nombre = limpio };
        _dbContext.Medicamento.Add(nuevo);
        _dbContext.SaveChanges();
        return nuevo;
    }

    public void EliminarMedicamento(Guid id)
    {
        var med = _dbContext.Medicamento.FirstOrDefault(x => x.IdMedicamento == id);
        if (med is null) return;
        _dbContext.Medicamento.Remove(med);
        _dbContext.SaveChanges();
    }

    public Guid DeleteDetalle(Guid idDetalleReceta)
    {
        var detalle= _dbContext.DetalleReceta.FirstOrDefault(x => x.IdDetalleReceta == idDetalleReceta);
        _dbContext.DetalleReceta.Remove(detalle);
        _dbContext.SaveChanges();

        return detalle.IdReceta;
    }
}

