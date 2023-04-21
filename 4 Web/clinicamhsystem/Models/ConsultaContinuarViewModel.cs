using ClinicaDomain;

namespace clinicamhsystem.Models
{
    public class ConsultaContinuarViewModel
    {
        public Consulta Consulta { get; set; }
        public List<Consulta> Consultas { get; set; }
        public Paciente Paciente { get; set; }
    }
}