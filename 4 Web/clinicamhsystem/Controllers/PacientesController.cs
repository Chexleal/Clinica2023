using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    public class PacientesController: Controller
    {

        private readonly IPacienteServices _pacienteServices;

        public PacientesController(IPacienteServices pacienteServices)
        {
            _pacienteServices = pacienteServices;
        }

        public IActionResult Index()
        {
            var pacientes = _pacienteServices.GetAll();
            return View(pacientes);
        }

        // GET: UsuariosController/Detalles/fj33-4ra4r
        public ActionResult Detalles(Guid id)
        {
            var paciente = _pacienteServices.GetPacienteById(id);
            return View("Detalles", paciente);
        }

        // GET: UsuariosController/Create
        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Paciente paciente)
        {
            try
            {
                _pacienteServices.AddPaciente(paciente);
                return RedirectToAction("Index");
            }
            catch
            {
                //LO QUE SEA QUE VAYAMOS A HACER ACA 
           
            }
            return RedirectToAction("Index");
        }

        // GET: UsuariosController/Editar/fj33-4ra4r
        public ActionResult Editar(Guid id)
        {
            var paciente = _pacienteServices.GetPacienteById(id);
            return View("Editar", paciente);
        }

        // POST: UsuariosController/Editar/fj33-4ra4r
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Paciente paciente)
        {
            try
            {
                _pacienteServices.UpdatePaciente(paciente);
                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

        // POST: UsuariosController/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(Guid id)
        {
            try
            {
                _pacienteServices.DeletePaciente(id);
            }
            catch { }
            var pacientes = _pacienteServices.GetAll();
            return RedirectToAction("Index", pacientes);
        }

        [HttpGet]
        public IActionResult GetPaciente(Guid pacienteId)
        {
            var paciente = _pacienteServices.GetPacienteById(pacienteId);
            return PartialView("Editar", paciente);
        }
    }
}
