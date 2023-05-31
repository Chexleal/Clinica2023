using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace clinicaWeb.Controllers;
[SecurityFilter("Citas")]
public class CitasController : Controller
{
    private readonly IPacienteServices _pacienteServices;
    private readonly ICitaServices _citaServices;
    private readonly IMemoryCache _cache;

    public CitasController(IPacienteServices pacienteServices , ICitaServices citaServices, IMemoryCache memoryCache)
    {
        _pacienteServices = pacienteServices;
        _citaServices = citaServices;
        _cache = memoryCache;
    }
    // GET: CitasController
    public ActionResult Index()
    {
        var pacientes = _pacienteServices.GetAll();
        var citas = _citaServices.GetAll();

        List<(string, Cita)> eventos = new List<(string, Cita)>();

        foreach (var cita in citas)
        {
            var pacienteInfo = pacientes.FirstOrDefault(x=>x.IdPaciente==cita.IdPaciente);
            eventos.Add((pacienteInfo.Nombre + " " + pacienteInfo.Apellido, cita));
        }

        return View(new CitasViewModel { Pacientes = pacientes, Citas = citas, Eventos = eventos });
    }

    [HttpPost]
    public ActionResult Add(Guid IdPaciente, string destiny)
    {

        if (_cache.TryGetValue("UserData", out string jsonUserData))
        {
            string fecha_str = Request.Form["fecha"];
            string hora_str = Request.Form["hora"];

            DateOnly fecha = DateOnly.ParseExact(fecha_str, "yyyy-MM-dd", null);
            TimeOnly hora = TimeOnly.ParseExact(hora_str, "HH:mm", null);
            DateTime combinedDateTime = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora.Hour, hora.Minute, hora.Second);

            Usuario userData = JsonConvert.DeserializeObject<Usuario>(jsonUserData);

            Paciente paciente = _pacienteServices.GetPacienteById(IdPaciente);

            Cita cita = new Cita();
            cita.FechaHora = combinedDateTime;
            cita.IdPaciente = IdPaciente;
            cita.IdUsuario = userData.IdUsuario;
            cita.Titulo = paciente.Nombre + " " + paciente.Apellido;
            _citaServices.Add(cita);
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
        catch { }
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

        return PartialView("_calendar", new CitasViewModel { Pacientes = pacientes, Citas = citas, Eventos = eventos, Paciente = paciente });
    }
}
