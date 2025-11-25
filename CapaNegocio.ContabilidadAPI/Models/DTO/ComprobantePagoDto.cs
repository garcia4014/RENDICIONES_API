using CapaDatos.ContabilidadAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO para mostrar información de ComprobantePago
    /// </summary>
    public class ComprobantePagoDto
    {
        /// <summary>
        /// ID único del comprobante
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la cabecera de viáticos asociada
        /// </summary>
        public int? SvIdCabecera { get; set; }

        /// <summary>
        /// ID del detalle de viáticos asociado
        /// </summary>
        public int? SvIdDetalle { get; set; }

        /// <summary>
        /// Tipo de comprobante (1: Factura, 2: Boleta, 3: Recibo, etc.)
        /// </summary>
        public int? TipoComprobante { get; set; }

        /// <summary>
        /// Descripción del tipo de comprobante
        /// </summary>
        public string? TipoComprobanteDescripcion { get; set; }

        /// <summary>
        /// Descripción del comprobante
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Serie del comprobante
        /// </summary>
        public string? Serie { get; set; }

        /// <summary>
        /// Número correlativo
        /// </summary>
        public string? Correlativo { get; set; }

        /// <summary>
        /// Fecha de emisión del comprobante
        /// </summary>
        public DateTime? FechaEmision { get; set; }

        /// <summary>
        /// Monto del comprobante
        /// </summary>
        public decimal? Monto { get; set; }

        /// <summary>
        /// RUC del emisor
        /// </summary>
        public long? Ruc { get; set; }

        /// <summary>
        /// Razón social del emisor
        /// </summary>
        public string? RazonSocial { get; set; }

        /// <summary>
        /// Ruta del archivo
        /// </summary>
        public string? Ruta { get; set; }

        /// <summary>
        /// Fecha de carga al sistema
        /// </summary>
        public DateTime? FechaCarga { get; set; }

        /// <summary>
        /// Indica si el comprobante es válido según SUNAT
        /// </summary>
        public bool? ValidoSunat { get; set; }

        /// <summary>
        /// Notas adicionales
        /// </summary>
        public string? Notas { get; set; }

        /// <summary>
        /// Estado activo
        /// </summary>
        public bool? Activo { get; set; }

        /// <summary>
        /// Número completo del comprobante (Serie-Correlativo)
        /// </summary>
        public string NumeroCompleto => $"{Serie}-{Correlativo}";
 
    }


    /// <summary>
    /// DTO para crear un nuevo ComprobantePago
    /// </summary>
    public class ComprobantePagoCreateDto
    {
        public int SvIdCabecera { get; set; }
        public int SvIdDetalle { get; set; }

        [Range(1, 10, ErrorMessage = "El tipo de comprobante debe estar entre 1 y 10")]
        public int? TipoComprobante { get; set; }
        [StringLength(100, ErrorMessage = "La descripción no puede exceder 100 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La serie es obligatoria")]
        [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres")]
        public string Serie { get; set; } = string.Empty;
        [Required(ErrorMessage = "El correlativo es obligatorio")]
        [StringLength(10, ErrorMessage = "El correlativo no puede exceder 10 caracteres")]
        public string Correlativo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de emisión es obligatoria")]
        public DateTime FechaEmision { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Range(10000000000, 99999999999, ErrorMessage = "El RUC debe tener 11 dígitos")]
        public long? Ruc { get; set; }

        [StringLength(300, ErrorMessage = "La razón social no puede exceder 300 caracteres")]
        public string? RazonSocial { get; set; }

        [StringLength(200, ErrorMessage = "La ruta no puede exceder 200 caracteres")]
        public string? Ruta { get; set; }

        public bool? ValidoSunat { get; set; }

        [StringLength(300, ErrorMessage = "Las notas no pueden exceder 300 caracteres")]
        public string? Notas { get; set; }
        public string? Extension { get; set; }
        public int? Observado { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un ComprobantePago existente
    /// </summary>
    public class ComprobantePagoUpdateDto
    {
        /// <summary>
        /// ID del comprobante a actualizar
        /// </summary>
        [Required(ErrorMessage = "El ID es obligatorio")]
        public int Id { get; set; }

        /// <summary>
        /// ID de la cabecera de viáticos asociada
        /// </summary>
        public int? SvIdCabecera { get; set; }

        /// <summary>
        /// ID del detalle de viáticos asociado
        /// </summary>
        public int? SvIdDetalle { get; set; }

        /// <summary>
        /// Tipo de comprobante
        /// </summary>
        [Range(1, 10, ErrorMessage = "El tipo de comprobante debe estar entre 1 y 10")]
        public int? TipoComprobante { get; set; }

        /// <summary>
        /// Descripción del comprobante
        /// </summary>
        [StringLength(100, ErrorMessage = "La descripción no puede exceder 100 caracteres")]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Serie del comprobante
        /// </summary>
        [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres")]
        public string? Serie { get; set; }

        /// <summary>
        /// Número correlativo
        /// </summary>
        [StringLength(10, ErrorMessage = "El correlativo no puede exceder 10 caracteres")]
        public string? Correlativo { get; set; }

        /// <summary>
        /// Fecha de emisión del comprobante
        /// </summary>
        public DateTime? FechaEmision { get; set; }

        /// <summary>
        /// Monto del comprobante
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal? Monto { get; set; }

        /// <summary>
        /// RUC del emisor
        /// </summary>
        [Range(10000000000, 99999999999, ErrorMessage = "El RUC debe tener 11 dígitos")]
        public long? Ruc { get; set; }

        /// <summary>
        /// Razón social del emisor
        /// </summary>
        [StringLength(300, ErrorMessage = "La razón social no puede exceder 300 caracteres")]
        public string? RazonSocial { get; set; }

        /// <summary>
        /// Ruta del archivo
        /// </summary>
        [StringLength(200, ErrorMessage = "La ruta no puede exceder 200 caracteres")]
        public string? Ruta { get; set; }

        /// <summary>
        /// Indica si el comprobante es válido según SUNAT
        /// </summary>
        public bool? ValidoSunat { get; set; }

        /// <summary>
        /// Notas adicionales
        /// </summary>
        [StringLength(300, ErrorMessage = "Las notas no pueden exceder 300 caracteres")]
        public string? Notas { get; set; }
        public string? Extension { get; set; }
        public string? ResultadoSunat { get; set; }
    }

    /// <summary>
    /// DTO para filtros de búsqueda de comprobantes
    /// </summary>
    public class ComprobantePagoFiltroDto
    {
        /// <summary>
        /// ID de cabecera de viáticos
        /// </summary>
        public int? SvIdCabecera { get; set; }

        /// <summary>
        /// ID de detalle de viáticos
        /// </summary>
        public int? SvIdDetalle { get; set; }

        /// <summary>
        /// Serie del comprobante
        /// </summary>
        public string? Serie { get; set; }

        /// <summary>
        /// Correlativo del comprobante
        /// </summary>
        public string? Correlativo { get; set; }

        /// <summary>
        /// RUC del emisor
        /// </summary>
        public long? Ruc { get; set; }

        /// <summary>
        /// Fecha de emisión desde
        /// </summary>
        public DateTime? FechaEmisionDesde { get; set; }

        /// <summary>
        /// Fecha de emisión hasta
        /// </summary>
        public DateTime? FechaEmisionHasta { get; set; }

        /// <summary>
        /// Validación SUNAT
        /// </summary>
        public bool? ValidoSunat { get; set; }

        /// <summary>
        /// Página para paginación
        /// </summary>
        public int Pagina { get; set; } = 1;

        /// <summary>
        /// Tamaño de página para paginación
        /// </summary>
        public int TamanoPagina { get; set; } = 10;
    }
}