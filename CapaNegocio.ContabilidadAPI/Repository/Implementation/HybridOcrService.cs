using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Servicio OCR híbrido que usa Azure IA cuando está habilitado, 
    /// o Tesseract como fallback
    /// </summary>
    public class HybridOcrService : IOcrService
    {
        private readonly IAzureDocumentIntelligenceService _azureService;
        private readonly IOcrService _tesseractService;
        private readonly ILogger<HybridOcrService> _logger;

        public HybridOcrService(
            IAzureDocumentIntelligenceService azureService,
            TesseractOcrService tesseractService,
            ILogger<HybridOcrService> logger)
        {
            _azureService = azureService;
            _tesseractService = tesseractService;
            _logger = logger;
        }

        public async Task<ApiResponse<OcrResponseDto>> ExtractTextAsync(OcrRequestDto request)
        {
            // Si Azure IA está habilitado, usarlo primero
            if (_azureService.IsEnabled())
            {
                try
                {
                    _logger.LogInformation("Usando Azure Document Intelligence para extracción de texto");

                    var azureResponse = await _azureService.AnalyzeDocumentFromBytesAsync(request.FileData);

                    if (azureResponse.Success && azureResponse.Data != null)
                    {
                        var ocrResponse = _azureService.ConvertToOcrResponse(azureResponse.Data);
                        ocrResponse.FileName = request.FileName;
                        ocrResponse.FileType = request.FileType;

                        return new ApiResponse<OcrResponseDto>(
                            ocrResponse,
                            "Texto extraído exitosamente usando Azure Document Intelligence");
                    }
                    else
                    {
                        _logger.LogWarning("Azure Document Intelligence falló: {Message}. Usando Tesseract como fallback",
                            azureResponse.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error usando Azure Document Intelligence. Usando Tesseract como fallback");
                }
            }
            else
            {
                _logger.LogInformation("Azure Document Intelligence no está habilitado. Usando Tesseract");
            }

            // Fallback a Tesseract
            return await _tesseractService.ExtractTextAsync(request);
        }

        public async Task<ApiResponse<OcrResponseDto>> ExtractTextFromImageAsync(
            byte[] imageBytes,
            string language = "spa",
            OcrPageSegMode pageSegMode = OcrPageSegMode.Auto)
        {
            // Si Azure IA está habilitado, intentar usarlo
            if (_azureService.IsEnabled())
            {
                try
                {
                    _logger.LogInformation("Usando Azure Document Intelligence para extracción de imagen");

                    var azureResponse = await _azureService.AnalyzeDocumentFromBytesAsync(imageBytes);

                    if (azureResponse.Success && azureResponse.Data != null)
                    {
                        var ocrResponse = _azureService.ConvertToOcrResponse(azureResponse.Data);
                        ocrResponse.Language = language;

                        return new ApiResponse<OcrResponseDto>(
                            ocrResponse,
                            "Texto extraído exitosamente usando Azure Document Intelligence");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error usando Azure Document Intelligence. Usando Tesseract como fallback");
                }
            }

            // Fallback a Tesseract
            return await _tesseractService.ExtractTextFromImageAsync(imageBytes, language, pageSegMode);
        }

        public async Task<ApiResponse<OcrResponseDto>> ExtractTextFromPdfAsync(
            byte[] pdfBytes,
            string language = "spa",
            OcrPageSegMode pageSegMode = OcrPageSegMode.Auto,
            int maxPages = 50)
        {
            // Si Azure IA está habilitado, intentar usarlo
            if (_azureService.IsEnabled())
            {
                try
                {
                    _logger.LogInformation("Usando Azure Document Intelligence para extracción de PDF");

                    var azureResponse = await _azureService.AnalyzeDocumentFromBytesAsync(pdfBytes);

                    if (azureResponse.Success && azureResponse.Data != null)
                    {
                        var ocrResponse = _azureService.ConvertToOcrResponse(azureResponse.Data);
                        ocrResponse.Language = language;

                        return new ApiResponse<OcrResponseDto>(
                            ocrResponse,
                            "Texto extraído exitosamente usando Azure Document Intelligence");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error usando Azure Document Intelligence. Usando Tesseract como fallback");
                }
            }

            // Fallback a Tesseract
            return await _tesseractService.ExtractTextFromPdfAsync(pdfBytes, language, pageSegMode, maxPages);
        }

        public Task<bool> IsConfiguredAsync()
        {
            // Está configurado si Azure está habilitado O Tesseract está configurado
            if (_azureService.IsEnabled())
            {
                return Task.FromResult(true);
            }

            return _tesseractService.IsConfiguredAsync();
        }

        public Task<List<string>> GetAvailableLanguagesAsync()
        {
            // Delegar a Tesseract para idiomas disponibles
            return _tesseractService.GetAvailableLanguagesAsync();
        }

        public Task<byte[]> PreprocessImageAsync(byte[] imageBytes)
        {
            // Delegar a Tesseract para preprocesamiento
            return _tesseractService.PreprocessImageAsync(imageBytes);
        }
    }
}
