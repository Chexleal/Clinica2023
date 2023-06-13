using ClinicaDomain;
using ClinicaServices;
using Microsoft.AspNetCore.Mvc;
using clinicaWeb.Models;
using clinicaWeb.Security;

namespace clinicaWeb.Controllers;

[SecurityFilter("Pacientes")]
public class PacientesController: Controller
{

    private readonly IPacienteServices _pacienteServices;
    private readonly IConsultaServices _consultaServices;

    public PacientesController(IPacienteServices pacienteServices, IConsultaServices consultaServices)
    {
        _pacienteServices = pacienteServices;
        _consultaServices = consultaServices;
    }

    public IActionResult Index()
    {
        var pacientes = _pacienteServices.GetAll();
        var consultas = _consultaServices.GetAll();
        return View(new PacientesViewModel { Pacientes=pacientes,Consultas=consultas });
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
            return View("Error");

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
    public ActionResult Eliminar(Guid id)
    {
        try
        {
            _pacienteServices.DeletePaciente(id);
        }
        catch {
            return View("Error");
        }
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
