using ClinicaDomain;
using ClinicaInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaServices
{
    public interface IServiciosServices
    {
        MotivoCobro? GetServicio(Guid id);
        public List<MotivoCobro>? GetAll();
        public void DeleteServicio(Guid id);

        public void AddServicio(MotivoCobro servicio);

        public void UpdateServicio(Guid id, string descripcion, decimal precioSugerido);

    }
    public class ServiciosServices(ClinicaContext dbContext) : IServiciosServices
    {

        public MotivoCobro? GetServicio(Guid id)
        {
            //return dbContext.Usuarios.Find(id);
            return dbContext.MotivoCobros.FirstOrDefault(p => p.IdMotivoCobro == id);
        }

        public List<MotivoCobro>? GetAll()
        {
            var lista = dbContext.MotivoCobros.Where(x => !x.EstadoEliminado).ToList();
            AuditoriaNombres.Completar(dbContext, lista);
            return lista;
        }

        public void AddServicio(MotivoCobro servicio)
        {
            //var consultaExistente = dbContext.Consulta.FirstOrDefault(X => X.IdConsulta == consulta.IdConsulta && !X.Terminada);
            //if consultaExistente.

            //var servicioExistente = dbContext.MotivoCobros.FirstOrDefault(x=>x.Descripcion.Trim().ToLower().Replace(" ", "") == "");

                servicio.IdMotivoCobro = Guid.NewGuid();
                servicio.EstadoEliminado = false;
                servicio.Descripcion = servicio.Descripcion.TextoCatalogo();
                servicio.BeforeSaveChanges();
                dbContext.MotivoCobros.Add(servicio);
                dbContext.SaveChanges();

            
        }
        public void DeleteServicio(Guid id)
        {
            var servicio = GetServicio(id);
            if (servicio is not null)
            {
                servicio.EstadoEliminado = true;
                dbContext.SaveChanges();
            }
        }
        public void UpdateServicio(Guid id, string descripcion, decimal precioSugerido)
        {
            var servicio = GetServicio(id);
            if (servicio is null) return;
            if (string.IsNullOrWhiteSpace(descripcion)) throw new ArgumentException("Descripción requerida.");
            servicio.Descripcion = descripcion.TextoCatalogo();
            servicio.PrecioSugerido = precioSugerido < 0 ? 0 : precioSugerido;
            servicio.BeforeSaveChanges();
            dbContext.SaveChanges();
        }
    }
}
