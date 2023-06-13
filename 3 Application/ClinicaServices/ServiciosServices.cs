using ClinicaDomain;
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

    }
    public class ServiciosServices : IServiciosServices
    {
        private readonly ClinicaContext _dbContext;
        public ServiciosServices(ClinicaContext dbContext)
        {
            _dbContext = dbContext;
        }

        public MotivoCobro? GetServicio(Guid id)
        {
            //return _dbContext.Usuarios.Find(id);
            return _dbContext.MotivoCobros.FirstOrDefault(p => p.IdMotivoCobro == id);
        }

        public List<MotivoCobro>? GetAll()
        {
            return _dbContext.MotivoCobros.Where(x => !x.EstadoEliminado).ToList();
        }

        public void AddServicio(MotivoCobro servicio)
        {
            //var consultaExistente = _dbContext.Consulta.FirstOrDefault(X => X.IdConsulta == consulta.IdConsulta && !X.Terminada);
            //if consultaExistente.

            //var servicioExistente = _dbContext.MotivoCobros.FirstOrDefault(x=>x.Descripcion.Trim().ToLower().Replace(" ", "") == "");

                servicio.IdMotivoCobro = Guid.NewGuid();
                servicio.EstadoEliminado = false;
                servicio.BeforeSaveChanges();
                _dbContext.MotivoCobros.Add(servicio);
                _dbContext.SaveChanges();

            
        }
        public void DeleteServicio(Guid id)
        {
            var servicio = GetServicio(id);
            if (servicio is not null)
            {
                servicio.EstadoEliminado = true;
                _dbContext.SaveChanges();
            }
        }
    }
}
