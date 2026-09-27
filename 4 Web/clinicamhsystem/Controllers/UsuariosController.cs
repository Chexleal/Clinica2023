using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;
[SecurityFilter("Usuarios")]
public class UsuariosController(IUserServices userServices) : ErrorHandlingController
{

    private static List<string> permisos = new()
    {
        "SuperAdmin",
        "Usuarios",
        "Citas",
        "Consultas",
        "ContinuarConsulta",
        "Pacientes",
        "Pagos",
        "Gastos",
        "Reportes",
        "Servicios",
        "Configuraciones"
    };

    // GET: UsuariosController
    public ActionResult Index()
    {
        var users = userServices.GetAll();
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
            userServices.AddUser(usuario, permissionsList);

        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return RedirectToAction("Index");
    }

    // POST: UsuariosController/Editar/fj33-4ra4r
    [HttpPost]
    public ActionResult Editar(Usuario usuario, List<string> permissionsListEdit)
    {
        try
        {
            userServices.UpdateUser(usuario, permissionsListEdit);
            var users = userServices.GetAll();
            return RedirectToAction("Index", users);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            return View("Error");
        }
    }

    // POST: UsuariosController/Eliminar/5
    [HttpPost]
    public ActionResult Eliminar(Guid id)
    {
        try
        {
            userServices.DeleteUser(id);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        var users = userServices.GetAll();
        return RedirectToAction("Index",users);
    }

    // POST: UsuariosController/Active/5
    [HttpPost]
    public ActionResult Activate(Guid id, bool state)
    {
        try
        {
            userServices.SetActive(id,state);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        var users = userServices.GetAll();
        return RedirectToAction("Index", users);
    }

    [HttpGet]
    public IActionResult GetUsuario(Guid usuarioId)
    {
        var usuario = userServices.GetUser(usuarioId);
        usuario.Permisos = userServices.GetPermissions(usuario.IdUsuario);
        return PartialView("Editar", new UsuariosViewModel { Usuario = usuario, Permisos = permisos }  );
    }
}
