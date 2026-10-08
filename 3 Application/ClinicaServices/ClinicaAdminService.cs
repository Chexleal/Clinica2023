using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public interface IClinicaAdminService
{
    List<Hospital> GetHospitales(bool soloActivos = false);
    Hospital? GetHospital(Guid id);
    void GuardarHospital(Hospital hospital);

    List<Clinica> GetClinicas(bool soloActivas = false);
    List<Clinica> GetClinicasDeHospital(Guid idHospital, bool soloActivas = false);
    Clinica? GetClinica(Guid id);
    void GuardarClinica(Clinica clinica);
}

public class ClinicaAdminService(ClinicaContext dbContext) : IClinicaAdminService
{
    public List<Hospital> GetHospitales(bool soloActivos = false)
    {
        var q = dbContext.Hospitales.AsQueryable();
        if (soloActivos) q = q.Where(x => x.Activo);
        return q.OrderBy(x => x.Nombre).ToList();
    }

    public Hospital? GetHospital(Guid id) => dbContext.Hospitales.FirstOrDefault(x => x.IdHospital == id);

    public void GuardarHospital(Hospital hospital)
    {
        hospital.BeforeSaveChanges();
        var db = GetHospital(hospital.IdHospital);
        if (db is null)
        {
            if (hospital.IdHospital == Guid.Empty) hospital.IdHospital = Guid.NewGuid();
            dbContext.Hospitales.Add(hospital);
        }
        else
        {
            db.Nombre = hospital.Nombre;
            db.Activo = hospital.Activo;
            db.BeforeSaveChanges();
        }
        dbContext.SaveChanges();
    }

    public List<Clinica> GetClinicas(bool soloActivas = false)
    {
        var q = dbContext.Clinicas.Include(x => x.Hospital).AsQueryable();
        if (soloActivas) q = q.Where(x => x.Activa && x.Hospital!.Activo);
        return q.OrderBy(x => x.Hospital!.Nombre).ThenBy(x => x.Nombre).ToList();
    }

    public List<Clinica> GetClinicasDeHospital(Guid idHospital, bool soloActivas = false)
    {
        var q = dbContext.Clinicas.Where(x => x.IdHospital == idHospital);
        if (soloActivas) q = q.Where(x => x.Activa);
        return q.OrderBy(x => x.Nombre).ToList();
    }

    public Clinica? GetClinica(Guid id) =>
        dbContext.Clinicas.Include(x => x.Hospital).FirstOrDefault(x => x.IdClinica == id);

    public void GuardarClinica(Clinica clinica)
    {
        clinica.BeforeSaveChanges();
        var db = dbContext.Clinicas.FirstOrDefault(x => x.IdClinica == clinica.IdClinica);
        if (db is null)
        {
            if (clinica.IdClinica == Guid.Empty) clinica.IdClinica = Guid.NewGuid();
            dbContext.Clinicas.Add(clinica);
        }
        else
        {
            db.Nombre = clinica.Nombre;
            db.IdHospital = clinica.IdHospital;
            db.Activa = clinica.Activa;
            db.BeforeSaveChanges();
        }
        dbContext.SaveChanges();
    }
}
