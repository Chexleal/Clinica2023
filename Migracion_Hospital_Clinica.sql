-- =============================================================
-- Migración: modelo Hospital -> Clínica (God Mode)
-- Todo lo existente pertenece a: Hospital de Antigua / Traumatología
-- Ejecutar UNA vez en cada ambiente (dev/test/prod).
-- Recomendado con codepage UTF-8 para preservar tildes:
--   sqlcmd -S <servidor> -d <bd> -b -f 65001 -i Migracion_Hospital_Clinica.sql
-- (El seed usa CHAR(237) para la í, así funciona aun sin -f 65001.)
-- =============================================================

DECLARE @IdHospital UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @IdClinica UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';

-- ---------- 1. Tablas nuevas ----------
IF OBJECT_ID('dbo.Hospital', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Hospital (
        id_hospital UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Hospital PRIMARY KEY,
        nombre VARCHAR(200) NOT NULL,
        activo BIT NOT NULL CONSTRAINT DF_Hospital_activo DEFAULT (1),
        fecha_creacion DATETIME2 NULL, creado_por UNIQUEIDENTIFIER NULL,
        fecha_modificacion DATETIME2 NULL, modificado_por UNIQUEIDENTIFIER NULL,
        fecha_eliminacion DATETIME2 NULL, eliminado_por UNIQUEIDENTIFIER NULL
    );
END

IF OBJECT_ID('dbo.Clinica', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clinica (
        id_clinica UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Clinica PRIMARY KEY,
        id_hospital UNIQUEIDENTIFIER NOT NULL,
        nombre VARCHAR(200) NOT NULL,
        activa BIT NOT NULL CONSTRAINT DF_Clinica_activa DEFAULT (1),
        fecha_creacion DATETIME2 NULL, creado_por UNIQUEIDENTIFIER NULL,
        fecha_modificacion DATETIME2 NULL, modificado_por UNIQUEIDENTIFIER NULL,
        fecha_eliminacion DATETIME2 NULL, eliminado_por UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_Clinica_Hospital FOREIGN KEY (id_hospital) REFERENCES dbo.Hospital(id_hospital)
    );
    CREATE INDEX IX_Clinica_Hospital_Nombre ON dbo.Clinica(id_hospital, nombre);
END

IF OBJECT_ID('dbo.Usuario_clinica', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuario_clinica (
        id_usuario_clinica UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UsuarioClinica PRIMARY KEY,
        id_usuario UNIQUEIDENTIFIER NOT NULL,
        id_clinica UNIQUEIDENTIFIER NOT NULL,
        es_default BIT NOT NULL CONSTRAINT DF_UsuarioClinica_default DEFAULT (0),
        activo BIT NOT NULL CONSTRAINT DF_UsuarioClinica_activo DEFAULT (1),
        fecha_creacion DATETIME2 NULL, creado_por UNIQUEIDENTIFIER NULL,
        fecha_modificacion DATETIME2 NULL, modificado_por UNIQUEIDENTIFIER NULL,
        fecha_eliminacion DATETIME2 NULL, eliminado_por UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_UsuarioClinica_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuario(id_usuario),
        CONSTRAINT FK_UsuarioClinica_Clinica FOREIGN KEY (id_clinica) REFERENCES dbo.Clinica(id_clinica),
        CONSTRAINT UQ_Usuario_Clinica UNIQUE (id_usuario, id_clinica)
    );
END
GO

-- ---------- 2. Seed inicial ----------
DECLARE @IdHospital UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @IdClinica UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
IF NOT EXISTS (SELECT 1 FROM dbo.Hospital WHERE id_hospital = @IdHospital)
    INSERT INTO dbo.Hospital (id_hospital, nombre, activo, fecha_creacion)
    VALUES (@IdHospital, 'Hospital de Antigua', 1, SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM dbo.Clinica WHERE id_clinica = @IdClinica)
    INSERT INTO dbo.Clinica (id_clinica, id_hospital, nombre, activa, fecha_creacion)
    VALUES (@IdClinica, @IdHospital, 'Traumatolog' + CHAR(237) + 'a', 1, SYSUTCDATETIME());

-- ---------- 3. Columnas de tenant (idempotente) ----------
-- Operativas por clínica:
IF COL_LENGTH('dbo.Cita', 'id_clinica') IS NULL ALTER TABLE dbo.Cita ADD id_clinica UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Consulta', 'id_clinica') IS NULL ALTER TABLE dbo.Consulta ADD id_clinica UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Orden_estudio', 'id_clinica') IS NULL ALTER TABLE dbo.Orden_estudio ADD id_clinica UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Nota_medica', 'id_clinica') IS NULL ALTER TABLE dbo.Nota_medica ADD id_clinica UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Estudio_imagen', 'id_clinica') IS NULL ALTER TABLE dbo.Estudio_imagen ADD id_clinica UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Venta', 'id_clinica') IS NULL ALTER TABLE dbo.Venta ADD id_clinica UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Gasto', 'id_clinica') IS NULL ALTER TABLE dbo.Gasto ADD id_clinica UNIQUEIDENTIFIER NULL;
-- Venta/Gasto además llevan hospital (folios por hospital):
IF COL_LENGTH('dbo.Venta', 'id_hospital') IS NULL ALTER TABLE dbo.Venta ADD id_hospital UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Gasto', 'id_hospital') IS NULL ALTER TABLE dbo.Gasto ADD id_hospital UNIQUEIDENTIFIER NULL;
-- Catálogos/inventario por hospital (stock único compartido):
IF COL_LENGTH('dbo.Producto', 'id_hospital') IS NULL ALTER TABLE dbo.Producto ADD id_hospital UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Lote_producto', 'id_hospital') IS NULL ALTER TABLE dbo.Lote_producto ADD id_hospital UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Movimiento_inventario', 'id_hospital') IS NULL ALTER TABLE dbo.Movimiento_inventario ADD id_hospital UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Motivo_cobro', 'id_hospital') IS NULL ALTER TABLE dbo.Motivo_cobro ADD id_hospital UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Categoria_producto', 'id_hospital') IS NULL ALTER TABLE dbo.Categoria_producto ADD id_hospital UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Categoria_gasto', 'id_hospital') IS NULL ALTER TABLE dbo.Categoria_gasto ADD id_hospital UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Metodo_pago', 'id_hospital') IS NULL ALTER TABLE dbo.Metodo_pago ADD id_hospital UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Catalogo_indicacion', 'id_hospital') IS NULL ALTER TABLE dbo.Catalogo_indicacion ADD id_hospital UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Medicamento', 'id_hospital') IS NULL ALTER TABLE dbo.Medicamento ADD id_hospital UNIQUEIDENTIFIER NULL;
-- Paciente global: hospital de creación, SOLO informativo (sin filtro de tenant).
IF COL_LENGTH('dbo.Paciente', 'id_hospital_creacion') IS NULL ALTER TABLE dbo.Paciente ADD id_hospital_creacion UNIQUEIDENTIFIER NULL;
GO

