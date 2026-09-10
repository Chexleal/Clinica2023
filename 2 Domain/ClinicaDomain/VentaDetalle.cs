using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaDomain;

public partial class VentaDetalle : Base
{
    public Guid IdVentaDetalle { get; set; }

    public Guid IdVenta { get; set; }

    /// <summary>Servicio | Producto</summary>
    public string TipoLinea { get; set; } = "Servicio";

    public Guid? IdMotivoCobro { get; set; }

    public Guid? IdProducto { get; set; }

    /// <summary>Opcional. Solo cuando el producto usa lotes.</summary>
    public Guid? IdLote { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    /// <summary>Monto de descuento en Q aplicado a esta línea. 0 = sin descuento.</summary>
    public decimal DescuentoMonto { get; set; }

    /// <summary>Motivo del descuento (catálogo + texto libre). Obligatorio si DescuentoMonto &gt; 0.</summary>
    public string? DescuentoMotivo { get; set; }

    /// <summary>Quién otorgó el descuento (auditoría). Se llena con el usuario en sesión al aplicar el descuento.</summary>
    public Guid? DescuentoOtorgadoPor { get; set; }

    [NotMapped]
    public string? DescuentoOtorgadoPorNombre { get; set; }

    /// <summary>Sobre pedido en esta línea (ej. plantilla mandada a hacer): no valida ni descuenta stock. Se marca con el check S/pedido al cobrar.</summary>
    public bool EsSobrePedido { get; set; }
}
