using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaDatos.ContabilidadAPI.Models
{
    [Table("USUARIO_TIPO_PERSONA")]
    public partial class UsuarioTipoPersona
    {
        [Key]
        [Column("Code")]
        [StringLength(100)]
        public string Code { get; set; } = null!;

        [Column("TP_ID")]
        public int TpId { get; set; }

        [Column("UserSAP")]
        [StringLength(100)]
        public string? UserSAP { get; set; }

        [Column("Activo")]
        public bool Activo { get; set; } = true;

        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Column("FechaModificacion")]
        public DateTime? FechaModificacion { get; set; }

        // Navegación hacia TipoPersona
        [ForeignKey("TpId")]
        [InverseProperty("UsuarioTipoPersonas")]
        public virtual TipoPersona TipoPersona { get; set; } = null!;
    }
}