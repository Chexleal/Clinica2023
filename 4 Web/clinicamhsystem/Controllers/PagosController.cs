using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace clinicaWeb.Controllers;
[SecurityFilter("Pagos")]
public class PagosController : Controller
{
    private readonly IConsultaServices _consultaServices;
    private readonly IDetallesServices _detallesServices;
    private readonly IServiciosServices _serviciosServices;
    private readonly IPacienteServices _pacientesServices;

    public PagosController(IConsultaServices consultaServices, IDetallesServices detallesServices, IServiciosServices serviciosServices, IPacienteServices pacientesServices)
    {
        _consultaServices = consultaServices;
        _detallesServices = detallesServices;
        _serviciosServices = serviciosServices;
        _pacientesServices = pacientesServices;
    }

    // GET: PagosController
    public ActionResult Index()
    {
        var consultas = _consultaServices.GetAllNotPaid();
        foreach(var consulta in consultas)
            consulta.PacienteInformacion ??= _pacientesServices.GetPacienteById(consulta.IdPaciente);
        //var detalles = _detallesServices.GetAll();
        var servicios = _serviciosServices.GetAll();
        return View(new PagarConsultaViewModel { Consultas = consultas, Servicios = servicios });
    }


    [HttpPost]
    public IActionResult Detalles(Guid idconsulta)
    {
        var detalles = _detallesServices.GetAll();
        //Console.Write("Detalless -------------------------------------------------" + detalles.Count);
        var servicios = _serviciosServices.GetAll();
        var consulta = _consultaServices.GetConsulta(idconsulta);
        return PartialView("Detalles", new DetallesPagarViewModel { Detalles = detalles, Servicios = servicios, consulta = consulta });
    }


        [HttpPost]
        // GET: ConsultasController/Pagos
        public IActionResult AddDetalle(DetalleCobro detalle)
        {
            //var detalleC = JsonSerializer.Deserialize<DetalleCobro>(detalle);
            //var detalleC = (DetalleCobro)detalle;
            //Console.WriteLine(detalle);

        _detallesServices.AddDetalle(detalle);

        var detalles = _detallesServices.GetAll();
        var servicios = _serviciosServices.GetAll();
        var consulta = _consultaServices.GetConsulta(detalle.IdConsulta);
        return PartialView("Detalles", (new DetallesPagarViewModel { Detalles = detalles, Servicios = servicios, consulta = consulta }));
    }

    [HttpPost]
    public ActionResult Eliminar(Guid id, Guid idConsulta)
    {
        try
        {
            _detallesServices.Delete(id);
        }
        catch { }
        var detalles = _detallesServices.GetAll();
        var servicios = _serviciosServices.GetAll();
        var consulta = _consultaServices.GetConsulta(idConsulta);
        return PartialView("Detalles", (new DetallesPagarViewModel { Detalles = detalles, Servicios = servicios, consulta = consulta }));
    }

    [HttpPost]
    public ActionResult Pagar(Guid id)
    {
        try
        {
            _detallesServices.Pagar(id);
        }
        catch { }
        return RedirectToAction("Index");
    }
}
