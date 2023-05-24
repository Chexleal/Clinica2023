using ClinicaDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaServices
{
    public interface ICitaServices
    {
        List<Cita> GetAll();
        Cita Get(Guid id);
        Cita GetByConsulta(Guid id);
        void Update(Receta receta);
        void Add(Cita cita);
    }
    public class CitaServices : ICitaServices
    {
        private readonly ClinicaContext _dbContext;
        public CitaServices(ClinicaContext dbContext)
        {
            _dbContext = dbContext;

        }

        public void Add(Cita cita)
        {
            cita.IdCita = Guid.NewGuid();
            _dbContext.Cita.Add(cita);
            _dbContext.SaveChanges();
        }

        public List<Cita> GetAll()
        {
            return _dbContext.Cita.ToList();
        }

        public void Update(Receta receta)
        {
            throw new NotImplementedException();
        }

        public Cita Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Cita GetByConsulta(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
