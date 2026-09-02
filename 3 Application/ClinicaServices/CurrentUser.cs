namespace ClinicaServices;

public sealed class CurrentUserInfo
{
    public Guid IdUsuario { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;
}

public interface ICurrentUser
{
    CurrentUserInfo? Usuario { get; }
}
