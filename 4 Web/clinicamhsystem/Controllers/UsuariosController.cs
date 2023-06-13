using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;
[SecurityFilter("Usuarios")]
public class UsuariosController : Controller
{
    private readonly IUserServices _userServices;
    private static List<string> permisos = new()
    {
        "SuperAdmin",
        "Usuarios",
        "Citas",
        "Consultas",
        "ContinuarConsulta",
        "Pacientes",
        "Pagos",
        "Reportes",
        "Servicios"
    };

    public UsuariosController(IUserServices userServices)
    {
        _userServices = userServices;
    }

    // GET: UsuariosController
    public ActionResult Index()
    {
        var users = _userServices.GetAll();
        return View(new UsuariosViewModel { Usuarios= users,Permisos=permisos } );
    }

    // GET: UsuariosController/Create
    public ActionResult Create()
    {
        return View("Create");
    }

    // POST: UsuariosController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Usuario usuario, List<string> permissionsList)
    {
        try
        {             
            _userServices.AddUser(usuario, permissionsList);

        }
        catch
        {
        }
        return RedirectToAction("Index");
    }

    // POST: UsuariosController/Editar/fj33-4ra4r
    [HttpPost]
    public ActionResult Editar(Usuario usuario, List<string> permissionsListEdit)
    {
        try
        {
            _userServices.UpdateUser(usuario, permissionsListEdit);
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

    [HttpGet]
    public IActionResult GetUsuario(Guid usuarioId)
    {
        var usuario = _userServices.GetUser(usuarioId);
        usuario.Permisos = _userServices.GetPermissions(usuario.IdUsuario);
        return PartialView("Editar", new UsuariosViewModel { Usuario = usuario, Permisos = permisos }  );
    }
}
