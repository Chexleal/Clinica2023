using ClinicaDomain;

namespace clinicamhsystem.Models
{
    public class DetallesPagarViewModel
    {
        public Consulta? consulta { get; set; }
        public List<MotivoCobro>? Servicios { get; set; }
        public List<DetalleCobro>? Detalles { get; set; }
    }
}
