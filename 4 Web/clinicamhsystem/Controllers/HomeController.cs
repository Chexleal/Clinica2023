using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using clinicaWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using ServiceStack.Script;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace clinicamhsystem.Controllers;
public class HomeController : Controller
{
    private readonly IUserServices _userServices;
    private readonly IMemoryCache _memoryCache;

    public HomeController(IUserServices userServices, IMemoryCache memoryCache)
    {
        _userServices = userServices;
        _memoryCache = memoryCache;
    }

    public IActionResult Index()
    {
        //var users = _userServices.GetAll();
        return View();
    }

    public IActionResult LogInAsync(string password, string user)
    {

        var existingUser = _userServices.Authenticate(user, password);
        //Usuario existingUser = new() { Nombre = "Dev", Apellido = "Test"};
        //existingUser.IdUsuario = new Guid();
        if (existingUser is not null)
        {
            existingUser.Permisos = _userServices.GetPermissions(existingUser.IdUsuario);
            existingUser.Permisos ??= new();
            string clave = "UserData";
            string valor = JsonConvert.SerializeObject(existingUser);
            _memoryCache.Set(clave, valor);
            TempData["UsuarioNombre"] = $"{existingUser.Nombre} {existingUser.Apellido}";
            return RedirectToAction("Index", "Pacientes");
        }
        else
        {
            TempData["Error"] = "Usuario o contraseña incorrectos";
            return RedirectToAction("Index");
        }
    }

    public IActionResult UsuarioExistente(string userName)
    {
        var existingUser = _userServices.CheckUserExist(userName);
        if (existingUser)
        {
            var userSecureQuestion = ViewData["securityQuestion"];
            var userEmail = ViewData["userEmail"];

            Usuario? userInfo = _userServices.GetUserByName(userName);
            ViewData["username"] = userInfo.NombreUsuario.ToString();
            userSecureQuestion = userInfo.PreguntaSeg.ToString(); // usa el servicio para recuperar la pregunta de seguridad del usuario, y se la asigna a un ViewData
            userEmail= userInfo.Correo.ToString();

            string[] emailSplited = userInfo.Correo.ToString().Split('@');
            string beforeArroba = emailSplited[0];
            var emailCensored = string.Concat(Enumerable.Repeat("*", 3)) + beforeArroba.Substring(3);
            var userEmailCesored = emailCensored.ToString() + "@" + emailSplited[1].ToString();

            //List<object> userInformation = new List<object> { userName, userSecureQuestion, userEmail };
            object[] userInformation = { userName, userSecureQuestion, userEmailCesored };

            return View("RecoverAccount", userInformation); //retorna la vista para recuperar cuenta       
            //return RedirectToAction("RecoverAccount", new { userName });
        }
        else
        {
            TempData["Error"] = "El usuario que introdujo no existe";
            return View("CheckUser");
        }

    }

    public IActionResult CheckAnswer(string hiddenUsername, string answer)
    {
        string? preguntaSegCheck = _userServices.CheckAnswer(answer);
        if (preguntaSegCheck != null)
        {
            TempData["Success"] = "Respuesta Correcta";
            return View("NewPassword", hiddenUsername);
        }else
        {
            TempData["Error"] = "Respuesta Incorrecta";
            Usuario? userInfo = _userServices.GetUserByName(hiddenUsername);  
            var userSecureQuestion = userInfo.PreguntaSeg.ToString(); // usa el servicio para recuperar la pregunta de seguridad del usuario, y se la asigna a un ViewData
            var userEmail = userInfo.Correo.ToString();
            object[] userInformation = { hiddenUsername, userSecureQuestion, userEmail };
            return View("RecoverAccount", userInformation);
        }
            
    }

    public IActionResult CheckEmails(string email, string emailConfirmed, string hiddenUsername)
    {

        bool emailCheck = _userServices.CheckEmails(email, emailConfirmed, hiddenUsername);
        if (emailCheck)
        {
            TempData["Success"] = "Correo enviado";
            return View("Index");
        }
        else
        {
            TempData["Error"] = "Email Incorrecto";
            return View("RecoverAccountEmail");
        }

    }

    public IActionResult RecuperarCuenta()
    {

        return View("CheckUser");
    }

    public IActionResult NuevaClave(string userName)
    {
        ViewData["username"] = userName;
        return View("NewPassword", userName);
    }

    public IActionResult CrearNuevaClave(string newPassword, string newPasswordConfirmed, string usModel)
    {
        string hiddenUsername = Request.Query["hiddenUsername"];
        Usuario? userInfo = _userServices.GetUserByName(usModel);
        bool checkPassword = _userServices.CheckNewPassword(newPassword, newPasswordConfirmed, userInfo.IdUsuario);
        if (checkPassword == true)
        {
            TempData["Success"] = "Has cambiado tu contraseña";
            return View("Index");
        }
        else
        {
            TempData["Error"] = "Las contraseñas no coinciden";
            return View("NewPassword", usModel);
        }
    }

    public IActionResult RecuperarCuentaEmail(string userName)
    {
        ViewData["username"] = userName;
        return View("RecoverAccountEmail");
    }

    [HttpGet]
    public IActionResult ChangePassWord()
    {
        return PartialView("_ChangePassword");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ChangePassWord(String Password)
    {
        try
        {
            if (_memoryCache.TryGetValue("UserData", out string jsonUserData))
            {
                Usuario userData = JsonConvert.DeserializeObject<Usuario>(jsonUserData);

                _userServices.ChangePassword(userData.IdUsuario, Password);
            }
            return RedirectToAction("Index");
        }
        catch
        {
            return View("Error");
        }
    }

    [HttpPost]
    public ActionResult CerrarSesion()
    {
        try
        {
            string clave = "UserData";
            _memoryCache.Remove(clave);
            TempData.Remove("UsuarioNombre");
            TempData.Remove("IdUsuario");
            return RedirectToAction("Index");
        }
        catch
        {
            return View("Error");
        }
    }

    public ActionResult Editar(Guid id)
    {
        var user = _userServices.GetUser(id);
        return View("Editar", user);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}