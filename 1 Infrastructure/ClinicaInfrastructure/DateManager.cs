namespace ClinicaInfrastructure;
public class DateManager
{
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

}
