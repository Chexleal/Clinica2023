using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    [SecurityFilter("Inicio")]
    public class InicioController(IConsultaServices consultaServices, ICitaServices citaServices) : Controller
    {

        public IActionResult Index()
        {
            var citasParaHoy = citaServices.CountForToday();
            var consultasAbiertas = consultaServices.CountOpen();
            var consultasPendientesPago = consultaServices.CountNotPaid();

            //    var graficaConsultas = new List<ChartData>
            //{
            //    new ChartData { Label = DateTime.Today.AddMonths(-2).Month.ToString() , Data = new List<int> { 10, 20, 30 } },
            //    new ChartData { Label = DateTime.Today.AddMonths(-1).Month.ToString(), Data = new List<int> { 40, 50, 60 } },
            //    new ChartData { Label = DateTime.Today.Month.ToString(), Data = new List<int> { 40, 50, 60 } }
            //    // Añade más datos según sea necesario
            //};

            var labels = new List<string>
            {Mes(DateTime.Today.AddMonths(-2).Month),
            Mes(DateTime.Today.AddMonths(-1).Month),
            Mes(DateTime.Today.Month)
            };

            var dataConsultas = new List<int>
            {
                consultaServices.CountByMonth(DateTime.Today.AddMonths(-2).Month),
                consultaServices.CountByMonth(DateTime.Today.AddMonths(-1).Month),
                consultaServices.CountByMonth(DateTime.Today.Month)
            };

            var dataIngresos = new List<decimal>
            {
                consultaServices.SumPaidByMonth(DateTime.Today.AddMonths(-2).Month),
                consultaServices.SumPaidByMonth(DateTime.Today.AddMonths(-1).Month),
                consultaServices.SumPaidByMonth(DateTime.Today.Month)
            };

            //DataIngresos


            return View(new InicioViewModel { CitasParaHoy = citasParaHoy, ConsultasAbiertas = consultasAbiertas, ConsultasPendientesPago = consultasPendientesPago, Labels = labels, DataConsultas = dataConsultas, DataIngresos = dataIngresos });
        }

        private string Mes(int month)
        {
            switch (month)
            {
                case 1: return "Enero";
                case 2: return "Febrero";
                case 3: return "Marzo";
                case 4: return "Abril";
                case 5: return "Mayo";
                case 6: return "Junio";
                case 7: return "Julio";
                case 8: return "Agosto";
                case 9: return "Septiembre";
                case 10: return "Octubre";
                case 11: return "Noviembre";
                case 12: return "Diciembre";
                default: return "NA";
            }
        }
    }
}
