﻿using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using System.Security.Policy;
using System.Text;
//using System.Web;
//using WkHtmlToPdfDotNet;
//using WkHtmlToPdfDotNet.Contracts;

namespace clinicaWeb.Controllers;

[SecurityFilter("ContinuarConsulta")]
public class ContinuarConsulta : Controller
{

    private readonly IConsultaServices _consultaServices;
    private readonly IRecetaServices _recetaServices;
    private readonly IPacienteServices _pacienteServices;
    //private readonly IConverter _converter;

    public ContinuarConsulta(IConsultaServices consultaServices, IPacienteServices pacienteServices, IRecetaServices recetaServices/*, IConverter converter*/)
    {
        _consultaServices = consultaServices;
        _pacienteServices = pacienteServices;
        _recetaServices = recetaServices;
        //_converter = converter;
    }


    public ActionResult Index(Guid consultaId)
    {
        var consulta = _consultaServices.GetConsulta(consultaId);
        var receta = _recetaServices.GetByConsulta(consultaId) ?? new();
        //var paciente = _pacienteServices.GetPacienteById(consulta.IdPaciente);
        return View(new ConsultaContinuarViewModel { Consulta = consulta,Receta=receta/*, Paciente = paciente */});
    }



    // POST: ConsultasController/Create
    [HttpPost]
    public ActionResult Guardar(Consulta consulta)
    {
        var consultaDb = _consultaServices.GetConsulta(consulta.IdConsulta);

        consultaDb.Diagnostico = consulta.Diagnostico;
        consultaDb.MotivoConsulta = consulta.MotivoConsulta;
        consultaDb.Observaciones = consulta.Observaciones;
        consultaDb.Peso = consulta.Peso;
        consultaDb.PresionArterial = consulta.PresionArterial;
        consultaDb.Radiografias = consulta.Radiografias;
        consultaDb.Temperatura = consulta.Temperatura;
        consultaDb.Terminada = consulta.Terminada;

        _consultaServices.UpdateConsulta(consulta);
        if (consulta.Terminada) return RedirectToAction("Index", "Consultas");
        return RedirectToAction("Index", new { consultaId =consulta.IdConsulta});
    }

    [HttpPost]
    public ActionResult GuardarReceta(Receta receta)
    {
        var updReceta = _recetaServices.GetByConsulta(receta.IdConsulta);
        if (updReceta is null)
        {
            var recetaCreate = new Receta()
            {
                Descripcion = receta.Descripcion,
                IdConsulta = receta.IdConsulta
            };
            _recetaServices.Create(recetaCreate);
        }
        else
        {
            updReceta.Descripcion = receta.Descripcion;
            _recetaServices.Update(receta);
        }

        return RedirectToAction("Index", new { consultaId = receta.IdConsulta });
    }

        [HttpGet]
        public ActionResult Descargar(Guid consultaId)
        {
            var receta = _recetaServices.GetByConsulta(consultaId);
            if (receta is null)
            {
                //eturn new Rotativa.ViewAsPdf("Index", consultaId);
                return null;
            }
            else
                return null;
        }

        public ActionResult DescargarPdf(Guid consultaId)
        {
                var receta = _recetaServices.GetByConsulta(consultaId);
                var consulta = _consultaServices.GetConsulta(consultaId);
                var pacienteInfo = _pacienteServices.GetPacienteById(consulta.IdPaciente);
                ViewData["Paciente"] = pacienteInfo.Nombre + " " + pacienteInfo.Apellido;
                ViewData["fecha"] = receta.Fecha.ToString();
                ViewData["detalleReceta"] = receta.Descripcion.ToString();

                return View("ConsultaPdf");
        }

    // GET: ConsultasController/Edit/5
    public ActionResult Editar(Guid id)
    {
        var consultas = _consultaServices.GetConsulta(id);
        return RedirectToAction("Editar", consultas);
    }

    // POST: ConsultasController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(Consulta consulta)
    {
        try
        {
            _consultaServices.UpdateConsulta(consulta);
            var consultas = _consultaServices.GetAll();
            return RedirectToAction("Index", consultas);
        }
        catch
        {
            return View("Error");
        }
    }


}
