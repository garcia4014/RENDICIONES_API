using System.ComponentModel.DataAnnotations;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO para mostrar el dashboard de estadísticas de viáticos por usuario
    /// </summary>
    public class ViaticoDashboardDto
    {
        /// <summary>
        /// Número de viáticos con estado "Solicitado" (SV_SEF_ID = 1)
        /// </summary>
        [Display(Name = "Viáticos Pendientes")]
        public int ViaticosPendientes { get; set; }

        /// <summary>
        /// Suma total de viáticos con estado "Aprobado" (SV_SEF_ID = 3)
        /// </summary>
        [Display(Name = "Rendiciones Aprobadas")]
        public decimal RendicionesAprobadas { get; set; }

        /// <summary>
        /// Número de viáticos con estado "Abierto" en revisión (SV_SEF_ID = 2)
        /// </summary>
        [Display(Name = "En Revisión")]
        public int EnRevision { get; set; }

        /// <summary>
        /// Suma total de viáticos del mes actual (independiente del estado)
        /// </summary>
        [Display(Name = "Total del Mes")]
        public decimal TotalDelMes { get; set; }

        /// <summary>
        /// Código del usuario para el cual se generó el dashboard
        /// </summary>
        public string CodigoUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de generación del reporte
        /// </summary>
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        /// <summary>
        /// Mes y año del reporte
        /// </summary>
        public string PeriodoReporte { get; set; } = DateTime.Now.ToString("MMMM yyyy");
    }

    /// <summary>
    /// DTO para la respuesta del endpoint de dashboard
    /// </summary>
    public class ViaticoDashboardResponseDto
    {
        /// <summary>
        /// Indica si la operación fue exitosa
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Datos del dashboard de viáticos
        /// </summary>
        public ViaticoDashboardDto? Data { get; set; }

        /// <summary>
        /// Detalles adicionales o errores
        /// </summary>
        public List<string> Details { get; set; } = new List<string>();
    }
}