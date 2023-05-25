using ClinicaDomain;
using ServiceStack;
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
        void Add(Cita cita);
        void Delete(Guid id);
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

    
        public void Delete(Guid id)
        {
            Cita cita = _dbContext.Cita.FirstOrDefault(p => p.IdCita == id);
            if (cita is not null)
            {
                _dbContext.Cita.Remove(cita);
                _dbContext.SaveChanges();
            }
        }
    }
}
