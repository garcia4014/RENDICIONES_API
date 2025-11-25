using System.ComponentModel.DataAnnotations;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO para la solicitud de validación de comprobante a SUNAT
    /// </summary>
    public class SunatComprobanteRequestDto
    {
        [Required]
        [StringLength(11, MinimumLength = 11)]
        public string numRuc { get; set; }

        [Required]
        [StringLength(2)]
        public string codComp { get; set; }

        [Required]
        [StringLength(4)]
        public string numeroSerie { get; set; }

        [Required]
        public string numero { get; set; }

        [Required]
        public string fechaEmision { get; set; } // Formato: dd/mm/yyyy

        public string? monto { get; set; } // Solo para electrónicos
    }
}