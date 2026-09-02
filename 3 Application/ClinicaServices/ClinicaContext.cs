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
            entity.Property(e => e.EstadoEliminado).HasColumnName("estado_eliminado");

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
                .HasMaxLength(120)
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
