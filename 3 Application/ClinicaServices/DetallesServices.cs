using ClinicaDomain;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ClinicaServices
{
    public interface IDetallesServices
    {
        DetalleCobro GetDetalle(Guid id);
        List<DetalleCobro> GetAll();
        List<DetalleCobro> GetDetallesByConsulta(Guid consultaId);
        void AddDetalle(DetalleCobro detalle);
        void Delete(Guid id);
        void Pagar(Guid IdConsulta);
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

        public void AddDetalle(DetalleCobro detalle)
        {
            detalle.IdDetalleCobro = Guid.NewGuid();
            detalle.Subtotal = detalle.Cantidad * detalle.Valor;
            Consulta consulta = _dbContext.Consulta.FirstOrDefault(p => p.IdConsulta == detalle.IdConsulta);
            consulta.Total += detalle.Subtotal;

            //var consulta = _dbContext.Consulta.FirstOrDefault(x => x.IdConsulta == detalle.IdConsulta);
            _dbContext.DetalleCobros.Add(detalle);
            _dbContext.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var detalle = GetDetalle(id);
            if (detalle is not null)
            {
                var consulta = _dbContext.Consulta.FirstOrDefault(p => p.IdConsulta == detalle.IdConsulta);
                consulta.Total -= detalle.Subtotal;
                _dbContext.DetalleCobros.Remove(detalle);
                _dbContext.SaveChanges();
            }
        }

        public void Pagar(Guid idConsulta)
        {
            var consulta = _dbContext.Consulta.FirstOrDefault(p => p.IdConsulta == idConsulta);
            consulta.Pagada = true;
            _dbContext.SaveChanges();
        }
    }
}

