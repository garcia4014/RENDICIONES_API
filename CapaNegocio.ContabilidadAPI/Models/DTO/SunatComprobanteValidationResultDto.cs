namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO con el resultado completo de la validación de comprobante
    /// </summary>
    public class SunatComprobanteValidationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        
        // Datos del comprobante
        public int EstadoComprobante { get; set; }
        public string DescripcionEstadoComprobante { get; set; }
        
        // Datos del RUC
        public string EstadoRuc { get; set; }
        public string DescripcionEstadoRuc { get; set; }
        
        // Condición domiciliaria
        public string CondicionDomiciliaria { get; set; }
        public string DescripcionCondicionDomiciliaria { get; set; }
        
        // Observaciones
        public string[] Observaciones { get; set; }
        
        // Datos de la consulta
        public DateTime FechaConsulta { get; set; }
        public string RucConsultante { get; set; }
        public string ComprobanteConsultado { get; set; }
    }
}