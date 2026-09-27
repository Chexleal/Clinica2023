using ClinicaDomain;
using clinicamhsystem.Models;
using ClinicaServices;
using clinicaWeb.Models;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;
[SecurityFilter("Reportes")]

public class ReportesController(IPacienteServices pacienteServices, IServiciosServices serviciosServices, IDetallesServices detallesServices, IConsultaServices consultaServices, IVentaService ventaService, IMetodoPagoService metodoPagoService, IGastoService gastoService, ICategoriaGastoService categoriaGastoService) : Controller
{

    // GET: UsuariosController
    public ActionResult Index()
    {
        var pacientes = pacienteServices.GetAll();
        //var consultas = consultaServices.GetAll();
        var servicios = serviciosServices.GetAll();
        ViewBag.TipoFiltro = "todos";

        return View(new ReportesViewModel { Pacientes = pacientes, Servicios = servicios, EsServicio = false, TipoFiltro = "todos" });
    }
    [HttpPost]
    public ActionResult Paciente(Guid? IdPaciente, DateTime from_dt, DateTime to_dt, bool incluirSinIngresos = false)
    {


        //DateTime from_dt = DateTime.ParseExact(from, "yyyy-MM-dd", null);
        //DateTime to_dt = DateTime.ParseExact(to, "yyyy-MM-dd", null);

        var pacientes = pacienteServices.GetAll();
        var servicios = serviciosServices.GetAll();

        // Sin paciente elegido: resumen de todos los pacientes con consulta en el rango.
        if (!IdPaciente.HasValue || IdPaciente.Value == Guid.Empty)
        {
            var consultas = consultaServices.GetAllByRangeWithPaciente(from_dt, to_dt);
            var enRango = consultas.Select(c => c.PacienteInformacion).DistinctBy(p => p.IdPaciente).ToList();
            var porPaciente = consultas.ToLookup(c => c.IdPaciente);
            foreach (var p in enRango)
                p.Consulta = porPaciente[p.IdPaciente].ToList();
            ViewBag.PacientesDropdown = pacientes;

            return View("Index", new ReportesViewModel { Pacientes = enRango, Servicios = servicios, From = from_dt, To = to_dt, EsServicio = false, IncluirSinIngresos = incluirSinIngresos, VerTodosPacientes = true });
        }

        var paciente = pacienteServices.GetPacienteById(IdPaciente.Value);
        paciente.Consulta = consultaServices.GetAllByPacienteId(paciente.IdPaciente, from_dt, to_dt);
        //var consultas = pacienteServices.GetConsultasFiltradas();   

        return View("Index",new ReportesViewModel { Pacientes = pacientes, Servicios = servicios, Paciente = paciente, From = from_dt, To = to_dt, EsServicio = false, IncluirSinIngresos = incluirSinIngresos });
    }

