namespace ClinicaInfrastructure;
public class DateManager
{
    private static readonly TimeZoneInfo GuatemalaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");

    public static string GetAge(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return string.Empty;
        }

        var fechaNacimiento = dateTime.Value;
        int edad = DateTime.Today.Year - fechaNacimiento.Year;
        if (DateTime.Today.Month < fechaNacimiento.Month ||
            (DateTime.Today.Month == fechaNacimiento.Month && DateTime.Today.Day < fechaNacimiento.Day))
        {
            edad--;
        }
        return edad.ToString();
    }

    public static DateTime GetDisplayDate(DateTime? fechaCreacionUtc, DateTime fechaLocal)
    {
        if (fechaCreacionUtc is null)
        {
            return fechaLocal;
        }

        var utcDate = DateTime.SpecifyKind(fechaCreacionUtc.Value, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, GuatemalaTimeZone);
    }

}
