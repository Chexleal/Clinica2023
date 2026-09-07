using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;
[SecurityFilter("Reportes")]

public class ReportesController : Controller
{
    private readonly IPacienteServices _pacienteServices;
    private readonly IServiciosServices _serviciosServices;
    private readonly IDetallesServices _detallesServices;
    private readonly IConsultaServices _consultaServices;
    private readonly IVentaService _ventaService;

    public ReportesController(IPacienteServices pacienteServices, IServiciosServices serviciosServices, IDetallesServices detallesServices, IConsultaServices consultaServices, IVentaService ventaService)
    {
        _pacienteServices = pacienteServices;
        _serviciosServices = serviciosServices;
        _detallesServices = detallesServices;
        _consultaServices = consultaServices;
        _ventaService = ventaService;
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
    public ActionResult Paciente(Guid IdPaciente, DateTime from_dt, DateTime to_dt)
    {

        //DateTime from_dt = DateTime.ParseExact(from, "yyyy-MM-dd", null);
        //DateTime to_dt = DateTime.ParseExact(to, "yyyy-MM-dd", null);

        var pacientes = _pacienteServices.GetAll();
        var servicios = _serviciosServices.GetAll();
        var paciente = _pacienteServices.GetPacienteById(IdPaciente);
        paciente.Consulta = _consultaServices.GetAllByPacienteId(paciente.IdPaciente, from_dt, to_dt);
        //var consultas = _pacienteServices.GetConsultasFiltradas();   

        return View("Index",new ReportesViewModel { Pacientes = pacientes, Servicios = servicios, Paciente = paciente, From = from_dt, To = to_dt, EsServicio = false});
    }

    [HttpPost]
    public ActionResult Servicios(DateTime from_dt, DateTime to_dt)
    {
        var consultas = _consultaServices.GetAllByRangeWithPaciente(from_dt, to_dt);
        var pacientes = consultas.Select(c => c.PacienteInformacion).DistinctBy(p => p.IdPaciente).ToList();
        var consultasPorPaciente = consultas.ToLookup(c => c.IdPaciente);
        foreach (var paciente in pacientes)
            paciente.Consulta = consultasPorPaciente[paciente.IdPaciente].ToList();
        var servicios = _serviciosServices.GetAll();
        var detalles = _detallesServices.GetByRange(from_dt, to_dt);
        // Nuevo: ventas (consulta + mostrador) del rango. Los servicios ya migrados
        // viven en VentaDetalle; se exponen vía ViewBag sin romper la tabla legada.
        var ventas = _ventaService.GetPorRango(from_dt, to_dt).Where(v => v.Estado == "Pagada").ToList();
        var ventaDetalles = ventas.SelectMany(v => _ventaService.GetDetalles(v.IdVenta)).ToList();
        ViewBag.Ventas = ventas;
        ViewBag.VentaDetalles = ventaDetalles;

        return View("Index", new ReportesViewModel { Pacientes = pacientes, Servicios = servicios, Detalles = detalles, EsServicio = true, From = from_dt, To = to_dt, Paciente = null});
    }

    [HttpPost]
    public IActionResult Detalles(Guid idconsulta)
    {
        var detalles = _detallesServices.GetDetallesByConsulta(idconsulta);
        var servicios = _serviciosServices.GetAll();
        return PartialView("Detalles", (new DetallesPagarViewModel { Detalles = detalles, Servicios = servicios}));
    }
}
