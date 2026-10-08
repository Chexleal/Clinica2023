namespace ClinicaDomain;

/// <summary>Marca entidades cuya fila pertenece a una clínica operativa.</summary>
public interface IClinicaTenant
{
    Guid IdClinica { get; set; }
}

/// <summary>Marca catálogos/inventario compartidos a nivel hospital.</summary>
public interface IHospitalTenant
{
    Guid IdHospital { get; set; }
}
