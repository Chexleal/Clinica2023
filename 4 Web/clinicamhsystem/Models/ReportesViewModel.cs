using ClinicaDomain;

namespace clinicaWeb.Models
{
    public class ReportesViewModel
    {
        public List<Consulta> Consultas { get; set; }
        public List<MotivoCobro> Servicios { get; set; }
        public List<Paciente> Pacientes { get; set; }
        public List<DetalleCobro> Detalles { get; set; }

        public MotivoCobro Servicio { get; set; }
        public Paciente Paciente { get; set; }
        public bool EsServicio { get;  set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        /// <summary>Por Servicios: incluir filas sin movimientos en el rango. Default apagado.</summary>
        public bool IncluirSinIngresos { get; set; }

        /// <summary>Filtro del reporte Por Servicios: todos|servicios|productos.</summary>
        public string TipoFiltro { get; set; } = "todos";

        /// <summary>Por Paciente sin paciente elegido: un resumen por cada paciente del rango.</summary>
        public bool VerTodosPacientes { get; set; }
    }
}
