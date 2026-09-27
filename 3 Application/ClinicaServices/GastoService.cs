using ClinicaDomain;
using ClinicaInfrastructure;
using Microsoft.AspNetCore.Http;

namespace ClinicaServices;

public interface ICategoriaGastoService
{
    List<CategoriaGasto> GetAll(bool soloActivos = true);
    CategoriaGasto? Get(Guid id);
    CategoriaGasto Crear(string nombre);
    void Actualizar(CategoriaGasto categoria);
    void CambiarActivo(Guid id, bool activo);
    void EnsureSeed();
}

public class CategoriaGastoService(ClinicaContext db) : ICategoriaGastoService
{

    public List<CategoriaGasto> GetAll(bool soloActivos = true)
    {
        var q = db.CategoriasGasto.AsQueryable();
        if (soloActivos) q = q.Where(x => x.Activo);
        var lista = q.OrderBy(x => x.Nombre).ToList();
        AuditoriaNombres.Completar(db, lista);
        return lista;
    }

    public CategoriaGasto? Get(Guid id) =>
        db.CategoriasGasto.FirstOrDefault(x => x.IdCategoriaGasto == id);

    public CategoriaGasto Crear(string nombre)
    {
        var limpio = nombre.TextoCatalogo();
        if (string.IsNullOrWhiteSpace(limpio)) throw new ArgumentException("Nombre requerido.");
        var existente = db.CategoriasGasto.FirstOrDefault(x => x.Nombre.ToLower() == limpio.ToLower());
        if (existente is not null) return existente;
        var nuevo = new CategoriaGasto
        {
            IdCategoriaGasto = Guid.NewGuid(),
            Nombre = limpio,
            Activo = true
        };
        nuevo.BeforeSaveChanges();
        db.CategoriasGasto.Add(nuevo);
        db.SaveChanges();
        return nuevo;
    }

    public void Actualizar(CategoriaGasto categoria)
    {
        categoria.Nombre = categoria.Nombre.TextoCatalogo();
        if (string.IsNullOrWhiteSpace(categoria.Nombre)) throw new ArgumentException("Nombre requerido.");
        categoria.BeforeSaveChanges();
        db.SaveChanges();
    }

    public void CambiarActivo(Guid id, bool activo)
    {
        var c = Get(id);
        if (c is null) return;
        c.Activo = activo;
        db.SaveChanges();
    }

    public void EnsureSeed()
    {
        if (db.CategoriasGasto.Any()) return;
        var seeds = new[]
        {
            "SUELDOS Y SALARIOS",
            "HONORARIOS MÉDICOS",
            "ALQUILER",
            "SERVICIOS BÁSICOS",
            "INSUMOS Y MATERIALES",
            "MANTENIMIENTO",
            "IMPUESTOS Y TASAS",
            "OTROS",
        };
        foreach (var s in seeds)
        {
            db.CategoriasGasto.Add(new CategoriaGasto
            {
                IdCategoriaGasto = Guid.NewGuid(),
                Nombre = s,
                Activo = true
            });
        }
        db.SaveChanges();
    }
}

public interface IGastoService
{
    List<Gasto> GetPorRango(DateTime from, DateTime to, Guid? categoriaId = null);
    Gasto? Get(Guid id);
    Gasto Crear(Gasto gasto);
    Gasto Actualizar(Gasto gasto);
    void Anular(Guid id, string motivo);
    /// <summary>Crea el gasto y guarda el comprobante (foto/PDF) en storage. Solo deja la ruta en BD.</summary>
    Task<Gasto> CrearAsync(Gasto gasto, IFormFile? comprobante);
    /// <summary>Actualiza el gasto; reemplaza o quita el comprobante si se indica.</summary>
    Task<Gasto> ActualizarAsync(Gasto gasto, IFormFile? comprobante, bool quitarComprobante = false);
    /// <summary>Lee el comprobante para servirlo inline. Null si no existe.</summary>
    Task<ComprobanteGasto?> GetComprobanteAsync(Guid id);
}

