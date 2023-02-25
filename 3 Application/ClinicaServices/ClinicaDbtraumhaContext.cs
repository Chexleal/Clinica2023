using System;
using System.Collections.Generic;
using clinicaWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaServices;

public partial class ClinicaDbtraumhaContext : DbContext
{
    public ClinicaDbtraumhaContext()
    {
    }

    public ClinicaDbtraumhaContext(DbContextOptions<ClinicaDbtraumhaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Consulta> Consulta { get; set; }

    public virtual DbSet<DetalleReceta> DetalleReceta { get; set; }

    public virtual DbSet<Detallecobro> Detallecobros { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Motivocobro> Motivocobros { get; set; }

    public virtual DbSet<Paciente> Pacientes { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Puesto> Puestos { get; set; }

    public virtual DbSet<Receta> Receta { get; set; }

    public virtual DbSet<RolDetalle> RolDetalles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Database=ClinicaDBTraumha;Trusted_Connection=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Consulta>(entity =>
        {
            entity.HasKey(e => e.IdConsulta);

            entity.ToTable("CONSULTA");

            entity.HasIndex(e => e.IdEmpleado, "IX_FK_CONSULTA_EMPLEADO");

            entity.HasIndex(e => e.IdPaciente, "IX_FK_CONSULTA_PACIENTE");

            entity.Property(e => e.IdConsulta)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_consulta");
            entity.Property(e => e.Diagnostico)
                .HasMaxLength(800)
                .IsUnicode(false);
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.Glucosa)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IdEmpleado)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_empleado");
            entity.Property(e => e.IdPaciente)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_paciente");
            entity.Property(e => e.MotivoConsulta)
                .HasMaxLength(800)
                .IsUnicode(false)
                .HasColumnName("Motivo_consulta");
            entity.Property(e => e.NoRegistro)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("No_registro");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(800)
                .IsUnicode(false);
            entity.Property(e => e.Peso)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PresionArterial)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Presion_arterial");
            entity.Property(e => e.Temperatura)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TiempoDuracion).HasColumnName("Tiempo_duracion");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Consulta)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_CONSULTA_EMPLEADO");

