using clinicamhsystem.Models;
using ClinicaServices;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    public class PacientesController: Controller
    {
            public IActionResult Pacientes()
            {
                return View();
            }
        }
}
