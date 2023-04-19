using ClinicaDomain;
using PaaS.Framework.Utils.Extensions;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicaServices
{
    public interface IConsultaServices
    {
        List<Consulta> GetAll();
        List<Consulta> SearchConsulta(string input);
        Consulta? GetConsulta(Guid id);
        void AddConsulta(Consulta consulta);
        void UpdateConsulta(Consulta consulta);
        void DeleteConsulta(Guid id);
    }
    public class ConsultaServices : IConsultaServices
    {
        private readonly ClinicaContext _dbContext;
        public ConsultaServices(ClinicaContext dbContext)
        {
            _dbContext = dbContext;
        }    

        public void AddConsulta(Consulta consulta)
        {
            consulta.IdConsulta = Guid.NewGuid();
            consulta.Terminada = false;
            consulta.BeforeSaveChanges();
            _dbContext.Consulta.Add(consulta);
            _dbContext.SaveChanges();
        }

        public Consulta? GetConsulta(Guid id)
        {
            //return _dbContext.Usuarios.Find(id);
            return _dbContext.Consulta.FirstOrDefault(p => p.IdConsulta == id);
        }

        public List<Consulta> GetAll()
        {
            return _dbContext.Consulta.ToList();
        }

        public void DeleteConsulta(Guid id)
        {
            var consulta = GetConsulta(id);
            if (consulta is not null)
            {
                consulta.Terminada = true;
                _dbContext.SaveChanges();
            }
        }

        public List<Consulta> SearchConsulta(string input)
        {

            if (input.IsNullOrEmpty()) return null;
            else
            {
                //DE ESTA FORMA NO SE NECESITA IR POR TODOS LOS USUARIOS, SINO SE FILTRAN DIRECTOS EN DB, LO CUAL LO HACE MAS RÁPIDO
                List<Consulta> result = _dbContext.Consulta.Where(x =>
                x.IdConsulta.ToString().Contains(input) ||
                x.Diagnostico.Contains(input) ||
                x.Fecha.ToString().Contains(input) ||
                x.MotivoConsulta.Contains(input) ||
                x.Observaciones.Contains(input) ||
                x.Peso.Contains(input) ||
                x.Pagada.ToString().Contains(input) ||
                x.PresionArterial.ToString().Contains(input) ||
                x.Radiografias.ToString().Contains(input) ||
                x.Temperatura.Contains(input) ||
                x.Terminada.ToString().Contains(input) ||
                x.TiempoDuracion.ToString().Contains(input) ||
                x.Total.ToString().Contains(input)).ToList();
                return result;
            }

            
        }

        public void UpdateConsulta(Consulta consulta)
        {
            var consultaDb = GetConsulta(consulta.IdConsulta);
            if (consultaDb is not null)
            {
                consultaDb.Diagnostico = consulta.Diagnostico;
                consultaDb.Fecha = consulta.Fecha;
                consultaDb.MotivoConsulta = consulta.MotivoConsulta;
                consultaDb.Observaciones = consulta.Observaciones;
                consultaDb.Pagada = consulta.Pagada;
                consultaDb.Peso = consulta.Peso;
                consultaDb.PresionArterial = consulta.PresionArterial;
                consultaDb.Radiografias = consulta.Radiografias;
                consultaDb.Temperatura = consulta.Temperatura;
                consultaDb.Terminada = consulta.Terminada;
                consultaDb.TiempoDuracion = consulta.TiempoDuracion;
                consultaDb.Total = consulta.Total;
                _dbContext.SaveChanges();
            }
        }
    }
}

