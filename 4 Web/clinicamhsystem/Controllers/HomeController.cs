using clinicamhsystem.Models;
using ClinicaServices;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace clinicamhsystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserServices _userServices;

        public HomeController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        public IActionResult Index()
        {
           var users= _userServices.GetAll();
            return View(new HomeViewModel { Usuarios=users});
        }

        public IActionResult LogIn(string password, string user)
        {
            var existingUser = _userServices.Authenticate(user,password);
            if (existingUser)
                return Content("alert('El usuario existe');", "application/javascript"); // se agrega return content solo para probar el metodo.
            //return View(new HomeViewModel { Usuarios = users }); SI EXISTE EL USUARIO ENVIAR A PAGINA SIGUIENTE
            else
                return Content("alert('El usuario no existe');", "application/javascript"); // se agrega return content solo para probar el metodo.
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}