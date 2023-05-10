using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;
using System.Text;
using System.Web;

namespace clinicaWeb.Controllers
{
    public class ContinuarConsulta : Controller
    {

        private readonly IConsultaServices _consultaServices;
        private readonly IRecetaServices _recetaServices;
        private readonly IPacienteServices _pacienteServices;
        private readonly IConverter _converter;

        public ContinuarConsulta(IConsultaServices consultaServices, IPacienteServices pacienteServices, IRecetaServices recetaServices, IConverter converter)
        {
            _consultaServices = consultaServices;
            _pacienteServices = pacienteServices;
            _recetaServices = recetaServices;
            _converter = converter;
        }


        public ActionResult Index(Guid consultaId)
        {
            var consulta = _consultaServices.GetConsulta(consultaId);
            //var paciente = _pacienteServices.GetPacienteById(consulta.IdPaciente);
            return View(new ConsultaContinuarViewModel { Consulta = consulta/*, Paciente = paciente */});
        }



        // POST: ConsultasController/Create
        [HttpPost]
        public ActionResult Guardar(Consulta consulta)
        {
            var consultaDb = _consultaServices.GetConsulta(consulta.IdConsulta);

            consultaDb.Diagnostico = consulta.Diagnostico;
            consultaDb.MotivoConsulta = consulta.MotivoConsulta;
            consultaDb.Observaciones = consulta.Observaciones;
            consultaDb.Peso = consulta.Peso;
            consultaDb.PresionArterial = consulta.PresionArterial;
            consultaDb.Radiografias = consulta.Radiografias;
            consultaDb.Temperatura = consulta.Temperatura;
            consultaDb.Terminada = consulta.Terminada;

            _consultaServices.UpdateConsulta(consulta);
            if (consulta.Terminada) return RedirectToAction("Index", "Consultas");
            return RedirectToAction("Index", new { consultaId =consulta.IdConsulta});
        }

        [HttpPost]
        public ActionResult GuardarReceta(Receta receta)
        {
            var updReceta = _recetaServices.GetByConsulta(receta.IdConsulta);
            if (updReceta is null)
            {
                var recetaCreate = new Receta()
                {
                    Descripcion = receta.Descripcion,
                    IdConsulta = receta.IdConsulta
                };
                _recetaServices.Create(recetaCreate);
            }
            else
            {
                updReceta.Descripcion = receta.Descripcion;
                _recetaServices.Update(receta);
            }

            return RedirectToAction("Index", new { consultaId = receta.IdConsulta });
        }

        [HttpGet]
        public ActionResult Descargar(Guid consultaId)
        {
            var receta = _recetaServices.GetByConsulta(consultaId);
            if (receta is null)
            {
                //eturn new Rotativa.ViewAsPdf("Index", consultaId);
                return null;
            }
            else
                return null;
        }

        public ActionResult VistaPdf(Guid consultaId)
        {
            if(consultaId == Guid.Empty)
            {

                //Si la consultaId viene vacia, se hace un query al url para obtener el id de la consulta.
                string urlPagina = HttpContext.Request.GetEncodedUrl();
                Uri uri = new Uri(urlPagina);
                string query = uri.Query; // retorna id de la cosnulta
                string consultaIdQ = query.Substring(1); // remueve el signo "?"

                //con el id de la consulta obtenido se parsea a un GUID para usar el servicio de  GetByCosnulta y mostrarlo en el pdf.
                Guid consultaIdQuery = Guid.Parse(consultaIdQ);
                var recetaQ = _recetaServices.GetByConsulta(consultaIdQuery);
                var consulta = _consultaServices.GetConsulta(consultaIdQuery);
                var pacienteInfo = _pacienteServices.GetPacienteById(consulta.IdPaciente);

                //se declaran variables en ViewData para llenar la informacion en el pdf
                ViewData["fecha"] = recetaQ.Fecha.ToString();
                ViewData["Paciente"] = pacienteInfo.Nombre + " " + pacienteInfo.Apellido;
                ViewData["detalleReceta"] = recetaQ.Descripcion.ToString();
                return View("ConsultaPdf");
            }
            else
            {
                var receta = _recetaServices.GetByConsulta(consultaId);

                var consulta = _consultaServices.GetConsulta(consultaId);
                var pacienteInfo = _pacienteServices.GetPacienteById(consulta.IdPaciente);
                ViewData["Paciente"] = pacienteInfo.Nombre + " " + pacienteInfo.Apellido;
                ViewData["fecha"] = receta.Fecha.ToString();
                ViewData["detalleReceta"] = receta.Descripcion.ToString();
                return View("ConsultaPdf");
            }
        }

        public ActionResult PdfPage()
        {

            string paginaActual = HttpContext.Request.Path;
            string urlPagina = HttpContext.Request.GetEncodedUrl();
            urlPagina = urlPagina.Replace(paginaActual, "");
            urlPagina = $"{urlPagina}/ContinuarConsulta/VistaPdf";

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    PaperSize = PaperKind.A5,
                    Orientation = Orientation.Portrait
                },
                Objects =
                {
                    new ObjectSettings()
                    {
                        Page = urlPagina
                    }
                }

            };

            var archivoPdf = _converter.Convert(pdf);

            return File(archivoPdf, "aplication/pdf");
        }

        public ActionResult DescargarPdf(Guid consultaIds)
        {

            string paginaActual = HttpContext.Request.Path;

            string urlPagina = HttpContext.Request.GetEncodedUrl();


            Uri uri = new Uri(urlPagina);
            string queryString = uri.Query;
            string consultaId = HttpUtility.ParseQueryString(queryString).Get("consultaId");
            string baseUrl = uri.OriginalString.Substring(0, uri.OriginalString.IndexOf("?"));

            string path = uri.LocalPath;
            string controller = path.Split('/')[1];
            string action = path.Split('/')[2];

            string originUrl = uri.GetLeftPart(UriPartial.Authority);

            StringBuilder newUrlBuild = new StringBuilder(originUrl);
            newUrlBuild.Append("/");
            newUrlBuild.Append(controller);
            newUrlBuild.Append("/VistaPdf?");
            newUrlBuild.Append(consultaId);

            urlPagina = newUrlBuild.ToString();

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait

                },
                Objects =
                {
                    new ObjectSettings()
                    {
                        Page = urlPagina // "https://localhost:7070?consultaId=966967ee-49ab-4d85-bc5a-2826deb75257"	

                    }
                }

            };

            string url = urlPagina.ToString();
            var archivoPdf = _converter.Convert(pdf);
            string nombrePDf = "receta_"+ DateTime.Now.ToString("ddMMyyyy")+".pdf";

            return File(archivoPdf, "aplication/pdf",nombrePDf);
        }



        // GET: ConsultasController/Edit/5
        public ActionResult Editar(Guid id)
        {
            var consultas = _consultaServices.GetConsulta(id);
            return RedirectToAction("Editar", consultas);
        }

        // POST: ConsultasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Consulta consulta)
        {
            try
            {
                _consultaServices.UpdateConsulta(consulta);
                var consultas = _consultaServices.GetAll();
                return RedirectToAction("Index", consultas);
            }
            catch
            {
                return View("Error");
            }
        }


    }
}
