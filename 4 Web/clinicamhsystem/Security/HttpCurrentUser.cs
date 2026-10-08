using System.Security.Claims;
using ClinicaServices;
using Microsoft.AspNetCore.Http;

namespace clinicaWeb.Security;

public static class TenantClaimTypes
{
    public const string ClinicaId = "clinica_id";
    public const string HospitalId = "hospital_id";
    public const string ClinicaAccess = "clinica_access";
}

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

            if (!Guid.TryParse(idValue, out var idUsuario))
                return null;

            Guid? clinicaId = null;
            var clinicaRaw = principal?.FindFirstValue(TenantClaimTypes.ClinicaId);
            if (Guid.TryParse(clinicaRaw, out var cId))
                clinicaId = cId;

            Guid? hospitalId = null;
            var hospRaw = principal?.FindFirstValue(TenantClaimTypes.HospitalId);
            if (Guid.TryParse(hospRaw, out var hId))
                hospitalId = hId;

            var accesos = principal?.FindAll(TenantClaimTypes.ClinicaAccess)
                .Select(x => Guid.TryParse(x.Value, out var g) ? (Guid?)g : null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<Guid>();

            return new CurrentUserInfo
            {
                IdUsuario = idUsuario,
                NombreUsuario = principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                ClinicaId = clinicaId,
                HospitalId = hospitalId,
                EsSuperAdmin = principal?.IsInRole("SuperAdmin") ?? false,
                ClinicasPermitidas = accesos
            };
        }
    }

    public bool PuedeOperarEn(Guid idClinica)
    {
        var u = Usuario;
        if (u is null) return false;
        if (u.EsSuperAdmin) return true;
        return u.ClinicasPermitidas.Contains(idClinica);
    }
}
