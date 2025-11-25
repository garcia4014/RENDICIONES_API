using System;
using System.Collections.Generic;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CapaDatos.ContabilidadAPI
{
    public partial class SvrendicionesContext : DbContext
    {
        public SvrendicionesContext()
        {
        }

        public SvrendicionesContext(DbContextOptions<SvrendicionesContext> options)
            : base(options)
        {
        }

        // DbSets existentes
        public virtual DbSet<Empleado> Empleados { get; set; }
        public virtual DbSet<PoliticaTipoGastoPersona> PoliticaTipoGastoPersonas { get; set; }
        public virtual DbSet<SviaticosCabecera> SviaticosCabeceras { get; set; }
        public virtual DbSet<SviaticosDetalle> SviaticosDetalles { get; set; }
        public virtual DbSet<TipoGasto> TipoGastos { get; set; }
        public virtual DbSet<TipoPersona> TipoPersonas { get; set; }
        public virtual DbSet<UsuarioTipoPersona> UsuarioTipoPersonas { get; set; }
        public virtual DbSet<SolicitudEstadoFlujo> SolicitudEstadoFlujos { get; set; } 

        // NUEVOS DbSets para rendiciones
        public virtual DbSet<RendicionCabecera> RendicionCabeceras { get; set; }
        public virtual DbSet<RendicionDetalle> RendicionDetalles { get; set; }

        // DbSet para comprobantes de pago
        public virtual DbSet<ComprobantePago> ComprobantesPago { get; set; }

        // DbSet para notificaciones
        public virtual DbSet<Notificacion> Notificaciones { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //    => optionsBuilder.UseSqlServer("");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ----- Configuraciones existentes (no tocar) -----
            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.HasKey(e => e.EmpCodigo);

                entity.ToTable("EMPLEADO");

                entity.Property(e => e.EmpCodigo)
                    .HasMaxLength(10)
                    .HasColumnName("EMP_CODIGO");
                entity.Property(e => e.EmpCentroCosto)
                    .HasMaxLength(25)
                    .HasColumnName("EMP_CENTRO_COSTO");
                entity.Property(e => e.EmpCorreo)
                    .HasMaxLength(255)
                    .HasColumnName("EMP_CORREO");
                entity.Property(e => e.EmpDni)
                    .HasMaxLength(12)
                    .HasColumnName("EMP_DNI");
                entity.Property(e => e.EmpNombres)
                    .HasMaxLength(150)
                    .HasColumnName("EMP_NOMBRES");
                entity.Property(e => e.EmpTelefono)
                    .HasMaxLength(20)
                    .HasColumnName("EMP_TELEFONO");
                entity.Property(e => e.EmpUnidadNegocio)
                    .HasMaxLength(25)
                    .HasColumnName("EMP_UNIDAD_NEGOCIO");
            });
  
            modelBuilder.Entity<PoliticaTipoGastoPersona>(entity =>
            {
                entity.HasKey(e => e.PtgpId).HasName("PK__POLITICA__CAC1F0C0FE2AC2E8");

                entity.ToTable("POLITICA_TIPO_GASTO_PERSONA");

                entity.Property(e => e.PtgpId).HasColumnName("PTGP_ID");
                entity.Property(e => e.PtgpEstado).HasColumnName("PTGP_ESTADO");
                entity.Property(e => e.PtgpIdTg).HasColumnName("PTGP_ID_TG");
                entity.Property(e => e.PtgpIdTp).HasColumnName("PTGP_ID_TP");
                entity.Property(e => e.PtgpMonto)
                    .HasColumnType("numeric(18, 2)")
                    .HasColumnName("PTGP_MONTO");

                entity.HasOne(d => d.PtgpIdTgNavigation).WithMany(p => p.PoliticaTipoGastoPersonas)
                    .HasForeignKey(d => d.PtgpIdTg)
                    .HasConstraintName("FK_TIPO_GASTO");

                entity.HasOne(d => d.PtgpIdTpNavigation).WithMany(p => p.PoliticaTipoGastoPersonas)
                    .HasForeignKey(d => d.PtgpIdTp)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_TIPO_PERSONA");
            });

            modelBuilder.Entity<SolicitudEstadoFlujo>(entity =>
            {
                entity.HasKey(e => e.SefId);

                entity.ToTable("SOLICITUD_ESTADO_FLUJO");

                entity.Property(e => e.SefId).HasColumnName("SEF_ID");
                entity.Property(e => e.SefDescripcion).HasColumnName("SEF_DESCRIPCION");
                entity.Property(e => e.SefAbreviatura).HasColumnName("SEF_ABREVIATURA");
                entity.Property(e => e.SefProceso).HasColumnName("SEF_PROCESO");
                entity.Property(e => e.SefEstado).HasColumnName("SEF_ESTADO");
            });

            modelBuilder.Entity<SviaticosCabecera>(entity =>
            {
                entity.HasKey(e => e.SvId);

                entity.ToTable("SVIATICOS_CABECERA");

                entity.Property(e => e.SvId).HasColumnName("SV_ID");
                entity.Property(e => e.SvContacto)
                    .HasMaxLength(100)
                    .HasColumnName("SV_CONTACTO");
                entity.Property(e => e.SvDescripcion)
                    .HasMaxLength(100)
                    .HasColumnName("SV_DESCRIPCION");

                entity.Property(e => e.SvFechaCreacion)
                    .HasColumnName("FechaCreacion")
                    .HasColumnType("datetime")
                    .IsRequired(false)
                    .HasDefaultValueSql("GETDATE()");


                entity.Property(e => e.SvEmpCantidad).HasColumnName("SV_EMP_CANTIDAD");
                entity.Property(e => e.SvEmpCodigo)
                    .HasMaxLength(10)
                    .HasColumnName("SV_EMP_CODIGO");
                entity.Property(e => e.SvEmpDni)
                    .HasMaxLength(12)
                    .HasColumnName("SV_EMP_DNI");
                entity.Property(e => e.SvSefId)
                    .HasColumnName("SV_SEF_ID");
                entity.Property(e => e.SvFechaInicio)
                    .HasColumnType("datetime")
                    .HasColumnName("SV_FECHA_INICIO");
                entity.Property(e => e.SvFechaRetorno)
                    .HasColumnType("datetime")
                    .HasColumnName("SV_FECHA_RETORNO");
                entity.Property(e => e.SvLocalidad)
                    .HasMaxLength(150)
                    .HasColumnName("SV_LOCALIDAD");
                entity.Property(e => e.SvNumero)
                    .HasMaxLength(10)
                    .HasColumnName("SV_NUMERO");
                entity.Property(e => e.SvNumeroDias).HasColumnName("SV_NUMERO_DIAS");
                entity.Property(e => e.SvObjetivoVisita)
                    .HasMaxLength(150)
                    .HasColumnName("SV_OBJETIVO_VISITA");
                entity.Property(e => e.SvOrdenVenta)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("SV_ORDEN_VENTA");
                entity.Property(e => e.SvRuc)
                    .HasMaxLength(12)
                    .HasColumnName("SV_RUC");
                entity.Property(e => e.SvPersonaEntrevistar)
                    .HasColumnName("SV_PERSONA_ENTREVISTAR");
                entity.Property(e => e.SvEmpresa)
                    .HasColumnName("SV_EMPRESA");
                entity.Property(e => e.SvTotalSolicitado)
                    .HasColumnType("numeric(18, 2)")
                    .HasColumnName("SV_TOTAL_SOLICITADO");
                entity.Property(e => e.SvPoliticas).HasColumnName("SV_POLITICAS");

                entity.HasOne(d => d.SolicitudEstadoFlujo)
                    .WithMany(p => p.SviaticosCabecera)
                    .HasForeignKey(d => d.SvSefId)
                    .HasConstraintName("FK_SolicitudEstadoFlujo_Cabecera");
            });

            modelBuilder.Entity<SviaticosDetalle>(entity =>
            {
                entity.HasKey(e => e.SvdId);

                entity.ToTable("SVIATICOS_DETALLE");

                entity.Property(e => e.SvdId).HasColumnName("SVD_ID");
                entity.Property(e => e.SvdCantEmpleado).HasColumnName("SVD_CANT_EMPLEADO");
                entity.Property(e => e.SvdDescripcion)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("SVD_DESCRIPCION");
                entity.Property(e => e.SvdFechaFin)
                    .HasColumnType("datetime")
                    .HasColumnName("SVD_FECHA_FIN");
                entity.Property(e => e.SvdFechaInicio)
                    .HasColumnType("datetime")
                    .HasColumnName("SVD_FECHA_INICIO");
                entity.Property(e => e.SvdIdCabecera).HasColumnName("SVD_ID_CABECERA");
                entity.Property(e => e.SvdImporteSolicitado)
                    .HasColumnType("numeric(18, 2)")
                    .HasColumnName("SVD_IMPORTE_SOLICITADO");
                entity.Property(e => e.SvdNumeroCabecera)
                    .HasMaxLength(10)
                    .HasColumnName("SVD_NUMERO_CABECERA");
                entity.Property(e => e.SvdPrecioUnitario)
                    .HasColumnType("numeric(18, 2)")
                    .HasColumnName("SVD_PRECIO_UNITARIO");
                entity.Property(e => e.SvdSubtotal)
                    .HasColumnType("numeric(18, 2)")
                    .HasColumnName("SVD_SUBTOTAL");
                entity.Property(e => e.SvdKilometraje)
                    .HasColumnType("numeric(18, 2)")
                    .HasColumnName("SVD_KILOMETRAJE");
                entity.Property(e => e.SvdNumeroDias).HasColumnName("SVD_NUMERO_DIAS");
                entity.Property(e => e.SvdTgId).HasColumnName("SVD_TG_ID");
            });

            modelBuilder.Entity<SviaticosDetalle>()
                .HasOne(d => d.Cabecera)
                .WithMany(c => c.Detalles)
                .HasForeignKey(d => d.SvdIdCabecera)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TipoGasto>(entity =>
            {
                entity.HasKey(e => e.TgId);

                entity.ToTable("TIPO_GASTO");

                entity.Property(e => e.TgId).HasColumnName("TG_ID");
                entity.Property(e => e.TgDescripcion)
                    .HasMaxLength(100)
                    .HasColumnName("TG_DESCRIPCION");
                entity.Property(e => e.TgEstado).HasColumnName("TG_ESTADO");
                entity.Property(e => e.TgPrecioUMax)
                    .HasColumnType("numeric(18, 2)")
                    .HasColumnName("TG_PRECIO_U_MAX");
            });

            modelBuilder.Entity<TipoPersona>(entity =>
            {
                entity.HasKey(e => e.TpId).HasName("PK__TIPO_PER__8106F2C44738C25C");

                entity.ToTable("TIPO_PERSONA");

                entity.Property(e => e.TpId).HasColumnName("TP_ID");
                entity.Property(e => e.TpDescripcion)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("TP_DESCRIPCION");
                entity.Property(e => e.TpDescripcionAbreviada)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("TP_DESCRIPCION_ABREVIADA");
                entity.Property(e => e.TpEstado).HasColumnName("TP_ESTADO");
            });

            modelBuilder.Entity<UsuarioTipoPersona>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("USUARIO_TIPO_PERSONA");

                entity.Property(e => e.Code)
                    .HasMaxLength(100)
                    .HasColumnName("Code");
                entity.Property(e => e.TpId)
                    .HasColumnName("TP_ID");
                entity.Property(e => e.UserSAP)
                    .HasMaxLength(100)
                    .HasColumnName("UserSAP");
                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime")
                    .HasColumnName("FechaCreacion")
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.FechaModificacion)
                    .HasColumnType("datetime")
                    .HasColumnName("FechaModificacion");

                entity.HasOne(d => d.TipoPersona)
                    .WithMany(p => p.UsuarioTipoPersonas)
                    .HasForeignKey(d => d.TpId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_USUARIO_TIPO_PERSONA_TIPO_PERSONA");
            });

            // ----- NUEVAS entidades: RENDICION_CABECERA / RENDICION_DETALLE -----
            modelBuilder.Entity<RendicionCabecera>(entity =>
            {
                entity.HasKey(e => e.RendId);
                entity.ToTable("RENDICION_CABECERA");

                entity.Property(e => e.RendId).HasColumnName("REND_ID");
                entity.Property(e => e.SolicitudId).HasColumnName("SOLICITUD_ID");
                entity.Property(e => e.UsrCod)
                    .HasMaxLength(50)
                    .HasColumnName("USR_COD");
                entity.Property(e => e.Estado)
                    .HasMaxLength(20)
                    .HasColumnName("ESTADO");
                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime2")
                    .HasColumnName("FECHA_CREACION");
                entity.Property(e => e.FechaEnvio)
                    .HasColumnType("datetime2")
                    .HasColumnName("FECHA_ENVIO");
                entity.Property(e => e.Total)
                    .HasColumnType("numeric(18,2)")
                    .HasColumnName("TOTAL");
                entity.Property(e => e.Observacion)
                    .HasMaxLength(1000)
                    .HasColumnName("OBSERVACION");
            });

            modelBuilder.Entity<RendicionDetalle>(entity =>
            {
                entity.HasKey(e => e.DetId);
                entity.ToTable("RENDICION_DETALLE");

                entity.Property(e => e.DetId).HasColumnName("DET_ID");
                entity.Property(e => e.RendId).HasColumnName("REND_ID");
                entity.Property(e => e.TipoGastoId).HasColumnName("TIPO_GASTO_ID");
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(1000)
                    .HasColumnName("DESCRIPCION");
                entity.Property(e => e.FechaGasto)
                    .HasColumnType("date")
                    .HasColumnName("FECHA_GASTO");
                entity.Property(e => e.Importe)
                    .HasColumnType("numeric(18,2)")
                    .HasColumnName("IMPORTE");
                entity.Property(e => e.Moneda)
                    .HasMaxLength(10)
                    .HasColumnName("MONEDA");
                entity.Property(e => e.EstadoValidacion)
                    .HasMaxLength(20)
                    .HasColumnName("ESTADO_VALIDACION");
                entity.Property(e => e.Observacion)
                    .HasMaxLength(1000)
                    .HasColumnName("OBSERVACION");
                entity.Property(e => e.ComprobanteUrl)
                    .HasMaxLength(500)
                    .HasColumnName("COMPROBANTE_URL");
                entity.Property(e => e.Ruc)
                    .HasMaxLength(11)
                    .HasColumnName("RUC");
                entity.Property(e => e.Serie)
                    .HasMaxLength(50)
                    .HasColumnName("SERIE");
                entity.Property(e => e.Numero)
                    .HasMaxLength(50)
                    .HasColumnName("NUMERO");
                entity.Property(e => e.FechaEmitida)
                    .HasColumnType("date")
                    .HasColumnName("FECHA_EMITIDA");
            });

            modelBuilder.Entity<RendicionCabecera>()
                .HasMany(r => r.Detalles)
                .WithOne(d => d.Rendicion)
                .HasForeignKey(d => d.RendId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----- CONFIGURACIÓN COMPROBANTE_PAGO -----
            modelBuilder.Entity<ComprobantePago>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("COMPROBANTE_PAGO");

                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.SvIdCabecera).HasColumnName("SV_ID_CABECERA");
                entity.Property(e => e.SvIdDetalle).HasColumnName("SV_ID_DETALLE");
                entity.Property(e => e.TipoComprobante).HasColumnName("TipoComprobante");
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(100)
                    .HasColumnName("Descripcion");
                entity.Property(e => e.Serie)
                    .HasMaxLength(10)
                    .HasColumnName("Serie");
                entity.Property(e => e.Correlativo)
                    .HasMaxLength(10)
                    .HasColumnName("Correlativo");
                entity.Property(e => e.FechaEmision)
                    .HasColumnType("datetime")
                    .HasColumnName("FechaEmision");
                entity.Property(e => e.Monto)
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("Monto");
                entity.Property(e => e.Ruc).HasColumnName("RUC");
                entity.Property(e => e.RazonSocial)
                    .HasMaxLength(300)
                    .HasColumnName("RazonSocial");
                entity.Property(e => e.Ruta)
                    .HasMaxLength(200)
                    .HasColumnName("Ruta");
                entity.Property(e => e.FechaCarga)
                    .HasColumnType("datetime")
                    .HasColumnName("FechaCarga")
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ValidoSunat).HasColumnName("ValidoSunat");
                entity.Property(e => e.Notas)
                    .HasMaxLength(300)
                    .HasColumnName("Notas");
                entity.Property(e => e.Activo)
                    .HasColumnName("Activo");

                // Relaciones FK
                entity.HasOne(d => d.SviaticosCabecera)
                    .WithMany(c => c.ComprobantesPago)
                    .HasForeignKey(d => d.SvIdCabecera)
                    .HasConstraintName("COMPROBANTE_PAGO_FK_CABECERA")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.SviaticosDetalle)
                    .WithMany(det => det.ComprobantesPago)
                    .HasForeignKey(d => d.SvIdDetalle)
                    .HasConstraintName("COMPROBANTE_PAGO_FK_DETALLE")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Llamada al partial para extensiones (mantener al final)
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
