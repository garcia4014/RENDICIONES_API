using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    /// <summary>
    /// Interface para el servicio de Azure Document Intelligence
    /// </summary>
    public interface IAzureDocumentIntelligenceService
    {
        /// <summary>
        /// Analiza un documento usando Azure Document Intelligence
        /// </summary>
        /// <param name="urlSource">URL pública del documento</param>
        /// <param name="queryFields">Campos personalizados a extraer (opcional)</param>
        /// <returns>Respuesta con los campos extraídos</returns>
        Task<ApiResponse<AzureDocumentIntelligenceResponseDto>> AnalyzeDocumentFromUrlAsync(
            string urlSource, 
            List<string>? queryFields = null);

        /// <summary>
        /// Analiza un documento usando Azure Document Intelligence desde bytes
        /// </summary>
        /// <param name="documentBytes">Bytes del documento</param>
        /// <param name="queryFields">Campos personalizados a extraer (opcional)</param>
        /// <returns>Respuesta con los campos extraídos</returns>
        Task<ApiResponse<AzureDocumentIntelligenceResponseDto>> AnalyzeDocumentFromBytesAsync(
            byte[] documentBytes, 
            List<string>? queryFields = null);

        /// <summary>
        /// Convierte la respuesta de Azure Document Intelligence a formato OCR estándar
        /// </summary>
        /// <param name="azureResponse">Respuesta de Azure</param>
        /// <returns>Respuesta en formato OCR estándar</returns>
        OcrResponseDto ConvertToOcrResponse(AzureDocumentIntelligenceResponseDto azureResponse);

        /// <summary>
        /// Convierte la respuesta de Azure Document Intelligence a formato ComprobanteExtractorResult
        /// </summary>
        /// <param name="azureResponse">Respuesta de Azure</param>
        /// <returns>Resultado con los campos extraídos del comprobante</returns>
        CapaDatos.ContabilidadAPI.Models.ComprobanteExtractorResult ConvertToComprobanteExtractorResult(AzureDocumentIntelligenceResponseDto azureResponse);

        /// <summary>
        /// Verifica si el servicio está habilitado y configurado correctamente
        /// </summary>
        /// <returns>True si está configurado y habilitado</returns>
        bool IsEnabled();
    }
}
