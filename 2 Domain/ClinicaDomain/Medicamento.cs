namespace ClinicaDomain;

public class Medicamento : Base, IHospitalTenant
{
    public Guid IdMedicamento {get; set; }

    public Guid IdHospital { get; set; }

    public string Nombre { get; set; }
}
