namespace ClinicaServices;

public sealed class CurrentUserInfo
{
    public Guid IdUsuario { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;

    /// <summary>Clínica operativa actual (selector por sesión). Null = God viendo todo.</summary>
    public Guid? ClinicaId { get; init; }

    public Guid? HospitalId { get; init; }

    public bool EsSuperAdmin { get; init; }

    /// <summary>God sin clínica seleccionada: omite filtros de tenant.</summary>
    public bool BypassTenant => EsSuperAdmin && ClinicaId is null;

    public IReadOnlyList<Guid> ClinicasPermitidas { get; init; } = Array.Empty<Guid>();
}

public interface ICurrentUser
{
    CurrentUserInfo? Usuario { get; }

    /// <summary>True si el usuario actual puede operar en la clínica indicada.</summary>
    bool PuedeOperarEn(Guid idClinica);
}
