-- =============================================================
-- Cotizaciones de mostrador (sin inventario, convertibles a venta)
-- Tablas nuevas, sin backfill. Idempotente.
-- =============================================================

IF OBJECT_ID('dbo.Cotizacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cotizacion (
        id_cotizacion UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Cotizacion PRIMARY KEY,
        id_clinica UNIQUEIDENTIFIER NOT NULL,
        id_hospital UNIQUEIDENTIFIER NOT NULL,
        folio VARCHAR(30) NOT NULL,
        fecha DATETIME2 NOT NULL,
        vigencia_dias INT NOT NULL CONSTRAINT DF_Cotizacion_vigencia DEFAULT (15),
        fecha_vence DATETIME2 NULL,
        id_paciente UNIQUEIDENTIFIER NULL,
        cliente_nombre VARCHAR(200) NULL,
        total DECIMAL(15, 2) NOT NULL CONSTRAINT DF_Cotizacion_total DEFAULT (0),
        estado VARCHAR(20) NOT NULL CONSTRAINT DF_Cotizacion_estado DEFAULT ('Borrador'),
        observaciones VARCHAR(300) NULL,
        id_venta_convertida UNIQUEIDENTIFIER NULL,
        fecha_creacion DATETIME2 NULL, creado_por UNIQUEIDENTIFIER NULL,
        fecha_modificacion DATETIME2 NULL, modificado_por UNIQUEIDENTIFIER NULL,
        fecha_eliminacion DATETIME2 NULL, eliminado_por UNIQUEIDENTIFIER NULL,
        CONSTRAINT UQ_Cotizacion_Folio UNIQUE (folio)
    );
    CREATE INDEX IX_Cotizacion_Clinica_Estado ON dbo.Cotizacion(id_clinica, estado);
END
GO

IF OBJECT_ID('dbo.Cotizacion_detalle', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cotizacion_detalle (
        id_cotizacion_detalle UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CotizacionDetalle PRIMARY KEY,
        id_cotizacion UNIQUEIDENTIFIER NOT NULL,
        tipo_linea VARCHAR(20) NOT NULL,
        id_motivo_cobro UNIQUEIDENTIFIER NULL,
        id_producto UNIQUEIDENTIFIER NULL,
        id_lote UNIQUEIDENTIFIER NULL,
        descripcion VARCHAR(250) NOT NULL,
        cantidad DECIMAL(18, 2) NOT NULL,
        precio_unitario DECIMAL(15, 2) NOT NULL,
        subtotal DECIMAL(15, 2) NOT NULL,
        descuento_monto DECIMAL(15, 2) NOT NULL CONSTRAINT DF_CotDet_desc DEFAULT (0),
        descuento_motivo VARCHAR(200) NULL,
        descuento_otorgado_por UNIQUEIDENTIFIER NULL,
        es_sobre_pedido BIT NOT NULL CONSTRAINT DF_CotDet_sp DEFAULT (0),
        fecha_creacion DATETIME2 NULL, creado_por UNIQUEIDENTIFIER NULL,
        fecha_modificacion DATETIME2 NULL, modificado_por UNIQUEIDENTIFIER NULL,
        fecha_eliminacion DATETIME2 NULL, eliminado_por UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_CotDet_Cotizacion FOREIGN KEY (id_cotizacion) REFERENCES dbo.Cotizacion(id_cotizacion)
    );
    CREATE INDEX IX_CotizacionDetalle_Cotizacion ON dbo.Cotizacion_detalle(id_cotizacion);
END
GO
