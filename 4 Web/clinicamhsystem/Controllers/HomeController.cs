using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using clinicaWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ServiceStack.Script;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Diagnostics;

namespace clinicamhsystem.Controllers;
    public class HomeController(
        IUserServices userServices,
        ICurrentUser currentUser,
        IClinicaAdminService clinicaAdminService,
        ClinicaContext dbContext,
        IErrorLogService errorLogService) : Controller
    {

    public IActionResult Index()
    {
        //var users = userServices.GetAll();
        return View();
    }

    [HttpGet]
    public IActionResult NoAutorizado()
    {
        return View();
    }

    public async Task<IActionResult> LogInAsync(string password, string user)
    {

        var existingUser = userServices.Authenticate(user, password);
        //Usuario existingUser = new() { Nombre = "Dev", Apellido = "Test"};
        //existingUser.IdUsuario = new Guid();
        if (existingUser is not null)
        {
            existingUser.Permisos = userServices.GetPermissions(existingUser.IdUsuario);
            existingUser.Permisos ??= new();
            var esSuper = existingUser.Permisos.Any(p => p.Permiso == "SuperAdmin");

            // Clínicas accesibles: SuperAdmin ve todas; resto solo sus asignaciones.
            List<Clinica> clinicas;
            if (esSuper)
            {
                clinicas = clinicaAdminService.GetClinicas(soloActivas: true);
            }
            else
            {
                var ids = userServices.GetClinicaIdsDeUsuario(existingUser.IdUsuario);
                clinicas = ids.Count == 0
                    ? new List<Clinica>()
                    : clinicaAdminService.GetClinicas(soloActivas: true).Where(c => ids.Contains(c.IdClinica)).ToList();
            }

            // Default: la marcada EsDefault, si no la primera; God sin clínicas = vista global.
            var accesos = userServices.GetClinicasDeUsuario(existingUser.IdUsuario);
            var defaultId = accesos.FirstOrDefault(a => a.EsDefault)?.IdClinica
                ?? clinicas.FirstOrDefault()?.IdClinica;
            if (!esSuper && defaultId is null)
            {
                TempData["Error"] = "Usuario sin clínicas asignadas. Pide a un administrador que te asigne una.";
                return RedirectToAction("Index");
            }
            if (esSuper && defaultId is null && clinicas.Count > 0)
                defaultId = clinicas.First().IdClinica;

            var clinicaDefault = defaultId.HasValue ? clinicas.FirstOrDefault(c => c.IdClinica == defaultId.Value) : null;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, existingUser.IdUsuario.ToString()),
                new(ClaimTypes.Name, existingUser.NombreUsuario)
            };
            claims.AddRange(existingUser.Permisos.Select(permission => new Claim(ClaimTypes.Role, permission.Permiso)));
            foreach (var c in clinicas)
                claims.Add(new Claim(clinicaWeb.Security.TenantClaimTypes.ClinicaAccess, c.IdClinica.ToString()));
            if (defaultId.HasValue)
            {
                claims.Add(new Claim(clinicaWeb.Security.TenantClaimTypes.ClinicaId, defaultId.Value.ToString()));
                var hospId = clinicaDefault?.IdHospital;
                if (hospId.HasValue)
                    claims.Add(new Claim(clinicaWeb.Security.TenantClaimTypes.HospitalId, hospId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            TempData["UsuarioNombre"] = $"{existingUser.Nombre} {existingUser.Apellido}";
            return RedirectToAction("Index", "Inicio");
        }
        else
        {
            TempData["Error"] = "Usuario o contraseña incorrectos";
            return RedirectToAction("Index");
        }
    }

    public IActionResult UsuarioExistente(string userName)
    {
        var existingUser = userServices.CheckUserExist(userName);
        if (existingUser)
        {
            var userSecureQuestion = ViewData["securityQuestion"];
            var userEmail = ViewData["userEmail"];

            Usuario? userInfo = userServices.GetUserByName(userName);
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
        string? preguntaSegCheck = userServices.CheckAnswer(answer);
        if (preguntaSegCheck != null)
        {
            TempData["Success"] = "Respuesta Correcta";
            return View("NewPassword", hiddenUsername);
        }else
        {
            TempData["Error"] = "Respuesta Incorrecta";
            Usuario? userInfo = userServices.GetUserByName(hiddenUsername);  
            var userSecureQuestion = userInfo.PreguntaSeg.ToString(); // usa el servicio para recuperar la pregunta de seguridad del usuario, y se la asigna a un ViewData
            var userEmail = userInfo.Correo.ToString();
            object[] userInformation = { hiddenUsername, userSecureQuestion, userEmail };
            return View("RecoverAccount", userInformation);
        }
            
    }

    public IActionResult CheckEmails(string email, string emailConfirmed, string hiddenUsername)
    {
        var userEmail = ViewData["userEmail"];

        Usuario? userInfo = userServices.GetUserByName(hiddenUsername);
        ViewData["username"] = userInfo.NombreUsuario.ToString();
        userEmail = userInfo.Correo.ToString();

        string[] emailSplited = userInfo.Correo.ToString().Split('@');
        string beforeArroba = emailSplited[0];
        var emailCensored = string.Concat(Enumerable.Repeat("*", 3)) + beforeArroba.Substring(3);
        var userEmailCesored = emailCensored.ToString() + "@" + emailSplited[1].ToString();

        //List<object> userInformation = new List<object> { userName, userSecureQuestion, userEmail };
        object[] userInformation = { hiddenUsername, userEmailCesored };


        bool emailCheck = userServices.CheckEmails(email, emailConfirmed, hiddenUsername);
        if (emailCheck)
        {
            TempData["Success"] = "Correo enviado";
            return View("Index");
        }
        else
        {
            TempData["Error"] = "Email Incorrecto";
            return View("RecoverAccountEmail", hiddenUsername);
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
        Usuario? userInfo = userServices.GetUserByName(usModel);
        bool checkPassword = userServices.CheckNewPassword(newPassword, newPasswordConfirmed, userInfo.IdUsuario);
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
        return View("RecoverAccountEmail", userName);
    }

    [HttpGet]
    public IActionResult ChangePassWord()
    {
        return PartialView("_ChangePassword");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ChangePassWord(String Password)
    {
        try
        {
            if (currentUser.Usuario is { } usuarioActual)
            {
                userServices.ChangePassword(usuarioActual.IdUsuario, Password);
            }
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            var errorLogId = await errorLogService.RegistrarAsync(ex, "Controlado", Request.Path, Request.Method, HttpContext.TraceIdentifier);
            return View("Error", new ErrorViewModel { ErrorLogId = errorLogId, RequestId = HttpContext.TraceIdentifier });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CerrarSesion()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            var errorLogId = await errorLogService.RegistrarAsync(ex, "Controlado", Request.Path, Request.Method, HttpContext.TraceIdentifier);
            return View("Error", new ErrorViewModel { ErrorLogId = errorLogId, RequestId = HttpContext.TraceIdentifier });
        }
    }

    /// <summary>Selector de clínica del topbar. God pasa Guid.Empty para "Todas".</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> CambiarClinica(Guid idClinica)
    {
        var usuario = currentUser.Usuario;
        if (usuario is null) return RedirectToAction("Index");

        var permisos = userServices.GetPermissions(usuario.IdUsuario);
        var esSuper = permisos.Any(p => p.Permiso == "SuperAdmin");

        List<Clinica> permitidas = esSuper
            ? clinicaAdminService.GetClinicas(soloActivas: true)
            : clinicaAdminService.GetClinicas(soloActivas: true)
                .Where(c => userServices.GetClinicaIdsDeUsuario(usuario.IdUsuario).Contains(c.IdClinica)).ToList();

        // God puede elegir "Todas" (Guid.Empty) para vista global.
        if (idClinica != Guid.Empty && !esSuper && !permitidas.Any(c => c.IdClinica == idClinica))
            return RedirectToAction("NoAutorizado");

        var identity = (ClaimsIdentity)User.Identity!;
        void ReemplazarClaim(string tipo, string? valor)
        {
            var existente = identity.FindFirst(tipo);
            if (existente is not null) identity.RemoveClaim(existente);
            if (valor is not null) identity.AddClaim(new Claim(tipo, valor));
        }

        foreach (var viejo in identity.FindAll(clinicaWeb.Security.TenantClaimTypes.ClinicaAccess).ToList())
            identity.RemoveClaim(viejo);
        foreach (var c in permitidas)
            identity.AddClaim(new Claim(clinicaWeb.Security.TenantClaimTypes.ClinicaAccess, c.IdClinica.ToString()));

        if (idClinica == Guid.Empty)
        {
            ReemplazarClaim(clinicaWeb.Security.TenantClaimTypes.ClinicaId, null);
            ReemplazarClaim(clinicaWeb.Security.TenantClaimTypes.HospitalId, null);
        }
        else
        {
            var elegida = permitidas.FirstOrDefault(c => c.IdClinica == idClinica) ?? clinicaAdminService.GetClinica(idClinica);
            if (elegida is null) return RedirectToAction("NoAutorizado");
            ReemplazarClaim(clinicaWeb.Security.TenantClaimTypes.ClinicaId, elegida.IdClinica.ToString());
            ReemplazarClaim(clinicaWeb.Security.TenantClaimTypes.HospitalId, elegida.IdHospital.ToString());
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return RedirectToAction("Index", "Inicio");
    }

    [HttpGet]
    public IActionResult MisClinicas()
    {
        var usuario = currentUser.Usuario;
        if (usuario is null) return Unauthorized();
        var esSuper = User.IsInRole("SuperAdmin");
        var lista = esSuper
            ? clinicaAdminService.GetClinicas(soloActivas: true)
            : clinicaAdminService.GetClinicas(soloActivas: true)
                .Where(c => userServices.GetClinicaIdsDeUsuario(usuario.IdUsuario).Contains(c.IdClinica)).ToList();
        return Json(new
        {
            actual = usuario.ClinicaId,
            esSuper,
            clinicas = lista.Select(c => new { id = c.IdClinica, nombre = c.Nombre, hospital = c.Hospital?.Nombre ?? "" })
        });
    }

    public ActionResult Editar(Guid id)
    {
        var user = userServices.GetUser(id);
        return View("Editar", user);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Error()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature?.Error is { } exception)
        {
            var errorLogId = await errorLogService.RegistrarAsync(
                exception,
                "No controlado",
                exceptionFeature.Path,
                Request.Method,
                HttpContext.TraceIdentifier);

            return View(new ErrorViewModel { ErrorLogId = errorLogId, RequestId = HttpContext.TraceIdentifier });
        }

        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
