using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        // GET: UsuariosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UsuariosController/Create
        public ActionResult Create()
        {   
            return View();
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                Guid id = Guid.NewGuid();
                Usuario usuario = new Usuario();
                usuario.IdUsuario = id;
                usuario.NombreUsuario = collection["NombreUsuario"];
                usuario.PreguntaSeg = collection["PreguntaSeg"];
                usuario.RespuestaSeg = collection["RespuestaSeg"];
                usuario.Dpi = collection["Dpi"];
                usuario.Nombre = collection["Nombre"];
                usuario.Apellido = collection["Apellido"];
                usuario.FechaNacimiento = DateTime.Parse(collection["FechaNacimiento"]);
                usuario.Telefono = Int32.Parse(collection["Telefono"]);
                usuario.Correo = collection["Correo"];
                usuario.EstadoCivil = collection["EstadoCivil"];
                usuario.Profesion = collection["Profesion"];
                usuario.Nacionalidad = collection["Nacionalidad"];
                usuario.Remitido = collection["Remitido"];
                usuario.Antecedentes = collection["Antecedentes"];
                usuario.TipoSange = collection["TipoSange"];
                usuario.NoRegistro = collection["NoRegistro"];
                usuario.Password = collection["Password"];
                usuario.EstadoEliminado = false;
                usuario.UsuarioActivo = true;

              
                _userServices.AddUser(usuario);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsuariosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        public ActionResult Search(string input)
        {
            var idResult = _userServices.SearchUser(input);
            if (idResult == null)
                return View("Index");
            else
                return View(idResult);
        }

        // POST: UsuariosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsuariosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UsuariosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
