using ClinicaDomain;
using ClinicaServices;
using clinicaWeb.Security;
using Microsoft.AspNetCore.Mvc;

namespace clinicaWeb.Controllers;

/// <summary>Egresos de caja: registro de gastos por categoría con respaldo de comprobante.
/// Paralelo a Ventas (ingresos): el gasto queda Registrado y solo se anula con motivo (auditoría).</summary>
[SecurityFilter("Gastos")]
public class GastosController(IGastoService gastos, ICategoriaGastoService categorias, IMetodoPagoService metodos)
    : ErrorHandlingController
{
    public ActionResult Index(DateTime? from, DateTime? to, Guid? categoriaId = null)
    {
        try
        {
            categorias.EnsureSeed();
            var f = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            var t = (to ?? DateTime.Today).Date;
            if (f > t) (f, t) = (t, f);
            ViewBag.Categorias = categorias.GetAll();
            ViewBag.CategoriasTodas = categorias.GetAll(false);
            ViewBag.Metodos = metodos.GetAll();
            ViewBag.From = f.ToString("yyyy-MM-dd");
            ViewBag.To = t.ToString("yyyy-MM-dd");
            ViewBag.CategoriaId = categoriaId;
            var lista = gastos.GetPorRango(f, t, categoriaId);
            ViewBag.Total = lista.Sum(g => g.Monto);
            return View(lista);
        }
        catch (Exception ex)
        {
            // Sin tabla en BD (pendiente script SQL): mostrar página con aviso en vez de 500.
            RegistrarError(ex);
            ViewBag.Categorias = new List<CategoriaGasto>();
            ViewBag.CategoriasTodas = new List<CategoriaGasto>();
            ViewBag.Metodos = new List<MetodoPago>();
            ViewBag.From = (from ?? DateTime.Today).ToString("yyyy-MM-dd");
            ViewBag.To = (to ?? DateTime.Today).ToString("yyyy-MM-dd");
            TempData["Error"] = $"No se pudo cargar gastos: {ex.Message}";
            return View(new List<Gasto>());
        }
    }

    [HttpPost]
    [RequestSizeLimit(22_000_000)]
    public async Task<ActionResult> Crear(Gasto gasto, IFormFile? comprobante)
    {
        try
        {
            var creado = await gastos.CrearAsync(gasto, comprobante);
            TempData["Ok"] = $"Gasto Q{creado.Monto:0.00} registrado.";
        }
        catch (Exception ex) { RegistrarError(ex); TempData["Error"] = ex.Message; }
        return RedirectToAction("Index", new { from = gasto.Fecha.ToString("yyyy-MM-dd"), to = gasto.Fecha.ToString("yyyy-MM-dd") });
    }

    [HttpPost]
    [RequestSizeLimit(22_000_000)]
    public async Task<ActionResult> Actualizar(Gasto gasto, IFormFile? comprobante, bool quitarComprobante = false)
    {
        try
        {
            await gastos.ActualizarAsync(gasto, comprobante, quitarComprobante);
            TempData["Ok"] = "Gasto actualizado.";
        }
        catch (Exception ex) { RegistrarError(ex); TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Anular(Guid id, string motivo)
    {
        try { gastos.Anular(id, motivo); TempData["Ok"] = "Gasto anulado."; }
        catch (Exception ex) { RegistrarError(ex); TempData["Error"] = ex.Message; }
        return RedirectToAction("Index");
    }

    /// <summary>Muestra el comprobante en el navegador (imagen/PDF inline).</summary>
    [HttpGet]
    public async Task<IActionResult> VerComprobante(Guid id)
    {
        var comprobante = await gastos.GetComprobanteAsync(id);
        if (comprobante is null) return NotFound();
        Response.Headers["Content-Disposition"] = $"inline; filename=\"{comprobante.Nombre}\"";
        return File(comprobante.Bytes, comprobante.ContentType);
    }
}
