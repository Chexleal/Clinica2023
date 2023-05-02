using ClinicaDomain;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaServices
{
    public interface IDetallesServices
    {
        DetalleCobro GetDetalle(Guid id);
        List<DetalleCobro> GetAll();
        void AddDetalle(DetalleCobro detalle);
        void DeleteDetalle(Guid id);
    }
    public class DetalleServices : IDetallesServices
    {
        private readonly ClinicaContext _dbContext;
        public DetalleServices(ClinicaContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DetalleCobro GetDetalle(Guid id)
        {
            //return _dbContext.Usuarios.Find(id);
            return _dbContext.DetalleCobros.FirstOrDefault(p => p.IdDetalleCobro == id);
        }

        public List<DetalleCobro> GetDetallesByConsulta(Guid consultaId)
        {
            return _dbContext.DetalleCobros.Where(x => x.IdConsulta.Equals(consultaId)).ToList();
        }

        public List<DetalleCobro> GetAll()
        {
            return _dbContext.DetalleCobros.ToList();
        }

        public void DeleteDetalle(Guid id)
        {
            var detalle = GetDetalle(id);
            if (detalle is not null)
            {
                _dbContext.DetalleCobros.Remove(detalle);
                _dbContext.SaveChanges();
            }
        }

        public void AddDetalle(DetalleCobro detalle)
        {
            detalle.IdDetalleCobro = Guid.NewGuid();
            _dbContext.DetalleCobros.Add(detalle);
            _dbContext.SaveChanges();
        }
    }
}