            entity.HasOne(d => d.IdPacienteNavigation).WithMany(p => p.Consulta)
                .HasForeignKey(d => d.IdPaciente)
                .HasConstraintName("FK_CONSULTA_PACIENTE");
        });

        modelBuilder.Entity<DetalleReceta>(entity =>
        {
            entity.HasKey(e => e.IdDetalleReceta);

            entity.ToTable("DETALLE_RECETA");

            entity.HasIndex(e => e.IdReceta, "IX_FK_DETALLE_RECETA_RECETA");

            entity.Property(e => e.IdDetalleReceta)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_detalle_receta");
            entity.Property(e => e.CantidadMed)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Cantidad_med");
            entity.Property(e => e.DosisDia)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("Dosis_dia");
            entity.Property(e => e.DosisTiempo)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("Dosis_tiempo");
            entity.Property(e => e.IdReceta)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_receta");
            entity.Property(e => e.Instrucciones)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Medicamento).HasMaxLength(50);

            entity.HasOne(d => d.IdRecetaNavigation).WithMany(p => p.DetalleReceta)
                .HasForeignKey(d => d.IdReceta)
                .HasConstraintName("FK_DETALLE_RECETA_RECETA");
        });

        modelBuilder.Entity<Detallecobro>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCobro);

            entity.ToTable("DETALLECOBRO");

            entity.HasIndex(e => e.IdConsulta, "IX_FK_DetalleCobro_CONSULTA");

            entity.HasIndex(e => e.IdMotivoCobro, "IX_FK_DetalleCobro_MotivoCobro");

            entity.Property(e => e.IdDetalleCobro)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_detalleCobro");
            entity.Property(e => e.IdConsulta)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_consulta");
            entity.Property(e => e.IdMotivoCobro)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_MotivoCobro");
            entity.Property(e => e.Producto)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Valor).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.IdMotivoCobroNavigation).WithMany(p => p.Detallecobros)
                .HasForeignKey(d => d.IdMotivoCobro)
                .HasConstraintName("FK_DetalleCobro_MotivoCobro");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.IdEmpleado);

            entity.ToTable("EMPLEADO");

            entity.HasIndex(e => e.IdPuesto, "IX_FK_EMPLEADO_PUESTO");

            entity.Property(e => e.IdEmpleado)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_empleado");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Dpi)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DPI");
            entity.Property(e => e.FechaNac)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_nac");
            entity.Property(e => e.Foto)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IdClinica)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_clinica");
            entity.Property(e => e.IdPuesto)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_puesto");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Telefono).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UserExist)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("User_exist");

            entity.HasOne(d => d.IdPuestoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdPuesto)
                .HasConstraintName("FK_EMPLEADO_PUESTO");
        });

        modelBuilder.Entity<Motivocobro>(entity =>
        {
            entity.HasKey(e => e.IdMotivoCobro);

            entity.ToTable("MOTIVOCOBRO");

            entity.Property(e => e.IdMotivoCobro)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_MotivoCobro");
            entity.Property(e => e.Descripción)
                .HasMaxLength(20)
                .IsFixedLength();
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(e => e.IdPaciente);

            entity.ToTable("PACIENTE");

            entity.Property(e => e.IdPaciente)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_paciente");
            entity.Property(e => e.Alergias)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Antecedentes)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Apellido)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Dpi)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DPI");
            entity.Property(e => e.EstadoCivil)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Estado_civil");
            entity.Property(e => e.FechaNac)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_nac");
            entity.Property(e => e.Genero)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Nacionalidad)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NoRegistro)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("No_Registro");
            entity.Property(e => e.Nombre)
                .HasMaxLength(90)
                .IsUnicode(false);
            entity.Property(e => e.Profesion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remitido)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TipoSangre)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.IdPermiso);

            entity.ToTable("PERMISO");

            entity.Property(e => e.IdPermiso)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_permiso");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Puesto>(entity =>
        {
            entity.HasKey(e => e.IdPuesto);

            entity.ToTable("PUESTO");

            entity.Property(e => e.IdPuesto)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_puesto");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Receta>(entity =>
        {
            entity.HasKey(e => e.IdReceta);

            entity.ToTable("RECETA");

            entity.HasIndex(e => e.IdConsulta, "IX_FK_RECETA_CONSULTA");

            entity.Property(e => e.IdReceta)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_receta");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.IdConsulta)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_consulta");

            entity.HasOne(d => d.IdConsultaNavigation).WithMany(p => p.Receta)
                .HasForeignKey(d => d.IdConsulta)
                .HasConstraintName("FK_RECETA_CONSULTA");
        });

        modelBuilder.Entity<RolDetalle>(entity =>
        {
            entity.HasKey(e => e.IdRolDetalle);

            entity.ToTable("ROL_DETALLE");

            entity.HasIndex(e => e.IdPermiso, "IX_FK_ROL_DETALLE_ROL");

            entity.HasIndex(e => e.IdUser, "IX_FK_ROL_DETALLE_USUARIO");

            entity.Property(e => e.IdRolDetalle)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_rol_detalle");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IdPermiso)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_permiso");
            entity.Property(e => e.IdUser)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_user");

            entity.HasOne(d => d.IdPermisoNavigation).WithMany(p => p.RolDetalles)
                .HasForeignKey(d => d.IdPermiso)
                .HasConstraintName("FK_ROL_DETALLE_ROL");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.RolDetalles)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_ROL_DETALLE_USUARIO");
        });


        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUser);

            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.IdEmpleado, "IX_FK_USUARIO_EMPLEADO");

            entity.Property(e => e.IdUser)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_user");
            entity.Property(e => e.Huella)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IdEmpleado)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("Id_empleado");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PreguntaSeg)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Pregunta_seg");
            entity.Property(e => e.RespuestaSeg)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Respuesta_seg");
            entity.Property(e => e.UsuarioNombre)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_USUARIO_EMPLEADO");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
