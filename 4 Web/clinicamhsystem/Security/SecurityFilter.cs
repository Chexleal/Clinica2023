using ClinicaServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace clinicaWeb.Security
{
    public class SecurityFilter: ActionFilterAttribute
    {
        public SecurityFilter(string requiredClaim)
        {
            RequiredClaim = requiredClaim;

        }

        public string RequiredClaim { get; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var currentUser = filterContext.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
            if (currentUser.Usuario is null)
            {
                filterContext.Result = new RedirectToActionResult("NoAutorizado", "Home", null);
                return;
            }

            if (RequiredClaim != "Inicio"
                && !filterContext.HttpContext.User.IsInRole(RequiredClaim)
                && !filterContext.HttpContext.User.IsInRole("SuperAdmin"))
            {
                filterContext.Result = new RedirectToActionResult("NoAutorizado", "Home", null);
            }
        }
        
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {}

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {}

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {}

    }
}
