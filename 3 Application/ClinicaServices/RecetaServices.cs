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
        return _dbContext.Medicamento.ToList();
    }

    public Guid DeleteDetalle(Guid idDetalleReceta)
    {
        var detalle= _dbContext.DetalleReceta.FirstOrDefault(x => x.IdDetalleReceta == idDetalleReceta);
        _dbContext.DetalleReceta.Remove(detalle);
        _dbContext.SaveChanges();

        return detalle.IdReceta;
    }
}

