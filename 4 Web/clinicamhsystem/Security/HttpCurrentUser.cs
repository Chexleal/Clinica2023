using System.Security.Claims;
using ClinicaServices;
using Microsoft.AspNetCore.Http;

namespace clinicaWeb.Security;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUserInfo? Usuario
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var idValue = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(idValue, out var idUsuario)
                ? new CurrentUserInfo
                {
                    IdUsuario = idUsuario,
                    NombreUsuario = principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty
                }
                : null;
        }
    }
}
