namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// Configuración para la integración con SUNAT
    /// </summary>
    public class SunatConfigurationDto
    {
        /// <summary>
        /// Client ID para autenticación con SUNAT
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Client Secret para autenticación con SUNAT  
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// URL del servicio de token de SUNAT
        /// </summary>
        public string TokenUrl { get; set; } = "https://api-seguridad.sunat.gob.pe/v1/clientesextranet/{0}/oauth2/token/";

        /// <summary>
        /// URL del servicio de validación de comprobantes de SUNAT
        /// </summary>
        public string ValidarComprobanteUrl { get; set; } = "https://api.sunat.gob.pe/v1/contribuyente/contribuyentes/{0}/validarcomprobante";

        /// <summary>
        /// Scope para autenticación
        /// </summary>
        public string Scope { get; set; } = "https://api.sunat.gob.pe/v1/contribuyente/contribuyentes";

        /// <summary>
        /// Tipo de grant para OAuth2
        /// </summary>
        public string GrantType { get; set; } = "client_credentials";
        public string RUC { get; set; }
    }
}