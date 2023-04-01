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
            var users = _userServices.GetAll();
            return View(new HomeViewModel { Usuarios = users });
        }

        public IActionResult LogIn(string password, string user)
        {
            /*
            var existingUser = _userServices.Authenticate(user, password);
            if (existingUser)
            {
                return View("Privacy"); // SI EXISTE EL USUARIO ENVIAR A PAGINA SIGUIENTE
            }
            else
            {
                return Content("alert('El usuario no existe');", "application/javascript"); // se agrega return content solo para probar el metodo.
            }
            */
            return View("/Pacientes/Pacientes");
        }

        public IActionResult UsuarioExistente(string userName)
        {

            var existingUser = _userServices.CheckUserExist(userName);
            if (existingUser)
                return View("RecoverAccount", userName); // Si el usuario existe en la db, entonces se mostrara su pregunta de seguridad
            else
                return Content("alert('El usuario no existe sss');", "application/javascript"); // se agrega return content solo para probar el metodo.
        }

        public IActionResult SecurityQuestion(string userName)
        {

            string? preguntaSeg = _userServices.SecurityQuestion(userName);
            if (preguntaSeg != null)
            {

                return Content($"La pregunta es{preguntaSeg}", "application/javascript");

            }
            else
            {
                return Content($"No hay pregunta", "application/javascript");
            }
        }

        public IActionResult CheckAnswer(string answer)
        {

            string? preguntaSegCheck = _userServices.CheckAnswer(answer);
            if (preguntaSegCheck != null)
                return Content("alert('Respuesta Correcta');", "application/javascript"); // Si la respuesta es correcta
            else
                return Content("alert('Respuesta Incorrecta');", "application/javascript"); // Si la respuesta es incorrecta

        }

        public IActionResult RecuperarCuenta()
        {
            return View("CheckUser");
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