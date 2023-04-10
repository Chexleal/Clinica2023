using ClinicaDomain;
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
            return RedirectToAction("Index", "Pacientes");
            //return View("/Pacientes/Pacientes");
        }

        public IActionResult UsuarioExistente(string userName)
        {
            var existingUser = _userServices.CheckUserExist(userName);
            if (existingUser)
            {
                Usuario userInfo = _userServices.GetUserByName(userName);
                ViewData["username"] = userInfo.NombreUsuario.ToString();
                ViewData["securityQuestion"] = userInfo.PreguntaSeg.ToString(); // usa el servicio para recuperar la pregunta de seguridad del usuario, y se la asigna a un ViewData
                ViewData["userEmail"] = userInfo.Correo.ToString();

                string[] emailSplited = userInfo.Correo.ToString().Split('@');
                string beforeArroba = emailSplited[0];
                var emailCensored = string.Concat(Enumerable.Repeat("*", 3)) + beforeArroba.Substring(3);
                var userEmailCesored = emailCensored.ToString() + "@" + emailSplited[1].ToString();

                ViewData["emailCensored"] = userEmailCesored.ToString();

                return View("RecoverAccount"); //retorna la vista para recuperar cuenta       
            }
            else
            {
                return Content("alert('El usuario no existe');", "application/javascript"); // se agrega return content solo para probar el metodo.
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

        public IActionResult CheckEmails(string email, string emailConfirmed)
        {

            bool emailCheck = _userServices.CheckEmails(email, emailConfirmed);
            if (emailCheck)
                return Content("alert('Email Correcto');", "application/javascript"); // Si la respuesta es correcta
            else
                return Content("alert('Email Incorrecto');", "application/javascript"); // Si la respuesta es incorrecta

        }

        public IActionResult RecuperarCuenta()
        {
            return View("CheckUser");
        }

        public IActionResult NuevaClave(string userName)
        {
            return View("NewPassword");
        }

        public IActionResult CrearNuevaClave(string newPassword, string newPasswordConfirmed)
        {
            ViewData["userName"] = Request.Query["userName"];
            string userName = ViewData["userName"].ToString();
            bool newPAssword = _userServices.UpdatePassword(newPassword, newPasswordConfirmed, userName);
            if (newPAssword)
                return Content("alert('Contrase;a igual y existe usuario');", "application/javascript"); // Si la respuesta es correcta
            else
                return Content("alert('contrase;a distinta');", "application/javascript"); // Si la respuesta es incorrecta
        }

        public IActionResult RecuperarCuentaEmail(string userName)
        {
            return View("RecoverAccountEmail");
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