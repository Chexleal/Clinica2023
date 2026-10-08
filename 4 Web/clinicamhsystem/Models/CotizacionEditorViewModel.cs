using ClinicaDomain;

namespace clinicamhsystem.Models
{
    /// <summary>Editor de cotización: espejo del diseño de cobro (_CobroDetalle) sin pagos.</summary>
    public class CotizacionEditorViewModel
    {
        public Cotizacion? Cotizacion { get; set; }
        public List<CotizacionDetalle>? Detalles { get; set; }
        public List<MotivoCobro>? Servicios { get; set; }
        public List<Producto>? Productos { get; set; }
        public bool Editable { get; set; }
        public string? PacienteNombre { get; set; }
    }
}
