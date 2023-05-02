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
            return _dbContext.MotivoCobros.ToList();
        }

        public void DeleteServicio(Guid id)
        {
            var servicio = GetServicio(id);
            if (servicio is not null)
            {
                _dbContext.MotivoCobros.Remove(servicio);
                _dbContext.SaveChanges();
            }
        }
    }
}
