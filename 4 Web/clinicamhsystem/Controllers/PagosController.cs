using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    public class PagosController : Controller
    {
        private readonly IConsultaServices _consultaServices;
        private readonly IDetallesServices _detallesServices;
        private readonly IServiciosServices _serviciosServices;

        public PagosController(IConsultaServices consultaServices, IDetallesServices detallesServices, IServiciosServices serviciosServices)
        {
            _consultaServices = consultaServices;
            _detallesServices = detallesServices;
            _serviciosServices = serviciosServices;
        }

        // GET: PagosController
        public ActionResult Index()
        {
            var consultas = _consultaServices.GetAll();
            var detalles = _detallesServices.GetAll();
            var servicios = _serviciosServices.GetAll();
            return View(new PagarConsultaViewModel { Consultas = consultas, Servicios = servicios });
        }


        // POST: PagosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        /*[HttpGet]
        public IActionResult GetConsulta(Guid consultaId)
        {
            var consulta = _consultaServices.GetConsulta(consultaId);
            return PartialView("Editar", consulta);
        }*/

        [HttpPost]
        public IActionResult Detalles(Guid idconsulta)
        {
            var detalles = _detallesServices.GetAll();
            var servicios = _serviciosServices.GetAll();
            return PartialView("Detalles", (new DetallesPagarViewModel { Detalles = detalles, Servicios = servicios, Id_consulta = idconsulta }));
        }

        [HttpPost]
        // GET: ConsultasController/Pagos
        public void Pagos(DetalleCobro detalle)
        {
            _detallesServices.AddDetalle(detalle);
        }
    }
}
