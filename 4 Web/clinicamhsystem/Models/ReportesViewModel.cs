using ClinicaDomain;

namespace clinicaWeb.Models
{
    public class ReportesViewModel
    {
        public List<Consulta> Consultas { get; set; }
        public List<MotivoCobro> Servicios { get; set; }
        public List<Paciente> Pacientes { get; set; }

        public MotivoCobro Servicio { get; set; }
    }
}
