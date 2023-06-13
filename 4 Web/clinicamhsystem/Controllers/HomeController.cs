using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
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
        bool usuarioEntra;
        var existingUser = _userServices.Authenticate(user, password);
        //Usuario existingUser = new() { Nombre = "Dev", Apellido = "Test"};
        //existingUser.IdUsuario = new Guid();
        if (existingUser is not null)
        {
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
            Usuario? userInfo = _userServices.GetUserByName(userName);
            ViewData["username"] = userInfo.NombreUsuario.ToString();
            ViewData["securityQuestion"] = userInfo.PreguntaSeg.ToString(); // usa el servicio para recuperar la pregunta de seguridad del usuario, y se la asigna a un ViewData
            ViewData["userEmail"] = userInfo.Correo.ToString();

            string[] emailSplited = userInfo.Correo.ToString().Split('@');
            string beforeArroba = emailSplited[0];
            var emailCensored = string.Concat(Enumerable.Repeat("*", 3)) + beforeArroba.Substring(3);
            var userEmailCesored = emailCensored.ToString() + "@" + emailSplited[1].ToString();

            ViewData["emailCensored"] = userEmailCesored.ToString();

            return View("RecoverAccount", userName); //retorna la vista para recuperar cuenta       
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
            // return RedirectToAction("NuevaClave", new { hiddenUsername }); // Si la respuesta es correcta
            // return Content($"alert('Respuesta correcta');", "application/javascript");
            return View("NewPassword", hiddenUsername);
        else
            TempData["Error"] = "Respuesta Incorrecta";
            return View("RecoverAccount", hiddenUsername);
    }

    public IActionResult CheckEmails(string email, string emailConfirmed, string hiddenUsername)
    {

        bool emailCheck = _userServices.CheckEmails(email, emailConfirmed, hiddenUsername);
        if (emailCheck)
            return Content($"alert('Email Correcto del compa {hiddenUsername} correo enviado ');", "application/javascript"); // Si la respuesta es correcta
        else
            return Content("alert('Email Incorrecto');", "application/javascript"); // Si la respuesta es incorrecta

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

    public IActionResult CrearNuevaClave(string newPassword, string newPasswordConfirmed)
    {
        ViewData["userName"] = Request.Query["userName"];
        string? userName = ViewData["userName"].ToString();
        bool checkPassword = _userServices.CheckNewPassword(newPassword, newPasswordConfirmed, userName);
        if (checkPassword)
            return Content("alert('Contrase;a igual y existe usuario');", "application/javascript"); // Si la respuesta es correcta
        else
            return Content($"alert('contrase;a distinta' usuario: {userName.ToString()});", "application/javascript"); // Si la respuesta es incorrecta
    }

    public IActionResult RecuperarCuentaEmail(string userName)
    {
        ViewData["username"] = userName;
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