using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PdfSharp.BigGustave;

namespace ContabilidadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OcrController : ControllerBase
    {
        private readonly IOcrService _ocrService;
        private readonly ILogger<OcrController> _logger;

        public OcrController(IOcrService ocrService, ILogger<OcrController> logger)
        {
            _ocrService = ocrService;
            _logger = logger;
        }

        /// <summary>
        /// Extrae texto de un archivo usando OCR (PDF o imagen)
        /// </summary>
        /// <param name="file">Archivo a procesar (PDF, JPG, PNG, etc.)</param>
        /// <param name="language">Idioma para OCR (por defecto: spa)</param>
        /// <param name="preprocessImage">Si preprocesar la imagen para mejor OCR</param>
        /// <returns>Texto extraído del archivo</returns>
        [HttpPost("extract-text")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ExtractText(
            IFormFile file,
            string language = "spa",
            bool preprocessImage = true)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No se ha proporcionado un archivo válido");
                }

                _logger.LogInformation("Procesando archivo: {FileName}, Tamaño: {FileSize} bytes", 
                    file.FileName, file.Length);

                // Convertir archivo a bytes
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                // Determinar tipo de archivo
                var fileType = GetFileType(file.FileName);
                if (fileType == null)
                {
                    return BadRequest("Tipo de archivo no soportado. Formatos válidos: PDF, JPG, JPEG, PNG, BMP, TIFF");
                }

                // Crear solicitud OCR
                var request = new OcrRequestDto
                {
                    FileData = fileBytes,
                    FileType = fileType.Value,
                    Language = language,
                    PreprocessImage = preprocessImage,
                    FileName = file.FileName
                };

                // Procesar con OCR
                var result = await _ocrService.ExtractTextAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation("OCR completado exitosamente para {FileName}. Texto extraído: {TextLength} caracteres", 
                        file.FileName, result.Data?.ExtractedText?.Length ?? 0);
                }
                else
                {
                    _logger.LogWarning("Error en OCR para {FileName}: {Error}", file.FileName, result.Message);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado procesando archivo OCR: {FileName}", file.FileName);
                return StatusCode(500, "Error interno del servidor procesando el archivo");
            }
        }

        /// <summary>
        /// Extrae texto solo de imágenes
        /// </summary>
        /// <param name="file">Archivo de imagen (JPG, PNG, BMP, TIFF)</param>
        /// <param name="language">Idioma para OCR</param>
        /// <returns>Texto extraído de la imagen</returns>
        [HttpPost("extract-text-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ExtractTextFromImage(
            IFormFile file,
            string language = "spa")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No se ha proporcionado una imagen válida");
                }

                var fileType = GetFileType(file.FileName);
                if (fileType == null || fileType == OcrFileType.PDF)
                {
                    return BadRequest("Solo se permiten imágenes (JPG, PNG, BMP, TIFF)");
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                var result = await _ocrService.ExtractTextFromImageAsync(imageBytes, language);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando imagen: {FileName}", file.FileName);
                return StatusCode(500, "Error procesando la imagen");
            }
        }

        /// <summary>
        /// Extrae texto solo de PDFs
        /// </summary>
        /// <param name="file">Archivo PDF</param>
        /// <param name="language">Idioma para OCR</param>
        /// <param name="maxPages">Máximo número de páginas a procesar</param>
        /// <returns>Texto extraído del PDF</returns>
        [HttpPost("extract-text-pdf")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ExtractTextFromPdf(
            IFormFile file,
            string language = "spa",
            int maxPages = 50)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No se ha proporcionado un PDF válido");
                }

                if (!file.FileName.ToLower().EndsWith(".pdf"))
                {
                    return BadRequest("Solo se permiten archivos PDF");
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var pdfBytes = memoryStream.ToArray();

                var result = await _ocrService.ExtractTextFromPdfAsync(pdfBytes, language, OcrPageSegMode.Auto, maxPages);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando PDF: {FileName}", file.FileName);
                return StatusCode(500, "Error procesando el PDF");
            }
        }

        /// <summary>
        /// Verifica si el servicio OCR está configurado correctamente
        /// </summary>
        /// <returns>Estado de la configuración</returns>
        [HttpGet("health")]
        public async Task<IActionResult> GetHealthStatus()
        {
            try
            {
                var isConfigured = await _ocrService.IsConfiguredAsync();
                var availableLanguages = await _ocrService.GetAvailableLanguagesAsync();

                var status = new
                {
                    IsConfigured = isConfigured,
                    AvailableLanguages = availableLanguages,
                    SupportedFormats = new[] { "PDF", "JPG", "JPEG", "PNG", "BMP", "TIFF" },
                    Status = isConfigured ? "OK" : "NOT_CONFIGURED",
                    Message = isConfigured ? "Servicio OCR configurado correctamente" : "Servicio OCR no configurado - verifique archivos .traineddata"
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando estado del servicio OCR");
                return StatusCode(500, "Error verificando estado del servicio");
            }
        }

        /// <summary>
        /// Preprocesa una imagen para mejorar el OCR
        /// </summary>
        /// <param name="file">Imagen a preprocesar</param>
        /// <returns>Imagen preprocesada</returns>
        [HttpPost("preprocess-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PreprocessImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No se ha proporcionado una imagen válida");
                }

                var fileType = GetFileType(file.FileName);
                if (fileType == null || fileType == OcrFileType.PDF)
                {
                    return BadRequest("Solo se permiten imágenes");
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                var processedImage = await _ocrService.PreprocessImageAsync(imageBytes);

                return File(processedImage, "image/png", $"processed_{file.FileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preprocesando imagen: {FileName}", file.FileName);
                return StatusCode(500, "Error preprocesando la imagen");
            }
        }

        private OcrFileType? GetFileType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            var extension = Path.GetExtension(fileName).ToLower();
            
            return extension switch
            {
                ".pdf" => OcrFileType.PDF,
                ".jpg" => OcrFileType.JPG,
                ".jpeg" => OcrFileType.JPEG,
                ".png" => OcrFileType.PNG,
                ".bmp" => OcrFileType.BMP,
                ".tiff" => OcrFileType.TIFF,
                ".tif" => OcrFileType.TIFF,
                _ => null
            };
        }

        [HttpPost("extract-text-all")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> GetDetailOCR(
            IFormFile file,
            string extension,
            string language = "spa",
            int maxPages = 50)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No se ha proporcionado un archivo válido");
                }

                if (!extension.ToLower().EndsWith("pdf") && !extension.ToLower().EndsWith("jpg") && 
                    !extension.ToLower().EndsWith("png") && !extension.ToLower().EndsWith("tiff") && 
                    !extension.ToLower().EndsWith("xml"))
                {
                    return BadRequest("Solo se permiten archivos PDF, imágenes (JPG, PNG, TIFF) o XML");
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                
                ComprobanteExtractorResult resultOcr;

                if (extension.ToUpper().Contains("XML"))
                {
                    // Procesar XML directamente
                    _logger.LogInformation("Procesando archivo XML: {FileName}", file.FileName);
                    var xmlContent = System.Text.Encoding.UTF8.GetString(fileBytes);
                    resultOcr = ComprobanteExtractor.ExtractFromXml(xmlContent);
                }
                else
                {
                    // Procesar PDF o imagen con OCR
                    ApiResponse<OcrResponseDto> result;
                    if (extension.ToUpper().Contains("PDF"))
                    {
                        _logger.LogInformation("Procesando archivo PDF: {FileName}", file.FileName);
                        result = await _ocrService.ExtractTextFromPdfAsync(fileBytes, language, OcrPageSegMode.Auto, maxPages);
                    }
                    else
                    {
                        _logger.LogInformation("Procesando imagen: {FileName}", file.FileName);
                        result = await _ocrService.ExtractTextFromImageAsync(fileBytes, language, OcrPageSegMode.Auto); 
                    }

                    resultOcr = ComprobanteExtractor.Extract(result.Data.ExtractedText);
                }

                return Ok(resultOcr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando archivo: {FileName}", file.FileName);
                return StatusCode(500, $"Error procesando el archivo: {ex.Message}");
            }

        }

        /// <summary>
        /// Extrae información de un archivo XML de comprobante electrónico
        /// </summary>
        /// <param name="file">Archivo XML del comprobante</param>
        /// <returns>Información extraída del XML</returns>
        [HttpPost("extract-text-xml")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ExtractTextFromXml(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No se ha proporcionado un archivo válido");
                }

                if (!file.FileName.ToLower().EndsWith(".xml"))
                {
                    return BadRequest("Solo se permiten archivos XML");
                }

                _logger.LogInformation("Procesando archivo XML: {FileName}, Tamaño: {FileSize} bytes", 
                    file.FileName, file.Length);

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var xmlBytes = memoryStream.ToArray();

                // Convertir bytes a string
                var xmlContent = System.Text.Encoding.UTF8.GetString(xmlBytes);

                // Extraer información del XML usando el extractor de comprobantes
                var resultOcr = ComprobanteExtractor.ExtractFromXml(xmlContent);

                _logger.LogInformation("XML procesado exitosamente para {FileName}", file.FileName);

                return Ok(resultOcr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando XML: {FileName}", file.FileName);
                return StatusCode(500, "Error procesando el archivo XML");
            }
        }

    }
}