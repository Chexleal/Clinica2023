namespace ClinicaInfrastructure;
public class DateManager
{
    private static readonly TimeZoneInfo GuatemalaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");

    public static string GetAge(DateTime dateTime)
    {
        int edad = DateTime.Today.Year - dateTime.Year;
        if (DateTime.Today.Month < dateTime.Month ||
            (DateTime.Today.Month == dateTime.Month && DateTime.Today.Day < dateTime.Day))
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
