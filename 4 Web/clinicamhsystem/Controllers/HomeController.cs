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