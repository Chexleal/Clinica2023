using ClinicaDomain;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public partial class ClinicaContext : DbContext
{
    public ClinicaContext()
    {
    }

    public ClinicaContext(DbContextOptions<ClinicaContext> options)
        : base(options)
    {
        //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
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

            entity.HasOne(d => d.IdPacienteNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdPaciente)
                .HasConstraintName("FK__Cita__id_pacient__2C3393D0");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Cita__id_usuario__2D27B809");
        });

        modelBuilder.Entity<Consulta>(entity =>
        {
            entity.HasKey(e => e.IdConsulta).HasName("PK__Consulta__6F53588B6FB55DAE");

            entity.Property(e => e.IdConsulta)
                .ValueGeneratedNever()
                .HasColumnName("id_consulta");
            entity.Property(e => e.Diagnostico)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("diagnostico");
            entity.Property(e => e.Fecha)
                .HasColumnName("fecha");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.MotivoConsulta)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("motivo_consulta");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(200)
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
            entity.Property(e => e.Terminada).HasColumnName("terminada");
            entity.Property(e => e.TiempoDuracion).HasColumnName("tiempo_duracion");
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

            //entity.HasOne(d => d.IdConsultaNavigation).WithMany(p => p.DetalleCobros)
            //    .HasForeignKey(d => d.IdConsulta)
            //    .HasConstraintName("FK__Detalle_c__id_co__34C8D9D1");

            entity.HasOne(d => d.IdMotivoCobroNavigation).WithMany(p => p.DetalleCobros)
                .HasForeignKey(d => d.IdMotivoCobro)
                .HasConstraintName("FK__Detalle_c__id_mo__35BCFE0A");
        });

        modelBuilder.Entity<DetalleReceta>(entity =>
        {
            entity.HasKey(e => e.IdDetalleReceta).HasName("PK__Detalle___2C99ACD3657294D0");

            entity.ToTable("Detalle_receta");

            entity.Property(e => e.IdDetalleReceta)
                .ValueGeneratedNever()
                .HasColumnName("id_detalle_receta");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdReceta).HasColumnName("id_receta");

            entity.HasOne(d => d.IdRecetaNavigation).WithMany(p => p.DetalleReceta)
                .HasForeignKey(d => d.IdReceta)
                .HasConstraintName("FK__Detalle_r__id_re__3B75D760");
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
                .HasMaxLength(50)
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
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("remitido");
            entity.Property(e => e.Telefono).HasColumnName("telefono");
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

            entity.HasOne(d => d.IdConsultaNavigation).WithMany(p => p.Receta)
                .HasForeignKey(d => d.IdConsulta)
                .HasConstraintName("FK__Receta__id_consu__38996AB5");
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
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Permiso)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("permiso");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.RolDetalles)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Rol_detal__id_us__276EDEB3");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
