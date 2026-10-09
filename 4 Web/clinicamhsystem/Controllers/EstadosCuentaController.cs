using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

[SecurityFilter("Pagos")]
public class EstadosCuentaController(IEstadoCuentaService estados, IAseguradoraService aseguradoras, IPacienteServices pacientes) : Controller
{
    public IActionResult Index(DateTime? desde, DateTime? hasta, Guid? idPaciente, Guid? idAseguradora, string? autorizacion)
    {
        ViewBag.Desde=desde?.ToString("yyyy-MM-dd"); ViewBag.Hasta=hasta?.ToString("yyyy-MM-dd");
        ViewBag.IdPaciente=idPaciente; ViewBag.IdAseguradora=idAseguradora; ViewBag.Autorizacion=autorizacion;
        ViewBag.Aseguradoras=aseguradoras.GetAll(); ViewBag.Pacientes=pacientes.GetAll();
        ViewBag.PacienteNombres=((IEnumerable<ClinicaDomain.Paciente>)ViewBag.Pacientes).ToDictionary(p=>p.IdPaciente,p=>$"{p.Nombre} {p.Apellido}".Trim());
        return View(estados.Buscar(desde,hasta,idPaciente,idAseguradora,autorizacion));
    }
    public IActionResult Ver(Guid id)
    {
        var cuenta=estados.GetEstadoCuenta(id); return cuenta is null ? NotFound() : View("EstadoCuentaPdf",cuenta);
    }
}
