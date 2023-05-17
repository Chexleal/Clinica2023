using ClinicaDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaServices;

public interface IRecetaServices
{
    Receta Get(Guid id);
    Receta GetByConsulta(Guid id);
    void Update(Receta receta);
    void Create(Receta receta);
}
public class RecetaServices : IRecetaServices
{
    private readonly ClinicaContext _dbContext;
    public RecetaServices(ClinicaContext dbContext)
    {
        _dbContext = dbContext;
       
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
}

