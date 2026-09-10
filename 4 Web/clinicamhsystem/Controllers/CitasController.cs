using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace clinicaWeb.Controllers;
[SecurityFilter("Citas")]
public class CitasController : ErrorHandlingController
{
    private readonly IPacienteServices _pacienteServices;
    private readonly ICitaServices _citaServices;
    private readonly ICurrentUser _currentUser;

    public CitasController(IPacienteServices pacienteServices, ICitaServices citaServices, ICurrentUser currentUser)
    {
        _pacienteServices = pacienteServices;
        _citaServices = citaServices;
        _currentUser = currentUser;
    }
    // GET: CitasController
    public ActionResult Index()
    {
        var pacientes = _pacienteServices.GetAll();
        var citas = _citaServices.GetAll();

        //List<(string, Cita)> eventos = new List<(string, Cita)>();

        //foreach (var cita in citas)
        //{
        //    eventos.Add((cita.Titulo, cita));
        //}

        return View(new CitasViewModel { Pacientes = pacientes, Citas = citas});
    }

    [HttpPost]
    public ActionResult Add(Guid IdPaciente, string destiny, string Fecha, string Hora)
    {
        if (_currentUser.Usuario is not { } usuarioActual)
        {
            return RedirectToAction("Index");
        }

        string fecha_str = Fecha ?? Request.Form["Fecha"];
        string hora_str = Hora ?? Request.Form["Hora"];
        if (string.IsNullOrWhiteSpace(fecha_str) || string.IsNullOrWhiteSpace(hora_str))
        {
            return BadRequest("Fecha y hora son requeridas.");
        }

        if (!DateOnly.TryParseExact(fecha_str, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var fecha))
        {
            return BadRequest("Fecha inválida.");
        }
        if (!TimeOnly.TryParseExact(hora_str, "HH:mm", null, System.Globalization.DateTimeStyles.None, out var hora))
        {
            return BadRequest("Hora inválida.");
        }
        DateTime combinedDateTime = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora.Hour, hora.Minute, 0);

        Paciente paciente = _pacienteServices.GetPacienteById(IdPaciente);
        if (paciente is null)
        {
            return NotFound("Paciente no encontrado.");
        }

        Cita cita = new Cita
        {
            FechaHora = combinedDateTime,
            IdPaciente = IdPaciente,
            IdUsuario = usuarioActual.IdUsuario,
            Titulo = paciente.Nombre + " " + paciente.Apellido
        };
        _citaServices.Add(cita);

        bool esAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        if (esAjax)
        {
            return Json(new { ok = true, id = cita.IdCita, titulo = cita.Titulo, fechaHora = cita.FechaHora });
        }

        return RedirectToAction("Index");
    }


    [HttpGet]
    public List<Cita> Get()
    {
        return _citaServices.GetAll();
    }


    [HttpPost]
    public ActionResult Eliminar(Guid id)
    {
        //Guid id_gui = new Guid(id.ToString());
        try
        {
            _citaServices.Delete(id);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        //var pacientes = _pacienteServices.GetAll();
        //var citas = _citaServices.GetAll();

        //List<(string, Cita)> eventos = new List<(string, Cita)>();

        //foreach (var cita in citas)
        //{
        //    eventos.Add((_pacienteServices.GetPacienteById(cita.IdPaciente).Nombre + " " + _pacienteServices.GetPacienteById(cita.IdPaciente).Apellido, cita));
        //}

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Calendar(Guid pacienteId)
    {
        var pacientes = _pacienteServices.GetAll();
        var paciente = pacientes.FirstOrDefault(x => x.IdPaciente == pacienteId);

        var citas = _citaServices.GetAll();

        List<(string, Cita)> eventos = new List<(string, Cita)>();

        foreach (var cita in citas)
        {
            var pacienteInfo = pacientes.FirstOrDefault(x => x.IdPaciente == cita.IdPaciente);
            eventos.Add((pacienteInfo.Nombre + " " + pacienteInfo.Apellido, cita));
        }

        return PartialView("_calendar", new CitasViewModel { Pacientes = pacientes, Citas = citas, Paciente = paciente });
    }
}