/// <summary>Comprobante listo para devolver con File(bytes, contentType).</summary>
public record ComprobanteGasto(byte[] Bytes, string ContentType, string Nombre);

public class GastoService(ClinicaContext db, Storage.IStorageService storage) : IGastoService
{


    /// <summary>Extensiones permitidas para el comprobante (foto o PDF).</summary>
    private static readonly HashSet<string> ExtensionesComprobante = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf" };

    private const long MaxComprobanteBytes = 20 * 1024 * 1024; // 20 MB

    public List<Gasto> GetPorRango(DateTime from, DateTime to, Guid? categoriaId = null)
    {
        var f = from.Date;
        var t = to.Date.AddDays(1);
        var q = db.Gastos.Where(g => g.Fecha >= f && g.Fecha < t && g.Estado != "Anulado");
        if (categoriaId.HasValue && categoriaId.Value != Guid.Empty)
            q = q.Where(g => g.IdCategoriaGasto == categoriaId.Value);
        var lista = q.OrderByDescending(g => g.Fecha).ThenByDescending(g => g.FechaCreacion).ToList();
        AuditoriaNombres.Completar(db, lista);
        return lista;
    }

    public Gasto? Get(Guid id) =>
        db.Gastos.FirstOrDefault(x => x.IdGasto == id);

    public Gasto Crear(Gasto gasto)
    {
        Validar(gasto, esNuevo: true);
        gasto.IdGasto = Guid.NewGuid();
        if (gasto.Fecha == default) gasto.Fecha = DateTime.Today;
        gasto.Estado = "Registrado";
        gasto.BeforeSaveChanges();
        db.Gastos.Add(gasto);
        db.SaveChanges();
        return gasto;
    }

    public Gasto Actualizar(Gasto gasto)
    {
        var actual = Get(gasto.IdGasto) ?? throw new InvalidOperationException("Gasto no encontrado.");
        if (actual.Estado != "Registrado")
            throw new InvalidOperationException("Solo se puede editar un gasto registrado (el anulado es histórico).");
        Validar(gasto, esNuevo: false);
        actual.Fecha = gasto.Fecha == default ? actual.Fecha : gasto.Fecha;
        actual.IdCategoriaGasto = gasto.IdCategoriaGasto;
        actual.Concepto = gasto.Concepto;
        actual.Proveedor = gasto.Proveedor;
        actual.NumeroComprobante = gasto.NumeroComprobante;
        actual.RutaComprobante = gasto.RutaComprobante;
        actual.NombreComprobante = gasto.NombreComprobante;
        actual.IdMetodoPago = gasto.IdMetodoPago;
        actual.Referencia = gasto.Referencia;
        actual.Monto = gasto.Monto;
        actual.Observaciones = gasto.Observaciones;
        actual.BeforeSaveChanges();
        db.SaveChanges();
        return actual;
    }

    public void Anular(Guid id, string motivo)
    {
        motivo = (motivo ?? "").Trim();
        if (string.IsNullOrWhiteSpace(motivo)) throw new ArgumentException("Indica el motivo de la anulación.");
        var gasto = Get(id) ?? throw new InvalidOperationException("Gasto no encontrado.");
        if (gasto.Estado == "Anulado") return;
        gasto.Estado = "Anulado";
        gasto.Observaciones = $"{(gasto.Observaciones ?? "").Trim()} [Anulado {DateTime.Now:dd/MM HH:mm}: {motivo}]".Trim();
        db.SaveChanges();
    }

    public async Task<Gasto> CrearAsync(Gasto gasto, IFormFile? comprobante)
    {
        var creado = Crear(gasto);
        if (comprobante is not null && comprobante.Length > 0)
        {
            creado.RutaComprobante = await GuardarComprobante(creado.IdGasto, creado.Fecha, comprobante);
            creado.NombreComprobante = comprobante.FileName;
            creado.BeforeSaveChanges();
            db.SaveChanges();
        }
        return creado;
    }

