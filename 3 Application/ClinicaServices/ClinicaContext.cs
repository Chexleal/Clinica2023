using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public partial class ClinicaContext : DbContext
{
    private readonly ICurrentUser? _currentUser;

    public ClinicaContext()
    {
    }

    public ClinicaContext(DbContextOptions<ClinicaContext> options)
        : base(options)
    {
        //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public ClinicaContext(DbContextOptions<ClinicaContext> options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public virtual DbSet<Hospital> Hospitales { get; set; }

    public virtual DbSet<Clinica> Clinicas { get; set; }

    public virtual DbSet<UsuarioClinica> UsuarioClinicas { get; set; }

    public virtual DbSet<Cotizacion> Cotizaciones { get; set; }

    public virtual DbSet<CotizacionDetalle> CotizacionDetalles { get; set; }

    public virtual DbSet<Cita> Cita { get; set; }

    public virtual DbSet<Consulta> Consulta { get; set; }

    public virtual DbSet<DetalleCobro> DetalleCobros { get; set; }

    public virtual DbSet<DetalleReceta> DetalleReceta { get; set; }

    public virtual DbSet<MotivoCobro> MotivoCobros { get; set; }

    public virtual DbSet<Paciente> Pacientes { get; set; }

    public virtual DbSet<Receta> Receta { get; set; }

    public virtual DbSet<RolDetalle> RolDetalles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Medicamento> Medicamento { get; set; }
    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<EstudioImagen> EstudiosImagen { get; set; }
    public virtual DbSet<ArchivoEstudio> ArchivosEstudio { get; set; }
    public virtual DbSet<OrdenEstudio> OrdenesEstudio { get; set; }
    public virtual DbSet<NotaMedica> NotasMedicas { get; set; }
    public virtual DbSet<CatalogoIndicacion> CatalogoIndicaciones { get; set; }
    public virtual DbSet<CategoriaProducto> CategoriasProducto { get; set; }
    public virtual DbSet<Producto> Productos { get; set; }
    public virtual DbSet<LoteProducto> LotesProducto { get; set; }
    public virtual DbSet<MovimientoInventario> MovimientosInventario { get; set; }
    public virtual DbSet<Venta> Ventas { get; set; }
    public virtual DbSet<VentaDetalle> VentaDetalles { get; set; }
    public virtual DbSet<MetodoPago> MetodosPago { get; set; }
    public virtual DbSet<VentaPago> VentaPagos { get; set; }
    public virtual DbSet<CategoriaGasto> CategoriasGasto { get; set; }
    public virtual DbSet<Gasto> Gastos { get; set; }
    public virtual DbSet<Aseguradora> Aseguradoras { get; set; }
    public virtual DbSet<AseguradoraTarifa> AseguradoraTarifas { get; set; }

    public override int SaveChanges()
    {
        AplicarAuditoria();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AplicarAuditoria();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AplicarAuditoria();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AplicarAuditoria();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AplicarAuditoria()
    {
        var ahora = DateTime.UtcNow;
        var usuarioId = _currentUser?.Usuario?.IdUsuario;
        AplicarTenant();

        foreach (var entry in ChangeTracker.Entries<Base>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.FechaCreacion ??= ahora;
                entry.Entity.CreadoPor ??= usuarioId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.FechaModificacion = ahora;
                entry.Entity.ModificadoPor = usuarioId;

                var estadoEliminado = entry.Metadata.FindProperty(nameof(Paciente.EstadoEliminado));
                if (estadoEliminado is not null
                    && entry.Property<bool>(estadoEliminado.Name).CurrentValue
                    && entry.Entity.FechaEliminacion is null)
                {
                    entry.Entity.FechaEliminacion = ahora;
                    entry.Entity.EliminadoPor = usuarioId;
                }
            }
        }
    }

    /// <summary>Ids de seed inicial (Hospital de Antigua / Clínica Traumatología).</summary>
    public static readonly Guid HospitalAntiguaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ClinicaTraumatologiaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private Guid TenantClinicaId => _currentUser?.Usuario?.ClinicaId ?? Guid.Empty;

    private Guid TenantHospitalId => _currentUser?.Usuario?.HospitalId ?? Guid.Empty;

    private bool BypassTenant =>
        _currentUser?.Usuario is null || _currentUser.Usuario.BypassTenant;

    private bool BypassHospitalTenant =>
        _currentUser?.Usuario is null
        || (_currentUser.Usuario.EsSuperAdmin && _currentUser.Usuario.HospitalId is null);

    /// <summary>Asigna IdClinica/IdHospital automáticamente al crear filas.</summary>
    private void AplicarTenant()
    {
        var clinicaId = _currentUser?.Usuario?.ClinicaId;
        var hospitalId = _currentUser?.Usuario?.HospitalId;

        // Si el usuario opera en una clínica pero el claim de hospital viene vacío,
        // se resuelve vía la clínica (necesario para catálogos/inventario por hospital).
        Guid? hospitalResuelto = hospitalId;
        if (clinicaId.HasValue && !hospitalResuelto.HasValue)
        {
            hospitalResuelto = Clinicas
                .Where(c => c.IdClinica == clinicaId.Value)
                .Select(c => (Guid?)c.IdHospital)
                .FirstOrDefault();
        }

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added)
                continue;

            if (entry.Entity is ClinicaDomain.IClinicaTenant conClinica
                && conClinica.IdClinica == Guid.Empty && clinicaId.HasValue)
            {
                conClinica.IdClinica = clinicaId.Value;
            }

            if (entry.Entity is ClinicaDomain.IHospitalTenant conHospital
                && conHospital.IdHospital == Guid.Empty && hospitalResuelto.HasValue)
            {
                conHospital.IdHospital = hospitalResuelto.Value;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cita>(entity =>
        {
            entity.HasKey(e => e.IdCita).HasName("PK__Cita__6AEC3C097CEA9947");

            entity.Property(e => e.IdCita)
                .ValueGeneratedNever()
                .HasColumnName("id_cita");
            entity.Property(e => e.FechaHora)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Titulo).HasMaxLength(50)
                .IsUnicode(false).HasColumnName("titulo");

        });

        modelBuilder.Entity<Consulta>(entity =>
        {
            entity.HasKey(e => e.IdConsulta).HasName("PK__Consulta__6F53588B6FB55DAE");

            entity.Property(e => e.IdConsulta)
                .ValueGeneratedNever()
                .HasColumnName("id_consulta");
            entity.Property(e => e.Diagnostico)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("diagnostico");
            entity.Property(e => e.HistoriaClinica)
               .HasMaxLength(1000)
               .IsUnicode(false)
               .HasColumnName("historia_clinica");
            entity.Property(e => e.Fecha)
                .HasColumnName("fecha");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.MotivoConsulta)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("motivo_consulta");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("observaciones");
            entity.Property(e => e.Pagada).HasColumnName("pagada");
            entity.Property(e => e.Peso)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("peso");
            entity.Property(e => e.PresionArterial)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("presion_arterial");
            entity.Property(e => e.Radiografias).HasColumnName("radiografias");
            entity.Property(e => e.Temperatura)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("temperatura");
            entity.Property(e => e.SaturacionOxigeno)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("saturacion_oxigeno");
            entity.Property(e => e.Glucometro)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("glucometro");
            entity.Property(e => e.FrecuenciaCardiaca)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("frecuencia_cardiaca");
            entity.Property(e => e.Terminada).HasColumnName("terminada");
            entity.Property(e => e.Eliminada).HasColumnName("eliminada");
            entity.Property(e => e.TiempoDuracion).HasMaxLength(25)
                .IsUnicode(false).HasColumnName("tiempo_duracion");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.PacienteInformacion).WithMany(p => p.Consulta)
                .HasForeignKey(d => d.IdPaciente)
                .HasConstraintName("FK__Consulta__id_pac__300424B4");

        });

        modelBuilder.Entity<DetalleCobro>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCobro).HasName("PK__Detalle___2764DD4706948581");

            entity.ToTable("Detalle_cobro");

            entity.Property(e => e.IdDetalleCobro)
                .ValueGeneratedNever()
                .HasColumnName("id_detalle_cobro");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.IdMotivoCobro).HasColumnName("id_motivo_cobro");
            entity.Property(e => e.Producto)
                .HasMaxLength(75)
                .IsUnicode(false)
                .HasColumnName("producto");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Valor)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("valor");
            entity.Property(e => e.Cantidad)
                .HasColumnType("numeric(18, 0)")
                .HasColumnName("cantidad");
            entity.Property(e => e.NombreServicio)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre_servicio");

            //entity.HasOne(d => d.IdConsultaNavigation).WithMany(p => p.DetalleCobros)
            //    .HasForeignKey(d => d.IdConsulta)
            //    .HasConstraintName("FK__Detalle_c__id_co__34C8D9D1");

            //entity.HasOne(d => d.IdMotivoCobroNavigation).WithMany(p => p.DetalleCobros)
            //    .HasForeignKey(d => d.IdMotivoCobro)
            //    .HasConstraintName("FK__Detalle_c__id_mo__35BCFE0A");
        });

        modelBuilder.Entity<DetalleReceta>(entity =>
        {
            entity.HasKey(e => e.IdDetalleReceta).HasName("PK__Detalle___2C99ACD3657294D0");

            entity.ToTable("Detalle_receta");

            entity.Property(e => e.IdDetalleReceta)
                .ValueGeneratedNever()
                .HasColumnName("id_detalle_receta");
            entity.Property(e => e.Medicamento)
               .HasMaxLength(100)
               .IsUnicode(false)
               .HasColumnName("medicamento");
            entity.Property(e => e.DosisDias)
               .HasMaxLength(50)
               .IsUnicode(false)
               .HasColumnName("dosis_dia");
            entity.Property(e => e.DosisTiempo)
               .HasMaxLength(50)
               .IsUnicode(false)
               .HasColumnName("dosis_tiempo");
            entity.Property(e => e.Instrucciones)
               .HasMaxLength(250)
               .IsUnicode(false)
               .HasColumnName("instrucciones");
            entity.Property(e => e.Cantidad)
           .HasMaxLength(100)
              .IsUnicode(false)
           .HasColumnName("cantidad_med");

            entity.Property(e => e.IdReceta).HasColumnName("id_receta");

            //entity.HasOne(d => d.IdRecetaNavigation).WithMany(p => p.DetalleReceta)
            //    .HasForeignKey(d => d.IdReceta)
            //    .HasConstraintName("FK__Detalle_r__id_re__3B75D760");
        });

        modelBuilder.Entity<MotivoCobro>(entity =>
        {
            entity.HasKey(e => e.IdMotivoCobro).HasName("PK__Motivo_c__A9C6114AD5B37C04");

            entity.ToTable("Motivo_cobro");

            entity.Property(e => e.IdMotivoCobro)
                .ValueGeneratedNever()
                .HasColumnName("id_motivo_cobro");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Area).HasMaxLength(100).IsUnicode(false).HasColumnName("area");
            entity.Property(e => e.EstadoEliminado).HasColumnName("estado_eliminado");
            entity.Property(e => e.PrecioSugerido)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("precio_sugerido");
            entity.Property(e => e.PrecioEmergenciaGeneral).HasColumnType("decimal(15, 2)").HasColumnName("precio_emergencia_general");
            entity.Property(e => e.IdCategoriaProducto).HasColumnName("id_categoria_producto");
            entity.HasIndex(e => e.IdCategoriaProducto).HasDatabaseName("IX_MotivoCobro_CategoriaProducto");

        });

        modelBuilder.Entity<Aseguradora>(entity =>
        {
            entity.HasKey(e => e.IdAseguradora).HasName("PK_Aseguradora");
            entity.ToTable("Aseguradora");
            entity.Property(e => e.IdAseguradora).ValueGeneratedNever().HasColumnName("id_aseguradora");
            entity.Property(e => e.IdHospital).HasColumnName("id_hospital");
            entity.Property(e => e.Nombre).HasMaxLength(150).IsUnicode(false).HasColumnName("nombre").IsRequired();
            entity.Property(e => e.IdentificadorFiscal).HasMaxLength(50).IsUnicode(false).HasColumnName("identificador_fiscal");
            entity.Property(e => e.Contacto).HasMaxLength(200).IsUnicode(false).HasColumnName("contacto");
            entity.Property(e => e.CopagoDefault).HasColumnType("decimal(15, 2)").HasColumnName("copago_default");
            entity.Property(e => e.CoaseguroPorcDefault).HasColumnType("decimal(5, 2)").HasColumnName("coaseguro_porc_default");
            entity.Property(e => e.Activa).HasColumnName("activa");
            entity.HasIndex(e => new { e.IdHospital, e.Nombre }).IsUnique().HasDatabaseName("UQ_Aseguradora_Hospital_Nombre");
        });

        modelBuilder.Entity<AseguradoraTarifa>(entity =>
        {
            entity.HasKey(e => e.IdAseguradoraTarifa).HasName("PK_AseguradoraTarifa");
            entity.ToTable("Aseguradora_tarifa");
            entity.Property(e => e.IdAseguradoraTarifa).ValueGeneratedNever().HasColumnName("id_aseguradora_tarifa");
            entity.Property(e => e.IdHospital).HasColumnName("id_hospital");
            entity.Property(e => e.IdAseguradora).HasColumnName("id_aseguradora");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdMotivoCobro).HasColumnName("id_motivo_cobro");
            entity.Property(e => e.PrecioConvenido).HasColumnType("decimal(15, 2)").HasColumnName("precio_convenido");
            entity.Property(e => e.PrecioEmergencia).HasColumnType("decimal(15, 2)").HasColumnName("precio_emergencia");
            entity.Property(e => e.Activa).HasColumnName("activa");
            entity.HasIndex(e => new { e.IdAseguradora, e.IdProducto }).IsUnique().HasFilter("[id_producto] IS NOT NULL").HasDatabaseName("UQ_AseguradoraTarifa_Producto");
            entity.HasIndex(e => new { e.IdAseguradora, e.IdMotivoCobro }).IsUnique().HasFilter("[id_motivo_cobro] IS NOT NULL").HasDatabaseName("UQ_AseguradoraTarifa_Servicio");
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(e => e.IdPaciente).HasName("PK__Paciente__2C2C72BB3843A1F5");

            entity.ToTable("Paciente");

            entity.Property(e => e.IdPaciente)
                .ValueGeneratedNever()
                .HasColumnName("id_paciente");
            entity.Property(e => e.Alergias)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("alergias");
            entity.Property(e => e.Antecedentes)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("antecedentes");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Correo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Direccion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.IdentificadorFiscal).HasMaxLength(50).IsUnicode(false).HasColumnName("identificador_fiscal");
            entity.Property(e => e.Dpi)
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasColumnName("dpi");
            entity.Property(e => e.EstadoCivil)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("estado_civil");
            entity.Property(e => e.EstadoEliminado).HasColumnName("estado_eliminado");
            entity.Property(e => e.FechaNacimiento)
                .HasColumnType("date")
                .HasColumnName("fecha_nacimiento");
            entity.Property(e => e.Genero)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("genero");
            entity.Property(e => e.Nacionalidad)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("nacionalidad");
            entity.Property(e => e.NoRegistro)
              .HasMaxLength(25)
              .IsUnicode(false)
              .HasColumnName("no_registro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Profesion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("profesion");
            entity.Property(e => e.Remitido)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("remitido");
            entity.Property(e => e.Telefono)
             .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoSange)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("tipo_sange");

        });

        modelBuilder.Entity<Receta>(entity =>
        {
            entity.HasKey(e => e.IdReceta).HasName("PK__Receta__11DB53AB1367EF10");

            entity.Property(e => e.IdReceta)
                .ValueGeneratedNever()
                .HasColumnName("id_receta");
            entity.Property(e => e.Fecha)
                .HasColumnType("date")
                .HasColumnName("fecha");
            entity.Property(e => e.Descripcion)
              .HasMaxLength(300)
              .IsUnicode(false)
              .HasColumnName("descripcion");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");

            //entity.HasOne(d => d.IdConsultaNavigation).WithMany(p => p.Receta)
            //    .HasForeignKey(d => d.IdConsulta)
            //    .HasConstraintName("FK__Receta__id_consu__38996AB5");
        });

        modelBuilder.Entity<RolDetalle>(entity =>
        {
            entity.HasKey(e => e.IdRolDetalle).HasName("PK__Rol_deta__2B721F85E6A17CE6");

            entity.ToTable("Rol_detalle");

            entity.Property(e => e.IdRolDetalle)
                .ValueGeneratedNever()
                .HasColumnName("id_rol_detalle");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.UsuarioId).HasColumnName("id_usuario");
            entity.Property(e => e.Permiso)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("permiso");
        });

        modelBuilder.Entity<Medicamento>(entity =>
        {
            entity.HasKey(e => e.IdMedicamento);

            entity.Property(e => e.IdMedicamento)
                .ValueGeneratedNever()
                .HasColumnName("id_medicamento");
            entity.Property(e => e.Nombre)
                .HasMaxLength(00)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__4E3E04ADDFC08DEF");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.NombreUsuario, "UQ__Usuario__D4D22D7402387FED").IsUnique();

            entity.Property(e => e.IdUsuario)
                .ValueGeneratedNever()
                .HasColumnName("id_usuario");
            entity.Property(e => e.Antecedentes)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("antecedentes");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Correo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Dpi)
                .HasMaxLength(16)
                .IsUnicode(false)
                .HasColumnName("dpi");
            entity.Property(e => e.EstadoCivil)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("estado_civil");
            entity.Property(e => e.EstadoEliminado).HasColumnName("estado_eliminado");
            entity.Property(e => e.FechaNacimiento)
                .HasColumnType("date")
                .HasColumnName("fecha_nacimiento");
            entity.Property(e => e.Nacionalidad)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("nacionalidad");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.Password)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PreguntaSeg)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("pregunta_seg");
            entity.Property(e => e.Profesion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("profesion");
            entity.Property(e => e.RespuestaSeg)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("respuesta_seg");
            entity.Property(e => e.Telefono).HasColumnName("telefono");
            entity.Property(e => e.TipoSange)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("tipo_sange");
            entity.Property(e => e.UsuarioActivo).HasColumnName("usuario_activo");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.IdErrorLog);
            entity.ToTable("Error_log");
            entity.Property(e => e.IdErrorLog)
                .ValueGeneratedNever()
                .HasColumnName("id_error_log");
            entity.Property(e => e.TipoError).HasMaxLength(50).IsUnicode(false).HasColumnName("tipo_error");
            entity.Property(e => e.Mensaje).HasMaxLength(500).IsUnicode(false).HasColumnName("mensaje");
            entity.Property(e => e.Detalle).HasMaxLength(4000).IsUnicode(false).HasColumnName("detalle");
            entity.Property(e => e.StackTrace).HasMaxLength(8000).IsUnicode(false).HasColumnName("stack_trace");
            entity.Property(e => e.Ruta).HasMaxLength(500).IsUnicode(false).HasColumnName("ruta");
            entity.Property(e => e.MetodoHttp).HasMaxLength(20).IsUnicode(false).HasColumnName("metodo_http");
            entity.Property(e => e.TraceIdentifier).HasMaxLength(100).IsUnicode(false).HasColumnName("trace_identifier");
            entity.Property(e => e.Nivel).HasMaxLength(20).IsUnicode(false).HasColumnName("nivel");
            entity.Property(e => e.Resuelto).HasColumnName("resuelto");
            entity.Property(e => e.Observaciones).HasMaxLength(1000).IsUnicode(false).HasColumnName("observaciones");
        });

        modelBuilder.Entity<OrdenEstudio>(entity =>
        {
            entity.HasKey(e => e.IdOrden).HasName("PK_OrdenEstudio");
            entity.ToTable("Orden_estudio");
            entity.Property(e => e.IdOrden).ValueGeneratedNever().HasColumnName("id_orden");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.Indicacion).HasMaxLength(500).IsUnicode(false).HasColumnName("indicacion");
            entity.Property(e => e.FechaOrden).HasColumnName("fecha_orden");
            entity.Property(e => e.EsExterna).HasColumnName("es_externa").HasDefaultValue(false);
            entity.HasOne(d => d.Paciente).WithMany().HasForeignKey(d => d.IdPaciente).HasConstraintName("FK_Orden_Paciente");
            entity.HasOne(d => d.Consulta).WithMany().HasForeignKey(d => d.IdConsulta).IsRequired(false).HasConstraintName("FK_Orden_Consulta");
        });

        modelBuilder.Entity<NotaMedica>(entity =>
        {
            entity.HasKey(e => e.IdNotaMedica).HasName("PK_NotaMedica");
            entity.ToTable("Nota_medica");
            entity.Property(e => e.IdNotaMedica).ValueGeneratedNever().HasColumnName("id_nota_medica");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.Motivo).HasMaxLength(500).IsUnicode(false).HasColumnName("motivo");
            entity.Property(e => e.IncluirMotivo).HasColumnName("incluir_motivo").HasDefaultValue(true);
            entity.Property(e => e.IncluirDiagnostico).HasColumnName("incluir_diagnostico").HasDefaultValue(true);
            entity.Property(e => e.Contenido).HasMaxLength(4000).IsUnicode(false).HasColumnName("contenido");
            entity.Property(e => e.FechaNota).HasColumnName("fecha_nota");
            entity.HasOne(d => d.Paciente).WithMany().HasForeignKey(d => d.IdPaciente).HasConstraintName("FK_Nota_Paciente");
            entity.HasOne(d => d.Consulta).WithMany().HasForeignKey(d => d.IdConsulta).IsRequired(false).HasConstraintName("FK_Nota_Consulta");
            entity.HasIndex(e => e.IdConsulta).HasDatabaseName("IX_Nota_Consulta");
        });

        modelBuilder.Entity<EstudioImagen>(entity =>
        {
            entity.HasKey(e => e.IdEstudio).HasName("PK_EstudioImagen");
            entity.ToTable("Estudio_imagen");
            entity.Property(e => e.IdEstudio).ValueGeneratedNever().HasColumnName("id_estudio");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.IdOrden).HasColumnName("id_orden");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
            entity.Property(e => e.Modalidad).HasColumnName("modalidad");
            entity.Property(e => e.Titulo).HasMaxLength(200).IsUnicode(false).HasColumnName("titulo");
            entity.Property(e => e.Descripcion).HasMaxLength(1000).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.FechaEstudio).HasColumnName("fecha_estudio");
            entity.HasOne(d => d.Paciente).WithMany().HasForeignKey(d => d.IdPaciente).HasConstraintName("FK_Estudio_Paciente");
            entity.HasOne(d => d.Consulta).WithMany().HasForeignKey(d => d.IdConsulta).IsRequired(false).HasConstraintName("FK_Estudio_Consulta");
            entity.HasOne(d => d.Orden).WithMany(p => p.Estudios).HasForeignKey(d => d.IdOrden).IsRequired(false).HasConstraintName("FK_Estudio_Orden");
            entity.HasIndex(e => new { e.IdPaciente, e.FechaEstudio }).HasDatabaseName("IX_Estudio_Paciente_Fecha");
        });

        modelBuilder.Entity<ArchivoEstudio>(entity =>
        {
            entity.HasKey(e => e.IdArchivo).HasName("PK_ArchivoEstudio");
            entity.ToTable("Archivo_estudio");
            entity.Property(e => e.IdArchivo).ValueGeneratedNever().HasColumnName("id_archivo");
            entity.Property(e => e.IdEstudio).HasColumnName("id_estudio");
            entity.Property(e => e.NombreOriginal).HasMaxLength(255).IsUnicode(false).HasColumnName("nombre_original");
            entity.Property(e => e.RutaStorage).HasMaxLength(500).IsUnicode(false).HasColumnName("ruta_storage");
            entity.Property(e => e.MimeType).HasMaxLength(100).IsUnicode(false).HasColumnName("mime_type");
            entity.Property(e => e.TamanoBytes).HasColumnName("tamano_bytes");
            entity.Property(e => e.TransferSyntax).HasMaxLength(100).IsUnicode(false).HasColumnName("transfer_syntax");
            entity.Property(e => e.NumeroSerie).HasColumnName("numero_serie");
            entity.Property(e => e.NumeroInstancia).HasColumnName("numero_instancia");
            entity.HasOne(d => d.Estudio).WithMany(p => p.Archivos).HasForeignKey(d => d.IdEstudio).HasConstraintName("FK_Archivo_Estudio");
        });

        modelBuilder.Entity<CatalogoIndicacion>(entity =>
        {
            entity.HasKey(e => e.IdCatalogo).HasName("PK_CatalogoIndicacion");
            entity.ToTable("Catalogo_indicacion");
            entity.Property(e => e.IdCatalogo).ValueGeneratedNever().HasColumnName("id_catalogo");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
            entity.Property(e => e.Codigo).HasMaxLength(50).IsUnicode(false).HasColumnName("codigo");
            entity.Property(e => e.Descripcion).HasMaxLength(300).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.HasIndex(e => new { e.Tipo, e.Activo }).HasDatabaseName("IX_Catalogo_Tipo_Activo");
        });

        modelBuilder.Entity<CategoriaProducto>(entity =>
        {
            entity.HasKey(e => e.IdCategoriaProducto).HasName("PK_CategoriaProducto");
            entity.ToTable("Categoria_producto");
            entity.Property(e => e.IdCategoriaProducto).ValueGeneratedNever().HasColumnName("id_categoria_producto");
            entity.Property(e => e.Nombre).HasMaxLength(120).IsUnicode(false).HasColumnName("nombre");
            entity.Property(e => e.Tipo).HasMaxLength(20).IsUnicode(false).HasColumnName("tipo");
            entity.Property(e => e.ExigeLoteDefault).HasColumnName("exige_lote_default");
            entity.Property(e => e.ExigeVencimientoDefault).HasColumnName("exige_vencimiento_default");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.Orden).HasColumnName("orden");
            entity.HasIndex(e => new { e.IdHospital, e.Orden }).HasDatabaseName("IX_CategoriaProducto_Hospital_Orden");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK_Producto");
            entity.ToTable("Producto");
            entity.Property(e => e.IdProducto).ValueGeneratedNever().HasColumnName("id_producto");
            entity.Property(e => e.Sku).HasMaxLength(50).IsUnicode(false).HasColumnName("sku");
            entity.Property(e => e.Nombre).HasMaxLength(200).IsUnicode(false).HasColumnName("nombre");
            entity.Property(e => e.IdCategoriaProducto).HasColumnName("id_categoria_producto");
            entity.Property(e => e.UnidadMedida).HasMaxLength(30).IsUnicode(false).HasColumnName("unidad_medida");
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(15, 2)").HasColumnName("precio_venta");
            entity.Property(e => e.PrecioEmergenciaGeneral).HasColumnType("decimal(15, 2)").HasColumnName("precio_emergencia_general");
            entity.Property(e => e.CostoUltimo).HasColumnType("decimal(15, 2)").HasColumnName("costo_ultimo");
            entity.Property(e => e.StockActual).HasColumnType("decimal(18, 2)").HasColumnName("stock_actual");
            entity.Property(e => e.StockMinimo).HasColumnType("decimal(18, 2)").HasColumnName("stock_minimo");
            entity.Property(e => e.RequiereLote).HasColumnName("requiere_lote");
            entity.Property(e => e.RequiereVencimiento).HasColumnName("requiere_vencimiento");
            entity.Property(e => e.EsSobrePedido).HasColumnName("es_sobre_pedido");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.HasIndex(e => e.Nombre).HasDatabaseName("IX_Producto_Nombre");
        });

        modelBuilder.Entity<LoteProducto>(entity =>
        {
            entity.HasKey(e => e.IdLote).HasName("PK_LoteProducto");
            entity.ToTable("Lote_producto");
            entity.Property(e => e.IdLote).ValueGeneratedNever().HasColumnName("id_lote");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.CodigoLote).HasMaxLength(80).IsUnicode(false).HasColumnName("codigo_lote");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.Stock).HasColumnType("decimal(18, 2)").HasColumnName("stock");
            entity.Property(e => e.CostoUnitario).HasColumnType("decimal(15, 2)").HasColumnName("costo_unitario");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.HasIndex(e => new { e.IdProducto, e.FechaVencimiento }).HasDatabaseName("IX_Lote_Producto_Vence");
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento).HasName("PK_MovimientoInventario");
            entity.ToTable("Movimiento_inventario");
            entity.Property(e => e.IdMovimiento).ValueGeneratedNever().HasColumnName("id_movimiento");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdLote).HasColumnName("id_lote");
            entity.Property(e => e.Tipo).HasMaxLength(30).IsUnicode(false).HasColumnName("tipo");
            entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 2)").HasColumnName("cantidad");
            entity.Property(e => e.CostoUnitario).HasColumnType("decimal(15, 2)").HasColumnName("costo_unitario");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.Motivo).HasMaxLength(300).IsUnicode(false).HasColumnName("motivo");
            entity.HasIndex(e => new { e.IdProducto, e.Fecha }).HasDatabaseName("IX_Movimiento_Producto_Fecha");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PK_Venta");
            entity.ToTable("Venta");
            entity.Property(e => e.IdVenta).ValueGeneratedNever().HasColumnName("id_venta");
            entity.Property(e => e.Folio).HasMaxLength(30).IsUnicode(false).HasColumnName("folio");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.IdConsulta).HasColumnName("id_consulta");
            entity.Property(e => e.Total).HasColumnType("decimal(15, 2)").HasColumnName("total");
            entity.Property(e => e.Estado).HasMaxLength(20).IsUnicode(false).HasColumnName("estado");
            entity.Property(e => e.Observaciones).HasMaxLength(300).IsUnicode(false).HasColumnName("observaciones");
            entity.Property(e => e.FiadoResponsable).HasMaxLength(200).IsUnicode(false).HasColumnName("fiado_responsable");
            entity.Property(e => e.FechaPromesa).HasColumnName("fecha_promesa");
            entity.Property(e => e.IdAseguradora).HasColumnName("id_aseguradora");
            entity.Property(e => e.PolizaCertificado).HasMaxLength(100).IsUnicode(false).HasColumnName("poliza_certificado");
            entity.Property(e => e.Autorizacion).HasMaxLength(100).IsUnicode(false).HasColumnName("autorizacion");
            entity.Property(e => e.ServicioAtencion).HasMaxLength(150).IsUnicode(false).HasColumnName("servicio_atencion");
            entity.Property(e => e.TipoAtencion).HasConversion<string>().HasMaxLength(20).IsUnicode(false).HasColumnName("tipo_atencion").HasDefaultValue(TipoAtencion.Normal);
            entity.Property(e => e.Copago).HasColumnType("decimal(15, 2)").HasColumnName("copago");
            entity.Property(e => e.CoaseguroPorc).HasColumnType("decimal(5, 2)").HasColumnName("coaseguro_porc");
            entity.HasIndex(e => e.Folio).IsUnique().HasDatabaseName("UQ_Venta_Folio");
            entity.HasIndex(e => e.IdConsulta).HasDatabaseName("IX_Venta_Consulta");
        });

        modelBuilder.Entity<VentaDetalle>(entity =>
        {
            entity.HasKey(e => e.IdVentaDetalle).HasName("PK_VentaDetalle");
            entity.ToTable("Venta_detalle");
            entity.Property(e => e.IdVentaDetalle).ValueGeneratedNever().HasColumnName("id_venta_detalle");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.TipoLinea).HasMaxLength(20).IsUnicode(false).HasColumnName("tipo_linea");
            entity.Property(e => e.IdMotivoCobro).HasColumnName("id_motivo_cobro");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdLote).HasColumnName("id_lote");
            entity.Property(e => e.Descripcion).HasMaxLength(250).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 2)").HasColumnName("cantidad");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(15, 2)").HasColumnName("precio_unitario");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(15, 2)").HasColumnName("subtotal");
            entity.Property(e => e.DescuentoMonto).HasColumnType("decimal(15, 2)").HasColumnName("descuento_monto");
            entity.Property(e => e.DescuentoMotivo).HasMaxLength(200).IsUnicode(false).HasColumnName("descuento_motivo");
            entity.Property(e => e.DescuentoOtorgadoPor).HasColumnName("descuento_otorgado_por");
            entity.Property(e => e.EsSobrePedido).HasColumnName("es_sobre_pedido");
        });

        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago).HasName("PK_MetodoPago");
            entity.ToTable("Metodo_pago");
            entity.Property(e => e.IdMetodoPago).ValueGeneratedNever().HasColumnName("id_metodo_pago");
            entity.Property(e => e.Nombre).HasMaxLength(80).IsUnicode(false).HasColumnName("nombre");
            entity.Property(e => e.RequiereReferencia).HasColumnName("requiere_referencia");
            entity.Property(e => e.Activo).HasColumnName("activo");
        });

        modelBuilder.Entity<VentaPago>(entity =>
        {
            entity.HasKey(e => e.IdVentaPago).HasName("PK_VentaPago");
            entity.ToTable("Venta_pago");
            entity.Property(e => e.IdVentaPago).ValueGeneratedNever().HasColumnName("id_venta_pago");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.IdMetodoPago).HasColumnName("id_metodo_pago");
            entity.Property(e => e.Monto).HasColumnType("decimal(15, 2)").HasColumnName("monto");
            entity.Property(e => e.Referencia).HasMaxLength(80).IsUnicode(false).HasColumnName("referencia");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.HasIndex(e => e.IdVenta).HasDatabaseName("IX_VentaPago_Venta");
        });

        modelBuilder.Entity<CategoriaGasto>(entity =>
        {
            entity.HasKey(e => e.IdCategoriaGasto).HasName("PK_CategoriaGasto");
            entity.ToTable("Categoria_gasto");
            entity.Property(e => e.IdCategoriaGasto).ValueGeneratedNever().HasColumnName("id_categoria_gasto");
            entity.Property(e => e.Nombre).HasMaxLength(120).IsUnicode(false).HasColumnName("nombre");
            entity.Property(e => e.Activo).HasColumnName("activo");
        });

        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.HasKey(e => e.IdGasto).HasName("PK_Gasto");
            entity.ToTable("Gasto");
            entity.Property(e => e.IdGasto).ValueGeneratedNever().HasColumnName("id_gasto");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdCategoriaGasto).HasColumnName("id_categoria_gasto");
            entity.Property(e => e.Concepto).HasMaxLength(250).IsUnicode(false).HasColumnName("concepto");
            entity.Property(e => e.Proveedor).HasMaxLength(200).IsUnicode(false).HasColumnName("proveedor");
            entity.Property(e => e.NumeroComprobante).HasMaxLength(80).IsUnicode(false).HasColumnName("numero_comprobante");
            entity.Property(e => e.RutaComprobante).HasMaxLength(500).IsUnicode(false).HasColumnName("ruta_comprobante");
            entity.Property(e => e.NombreComprobante).HasMaxLength(255).IsUnicode(false).HasColumnName("nombre_comprobante");
            entity.Property(e => e.IdMetodoPago).HasColumnName("id_metodo_pago");
            entity.Property(e => e.Referencia).HasMaxLength(80).IsUnicode(false).HasColumnName("referencia");
            entity.Property(e => e.Monto).HasColumnType("decimal(15, 2)").HasColumnName("monto");
            entity.Property(e => e.Observaciones).HasMaxLength(300).IsUnicode(false).HasColumnName("observaciones");
            entity.Property(e => e.Estado).HasMaxLength(20).IsUnicode(false).HasColumnName("estado");
            entity.HasIndex(e => e.Fecha).HasDatabaseName("IX_Gasto_Fecha");
            entity.HasIndex(e => e.IdCategoriaGasto).HasDatabaseName("IX_Gasto_Categoria");
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(type => typeof(Base).IsAssignableFrom(type.ClrType)))
        {
            var entity = modelBuilder.Entity(entityType.ClrType);
            entity.Property(nameof(Base.FechaCreacion)).HasColumnName("fecha_creacion");
            entity.Property(nameof(Base.CreadoPor)).HasColumnName("creado_por");
            entity.Property(nameof(Base.FechaModificacion)).HasColumnName("fecha_modificacion");
            entity.Property(nameof(Base.ModificadoPor)).HasColumnName("modificado_por");
            entity.Property(nameof(Base.FechaEliminacion)).HasColumnName("fecha_eliminacion");
            entity.Property(nameof(Base.EliminadoPor)).HasColumnName("eliminado_por");
        }

        ConfigurarTenant(modelBuilder);

        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>Tablas del modelo Hospital -&gt; Clínica + filtros de tenant por sesión.</summary>
    private void ConfigurarTenant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hospital>(entity =>
        {
            entity.HasKey(e => e.IdHospital).HasName("PK_Hospital");
            entity.ToTable("Hospital");
            entity.Property(e => e.IdHospital).ValueGeneratedNever().HasColumnName("id_hospital");
            entity.Property(e => e.Nombre).HasMaxLength(200).IsUnicode(false).HasColumnName("nombre");
            entity.Property(e => e.Activo).HasColumnName("activo");
        });

        modelBuilder.Entity<Clinica>(entity =>
        {
            entity.HasKey(e => e.IdClinica).HasName("PK_Clinica");
            entity.ToTable("Clinica");
            entity.Property(e => e.IdClinica).ValueGeneratedNever().HasColumnName("id_clinica");
            entity.Property(e => e.IdHospital).HasColumnName("id_hospital");
            entity.Property(e => e.Nombre).HasMaxLength(200).IsUnicode(false).HasColumnName("nombre");
            entity.Property(e => e.Activa).HasColumnName("activa");
            entity.HasOne(d => d.Hospital).WithMany(p => p.Clinicas)
                .HasForeignKey(d => d.IdHospital).HasConstraintName("FK_Clinica_Hospital");
            entity.HasIndex(e => new { e.IdHospital, e.Nombre }).HasDatabaseName("IX_Clinica_Hospital_Nombre");
        });

        modelBuilder.Entity<UsuarioClinica>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioClinica).HasName("PK_UsuarioClinica");
            entity.ToTable("Usuario_clinica");
            entity.Property(e => e.IdUsuarioClinica).ValueGeneratedNever().HasColumnName("id_usuario_clinica");
            entity.Property(e => e.UsuarioId).HasColumnName("id_usuario");
            entity.Property(e => e.IdClinica).HasColumnName("id_clinica");
            entity.Property(e => e.EsDefault).HasColumnName("es_default");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.HasOne(d => d.Usuario).WithMany().HasForeignKey(d => d.UsuarioId).HasConstraintName("FK_UsuarioClinica_Usuario");
            entity.HasOne(d => d.Clinica).WithMany().HasForeignKey(d => d.IdClinica).HasConstraintName("FK_UsuarioClinica_Clinica");
            entity.HasIndex(e => new { e.UsuarioId, e.IdClinica }).IsUnique().HasDatabaseName("UQ_Usuario_Clinica");
        });

        // Paciente global: solo informativo, sin filtro de tenant.
        modelBuilder.Entity<Paciente>().Property(e => e.IdHospitalCreacion).HasColumnName("id_hospital_creacion");
        modelBuilder.Entity<Paciente>()
            .HasOne(e => e.HospitalCreacion).WithMany()
            .HasForeignKey(e => e.IdHospitalCreacion).IsRequired(false)
            .HasConstraintName("FK_Paciente_HospitalCreacion");

        // Columnas de tenant en tablas existentes (migración SQL las crea físicamente).
        modelBuilder.Entity<Cita>().Property(e => e.IdClinica).HasColumnName("id_clinica");
        modelBuilder.Entity<Consulta>().Property(e => e.IdClinica).HasColumnName("id_clinica");
        modelBuilder.Entity<OrdenEstudio>().Property(e => e.IdClinica).HasColumnName("id_clinica");
        modelBuilder.Entity<NotaMedica>().Property(e => e.IdClinica).HasColumnName("id_clinica");
        modelBuilder.Entity<EstudioImagen>().Property(e => e.IdClinica).HasColumnName("id_clinica");
        modelBuilder.Entity<Venta>().Property(e => e.IdClinica).HasColumnName("id_clinica");
        modelBuilder.Entity<Venta>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<Aseguradora>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<AseguradoraTarifa>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<Gasto>().Property(e => e.IdClinica).HasColumnName("id_clinica");
        modelBuilder.Entity<Gasto>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<Producto>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<LoteProducto>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<MovimientoInventario>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<MotivoCobro>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<CategoriaProducto>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<CategoriaGasto>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<MetodoPago>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<CatalogoIndicacion>().Property(e => e.IdHospital).HasColumnName("id_hospital");
        modelBuilder.Entity<Medicamento>().Property(e => e.IdHospital).HasColumnName("id_hospital");

        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.HasKey(e => e.IdCotizacion).HasName("PK_Cotizacion");
            entity.ToTable("Cotizacion");
            entity.Property(e => e.IdCotizacion).ValueGeneratedNever().HasColumnName("id_cotizacion");
            entity.Property(e => e.IdClinica).HasColumnName("id_clinica");
            entity.Property(e => e.IdHospital).HasColumnName("id_hospital");
            entity.Property(e => e.Folio).HasMaxLength(30).IsUnicode(false).HasColumnName("folio");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.VigenciaDias).HasColumnName("vigencia_dias");
            entity.Property(e => e.FechaVence).HasColumnName("fecha_vence");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.ClienteNombre).HasMaxLength(200).IsUnicode(false).HasColumnName("cliente_nombre");
            entity.Property(e => e.Total).HasColumnType("decimal(15, 2)").HasColumnName("total");
            entity.Property(e => e.Estado).HasMaxLength(20).IsUnicode(false).HasColumnName("estado");
            entity.Property(e => e.Observaciones).HasMaxLength(300).IsUnicode(false).HasColumnName("observaciones");
            entity.Property(e => e.IdVentaConvertida).HasColumnName("id_venta_convertida");
            entity.HasIndex(e => e.Folio).IsUnique().HasDatabaseName("UQ_Cotizacion_Folio");
            entity.HasIndex(e => new { e.IdClinica, e.Estado }).HasDatabaseName("IX_Cotizacion_Clinica_Estado");
        });

        modelBuilder.Entity<CotizacionDetalle>(entity =>
        {
            entity.HasKey(e => e.IdCotizacionDetalle).HasName("PK_CotizacionDetalle");
            entity.ToTable("Cotizacion_detalle");
            entity.Property(e => e.IdCotizacionDetalle).ValueGeneratedNever().HasColumnName("id_cotizacion_detalle");
            entity.Property(e => e.IdCotizacion).HasColumnName("id_cotizacion");
            entity.Property(e => e.TipoLinea).HasMaxLength(20).IsUnicode(false).HasColumnName("tipo_linea");
            entity.Property(e => e.IdMotivoCobro).HasColumnName("id_motivo_cobro");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdLote).HasColumnName("id_lote");
            entity.Property(e => e.Descripcion).HasMaxLength(250).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 2)").HasColumnName("cantidad");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(15, 2)").HasColumnName("precio_unitario");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(15, 2)").HasColumnName("subtotal");
            entity.Property(e => e.DescuentoMonto).HasColumnType("decimal(15, 2)").HasColumnName("descuento_monto");
            entity.Property(e => e.DescuentoMotivo).HasMaxLength(200).IsUnicode(false).HasColumnName("descuento_motivo");
            entity.Property(e => e.DescuentoOtorgadoPor).HasColumnName("descuento_otorgado_por");
            entity.Property(e => e.EsSobrePedido).HasColumnName("es_sobre_pedido");
            entity.HasIndex(e => e.IdCotizacion).HasDatabaseName("IX_CotizacionDetalle_Cotizacion");
        });

        // Filtros: God sin clínica ve todo; filas legado con Guid.Empty quedan visibles hasta migrar.
        modelBuilder.Entity<Cita>().HasQueryFilter(e => BypassTenant || e.IdClinica == TenantClinicaId || e.IdClinica == Guid.Empty);
        modelBuilder.Entity<Consulta>().HasQueryFilter(e => BypassTenant || e.IdClinica == TenantClinicaId || e.IdClinica == Guid.Empty);
        modelBuilder.Entity<OrdenEstudio>().HasQueryFilter(e => BypassTenant || e.IdClinica == TenantClinicaId || e.IdClinica == Guid.Empty);
        modelBuilder.Entity<NotaMedica>().HasQueryFilter(e => BypassTenant || e.IdClinica == TenantClinicaId || e.IdClinica == Guid.Empty);
        modelBuilder.Entity<EstudioImagen>().HasQueryFilter(e => BypassTenant || e.IdClinica == TenantClinicaId || e.IdClinica == Guid.Empty);
        modelBuilder.Entity<Venta>().HasQueryFilter(e => BypassTenant || e.IdClinica == TenantClinicaId || e.IdClinica == Guid.Empty);
        modelBuilder.Entity<Gasto>().HasQueryFilter(e => BypassTenant || e.IdClinica == TenantClinicaId || e.IdClinica == Guid.Empty);
        modelBuilder.Entity<Cotizacion>().HasQueryFilter(e => BypassTenant || e.IdClinica == TenantClinicaId || e.IdClinica == Guid.Empty);
        modelBuilder.Entity<Producto>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
        modelBuilder.Entity<LoteProducto>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
        modelBuilder.Entity<MovimientoInventario>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
        modelBuilder.Entity<MotivoCobro>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
        modelBuilder.Entity<CategoriaProducto>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
        modelBuilder.Entity<Aseguradora>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
        modelBuilder.Entity<AseguradoraTarifa>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
        modelBuilder.Entity<CategoriaGasto>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
        modelBuilder.Entity<MetodoPago>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
        modelBuilder.Entity<CatalogoIndicacion>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
        modelBuilder.Entity<Medicamento>().HasQueryFilter(e => BypassHospitalTenant || e.IdHospital == TenantHospitalId || e.IdHospital == Guid.Empty);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
