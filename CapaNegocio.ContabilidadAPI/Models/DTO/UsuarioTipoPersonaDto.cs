using System;
using System.ComponentModel.DataAnnotations;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class UsuarioTipoPersonaDto
    {
        [Required]
        [StringLength(100)]
        public string Code { get; set; } = null!;

        [Required]
        public int TpId { get; set; }

        [StringLength(100)]
        public string? UserSAP { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }

        // Información del tipo de persona para mostrar
        public string? TipoPersonaDescripcion { get; set; }
        public string? TipoPersonaAbreviada { get; set; }
    }

    public class UsuarioTipoPersonaCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Code { get; set; } = null!;

        [Required]
        public int TpId { get; set; }

        [StringLength(100)]
        public string? UserSAP { get; set; }
    }

    public class UsuarioTipoPersonaUpdateDto
    {
        [Required]
        public int TpId { get; set; }

        [StringLength(100)]
        public string? UserSAP { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class UsuarioTipoPersonaFiltroDto
    {
        public string? Code { get; set; }
        public int? TpId { get; set; }
        public string? UserSAP { get; set; }
        public bool? Activo { get; set; }

        // Paginación
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 10;
    }
}