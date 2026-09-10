using ClinicaDomain;
using ClinicaInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public interface ICategoriaProductoService
{
    List<CategoriaProducto> GetAll(bool soloActivos = true);
    CategoriaProducto? Get(Guid id);
    CategoriaProducto Crear(string nombre, string tipo, bool exigeLote, bool exigeVencimiento);
    void Actualizar(CategoriaProducto categoria);
    void CambiarActivo(Guid id, bool activo);
    void EnsureSeed();
}

public class CategoriaProductoService : ICategoriaProductoService
{
    private readonly ClinicaContext _db;
    public CategoriaProductoService(ClinicaContext db) { _db = db; }

    public List<CategoriaProducto> GetAll(bool soloActivos = true)
    {
        var q = _db.CategoriasProducto.AsQueryable();
        if (soloActivos) q = q.Where(x => x.Activo);
        var lista = q.OrderBy(x => x.Nombre).ToList();
        AuditoriaNombres.Completar(_db, lista);
        return lista;
    }

    public CategoriaProducto? Get(Guid id) =>
        _db.CategoriasProducto.FirstOrDefault(x => x.IdCategoriaProducto == id);

    public CategoriaProducto Crear(string nombre, string tipo, bool exigeLote, bool exigeVencimiento)
    {
        var limpio = nombre.TextoCatalogo();
        if (string.IsNullOrWhiteSpace(limpio)) throw new ArgumentException("Nombre requerido.");
        if (tipo != "Bien" && tipo != "Servicio") tipo = "Bien";
        var existente = _db.CategoriasProducto.FirstOrDefault(x => x.Nombre.ToLower() == limpio.ToLower());
        if (existente is not null) return existente;
        var nuevo = new CategoriaProducto
        {
            IdCategoriaProducto = Guid.NewGuid(),
            Nombre = limpio,
            Tipo = tipo,
            ExigeLoteDefault = exigeLote,
            ExigeVencimientoDefault = exigeVencimiento,
            Activo = true
        };
        nuevo.BeforeSaveChanges();
        _db.CategoriasProducto.Add(nuevo);
        _db.SaveChanges();
        return nuevo;
    }

    public void Actualizar(CategoriaProducto categoria)
    {
        categoria.Nombre = categoria.Nombre.TextoCatalogo();
        categoria.BeforeSaveChanges();
        _db.SaveChanges();
    }

    public void CambiarActivo(Guid id, bool activo)
    {
        var c = Get(id);
        if (c is null) return;
        c.Activo = activo;
        _db.SaveChanges();
    }

    public void EnsureSeed()
    {
        if (_db.CategoriasProducto.Any()) return;
        var seeds = new (string Nombre, string Tipo, bool Lote, bool Vence)[]
        {
            ("MEDICAMENTO", "Bien", true, true),
            ("MATERIAL ORTOPÉDICO", "Bien", false, false),
            ("INSUMO DESECHABLE", "Bien", false, false),
            ("MATERIAL DE CURACIÓN", "Bien", false, false),
            ("SUPLEMENTOS / DERMOCOSMÉTICA", "Bien", false, false),
            ("SERVICIOS MÉDICOS", "Servicio", false, false),
        };
        foreach (var s in seeds)
        {
            _db.CategoriasProducto.Add(new CategoriaProducto
            {
                IdCategoriaProducto = Guid.NewGuid(),
                Nombre = s.Nombre,
                Tipo = s.Tipo,
                ExigeLoteDefault = s.Lote,
                ExigeVencimientoDefault = s.Vence,
                Activo = true
            });
        }
        _db.SaveChanges();
    }
}

public interface IProductoService
{
    List<Producto> GetAll(bool soloActivos = true);
    List<Producto> Buscar(string texto, int top = 20);
    Producto? Get(Guid id);
    Producto Crear(Producto p);
    void Actualizar(Producto p);
    void CambiarActivo(Guid id, bool activo);
    List<Producto> GetStockBajo();
    List<LoteProducto> GetProximosAVencer(int dias = 30);
    List<LoteProducto> GetLotesDisponibles(Guid productoId);
}

