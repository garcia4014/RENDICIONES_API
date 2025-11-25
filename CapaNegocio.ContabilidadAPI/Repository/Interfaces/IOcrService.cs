using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    /// <summary>
    /// Interface para el servicio de OCR con Tesseract
    /// </summary>
    public interface IOcrService
    {
        /// <summary>
        /// Extrae texto de un archivo (PDF o imagen) usando OCR
        /// </summary>
        /// <param name="request">Datos del archivo y configuración de OCR</param>
        /// <returns>Texto extraído con información adicional</returns>
        Task<ApiResponse<OcrResponseDto>> ExtractTextAsync(OcrRequestDto request);

        /// <summary>
        /// Extrae texto de una imagen específica
        /// </summary>
        /// <param name="imageBytes">Bytes de la imagen</param>
        /// <param name="language">Idioma para OCR (por defecto español)</param>
        /// <param name="pageSegMode">Modo de segmentación</param>
        /// <returns>Texto extraído</returns>
        Task<ApiResponse<OcrResponseDto>> ExtractTextFromImageAsync(
            byte[] imageBytes, 
            string language = "spa", 
            OcrPageSegMode pageSegMode = OcrPageSegMode.Auto);

        /// <summary>
        /// Extrae texto de un PDF página por página
        /// </summary>
        /// <param name="pdfBytes">Bytes del PDF</param>
        /// <param name="language">Idioma para OCR</param>
        /// <param name="pageSegMode">Modo de segmentación</param>
        /// <param name="maxPages">Máximo número de páginas a procesar</param>
        /// <returns>Texto extraído de todas las páginas</returns>
        Task<ApiResponse<OcrResponseDto>> ExtractTextFromPdfAsync(
            byte[] pdfBytes, 
            string language = "spa", 
            OcrPageSegMode pageSegMode = OcrPageSegMode.Auto,
            int maxPages = 50);

        /// <summary>
        /// Verifica si Tesseract está correctamente configurado
        /// </summary>
        /// <returns>True si está configurado correctamente</returns>
        Task<bool> IsConfiguredAsync();

        /// <summary>
        /// Obtiene los idiomas disponibles en Tesseract
        /// </summary>
        /// <returns>Lista de códigos de idioma disponibles</returns>
        Task<List<string>> GetAvailableLanguagesAsync();

        /// <summary>
        /// Preprocesa una imagen para mejorar la precisión del OCR
        /// </summary>
        /// <param name="imageBytes">Imagen original</param>
        /// <returns>Imagen procesada</returns>
        Task<byte[]> PreprocessImageAsync(byte[] imageBytes);
    }
}