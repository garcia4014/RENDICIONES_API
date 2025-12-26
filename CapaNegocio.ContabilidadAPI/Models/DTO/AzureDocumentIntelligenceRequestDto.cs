using System.Text.Json.Serialization;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO para la solicitud a Azure Document Intelligence
    /// </summary>
    public class AzureDocumentIntelligenceRequestDto
    {
        /// <summary>
        /// URL pública del documento a analizar
        /// </summary>
        [JsonPropertyName("urlSource")]
        public string UrlSource { get; set; } = string.Empty;

        /// <summary>
        /// Campos personalizados a extraer (opcional)
        /// </summary>
        [JsonPropertyName("queryFields")]
        public List<string>? QueryFields { get; set; }
    }

    /// <summary>
    /// DTO alternativo para enviar el documento como Base64
    /// </summary>
    public class AzureDocumentIntelligenceBase64RequestDto
    {
        /// <summary>
        /// Documento codificado en Base64
        /// </summary>
        [JsonPropertyName("base64Source")]
        public string Base64Source { get; set; } = string.Empty;

        /// <summary>
        /// Campos personalizados a extraer (opcional)
        /// </summary>
        [JsonPropertyName("queryFields")]
        public List<string>? QueryFields { get; set; }
    }
}
