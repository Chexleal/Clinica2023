using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    public class ContinuarConsulta : Controller
    {

        private readonly IConsultaServices _consultaServices;
        private readonly IPacienteServices _pacienteServices;

        public ContinuarConsulta(IConsultaServices consultaServices, IPacienteServices pacienteServices)
        {
            _consultaServices = consultaServices;
            _pacienteServices = pacienteServices;
        }


        public ActionResult Index(Guid consultaId)
        {
            var consulta = _consultaServices.GetConsulta(consultaId);
            var paciente = _pacienteServices.GetPacienteById(consulta.IdPaciente);
            return View(new ConsultaContinuarViewModel { Consulta = consulta, Paciente=paciente });
        }



        // POST: ConsultasController/Create
        [HttpPost]
        public ActionResult Create(Consulta consulta)
        {
            try
            {
                _consultaServices.AddConsulta(consulta);
            }
            catch
            {           
            }
            return RedirectToAction("Index");
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
}
