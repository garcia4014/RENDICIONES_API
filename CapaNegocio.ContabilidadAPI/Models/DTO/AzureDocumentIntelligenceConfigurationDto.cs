namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// Configuración para Azure Document Intelligence (Form Recognizer)
    /// </summary>
    public class AzureDocumentIntelligenceConfigurationDto
    {
        /// <summary>
        /// Indica si el servicio de Azure IA está habilitado
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// URL del endpoint de Azure Document Intelligence
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Clave de suscripción de Azure (Ocp-Apim-Subscription-Key)
        /// </summary>
        public string SubscriptionKey { get; set; } = string.Empty;

        /// <summary>
        /// Versión de la API
        /// </summary>
        public string ApiVersion { get; set; } = "2023-07-31";

        /// <summary>
        /// Modelo a utilizar (por defecto: prebuilt-invoice)
        /// </summary>
        public string ModelId { get; set; } = "prebuilt-invoice";

        /// <summary>
        /// Tiempo máximo de espera para el análisis en segundos
        /// </summary>
        public int TimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Intervalo entre reintentos al consultar el resultado en milisegundos
        /// </summary>
        public int PollingIntervalMs { get; set; } = 2000;

        /// <summary>
        /// Número máximo de reintentos para consultar el resultado
        /// </summary>
        public int MaxPollingAttempts { get; set; } = 60;
    }
}
