using ClinicaDomain;
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
    }
    public class ConsultaServices : IConsultaServices
    {
        private readonly ClinicaContext _dbContext;
        public ConsultaServices(ClinicaContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Consulta> GetAll()
        {
            return _dbContext.Consulta.ToList();
        }
    }
}
