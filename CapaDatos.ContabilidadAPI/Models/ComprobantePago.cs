using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CapaDatos.ContabilidadAPI.Models
{

    [Table("COMPROBANTE_PAGO")]
    public partial class ComprobantePago
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SV_ID_CABECERA")]
        public int? SvIdCabecera { get; set; }

        [Column("SV_ID_DETALLE")]
        public int? SvIdDetalle { get; set; }

        [Column("TipoComprobante")]
        public string? TipoComprobante { get; set; }

        [Column("Descripcion")]
        [StringLength(100)]
        public string? Descripcion { get; set; }

        [Column("Serie")]
        [StringLength(10)]
        public string? Serie { get; set; }

        [Column("Correlativo")]
        [StringLength(10)]
        public string? Correlativo { get; set; }

        [Column("FechaEmision")]
        public DateTime? FechaEmision { get; set; }

        [Column("Monto", TypeName = "decimal(18,2)")]
        public decimal? Monto { get; set; }

        [Column("RUC")]
        public long? Ruc { get; set; }

        [Column("RazonSocial")]
        [StringLength(300)]
        public string? RazonSocial { get; set; }

        [Column("Ruta")]
        [StringLength(200)]
        public string? Ruta { get; set; }

        [Column("FechaCarga")]
        public DateTime? FechaCarga { get; set; } = DateTime.Now;

        [Column("ValidoSunat")]
        public bool? ValidoSunat { get; set; }

        [Column("Notas")]
        [StringLength(300)]
        public string? Notas { get; set; }

        [Column("Activo")]
        public bool Activo { get; set; } = true;

        [Column("Extension")]
        public string? Extension { get; set; } = string.Empty;

        [Column("ResultadoSunat")]
        public string? ResultadoSunat { get; set; } = string.Empty;

        [Column("Leido")]
        public bool? Leido { get; set; } = false;

        [Column("FechaLectura ")]
        public DateTime? FechaLectura { get; set; }

        [Column("SV_TG_ID")]
        public int? SvTgId { get; set; }

        //[Column("SV_TipoGasto")]
        //public string? SvTipoGasto { get; set; }
        public bool? Observado { get; set; } = false;
        public bool? Aprobado { get; set; } = false;
        public string? Observacion { get; set; } = string.Empty;

        [Column("EXONERADO")]
        public bool? Exonerado { get; set; } = false;
        
        [Column("INAFECTO")]
        public bool? Inafecto { get; set; } = false;
        
        [Column("IGV")]
        public decimal? Igv { get; set; } = 0m;
        
        [Column("SUBTOTAL")]
        public decimal? Subtotal { get; set; } = 0m;

        [Column("IGV_ESPECIAL")]
        public bool? IgvEspecial { get; set; } = false;

        [Column("IGVPorcentaje")]
        public int? IgvPorcentaje { get; set; }

        [ForeignKey("SvIdCabecera")]
        [JsonIgnore]
        public virtual SviaticosCabecera? SviaticosCabecera { get; set; }

        [ForeignKey("SvTgId")]
        [JsonIgnore]
        public virtual TipoGasto? TipoGasto { get; set; }

        // Relación con SviaticosDetalle - COMENTADA: Ahora solo se relaciona con Cabecera
        // [ForeignKey("SvIdDetalle")]
        // [JsonIgnore]
        // public virtual SviaticosDetalle? SviaticosDetalle { get; set; }
    }
}