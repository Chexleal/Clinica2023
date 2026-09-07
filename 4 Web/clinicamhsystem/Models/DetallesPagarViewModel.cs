using ClinicaDomain;

namespace clinicamhsystem.Models
{
    public class DetallesPagarViewModel
    {
        public Consulta? consulta { get; set; }
        public List<MotivoCobro>? Servicios { get; set; }
        public List<DetalleCobro>? Detalles { get; set; }
        // Nuevo modelo de cobro unificado (vía consulta + venta libre)
        public Venta? Venta { get; set; }
        public List<VentaDetalle>? VentaDetalles { get; set; }
        public List<Producto>? Productos { get; set; }
        // Tipos de pago de la cuenta
        public List<MetodoPago>? Metodos { get; set; }
        public List<VentaPago>? Pagos { get; set; }
    }
}
