using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using ClinicaInfrastructure;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;
[SecurityFilter("Consultas")]
public class ConsultasController(IConsultaServices consultaServices, IPacienteServices pacienteServices) : ErrorHandlingController
{
    private string inhtmlPath = "C:\\Users\\futjo\\source\\repos\\ClinicaProject\\4 Web\\clinicamhsystem\\Views\\Consultas\\consultaBase.html";
    private string toPdfPath = "C:\\Users\\futjo\\OneDrive\\Receta.pdf";


    // GET: UsuariosController
    public ActionResult Index()
    {
        var pacientes = pacienteServices.GetAll();
        return View(new ConsultasViewModel { Consultas = new(), Pacientes = pacientes });
    }

    [HttpPost]
    public IActionResult GetConsultasTable(DataTableRequest request)
    {
        var result = consultaServices.GetPaginatedOpen(request.Start, request.Length, request.SearchValue, request.SortColumn, request.SortDir);

        var data = result.Data.Select(c => new
        {
            c.IdConsulta,
            Fecha = DateManager.GetDisplayDate(c.FechaCreacion, c.Fecha).ToString("dd/MM/yyyy HH:mm"),
            PacienteNombre = c.PacienteInformacion?.Nombre,
            PacienteApellido = c.PacienteInformacion?.Apellido,
            c.MotivoConsulta,
            CreadoPor = !string.IsNullOrWhiteSpace(c.CreadoPorNombre) ? c.CreadoPorNombre
                : c.CreadoPor.HasValue ? c.CreadoPor.Value.ToString().Substring(0, 8) : "—"
        });

        return Json(new DataTableResponse<object>
        {
            Draw = request.Draw,
            RecordsTotal = result.Total,
            RecordsFiltered = result.TotalFiltered,
            Data = data
        });
    }

    /*
    //GET: Usuarios/Search? input = t
    public ActionResult Search(string input)
    {
        if (String.IsNullOrEmpty(input))
        {
            var consultas = consultaServices.GetAll();
            return RedirectToAction("Index", consultas);
        }
        else
        {
            var idResult = consultaServices.SearchConsulta(input);
            return RedirectToAction("Search", idResult);
        }
    }
    */

    // GET: ConsultasController/Details/5
    public ActionResult Detalles(Guid id)
    {
        var consultas = consultaServices.GetConsulta(id);
        return View("Detalles", consultas);
    }

    /*
    // GET: ConsultasController/Create
    public ActionResult Create()
    {
        return View("Create");
    }
    */
    // POST: ConsultasController/Create
    [HttpPost]
    public ActionResult Create(Consulta consulta)
    {
        try
        {
            consultaServices.AddConsulta(consulta);
        }
        catch (Exception ex)
        {           
            RegistrarError(ex);
        }
        return RedirectToAction("Index");
    }

    /*
    // GET: ConsultasController/Edit/5
    public ActionResult Editar(Guid id)
    {
        var consultas = consultaServices.GetConsulta(id);
        return RedirectToAction("Editar", consultas);
    }
    */

    // POST: ConsultasController/Edit/5
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public ActionResult Edit(Consulta consulta)
    //{
    //    try
    //    {
    //        consultaServices.UpdateConsulta(consulta);
    //        var consultas = consultaServices.GetAll();
    //        return RedirectToAction("Index", consultas);
    //    }
    //    catch
    //    {
    //        return View("Error");
    //    }
    //}

    // POST: ConsultasController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Eliminar(Guid id)
    {
        try
        {
            consultaServices.DeleteConsulta(id);
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
        }
        return RedirectToAction("Index");
    }


    public ActionResult crearPdf()
    {
        consultaServices.createPdf(inhtmlPath, toPdfPath);
        return View("Index");
        
    }
}
