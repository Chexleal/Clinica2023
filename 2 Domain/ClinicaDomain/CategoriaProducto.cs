namespace ClinicaDomain;

public partial class CategoriaProducto : Base
{
    public Guid IdCategoriaProducto { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Bien | Servicio. Solo Bien mueve inventario.</summary>
    public string Tipo { get; set; } = "Bien";

    /// <summary>Valores por defecto al crear un producto en esta categoría. Opcionales, editables por producto.</summary>
    public bool ExigeLoteDefault { get; set; }

    public bool ExigeVencimientoDefault { get; set; }

    public bool Activo { get; set; } = true;

    public void BeforeSaveChanges()
    {
        Nombre ??= string.Empty;
        Tipo ??= "Bien";
    }
}
