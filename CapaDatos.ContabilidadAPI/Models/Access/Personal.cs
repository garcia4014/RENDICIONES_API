using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaDatos.ContabilidadAPI.Models.Access
{
    [Table("personal")]
    public class Personal
    {
        [Key]
        [Column("idDocumento")]
        public string IdDocumento { get; set; } = string.Empty;

        [Column("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Column("huella")]
        public string? Huella { get; set; }

        [Column("empresa")]
        public string Empresa { get; set; } = string.Empty;

        [Column("fechaRegistro")]
        public DateTime? FechaRegistro { get; set; }

        [Column("usrSidige")]
        public string? UsrSidige { get; set; } = string.Empty;

        [Column("pswSidige")]
        public string? PswSidige { get; set; } = string.Empty;

        [Column("emailSidige")]
        public string? EmailSidige { get; set; }
        [NotMapped]
        public int? TP_ID { get; set;  } 
        [NotMapped]
        public string? TP_DESCRIPCION { get; set; }
    }
}
