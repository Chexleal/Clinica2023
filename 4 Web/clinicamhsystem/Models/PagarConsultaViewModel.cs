using ClinicaDomain;

namespace clinicamhsystem.Models
{
    public class PagarConsultaViewModel
    {
        public List<Consulta> Consultas { get; set; }
        public List<MotivoCobro> Servicios { get; set; }
        public List<DetalleCobro> Detalles { get; set; }
    }
}