    public async Task<Gasto> ActualizarAsync(Gasto gasto, IFormFile? comprobante, bool quitarComprobante = false)
    {
        var actual = Get(gasto.IdGasto) ?? throw new InvalidOperationException("Gasto no encontrado.");
        // Preservar el comprobante actual salvo que se reemplace o se quite.
        gasto.RutaComprobante = actual.RutaComprobante;
        gasto.NombreComprobante = actual.NombreComprobante;
        if (comprobante is not null && comprobante.Length > 0)
        {
            await BorrarComprobante(actual.RutaComprobante);
            gasto.RutaComprobante = await GuardarComprobante(actual.IdGasto, gasto.Fecha, comprobante);
            gasto.NombreComprobante = comprobante.FileName;
        }
        else if (quitarComprobante && !string.IsNullOrWhiteSpace(actual.RutaComprobante))
        {
            await BorrarComprobante(actual.RutaComprobante);
            gasto.RutaComprobante = string.Empty;
            gasto.NombreComprobante = string.Empty;
        }
        return Actualizar(gasto);
    }

    public async Task<ComprobanteGasto?> GetComprobanteAsync(Guid id)
    {
        var gasto = Get(id);
        if (gasto is null || string.IsNullOrWhiteSpace(gasto.RutaComprobante)) return null;
        var bytes = await storage.ReadAsync(gasto.RutaComprobante);
        var contentType = storage.GetContentType(gasto.NombreComprobante ?? gasto.RutaComprobante);
        return new ComprobanteGasto(bytes, contentType, gasto.NombreComprobante ?? "comprobante");
    }

    private async Task<string> GuardarComprobante(Guid idGasto, DateTime fecha, IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ExtensionesComprobante.Contains(ext))
            throw new ArgumentException("Comprobante no válido: solo imagen (JPG/PNG/WEBP/GIF) o PDF.");
        if (file.Length > MaxComprobanteBytes)
            throw new ArgumentException("El comprobante supera los 20 MB.");
        var relPath = $"gastos/{fecha:yyyy-MM}/{idGasto}/{Guid.NewGuid()}{ext}";
        return await storage.SaveAsync(file, relPath);
    }

    private async Task BorrarComprobante(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta)) return;
        try { await storage.DeleteAsync(ruta); }
        catch { /* no bloquear la operación si el archivo ya no existe */ }
    }

    private void Validar(Gasto gasto, bool esNuevo)
    {
        gasto.Concepto = gasto.Concepto.TextoLibre();
        if (string.IsNullOrWhiteSpace(gasto.Concepto)) throw new ArgumentException("Concepto requerido.");
        if (gasto.Concepto.Length > 250) gasto.Concepto = gasto.Concepto[..250];
        if (gasto.Monto <= 0) throw new ArgumentException("El monto debe ser mayor a cero.");
        var categoria = db.CategoriasGasto.FirstOrDefault(c => c.IdCategoriaGasto == gasto.IdCategoriaGasto);
        if (categoria is null) throw new ArgumentException("Categoría no válida.");
        if (!categoria.Activo) throw new ArgumentException("La categoría está inactiva.");
        var metodo = db.MetodosPago.FirstOrDefault(m => m.IdMetodoPago == gasto.IdMetodoPago);
        if (metodo is null) throw new ArgumentException("Tipo de pago no válido.");
        if (!metodo.Activo) throw new ArgumentException("El tipo de pago está inactivo.");
        if (metodo.RequiereReferencia && string.IsNullOrWhiteSpace(gasto.Referencia))
            throw new ArgumentException($"'{metodo.Nombre}' exige número de referencia/autorización.");
        gasto.Proveedor = gasto.Proveedor.TextoLibre();
        gasto.NumeroComprobante = gasto.NumeroComprobante.TextoLibre();
        gasto.RutaComprobante = (gasto.RutaComprobante ?? "").Trim();
        gasto.NombreComprobante = (gasto.NombreComprobante ?? "").Trim();
        gasto.Referencia = gasto.Referencia.TextoLibre();
        gasto.Observaciones = gasto.Observaciones.TextoLibre();
        if (gasto.Fecha != default && gasto.Fecha.Date > DateTime.Today.AddDays(1))
            throw new ArgumentException("La fecha no puede ser futura.");
        _ = esNuevo;
    }
}
