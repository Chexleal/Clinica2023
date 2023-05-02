﻿using ClinicaDomain;
using Microsoft.EntityFrameworkCore;


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
        _dbContext.SaveChanges();
    }

    public void Create(Receta receta)
    {
            receta.IdReceta = Guid.NewGuid();
            receta.Fecha = DateTime.Now;
            receta.Descripcion ??= string.Empty;
            var consulta = _dbContext.Consulta.FirstOrDefault(x => x.IdConsulta == receta.IdConsulta);
             consulta.Receta.Add(receta);
            _dbContext.SaveChanges();
    }
}
