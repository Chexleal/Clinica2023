using ClinicaDomain;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ClinicaServices
{
    public interface ICitaServices
    {
        List<Cita> GetAll();
        List<Cita> GetAllForToday();
        void Add(Cita cita);
        void Delete(Guid id);
        DateTime GetNextCita(DateTime fecha, Guid idPaciente);
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

        public List<Cita> GetAllForToday()
        {
            return _dbContext.Cita.Where(x => x.FechaHora.Year == DateTime.Today.Year & x.FechaHora.Month == DateTime.Today.Month & x.FechaHora.Day == DateTime.Today.Day).ToList();
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

        public DateTime GetNextCita(DateTime fecha, Guid idPaciente)
        {
            Cita cita = _dbContext.Cita.FirstOrDefault(p => p.IdPaciente == idPaciente && p.FechaHora>= fecha);

            return cita is null ? new DateTime() : cita.FechaHora;
        }
    }
}