public class ProductoService : IProductoService
{
    private readonly ClinicaContext _db;
    public ProductoService(ClinicaContext db) { _db = db; }

    public List<Producto> GetAll(bool soloActivos = true)
    {
        var q = _db.Productos.AsQueryable();
        if (soloActivos) q = q.Where(x => x.Activo);
        var lista = q.OrderBy(x => x.Nombre).ToList();
        AuditoriaNombres.Completar(_db, lista);
        return lista;
    }

    public List<Producto> Buscar(string texto, int top = 20)
    {
        texto = (texto ?? "").Trim().ToLower();
        var q = _db.Productos.Where(x => x.Activo);
        if (!string.IsNullOrWhiteSpace(texto))
            q = q.Where(x => x.Nombre.ToLower().Contains(texto) || (x.Sku ?? "").ToLower().Contains(texto));
        return q.OrderBy(x => x.Nombre).Take(top).ToList();
    }

    public Producto? Get(Guid id) =>
        _db.Productos.FirstOrDefault(x => x.IdProducto == id);

    public Producto Crear(Producto p)
    {
        p.IdProducto = Guid.NewGuid();
        p.StockActual = 0;
        p.Activo = true;
        p.Nombre = p.Nombre.TextoCatalogo();
        p.Sku = p.Sku.TextoCatalogo();
        p.UnidadMedida = p.UnidadMedida.TextoCatalogo();
        // Si la categoría exige lote/vence, heredar salvo que ya venga marcado.
        var cat = _db.CategoriasProducto.FirstOrDefault(c => c.IdCategoriaProducto == p.IdCategoriaProducto);
        if (cat is not null)
        {
            if (!p.RequiereLote && cat.ExigeLoteDefault) p.RequiereLote = true;
            if (!p.RequiereVencimiento && cat.ExigeVencimientoDefault) p.RequiereVencimiento = true;
        }
        if (!p.RequiereLote) p.RequiereVencimiento = false; // sin lote no hay vencimiento
        p.EsSobrePedido = false; // LEGADO: columna aún en BD, ya sin uso (todo requiere stock)
        p.BeforeSaveChanges();
        _db.Productos.Add(p);
        _db.SaveChanges();
        return p;
    }

    public void Actualizar(Producto p)
    {
        p.Nombre = p.Nombre.TextoCatalogo();
        p.Sku = p.Sku.TextoCatalogo();
        p.UnidadMedida = p.UnidadMedida.TextoCatalogo();
        if (!p.RequiereLote) p.RequiereVencimiento = false;
        p.EsSobrePedido = false; // LEGADO: columna aún en BD, ya sin uso
        p.BeforeSaveChanges();
        // Blindaje: si la entidad llegara detached (o tracking deshabilitado),
        // marcarla explícitamente como modificada para que el UPDATE sí se genere.
        // Antes, un SaveChanges sobre entidad detached terminaba en "recarga sin
        // mensaje y sin cambio", que es el síntoma reportado.
        var entry = _db.Entry(p);
        if (entry.State == EntityState.Detached)
        {
            _db.Productos.Attach(p);
            entry.State = EntityState.Modified;
        }
        var habiaCambios = _db.ChangeTracker.HasChanges();
        var filas = _db.SaveChanges();
        if (habiaCambios && filas == 0)
            throw new InvalidOperationException("No se pudo guardar el cambio de precio (0 filas afectadas).");
    }

    public void CambiarActivo(Guid id, bool activo)
    {
        var p = Get(id);
        if (p is null) return;
        p.Activo = activo;
        _db.SaveChanges();
    }

    public List<Producto> GetStockBajo() =>
        _db.Productos.Where(x => x.Activo && x.StockActual <= x.StockMinimo).OrderBy(x => x.Nombre).ToList();

    public List<LoteProducto> GetProximosAVencer(int dias = 30)
    {
        var limite = DateTime.Today.AddDays(dias);
        return _db.LotesProducto
            .Where(l => l.Activo && l.Stock > 0 && l.FechaVencimiento.HasValue && l.FechaVencimiento.Value.Date <= limite)
            .OrderBy(l => l.FechaVencimiento).ToList();
    }

