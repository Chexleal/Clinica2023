using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;

namespace clinicaWeb.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IUserServices _userServices;

        public UsuariosController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        // GET: UsuariosController
        public ActionResult Index()
        {
            var users = _userServices.GetAll();
            return View(users);
        }

        //GET: Usuarios/Search?input=t
        public ActionResult Search(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                var users = _userServices.GetAll();
                return RedirectToAction("Index", users);
            }
            else
            {
                var idResult = _userServices.SearchUser(input);
                return RedirectToAction("Search", idResult);
            }
        }

        // GET: UsuariosController/Detalles/fj33-4ra4r
        public ActionResult Detalles(Guid id)
        {
            var user = _userServices.GetUser(id);
            return View("Detalles", user);
        }

        // GET: UsuariosController/Create
        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Usuario usuario)
        {
            try
            {             
                _userServices.AddUser(usuario);

            }
            catch
            {
            }
            return RedirectToAction("Index");
        }

        // GET: UsuariosController/Editar/fj33-4ra4r
        public ActionResult Editar(Guid id)
        {
            var user = _userServices.GetUser(id);
            return View("Editar", user);
        }

        // POST: UsuariosController/Editar/fj33-4ra4r
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Usuario usuario)
        {
            try
            {
                _userServices.UpdateUser(usuario);
                var users = _userServices.GetAll();
                return RedirectToAction("Index", users);
            }
            catch
            {
                return View("Error");
            }
        }

        // POST: UsuariosController/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(Guid id)
        {
            try
            {
                _userServices.DeleteUser(id);
            }
            catch { }
            var users = _userServices.GetAll();
            return RedirectToAction("Index",users);
        }

        // POST: UsuariosController/Active/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Activate(Guid id, bool state)
        {
            try
            {
                _userServices.SetActive(id,state);
            }
            catch { }
            var users = _userServices.GetAll();
            return RedirectToAction("Index", users);
        }
    }
}
