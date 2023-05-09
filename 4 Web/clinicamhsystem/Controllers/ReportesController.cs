using ClinicaServices;
using clinicaWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IPacienteServices _pacienteServices;
        private readonly IConsultaServices _consultaServices;
        private readonly IServiciosServices _serviciosServices;

        public ReportesController(IConsultaServices consultaServices, IPacienteServices pacienteServices, IServiciosServices serviciosServices)
        {
            _consultaServices = consultaServices;
            _pacienteServices = pacienteServices;
            _serviciosServices = serviciosServices;
        }

        // GET: UsuariosController
        public ActionResult Index()
        {
            var pacientes = _pacienteServices.GetAll();
            //var consultas = _consultaServices.GetAll();
            var servicios = _serviciosServices.GetAll();

            return View(new ReportesViewModel { Pacientes = pacientes, Servicios = servicios, Servicio = null });
        }

        [HttpPost]
        public ActionResult Search(Guid servicioid)
        {
            var pacientes = _pacienteServices.GetAll();
            var servicios = _serviciosServices.GetAll();
            //var consultas = _pacienteServices.GetConsultasFiltradas();
            var servicio = _serviciosServices.GetServicio(servicioid);

            return View(new ReportesViewModel { Pacientes = pacientes, Servicios = servicios, Servicio = servicio });
        }
    }
}
