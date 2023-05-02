using ClinicaDomain;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

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
            try
            {

                var country = filterContext.HttpContext.Session.GetString("Country");
                var accountcode = filterContext.HttpContext.Session.GetString("AccountCode");


                var accountData = JsonConvert.DeserializeObject<Usuario>(filterContext.HttpContext.Session.GetString($"UserData({country + "|" + accountcode})"));
                if (!ContainFuncionality(accountData.RolDetalles.ToList(),(RequiredClaim)) )
                    throw new UnauthorizedAccessException();
            }
            catch (Exception e)
            {
                throw new UnauthorizedAccessException();
            }
        }
        
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {}

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {}

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {}


        public static bool ContainFuncionality(List<RolDetalle> functionalities, string funcionalityName)
        {

            if (string.IsNullOrEmpty(funcionalityName))
                return false;

            if (functionalities.Exists(x=>x.Permiso==funcionalityName))
                return true;

            return false;
        }
    }
}
