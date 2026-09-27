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
        List<DetalleCobro> GetDetallesByConsulta(Guid consultaId);
        List<DetalleCobro> GetByRange(DateTime from, DateTime to);
        void AddDetalle(DetalleCobro detalle);
        void Delete(Guid id);
        void Pagar(Guid IdConsulta);
    }
    public class DetalleServices(ClinicaContext dbContext) : IDetallesServices
    {

        public DetalleCobro GetDetalle(Guid id)
        {
            //return dbContext.Usuarios.Find(id);
            return dbContext.DetalleCobros.FirstOrDefault(p => p.IdDetalleCobro == id);
        }

        public List<DetalleCobro> GetDetallesByConsulta(Guid consultaId)
        {
            return dbContext.DetalleCobros.Where(x => x.IdConsulta.Equals(consultaId)).ToList();
        }

        public List<DetalleCobro> GetByRange(DateTime from, DateTime to)
        {
            var fromDate = from.Date;
            var toDateExclusive = to.Date.AddDays(1);

            return dbContext.DetalleCobros
                .Where(d => dbContext.Consulta.Any(c => c.IdConsulta == d.IdConsulta
                    && c.Fecha >= fromDate
                    && c.Fecha < toDateExclusive
                    && !c.Eliminada))
                .ToList();
        }

        public void AddDetalle(DetalleCobro detalle)
        {
            detalle.IdDetalleCobro = Guid.NewGuid();
            detalle.Subtotal = detalle.Cantidad * detalle.Valor;
            detalle.NombreServicio = dbContext.MotivoCobros.FirstOrDefault(x => x.IdMotivoCobro == detalle.IdMotivoCobro).Descripcion;
            Consulta consulta = dbContext.Consulta.FirstOrDefault(p => p.IdConsulta == detalle.IdConsulta);
            consulta.Total += detalle.Subtotal;
            
            //var consulta = dbContext.Consulta.FirstOrDefault(x => x.IdConsulta == detalle.IdConsulta);
            dbContext.DetalleCobros.Add(detalle);
            dbContext.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var detalle = GetDetalle(id);
            if (detalle is not null)
            {
                var consulta = dbContext.Consulta.FirstOrDefault(p => p.IdConsulta == detalle.IdConsulta);
                consulta.Total -= detalle.Subtotal;
                dbContext.DetalleCobros.Remove(detalle);
                dbContext.SaveChanges();
            }
        }

        public void Pagar(Guid idConsulta)
        {
            var consulta = dbContext.Consulta.FirstOrDefault(p => p.IdConsulta == idConsulta);
            consulta.Pagada = true;
            dbContext.SaveChanges();
        }
    }
}

