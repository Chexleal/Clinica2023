﻿using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IPacienteServices _pacienteServices;
        private readonly IServiciosServices _serviciosServices;
        private readonly IDetallesServices _detallesServices;

        public ReportesController(IPacienteServices pacienteServices, IServiciosServices serviciosServices, IDetallesServices detallesServices)
        {
            _pacienteServices = pacienteServices;
            _serviciosServices = serviciosServices;
            _detallesServices = detallesServices;
        }

    // GET: UsuariosController
    public ActionResult Index()
    {
        var pacientes = _pacienteServices.GetAll();
        //var consultas = _consultaServices.GetAll();
        var servicios = _serviciosServices.GetAll();

            return View(new ReportesViewModel { Pacientes = pacientes, Servicios = servicios, EsServicio = false });
        }

        [HttpPost]
        public ActionResult Paciente(Guid IdPaciente)
        {
            string from = Request.Form["from"];
            string to = Request.Form["to"];

            DateTime from_dt = DateTime.ParseExact(from, "yyyy-MM-dd", null);
            DateTime to_dt = DateTime.ParseExact(to, "yyyy-MM-dd", null);

            var pacientes = _pacienteServices.GetAll();
            var servicios = _serviciosServices.GetAll();
            var paciente = _pacienteServices.GetPacienteById(IdPaciente);
            //var consultas = _pacienteServices.GetConsultasFiltradas();   

            return View("Index",new ReportesViewModel { Pacientes = pacientes, Servicios = servicios, Paciente = paciente, From = from_dt, To = to_dt, EsServicio = false});
        }

        [HttpPost]
        public ActionResult Servicios()
        {
            string from = Request.Form["from"];
            string to = Request.Form["to"];

            DateTime from_dt = DateTime.ParseExact(from, "yyyy-MM-dd", null);
            DateTime to_dt = DateTime.ParseExact(to, "yyyy-MM-dd", null);

            var pacientes = _pacienteServices.GetAll();
            var servicios = _serviciosServices.GetAll();
            var detalles = _detallesServices.GetAll();

            return View("Index",new ReportesViewModel { Pacientes = pacientes, Servicios = servicios, Detalles = detalles, EsServicio = true, From = from_dt, To = to_dt, Paciente = null});
        }

        [HttpPost]
        public IActionResult Detalles(Guid idconsulta)
        {
            var detalles = _detallesServices.GetDetallesByConsulta(idconsulta);
            var servicios = _serviciosServices.GetAll();
            return PartialView("Detalles", (new DetallesPagarViewModel { Detalles = detalles, Servicios = servicios}));
        }
    }
}