-- ---------- 4. Backfill: todo lo existente -> Antigua/Traumatología ----------
DECLARE @IdHospital UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @IdClinica UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
UPDATE dbo.Cita SET id_clinica = @IdClinica WHERE id_clinica IS NULL;
UPDATE dbo.Consulta SET id_clinica = @IdClinica WHERE id_clinica IS NULL;
UPDATE dbo.Orden_estudio SET id_clinica = @IdClinica WHERE id_clinica IS NULL;
UPDATE dbo.Nota_medica SET id_clinica = @IdClinica WHERE id_clinica IS NULL;
UPDATE dbo.Estudio_imagen SET id_clinica = @IdClinica WHERE id_clinica IS NULL;
UPDATE dbo.Venta SET id_clinica = @IdClinica, id_hospital = @IdHospital WHERE id_clinica IS NULL OR id_hospital IS NULL;
UPDATE dbo.Gasto SET id_clinica = @IdClinica, id_hospital = @IdHospital WHERE id_clinica IS NULL OR id_hospital IS NULL;
UPDATE dbo.Producto SET id_hospital = @IdHospital WHERE id_hospital IS NULL;
UPDATE dbo.Lote_producto SET id_hospital = @IdHospital WHERE id_hospital IS NULL;
UPDATE dbo.Movimiento_inventario SET id_hospital = @IdHospital WHERE id_hospital IS NULL;
UPDATE dbo.Motivo_cobro SET id_hospital = @IdHospital WHERE id_hospital IS NULL;
UPDATE dbo.Categoria_producto SET id_hospital = @IdHospital WHERE id_hospital IS NULL;
UPDATE dbo.Categoria_gasto SET id_hospital = @IdHospital WHERE id_hospital IS NULL;
UPDATE dbo.Metodo_pago SET id_hospital = @IdHospital WHERE id_hospital IS NULL;
UPDATE dbo.Catalogo_indicacion SET id_hospital = @IdHospital WHERE id_hospital IS NULL;
UPDATE dbo.Medicamento SET id_hospital = @IdHospital WHERE id_hospital IS NULL;
-- Pacientes existentes: creados en Hospital de Antigua (informativo).
UPDATE dbo.Paciente SET id_hospital_creacion = @IdHospital WHERE id_hospital_creacion IS NULL;
GO

-- ---------- 5. Accesos: todos los usuarios activos -> Traumatología (default) ----------
-- Decisión: todos quedan God temporalmente (permisos intactos); depurar SuperAdmin desde /God luego.
DECLARE @IdClinica2 UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
INSERT INTO dbo.Usuario_clinica (id_usuario_clinica, id_usuario, id_clinica, es_default, activo, fecha_creacion)
SELECT NEWID(), u.id_usuario, @IdClinica2, 1, 1, SYSUTCDATETIME()
FROM dbo.Usuario u
WHERE u.estado_eliminado = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.Usuario_clinica uc WHERE uc.id_usuario = u.id_usuario AND uc.id_clinica = @IdClinica2);

-- ---------- 6. Endurecer (opcional, tras verificar backfill) ----------
-- Descomentar cuando las columnas ya no tengan NULLs:
-- ALTER TABLE dbo.Cita ALTER COLUMN id_clinica UNIQUEIDENTIFIER NOT NULL;
-- ALTER TABLE dbo.Consulta ALTER COLUMN id_clinica UNIQUEIDENTIFIER NOT NULL;
-- ALTER TABLE dbo.Venta ALTER COLUMN id_clinica UNIQUEIDENTIFIER NOT NULL;
-- ALTER TABLE dbo.Venta ALTER COLUMN id_hospital UNIQUEIDENTIFIER NOT NULL;
-- NOTA folios: UQ_Venta_Folio hoy es global. Para folio por hospital con prefijo,
-- mantenerla global por ahora; si quieres correlativo por hospital, crear secuencia por hospital
-- y cambiar a UNIQUE(id_hospital, folio) en una segunda ventana.
