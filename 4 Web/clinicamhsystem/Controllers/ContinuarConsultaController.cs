using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using System.Globalization;
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
    private readonly ICitaServices _citasServices;
    //private readonly IConverter _converter;

    public ContinuarConsulta(IConsultaServices consultaServices, IPacienteServices pacienteServices, IRecetaServices recetaServices,ICitaServices citaServices/*, IConverter converter*/)
    {
        _consultaServices = consultaServices;
        _pacienteServices = pacienteServices;
        _recetaServices = recetaServices;
        _citasServices = citaServices;
        //_converter = converter;
    }


    public ActionResult Index(Guid consultaId)
    {
        var consulta = _consultaServices.GetConsulta(consultaId);
        consulta.PacienteInformacion ??= _pacienteServices.GetPacienteById(consulta.IdPaciente);
        consulta.PacienteInformacion.Consulta = _consultaServices.GetAllByPacienteId(consulta.IdPaciente);
        var receta = _recetaServices.GetByConsulta(consultaId) ?? new();
        var medicamentos = _recetaServices.GetAllMedicamentos() ?? new();
        //var paciente = _pacienteServices.GetPacienteById(consulta.IdPaciente);
        return View(new ConsultaContinuarViewModel { Consulta = consulta, Receta = receta,Medicamentos= medicamentos/*, Paciente = paciente */});
    }



    // POST: ConsultasController/Create
    [HttpPost]
    public ActionResult Guardar(Consulta consulta)
    {
        var consultaDb = _consultaServices.GetConsulta(consulta.IdConsulta);

        TimeSpan duracion = DateTime.Now.Subtract(consultaDb.Fecha);

        consultaDb.Diagnostico = consulta.Diagnostico;
        consultaDb.HistoriaClinica = consulta.HistoriaClinica;
        consultaDb.MotivoConsulta = consulta.MotivoConsulta;
        consultaDb.Observaciones = consulta.Observaciones;
        consultaDb.Peso = consulta.Peso;
        consultaDb.PresionArterial = consulta.PresionArterial;
        consultaDb.Radiografias = consulta.Radiografias;
        consultaDb.Temperatura = consulta.Temperatura;
        consultaDb.Terminada = consulta.Terminada;
        consultaDb.TiempoDuracion = duracion;

        _consultaServices.UpdateConsulta(consultaDb);
        if (consulta.Terminada) return RedirectToAction("Index", "Consultas");
        return RedirectToAction("Index", new { consultaId = consulta.IdConsulta });
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
            _recetaServices.Update(updReceta);
        }

        return RedirectToAction("Index", new { consultaId = receta.IdConsulta });
    }



    public ActionResult DescargarPdf(Guid consultaId)
    {
            var receta = _recetaServices.GetByConsulta(consultaId);
            var consulta = _consultaServices.GetConsulta(consultaId);
            var pacienteInfo = _pacienteServices.GetPacienteById(consulta.IdPaciente);
            var proximaCita = _citasServices.GetNextCita(consulta.Fecha, consulta.IdPaciente);

            ViewData["Paciente"] = pacienteInfo.Nombre + " " + pacienteInfo.Apellido;
            ViewData["fecha"] = receta.Fecha.ToString("dd/MM/yyyy");
            ViewData["observacionesReceta"] = receta.Descripcion.ToString();
            ViewData["detalleReceta"] = _recetaServices.GetAllDetalles(receta.IdReceta);
            ViewData["CitaProx"] = proximaCita;
        return View("ConsultaPdf");
    }


    // GET: ConsultasController/Edit/5
    public ActionResult Editar(Guid id)
    {
        var consultas = _consultaServices.GetConsulta(id);
        return RedirectToAction("Editar", consultas);
    }

    //// POST: ConsultasController/Edit/5
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public ActionResult Edit(Consulta consulta)
    //{
    //    try
    //    {
    //        _consultaServices.UpdateConsulta(consulta);
    //        var consultas = _consultaServices.GetAll();
    //        return RedirectToAction("Index", consultas);
    //    }
    //    catch
    //    {
    //        return View("Error");
    //    }
    //}

    [HttpGet]
    public ActionResult GetMedicamentos(Guid idReceta)
    {
        var detalles=_recetaServices.GetAllDetalles(idReceta);

        return PartialView("Partials/_medicamentosTabla", new ConsultaContinuarViewModel {  DetallesReceta= detalles });
    }

    [HttpPost]
    public ActionResult DeleteMedicamentos(Guid idDetalleReceta)
    { 
        var idReceta = _recetaServices.DeleteDetalle(idDetalleReceta);

        return RedirectToAction("GetMedicamentos", new { idReceta });
    }


    [HttpPost]
    public ActionResult AddDetalleReceta(DetalleReceta detalleReceta)
    {
        _recetaServices.AddDetalleReceta(detalleReceta);

        return RedirectToAction("GetMedicamentos", new { idReceta = detalleReceta.IdReceta });
    }


}
