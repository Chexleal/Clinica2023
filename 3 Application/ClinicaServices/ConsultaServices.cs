﻿using ClinicaDomain;
using iText.Html2pdf;
using Microsoft.EntityFrameworkCore;

//using Microsoft.EntityFrameworkCore;
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
        Consulta GetConsulta(Guid id);
        List<Consulta> GetAll();
        List<Consulta> GetAllByPacienteId(Guid pacienteId);
        List<Consulta> GetAllByPacienteId(Guid pacienteId, DateTime from, DateTime to);
        List<Consulta> GetAllByMonth(int month);
        List<Consulta> GetAllPaidByMonth(int month);
        List<Consulta> GetAllOpen();
        List<Consulta> GetAllNotPaid();
        //List<Consulta> SearchConsulta(string input);
        void AddConsulta(Consulta consulta);
        void UpdateConsulta(Consulta consulta);
        void DeleteConsulta(Guid id);
        void createPdf(string inHtmlPath, string toPdfPath);

    }
    public class ConsultaServices : IConsultaServices
    {
        private readonly ClinicaContext _dbContext;
        private readonly IRecetaServices _recetaServices;
        public ConsultaServices(ClinicaContext dbContext, IRecetaServices recetaServices)
        {
            _dbContext = dbContext;
            _recetaServices = recetaServices;
        }    

        public Consulta GetConsulta(Guid id)
        {
            //return _dbContext.Usuarios.Find(id);
            return _dbContext.Consulta.FirstOrDefault(p => p.IdConsulta == id);
        }

        public List<Consulta> GetAll()
        {
            return _dbContext.Consulta.Where(x => x.Eliminada == false).ToList();
        }
        public List<Consulta> GetAllByPacienteId(Guid pacienteId)
        {
            return _dbContext.Consulta.Where(x=>x.IdPaciente== pacienteId && x.Eliminada == false).ToList();
        }

        public List<Consulta> GetAllByPacienteId(Guid pacienteId, DateTime from, DateTime to)
        {
            return _dbContext.Consulta.Where(x => x.IdPaciente == pacienteId && x.Fecha>=from && x.Fecha<=to && x.Eliminada == false).ToList();
        }

        public List<Consulta> GetAllByMonth(int month)
        {
            return _dbContext.Consulta.Where(x => x.Fecha.Month.Equals(month) && x.Fecha.Year.Equals(DateTime.Today.Year) && x.Eliminada == false).ToList();
        }

        public List<Consulta> GetAllPaidByMonth(int month)
        {
            return _dbContext.Consulta.Where(x => x.Fecha.Month.Equals(month) && x.Fecha.Year.Equals(DateTime.Today.Year) && x.Pagada && x.Eliminada == false).ToList();
        }

        public List<Consulta> GetAllOpen()
        {
            return _dbContext.Consulta.Where(x => !x.Terminada && x.Eliminada == false).ToList();
        }

        public List<Consulta> GetAllNotPaid()
        {
            return _dbContext.Consulta.Where(x => x.Terminada && !x.Pagada && x.Eliminada == false).ToList();
        }

        public void DeleteConsulta(Guid id)
        {
            var consulta = GetConsulta(id);
            if (consulta is not null)
            {
                try
                {
                    consulta.Terminada = true;
                    _dbContext.Consulta.Remove(consulta);
                    _dbContext.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("The DELETE statement conflicted with the REFERENCE constraint"))
                    {
                        // Desmarcar el estado Deleted para evitar que vuelva a intentar borrar
                        _dbContext.Entry(consulta).State = EntityState.Modified;

                        consulta.Eliminada = true;
                        UpdateConsulta(consulta);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        //public List<Consulta> SearchConsulta(string input)
        //{

        //    if (input.IsNullOrEmpty()) return null;
        //    else
        //    {
        //        //DE ESTA FORMA NO SE NECESITA IR POR TODOS LOS USUARIOS, SINO SE FILTRAN DIRECTOS EN DB, LO CUAL LO HACE MAS RÁPIDO
        //        List<Consulta> result = _dbContext.Consulta.Where(x =>
        //        x.IdConsulta.ToString().Contains(input) ||
        //        x.Diagnostico.Contains(input) ||
        //        x.Fecha.ToString().Contains(input) ||
        //        x.MotivoConsulta.Contains(input) ||
        //        x.Observaciones.Contains(input) ||
        //        x.Peso.Contains(input) ||
        //        x.Pagada.ToString().Contains(input) ||
        //        x.PresionArterial.ToString().Contains(input) ||
        //        x.Radiografias.ToString().Contains(input) ||
        //        x.Temperatura.Contains(input) ||
        //        x.Terminada.ToString().Contains(input) ||
        //        x.TiempoDuracion.ToString().Contains(input) ||
        //        x.Total.ToString().Contains(input)).ToList();
        //        return result;
        //    }

            
        //}

        public void UpdateConsulta(Consulta consulta)
        {
          consulta.BeforeSaveChanges();
         _dbContext.SaveChanges();
        }

        public void AddConsulta(Consulta consulta)
        {
            //var consultaExistente = _dbContext.Consulta.FirstOrDefault(X => X.IdConsulta == consulta.IdConsulta && !X.Terminada);
            //if consultaExistente.

            consulta.IdConsulta = Guid.NewGuid();
            consulta.Fecha = DateTime.Now;
            consulta.Terminada = false;
            consulta.Eliminada = false;
            consulta.Pagada = false;
            consulta.BeforeSaveChanges();
            _dbContext.Consulta.Add(consulta);
            _dbContext.SaveChanges();

            _recetaServices.Create(new Receta { IdConsulta = consulta.IdConsulta });
        }

        public void createPdf(string inHtmlPath, string toPdfPath)
        {
            string htmlDocument = File.ReadAllText(inHtmlPath);
            using (FileStream pdf = new FileStream(toPdfPath, FileMode.Create))
            {
                ConverterProperties properties = new ConverterProperties();

                HtmlConverter.ConvertToPdf(htmlDocument, pdf, properties);
            }


            //_recetaServices.Create(new Receta { IdConsulta = consulta.IdConsulta });

        }
    }
}

