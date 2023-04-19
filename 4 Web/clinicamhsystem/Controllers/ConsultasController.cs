using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    public class ConsultasController : Controller
    {

        private readonly IConsultaServices _consultaServices;
        private readonly IPacienteServices _pacienteServices;

        public ConsultasController(IConsultaServices consultaServices, IPacienteServices pacienteServices)
        {
            _consultaServices = consultaServices;
            _pacienteServices = pacienteServices;
        }

        // GET: UsuariosController
        public ActionResult Index()
        {
            var consultas = _consultaServices.GetAll();
            var pacientes = _pacienteServices.GetAll();
            //var pacientes = _pacienteServices.GetAll();
            return View(new ConsultaViewModel { Consultas = consultas, Pacientes=pacientes });
        }

        //GET: Usuarios/Search? input = t
        public ActionResult Search(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                var consultas = _consultaServices.GetAll();
                return RedirectToAction("Index", consultas);
            }
            else
            {
                var idResult = _consultaServices.SearchConsulta(input);
                return RedirectToAction("Search", idResult);
            }
        }

        // GET: ConsultasController/Details/5
        public ActionResult Detalles(Guid id)
        {
            var consultas = _consultaServices.GetConsulta(id);
            return View("Detalles", consultas);
        }

        // GET: ConsultasController/Create
        public ActionResult Create()
        {
            return View("Create");
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

        // GET: ConsultasController/Delete/5
        //public ActionResult Delete(int id)
        //{
       //     return View();
        //

        // POST: ConsultasController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(Guid id)
        {
            try
            {
                _consultaServices.DeleteConsulta(id);
            }
            catch {  }
            var consultas = _consultaServices.GetAll();
            return View("Index", consultas);
        }
    }
}
