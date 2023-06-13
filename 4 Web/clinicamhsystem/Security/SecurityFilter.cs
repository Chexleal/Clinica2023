using ClinicaDomain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using ServiceStack;

namespace clinicaWeb.Security
{
    public class SecurityFilter: ActionFilterAttribute
    {
        public SecurityFilter(string requiredClaim)
        {
            RequiredClaim = requiredClaim;

        }

        public string RequiredClaim { get; }

        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var _cache = (IMemoryCache)filterContext.HttpContext.RequestServices.GetService(typeof(IMemoryCache));

                if (_cache.TryGetValue("UserData", out string jsonUserData))
                {
                   // string jsonUserData = await _cache.GetStringAsync("UserData");

                    Usuario userData = JsonConvert.DeserializeObject<Usuario>(jsonUserData);
                    // var accountData = JsonConvert.DeserializeObject<Usuario>(filterContext.HttpContext.Session.GetString($"UserData({country + "|" + accountcode})"));
                    if (!ContainFuncionality(userData.Permisos.ToList() ?? new(), (RequiredClaim)))
                        throw new UnauthorizedAccessException();
                }
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
            return true;
            //if (functionalities.Exists(x=>x.Permiso=="SuperAdmin"))
            //    return true;
            //if (string.IsNullOrEmpty(funcionalityName))
            //    return false;

            //if (functionalities.Exists(x => x.Permiso == funcionalityName))
            //    return true;

            //return false;
        }
    }
}
