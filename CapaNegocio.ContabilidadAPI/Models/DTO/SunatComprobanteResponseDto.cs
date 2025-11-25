namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO para la respuesta de validación de comprobante de SUNAT
    /// </summary>
    public class SunatComprobanteResponseDto
    {
        public bool success { get; set; }
        public string message { get; set; }
        public SunatComprobanteDataDto data { get; set; }
        public string? errorCode { get; set; }
    }

    public class SunatComprobanteDataDto
    {
        public string estadoCp { get; set; }
        public string estadoRuc { get; set; }
        public string condDomiRuc { get; set; }
        public string[] observaciones { get; set; }
    }
}