    public List<LoteProducto> GetLotesDisponibles(Guid productoId) =>
        _db.LotesProducto
            .Where(l => l.IdProducto == productoId && l.Activo && l.Stock > 0)
            .OrderBy(l => l.FechaVencimiento == null ? 1 : 0)
            .ThenBy(l => l.FechaVencimiento)
            .ToList();
}

public interface IMovimientoInventarioService
{
    MovimientoInventario RegistrarEntrada(Guid productoId, decimal cantidad, decimal costoUnitario,
        string? codigoLote, DateTime? vencimiento, string? motivo, Guid? idConsulta = null);
    List<MovimientoInventario> GetKardex(Guid productoId, int top = 100);
    void RegistrarAjuste(Guid productoId, decimal cantidadAjuste, string motivo);
}

public class MovimientoInventarioService : IMovimientoInventarioService
{
    private readonly ClinicaContext _db;
    public MovimientoInventarioService(ClinicaContext db) { _db = db; }

    public MovimientoInventario RegistrarEntrada(Guid productoId, decimal cantidad, decimal costoUnitario,
        string? codigoLote, DateTime? vencimiento, string? motivo, Guid? idConsulta = null)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad de entrada debe ser mayor a cero.");
        var producto = _db.Productos.FirstOrDefault(x => x.IdProducto == productoId)
            ?? throw new InvalidOperationException("Producto no encontrado.");

        Guid? loteId = null;
        var codigoLoteNorm = codigoLote.TextoCatalogo();
        if (producto.RequiereLote)
        {
            if (string.IsNullOrWhiteSpace(codigoLoteNorm)) throw new ArgumentException("Este producto exige lote.");
            if (producto.RequiereVencimiento && !vencimiento.HasValue)
                throw new ArgumentException("Este producto exige fecha de vencimiento.");
            var lote = new LoteProducto
            {
                IdLote = Guid.NewGuid(),
                IdProducto = productoId,
                CodigoLote = codigoLoteNorm,
                FechaVencimiento = vencimiento,
                Stock = cantidad,
                CostoUnitario = costoUnitario,
                Activo = true
            };
            lote.BeforeSaveChanges();
            _db.LotesProducto.Add(lote);
            loteId = lote.IdLote;
        }

        producto.StockActual += cantidad;
        producto.CostoUltimo = costoUnitario;

        var mov = new MovimientoInventario
        {
            IdMovimiento = Guid.NewGuid(),
            Fecha = DateTime.Now,
            IdProducto = productoId,
            IdLote = loteId,
            Tipo = "EntradaCompra",
            Cantidad = cantidad,
            CostoUnitario = costoUnitario,
            IdConsulta = idConsulta,
            Motivo = motivo.TextoLibre()
        };
        _db.MovimientosInventario.Add(mov);
        _db.SaveChanges();
        return mov;
    }

    public List<MovimientoInventario> GetKardex(Guid productoId, int top = 100) =>
        _db.MovimientosInventario.Where(x => x.IdProducto == productoId)
            .OrderByDescending(x => x.Fecha).Take(top).ToList();

    public void RegistrarAjuste(Guid productoId, decimal cantidadAjuste, string motivo)
    {
        var producto = _db.Productos.FirstOrDefault(x => x.IdProducto == productoId)
            ?? throw new InvalidOperationException("Producto no encontrado.");
        if (producto.RequiereLote)
            throw new InvalidOperationException("Este producto usa lotes: ajuste por lote desde kardex.");
        producto.StockActual += cantidadAjuste;
        _db.MovimientosInventario.Add(new MovimientoInventario
        {
            IdMovimiento = Guid.NewGuid(),
            Fecha = DateTime.Now,
            IdProducto = productoId,
            Tipo = cantidadAjuste >= 0 ? "Ajuste" : "Merma",
            Cantidad = cantidadAjuste,
            CostoUnitario = producto.CostoUltimo,
            Motivo = motivo.TextoLibre()
        });
        _db.SaveChanges();
    }
}
