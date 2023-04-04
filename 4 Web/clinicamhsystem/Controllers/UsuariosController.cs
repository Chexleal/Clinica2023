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
                return View("Index", users);
            }
            else
            {
                var idResult = _userServices.SearchUser(input);
                return View(idResult);
            }
        }

        // GET: UsuariosController/Detalles/fj33-4ra4r
        public ActionResult Detalles(Guid id)
        {
            var user = _userServices.GetUser(id);
            return View(user);
        }

        // GET: UsuariosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Usuario usuario)
        {
            try
            {             
                _userServices.AddUser(usuario);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: UsuariosController/Editar/fj33-4ra4r
        public ActionResult Editar(Guid id)
        {
            var user = _userServices.GetUser(id);
            return View(user);
        }

        // POST: UsuariosController/Editar/fj33-4ra4r
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Usuario usuario)
        {
            try
            {
                //Usuario usuario = new Usuario();
                //usuario.IdUsuario = id;
                //usuario.NombreUsuario = collection["NombreUsuario"];
                //usuario.PreguntaSeg = collection["PreguntaSeg"];
                //usuario.RespuestaSeg = collection["RespuestaSeg"];
                //usuario.Dpi = collection["Dpi"];
                //usuario.Nombre = collection["Nombre"];
                //usuario.Apellido = collection["Apellido"];
                //usuario.FechaNacimiento = DateTime.Parse(collection["FechaNacimiento"]);
                //usuario.Telefono = Int32.Parse(collection["Telefono"]);
                //usuario.Correo = collection["Correo"];
                //usuario.EstadoCivil = collection["EstadoCivil"];
                //usuario.Profesion = collection["Profesion"];
                //usuario.Nacionalidad = collection["Nacionalidad"];
                //usuario.Remitido = collection["Remitido"];
                //usuario.Antecedentes = collection["Antecedentes"];
                //usuario.TipoSange = collection["TipoSange"];
                //usuario.Password = collection["Password"];
                //usuario.NoRegistro = 1;
                //usuario.EstadoEliminado = false;
                //usuario.UsuarioActivo = true;


                _userServices.UpdateUser(usuario);
                return RedirectToAction("Index");
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
            return View("Index",users);
        }
    }
}
