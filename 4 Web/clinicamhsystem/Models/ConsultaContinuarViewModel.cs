using ClinicaDomain;

namespace clinicamhsystem.Models
{
    public class ConsultaContinuarViewModel
    {
        public Consulta Consulta { get; set; }
        public Receta Receta { get; set; }
        public List<DetalleReceta> DetallesReceta { get; set; }
        public List<Medicamento> Medicamentos { get; set; }
    }
}