    [HttpPost]
    public ActionResult Servicios(DateTime from_dt, DateTime to_dt, bool incluirSinIngresos = false, string tipo = "todos")
    {
        tipo = NormalizarTipo(tipo);
        var consultas = consultaServices.GetAllByRangeWithPaciente(from_dt, to_dt);
        var pacientes = consultas.Select(c => c.PacienteInformacion).DistinctBy(p => p.IdPaciente).ToList();
        var consultasPorPaciente = consultas.ToLookup(c => c.IdPaciente);
        foreach (var paciente in pacientes)
            paciente.Consulta = consultasPorPaciente[paciente.IdPaciente].ToList();
        var servicios = serviciosServices.GetAll();
        var detalles = detallesServices.GetByRange(from_dt, to_dt);
        // Ventas del rango (GetPorRango excluye Anuladas): se separan cobradas de pendientes.
        // El reporte principal solo suma cobros efectivos; lo pendiente va en tabla aparte.
        var ventasTodas = ventaService.GetPorRango(from_dt, to_dt);
        var ventas = ventasTodas.Where(v => v.Estado == "Pagada").ToList();
        var ventasPend = ventasTodas.Where(v => v.Estado == "Pendiente" || v.Estado == "Pendiente de pago").ToList();
        var ventaDetalles = ventaService.GetDetallesPorVentas(ventas.Select(v => v.IdVenta));
        var ventaDetallesPend = ventaService.GetDetallesPorVentas(ventasPend.Select(v => v.IdVenta));
        ViewBag.Ventas = ventas;
        ViewBag.VentaDetalles = ventaDetalles;
        ViewBag.VentasPendientes = ventasPend;
        ViewBag.VentaDetallesPendientes = ventaDetallesPend;
        // Consultas que ya tienen venta (para no duplicar su legado en la tabla de pendientes).
        ViewBag.ConsultasConVenta = ventasTodas
            .Where(v => v.IdConsulta.HasValue)
            .Select(v => v.IdConsulta!.Value)
            .ToHashSet();
        // Legado: solo las consultas Terminadas y Pagadas cuentan como ingreso cobrado.
        // Lo demás (no pagado o sin consulta verificable) va a la tabla de pendientes.
        var consultasDict = consultas.ToDictionary(c => c.IdConsulta);
        var detallesCobrados = new List<DetalleCobro>();
        var detallesPend = new List<DetalleCobro>();
        foreach (var d in detalles)
        {
            if (consultasDict.TryGetValue(d.IdConsulta, out var c) && c.Terminada && c.Pagada)
                detallesCobrados.Add(d);
            else
                detallesPend.Add(d);
        }
        ViewBag.DetallesPendientes = detallesPend;
        // Mapa IdPaciente -> "Nombre Apellido" para resolver el paciente de cada venta
        // (incluye pacientes sin consulta en el rango y ventas de mostrador).
        ViewBag.PacientesMap = pacienteServices.GetAll()
            .ToDictionary(p => p.IdPaciente, p => $"{p.Nombre} {p.Apellido}");
        // Pagos de todas las ventas del rango (cobradas + pendientes): alimentan la
        // columna "Tipo pago", la sumatoria por tipo y los abonos de la tabla pendientes.
        var pagos = ventaService.GetPagosPorVentas(ventasTodas.Select(v => v.IdVenta));
        ViewBag.Pagos = pagos;
        ViewBag.PagosPorVenta = pagos.GroupBy(p => p.IdVenta)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Fecha).ToList());
        ViewBag.MetodosMap = metodoPagoService.GetAll(false)
            .ToDictionary(m => m.IdMetodoPago, m => m.Nombre);
        // Gastos del rango (egresos de caja, excluye Anulados) para el balance del período.
        // Mismo criterio de fechas que las ventas (fecha de caja). Si la tabla aún no
        // existe en el ambiente, el reporte de ingresos sigue funcionando.
        List<Gasto> gastosReporte = new();
        Dictionary<Guid, string> categoriasGastoMap = new();
        try
        {
            gastosReporte = gastoService.GetPorRango(from_dt, to_dt);
            categoriasGastoMap = categoriaGastoService.GetAll(false)
                .ToDictionary(c => c.IdCategoriaGasto, c => c.Nombre);
        }
        catch { gastosReporte = new(); categoriasGastoMap = new(); }
        ViewBag.Gastos = gastosReporte;
        ViewBag.CategoriasGastoMap = categoriasGastoMap;
        // Filtro Todos | Solo servicios | Solo productos (se preserva en la vista).
        ViewBag.TipoFiltro = tipo;
        // Dropdown de "Por Paciente" debe seguir mostrando el catálogo completo
        // aunque Model.Pacientes se reutiliza filtrado para el reporte.
        ViewBag.PacientesDropdown = pacienteServices.GetAll();

        return View("Index", new ReportesViewModel { Pacientes = pacientes, Servicios = servicios, Detalles = detallesCobrados, EsServicio = true, From = from_dt, To = to_dt, Paciente = null, IncluirSinIngresos = incluirSinIngresos, TipoFiltro = tipo });
    }

    /// <summary>Normaliza el filtro del reporte a todos|servicios|productos|gastos.</summary>
    private static string NormalizarTipo(string? tipo)
    {
        tipo = (tipo ?? "todos").Trim().ToLowerInvariant();
        return tipo switch
        {
            "servicio" or "servicios" => "servicios",
            "producto" or "productos" => "productos",
            "gasto" or "gastos" => "gastos",
            _ => "todos",
        };
    }

    [HttpPost]
    public IActionResult Detalles(Guid idconsulta)
    {
        var detalles = detallesServices.GetDetallesByConsulta(idconsulta);
        var servicios = serviciosServices.GetAll();
        return PartialView("Detalles", (new DetallesPagarViewModel { Detalles = detalles, Servicios = servicios}));
    }
}
