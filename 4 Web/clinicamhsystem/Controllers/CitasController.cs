using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

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
        return View(new CitasViewModel { Pacientes = pacientes });
    }

    [HttpPost]
    public ActionResult Add(Guid IdPaciente)
    {

        if (_cache.TryGetValue("UserData", out string jsonUserData))
        {
            string fecha_str = Request.Form["fecha"];
            string hora_str = Request.Form["hora"];

            DateOnly fecha = DateOnly.ParseExact(fecha_str, "yyyy-MM-dd", null);
            TimeOnly hora = TimeOnly.ParseExact(hora_str, "HH:mm", null);
            DateTime combinedDateTime = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora.Hour, hora.Minute, hora.Second);

            Usuario userData = JsonConvert.DeserializeObject<Usuario>(jsonUserData);

            Cita cita = new Cita();
            cita.FechaHora = combinedDateTime;
            cita.IdPaciente = IdPaciente;
            cita.IdUsuario = userData.IdUsuario;

            _citaServices.Add(cita);

        }
        var pacientes = _pacienteServices.GetAll();
        return RedirectToAction("Index",new CitasViewModel { Pacientes = pacientes });
    }

}
