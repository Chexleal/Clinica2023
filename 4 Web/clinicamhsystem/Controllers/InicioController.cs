using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers
{
    [SecurityFilter("Inicio")]
    public class InicioController : Controller
    {
        private readonly IConsultaServices _consultaServices;
        private readonly ICitaServices _citaServices;

        public InicioController(IConsultaServices consultaServices, ICitaServices citaServices)
        {
            _consultaServices = consultaServices;
            _citaServices = citaServices;
        }

        public IActionResult Index()
        {
            var citasParaHoy = _citaServices.CountForToday();
            var consultasAbiertas = _consultaServices.CountOpen();
            var consultasPendientesPago = _consultaServices.CountNotPaid();

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
                _consultaServices.CountByMonth(DateTime.Today.AddMonths(-2).Month),
                _consultaServices.CountByMonth(DateTime.Today.AddMonths(-1).Month),
                _consultaServices.CountByMonth(DateTime.Today.Month)
            };

            var dataIngresos = new List<decimal>
            {
                _consultaServices.SumPaidByMonth(DateTime.Today.AddMonths(-2).Month),
                _consultaServices.SumPaidByMonth(DateTime.Today.AddMonths(-1).Month),
                _consultaServices.SumPaidByMonth(DateTime.Today.Month)
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
