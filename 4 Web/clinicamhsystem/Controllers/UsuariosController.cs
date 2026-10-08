using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;
[SecurityFilter("Usuarios")]
public class UsuariosController(
    IUserServices userServices,
    IClinicaAdminService clinicaAdminService,
    ICurrentUser currentUser) : ErrorHandlingController
{

    private static List<string> permisos = new()
    {
        "SuperAdmin",
        "Administrador",
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

    // Solo SuperAdmin puede ver/otorgar SuperAdmin. Administrador no lo ve en la lista.
    private List<string> PermisosVisibles =>
        currentUser.Usuario?.EsSuperAdmin == true ? permisos : permisos.Where(p => p != "SuperAdmin").ToList();

    private List<Clinica> ClinicasVisibles =>
        currentUser.Usuario?.EsSuperAdmin == true
            ? clinicaAdminService.GetClinicas(soloActivas: true)
            : clinicaAdminService.GetClinicas(soloActivas: true)
                .Where(c => c.IdClinica == currentUser.Usuario?.ClinicaId)
                .ToList();

    // GET: UsuariosController
    public ActionResult Index()
    {
        var users = UsuariosVisibles();
        ViewBag.Clinicas = currentUser.Usuario?.EsSuperAdmin == true
            ? clinicaAdminService.GetClinicas(soloActivas: true)
            : clinicaAdminService.GetClinicas(soloActivas: true)
                .Where(c => c.IdClinica == currentUser.Usuario?.ClinicaId).ToList();
        return View(new UsuariosViewModel { Usuarios = users, Permisos = PermisosVisibles });
    }

    private List<Usuario> UsuariosVisibles()
    {
        var todos = userServices.GetAll();
        if (currentUser.Usuario?.EsSuperAdmin == true)
            return todos;

        var clinicaId = currentUser.Usuario?.ClinicaId;
        if (clinicaId is null) return new List<Usuario>();
        var idsEnMiClinica = userServices.GetUsuarioIdsEnClinica(clinicaId.Value);
        return todos.Where(u => idsEnMiClinica.Contains(u.IdUsuario)).ToList();
    }

    // GET: UsuariosController/Create
    public ActionResult Create()
    {
        ViewBag.Clinicas = currentUser.Usuario?.EsSuperAdmin == true
            ? clinicaAdminService.GetClinicas(soloActivas: true)
            : clinicaAdminService.GetClinicas(soloActivas: true)
                .Where(c => c.IdClinica == currentUser.Usuario?.ClinicaId).ToList();
        ViewBag.Permisos = PermisosVisibles;
        return View("Create");
    }

    // POST: UsuariosController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Usuario usuario, List<string> permissionsList, List<Guid>? clinicaIds)
    {
        try
        {
            // Administrador no puede crear SuperAdmin.
            if (currentUser.Usuario?.EsSuperAdmin != true)
                permissionsList = (permissionsList ?? new()).Where(p => p != "SuperAdmin").ToList();

            userServices.AddUser(usuario, permissionsList ?? new());

            var destino = (clinicaIds is { Count: > 0 })
                ? clinicaIds
                : (currentUser.Usuario?.ClinicaId.HasValue == true
                    ? new List<Guid> { currentUser.Usuario.ClinicaId.Value }
                    : new List<Guid>());

            if (currentUser.Usuario?.EsSuperAdmin != true && currentUser.Usuario?.ClinicaId.HasValue == true)
                destino = new List<Guid> { currentUser.Usuario.ClinicaId.Value };

            userServices.AsignarClinicas(usuario.IdUsuario, destino, destino.FirstOrDefault() is Guid d && d != Guid.Empty ? d : null);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return RedirectToAction("Index");
    }

    // POST: UsuariosController/Editar/fj33-4ra4r
    [HttpPost]
    public ActionResult Editar(Usuario usuario, List<string> permissionsListEdit, List<Guid>? clinicaIds)
    {
        try
        {
            if (currentUser.Usuario?.EsSuperAdmin != true)
                permissionsListEdit = (permissionsListEdit ?? new()).Where(p => p != "SuperAdmin").ToList();

            userServices.UpdateUser(usuario, permissionsListEdit ?? new());

            // Solo SuperAdmin reasigna clínicas; el Administrador no saca usuarios de su clínica.
            if (currentUser.Usuario?.EsSuperAdmin == true && clinicaIds is { Count: > 0 })
            {
                var actualDefault = userServices.GetClinicasDeUsuario(usuario.IdUsuario)
                    .FirstOrDefault(a => a.EsDefault)?.IdClinica;
                userServices.AsignarClinicas(usuario.IdUsuario, clinicaIds, actualDefault ?? clinicaIds.First());
            }
            var users = UsuariosVisibles();
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
            // No permitir que un Administrador elimine a un SuperAdmin.
            var objetivo = userServices.GetUser(id);
            var esObjetivoSuper = objetivo is not null && userServices.GetPermissions(id).Any(p => p.Permiso == "SuperAdmin");
            if (esObjetivoSuper && currentUser.Usuario?.EsSuperAdmin != true)
                return RedirectToAction("Index", UsuariosVisibles());

            userServices.DeleteUser(id);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        var users = UsuariosVisibles();
        return RedirectToAction("Index", users);
    }

    // POST: UsuariosController/Active/5
    [HttpPost]
    public ActionResult Activate(Guid id, bool state)
    {
        try
        {
            userServices.SetActive(id, state);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        var users = UsuariosVisibles();
        return RedirectToAction("Index", users);
    }

    [HttpGet]
    public IActionResult GetUsuario(Guid usuarioId)
    {
        var usuario = userServices.GetUser(usuarioId);
        if (usuario is null) return NotFound();
        usuario.Permisos = userServices.GetPermissions(usuario.IdUsuario);
        ViewBag.ClinicasAsignadas = userServices.GetClinicaIdsDeUsuario(usuario.IdUsuario);
        ViewBag.Clinicas = currentUser.Usuario?.EsSuperAdmin == true
            ? clinicaAdminService.GetClinicas(soloActivas: true)
            : clinicaAdminService.GetClinicas(soloActivas: true)
                .Where(c => c.IdClinica == currentUser.Usuario?.ClinicaId).ToList();
        return PartialView("Editar", new UsuariosViewModel { Usuario = usuario, Permisos = PermisosVisibles });
    }
}
