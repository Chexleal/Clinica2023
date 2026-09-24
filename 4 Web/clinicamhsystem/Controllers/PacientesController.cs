using ClinicaDomain;
using ClinicaServices;
using Microsoft.AspNetCore.Mvc;
using clinicaWeb.Models;
using clinicaWeb.Security;
using System.Threading.Tasks;

namespace clinicaWeb.Controllers;

[SecurityFilter("Pacientes")]
public class PacientesController: ErrorHandlingController
{

    private readonly IPacienteServices _pacienteServices;
    private readonly IConsultaServices _consultaServices;
    private readonly IVentaService _ventas;

    public PacientesController(IPacienteServices pacienteServices, IConsultaServices consultaServices, IVentaService ventas)
    {
        _pacienteServices = pacienteServices;
        _consultaServices = consultaServices;
        _ventas = ventas;
    }

    public IActionResult Index()
    {
        return View(new PacientesViewModel { Pacientes = new(), Consultas = new() });
    }

    [HttpPost]
    public IActionResult GetPacientesTable(DataTableRequest request)
    {
        var result = _pacienteServices.GetPaginated(request.Start, request.Length, request.SearchValue, request.SortColumn, request.SortDir);

        var data = result.Data.Select(p => new
        {
            p.IdPaciente,
            p.NoRegistro,
            p.Nombre,
            p.Apellido,
            p.Dpi,
            p.FechaNacimiento,
            p.Telefono,
            p.Correo
        });

        return Json(new DataTableResponse<object>
        {
            Draw = request.Draw,
            RecordsTotal = result.Total,
            RecordsFiltered = result.TotalFiltered,
            Data = data
        });
    }

    // GET: UsuariosController/Detalles/fj33-4ra4r
    public ActionResult Detalles(Guid id)
    {
        var paciente = _pacienteServices.GetPacienteById(id);
        return View("Detalles", paciente);
    }

    // GET: UsuariosController/Create
    public ActionResult Create()
    {
        return View("Create");
    }

    // POST: UsuariosController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Paciente paciente)
    {
        try
        {
            _pacienteServices.AddPaciente(paciente);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            RegistrarError(ex);
            return View("Error");

        }
        //return RedirectToAction("Index");
    }

    // GET: UsuariosController/Editar/fj33-4ra4r
    public ActionResult Editar(Guid id)
    {
        var paciente = _pacienteServices.GetPacienteById(id);
        return View("Editar", paciente);
    }

    // POST: UsuariosController/Editar/fj33-4ra4r
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Editar(Paciente paciente)
    {
        try
        {
            _pacienteServices.UpdatePaciente(paciente);
            return RedirectToAction("Index");
        }
        catch(Exception ex)
        {
            RegistrarError(ex);
            return View("Error");
        }
    }

    // POST: UsuariosController/Eliminar/5
    [HttpPost]
    public ActionResult Eliminar(Guid id)
    {
        try
        {
            _pacienteServices.DeletePaciente(id);
            return RedirectToAction("Index");
        }
        catch (Exception ex) {
            RegistrarError(ex);
            return View("Error");
        }
        //var pacientes = _pacienteServices.GetAll();
        //return RedirectToAction("Index", pacientes);
    }

    [HttpGet]
    public IActionResult GetPaciente(Guid pacienteId)
    {
        var paciente = _pacienteServices.GetPacienteById(pacienteId);
        return PartialView("Editar", paciente);
    }
    [HttpGet]
    public IActionResult GetHistorialConsultas(Guid pacienteId)
    {
        var consultas = (_consultaServices.GetAllByPacienteId(pacienteId) ?? []).OrderByDescending(c => c.Fecha).ToList();
        // Cobro por consulta: folio/total/estado/saldo para pagar o ver desde el historial.
        var ventasPorConsulta = new Dictionary<Guid, List<ClinicaDomain.Venta>>();
        var saldos = new Dictionary<Guid, decimal>();
        foreach (var c in consultas)
        {
            var ventas = _ventas.GetVentasPorConsulta(c.IdConsulta);
            ventasPorConsulta[c.IdConsulta] = ventas;
            foreach (var v in ventas)
            {
                try { saldos[v.IdVenta] = _ventas.SaldoPendiente(v.IdVenta); }
                catch { saldos[v.IdVenta] = v.Total; }
            }
        }
        // Compras de mostrador del paciente (sin consulta).
        var libres = _ventas.GetVentasPorPaciente(pacienteId).Where(v => !v.IdConsulta.HasValue).ToList();
        foreach (var v in libres)
        {
            try { if (!saldos.ContainsKey(v.IdVenta)) saldos[v.IdVenta] = _ventas.SaldoPendiente(v.IdVenta); }
            catch { saldos[v.IdVenta] = v.Total; }
        }
        ViewBag.VentasPorConsulta = ventasPorConsulta;
        ViewBag.Saldos = saldos;
        ViewBag.VentasLibres = libres;
        return PartialView("Partials/_tablaHistorial", consultas);
    }
}
