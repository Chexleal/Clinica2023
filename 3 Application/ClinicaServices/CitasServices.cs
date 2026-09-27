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
        int CountForToday();
        void Add(Cita cita);
        void Delete(Guid id);
        DateTime GetNextCita(DateTime fecha, Guid idPaciente);
    }
    public class CitaServices(ClinicaContext dbContext) : ICitaServices
    {

        public void Add(Cita cita)
        {
            cita.IdCita = Guid.NewGuid();
            dbContext.Cita.Add(cita);
            dbContext.SaveChanges();
        }

        public List<Cita> GetAll()
        {
            return dbContext.Cita.ToList();
        }

        public List<Cita> GetAllForToday()
        {
            return dbContext.Cita.Where(x => x.FechaHora.Year == DateTime.Today.Year && x.FechaHora.Month == DateTime.Today.Month && x.FechaHora.Day == DateTime.Today.Day).ToList();
        }

        public int CountForToday()
        {
            return dbContext.Cita.Count(x => x.FechaHora.Year == DateTime.Today.Year && x.FechaHora.Month == DateTime.Today.Month && x.FechaHora.Day == DateTime.Today.Day);
        }


        public void Delete(Guid id)
        {
            Cita cita = dbContext.Cita.FirstOrDefault(p => p.IdCita == id);
            if (cita is not null)
            {
                dbContext.Cita.Remove(cita);
                dbContext.SaveChanges();
            }
        }

        public DateTime GetNextCita(DateTime fecha, Guid idPaciente)
        {
            Cita cita = dbContext.Cita.FirstOrDefault(p => p.IdPaciente == idPaciente && p.FechaHora>= fecha);

            return cita is null ? new DateTime() : cita.FechaHora;
        }
    }
}
