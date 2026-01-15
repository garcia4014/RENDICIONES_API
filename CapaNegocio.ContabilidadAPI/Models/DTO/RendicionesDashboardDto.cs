namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO para el dashboard de rendiciones de un empleado
    /// </summary>
    public class RendicionesDashboardDto
    {
        /// <summary>
        /// Cantidad de rendiciones en estado pendiente (Estado = 5)
        /// </summary>
        public int RendicionesPendientes { get; set; }

        /// <summary>
        /// Cantidad total de comprobantes de pago cargados (Activo = 1)
        /// </summary>
        public int ComprobantesCargados { get; set; }

        /// <summary>
        /// Cantidad de comprobantes validados por SUNAT (ValidoSunat = 1)
        /// </summary>
        public int ValidadosSunat { get; set; }

        /// <summary>
        /// Cantidad de comprobantes pendientes de validación (ValidoSunat = false y Activo = true)
        /// </summary>
        public int PendientesValidacion { get; set; }

        /// <summary>
        /// DNI del empleado consultado
        /// </summary>
        public string SvEmpDni { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de inicio del período consultado
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha de fin del período consultado
        /// </summary>
        public DateTime FechaFin { get; set; }

        /// <summary>
        /// Fecha y hora de generación del dashboard
        /// </summary>
        public DateTime FechaGeneracion { get; set; }
    }
}