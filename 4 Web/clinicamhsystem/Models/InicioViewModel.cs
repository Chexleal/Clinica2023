using ClinicaDomain;

namespace clinicaWeb.Models
{
    public class InicioViewModel
    {
        public int CitasParaHoy { get; set; }
        public int ConsultasAbiertas { get; set; }
        public int ConsultasPendientesPago {  get; set; }
        public List<string> Labels { get; set; }
        public List<int> DataConsultas { get; set; }
        public List<decimal> DataIngresos { get; set; }
    }
}
