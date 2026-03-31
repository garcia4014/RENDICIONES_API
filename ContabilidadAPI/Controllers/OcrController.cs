using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using CapaDatos.ContabilidadAPI;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.IO.Compression;
using System.Text;

namespace ContabilidadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OcrController : ControllerBase
    {
        private readonly IOcrService _ocrService;
        private readonly IAzureDocumentIntelligenceService _azureDocService;
        private readonly ILogger<OcrController> _logger;
        private readonly SvrendicionesContext _dbContext;
        private readonly HttpClient _httpClient;

        public OcrController(
            IOcrService ocrService,
            IAzureDocumentIntelligenceService azureDocService,
            ILogger<OcrController> logger,
            SvrendicionesContext dbContext,
            IHttpClientFactory httpClientFactory)
        {
            _ocrService = ocrService;
            _azureDocService = azureDocService;
            _logger = logger;
            _dbContext = dbContext;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
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
                    !extension.ToLower().EndsWith("xml") && !extension.ToLower().EndsWith("jpeg") )
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
                    // Verificar si Azure IA está habilitado
                    if (_azureDocService.IsEnabled())
                    {
                        try
                        {
                            _logger.LogInformation("Procesando con Azure Document Intelligence: {FileName}", file.FileName);
                            
                            // Procesar con Azure IA
                            var azureResult = await _azureDocService.AnalyzeDocumentFromBytesAsync(fileBytes);
                            
                            if (azureResult.Success && azureResult.Data != null)
                            {
                                _logger.LogInformation("Documento procesado exitosamente con Azure IA: {FileName}", file.FileName);
                                
                                // Convertir la respuesta de Azure al formato ComprobanteExtractorResult
                                var extractorResult = _azureDocService.ConvertToComprobanteExtractorResult(azureResult.Data);
                                
                                // Limpiar saltos de línea de todos los campos
                                CleanLineBreaks(extractorResult);
                                
                                // Sumarizar montos de afectación
                                SumarizarMontosAfectacion(extractorResult);
                                
                                return Ok(extractorResult);
                            }
                            else
                            {
                                _logger.LogWarning("Azure IA falló para {FileName}: {Message}. Usando fallback a Tesseract", 
                                    file.FileName, azureResult.Message);
                            }
                        }
                        catch (Exception azureEx)
                        {
                            _logger.LogError(azureEx, "Error con Azure IA para {FileName}. Usando fallback a Tesseract", file.FileName);
                        }
                    }

                    // Flujo original con Tesseract (fallback o cuando Azure no está habilitado)
                    _logger.LogInformation("Procesando con Tesseract OCR: {FileName}", file.FileName);
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

                // Limpiar saltos de línea de todos los campos
                CleanLineBreaks(resultOcr);
                
                // Sumarizar montos de afectación
                SumarizarMontosAfectacion(resultOcr);

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

        /// <summary>
        /// Analiza un documento usando Azure Document Intelligence desde una URL
        /// </summary>
        /// <param name="request">Solicitud con URL del documento y campos opcionales</param>
        /// <returns>Respuesta con los campos extraídos por Azure IA</returns>
        [HttpPost("azure-analyze-url")]
        public async Task<IActionResult> AzureAnalyzeFromUrl([FromBody] AzureDocumentIntelligenceRequestDto request)
        {
            try
            {
                if (!_azureDocService.IsEnabled())
                {
                    return BadRequest(new ApiResponse<object>(
                        "Azure Document Intelligence no está habilitado. Verifique la configuración en appsettings."));
                }

                if (string.IsNullOrWhiteSpace(request.UrlSource))
                {
                    return BadRequest(new ApiResponse<object>(
                        "La URL del documento es requerida"));
                }

                _logger.LogInformation("Analizando documento desde URL: {Url}", request.UrlSource);

                //var result = await _azureDocService.AnalyzeDocumentFromUrlAsync(
                //    request.UrlSource,
                //    request.QueryFields);
                var result = await _azureDocService.AnalyzeDocumentFromUrlAsync(
                request.UrlSource,
                null);

                if (result.Success)
                {
                    _logger.LogInformation("Documento analizado exitosamente desde URL: {Url}", request.UrlSource);
                }
                else
                {
                    _logger.LogWarning("Error al analizar documento desde URL {Url}: {Message}",
                        request.UrlSource, result.Message);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado analizando documento desde URL");
                return StatusCode(500, new ApiResponse<object>(
                    "Error interno del servidor al analizar el documento"));
            }
        }

        /// <summary>
        /// Analiza un documento usando Azure Document Intelligence desde un archivo
        /// </summary>
        /// <param name="file">Archivo del documento (PDF o imagen)</param>
        /// <param name="queryFields">Campos personalizados a extraer (opcional, separados por coma)</param>
        /// <returns>Respuesta con los campos extraídos por Azure IA</returns>
        [HttpPost("azure-analyze-file")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AzureAnalyzeFromFile(
            IFormFile file,
            string? queryFields = null)
        {
            try
            {
                if (!_azureDocService.IsEnabled())
                {
                    return BadRequest(new ApiResponse<object>(
                        "Azure Document Intelligence no está habilitado. Verifique la configuración en appsettings."));
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse<object>(
                        "No se ha proporcionado un archivo válido"));
                }

                _logger.LogInformation("Analizando archivo con Azure IA: {FileName}, Tamaño: {FileSize} bytes",
                    file.FileName, file.Length);

                // Convertir archivo a bytes
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                // Parsear queryFields si están presentes
                List<string>? fieldsList = null;
                if (!string.IsNullOrWhiteSpace(queryFields))
                {
                    fieldsList = queryFields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .ToList();
                }

                var result = await _azureDocService.AnalyzeDocumentFromBytesAsync(fileBytes, fieldsList);

                if (result.Success)
                {
                    _logger.LogInformation("Archivo analizado exitosamente con Azure IA: {FileName}", file.FileName);
                }
                else
                {
                    _logger.LogWarning("Error al analizar archivo {FileName}: {Message}",
                        file.FileName, result.Message);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado analizando archivo con Azure IA");
                return StatusCode(500, new ApiResponse<object>(
                    "Error interno del servidor al analizar el archivo"));
            }
        }

        /// <summary>
        /// Obtiene el estado de los servicios OCR (Tesseract y Azure IA)
        /// </summary>
        /// <returns>Estado de configuración de ambos servicios</returns>
        [HttpGet("services-status")]
        public async Task<IActionResult> GetServicesStatus()
        {
            try
            {
                var tesseractConfigured = await _ocrService.IsConfiguredAsync();
                var azureEnabled = _azureDocService.IsEnabled();

                var status = new
                {
                    Tesseract = new
                    {
                        IsConfigured = tesseractConfigured,
                        Status = tesseractConfigured ? "OK" : "NOT_CONFIGURED",
                        Message = tesseractConfigured
                            ? "Tesseract OCR configurado correctamente"
                            : "Tesseract OCR no configurado"
                    },
                    AzureDocumentIntelligence = new
                    {
                        IsEnabled = azureEnabled,
                        Status = azureEnabled ? "ENABLED" : "DISABLED",
                        Message = azureEnabled
                            ? "Azure Document Intelligence está habilitado y configurado"
                            : "Azure Document Intelligence no está habilitado"
                    },
                    PreferredService = azureEnabled ? "Azure Document Intelligence" : "Tesseract OCR",
                    SupportedFormats = new[] { "PDF", "JPG", "JPEG", "PNG", "BMP", "TIFF" }
                };

                return Ok(new ApiResponse<object>(
                    status,
                    "Estado de servicios OCR obtenido exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando estado de servicios OCR");
                return StatusCode(500, new ApiResponse<object>(
                    "Error verificando estado de servicios"));
            }
        }

        /// <summary>
        /// Limpia los saltos de línea de todos los campos del resultado
        /// </summary>
        /// <param name="result">Resultado del extractor de comprobantes</param>
        /// <summary>
        /// Obtiene datos del comprobante directamente desde SUNAT usando RUC, Serie, Correlativo y Tipo
        /// </summary>
        /// <param name="ruc">RUC del emisor</param>
        /// <param name="serie">Serie del comprobante</param>
        /// <param name="correlativo">Correlativo del comprobante</param>
        /// <param name="tipoComprobante">Tipo de comprobante (01=Factura, 03=Boleta, etc.)</param>
        /// <returns>Datos del comprobante en formato ComprobanteExtractorResult</returns>
        [HttpGet("consultar-sunat")]
        public async Task<IActionResult> ConsultarComprobanteSunat(
            [FromQuery] string ruc,
            [FromQuery] string serie,
            [FromQuery] string correlativo,
            [FromQuery] string? tipoComprobante = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ruc) || string.IsNullOrWhiteSpace(serie) || string.IsNullOrWhiteSpace(correlativo))
                {
                    return BadRequest("Debe proporcionar RUC, Serie y Correlativo");
                }

                _logger.LogInformation("Consultando comprobante en SUNAT: RUC={Ruc}, Serie={Serie}, Correlativo={Correlativo}, Tipo={Tipo}",
                    ruc, serie, correlativo, tipoComprobante);

                // Si no se proporciona tipo, determinarlo por la serie
                if (string.IsNullOrWhiteSpace(tipoComprobante))
                {
                    tipoComprobante = DeterminarTipoComprobantePorSerie(serie);
                    _logger.LogInformation("Tipo de comprobante determinado automáticamente: {Tipo}", tipoComprobante);
                }

                // Obtener el XML desde SUNAT
                var result = await ObtenerDatosComprobanteSunatAsync(ruc, serie, correlativo, tipoComprobante);

                if (result == null)
                {
                    return NotFound(new { message = "No se pudo obtener el comprobante desde SUNAT. Verifique los datos o el token de autorización." });
                }

                // Limpiar saltos de línea
                CleanLineBreaks(result);

                // Sumarizar montos de afectación
                SumarizarMontosAfectacion(result);

                _logger.LogInformation("Comprobante obtenido exitosamente desde SUNAT");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar comprobante en SUNAT");
                return StatusCode(500, new { message = "Error interno al consultar SUNAT", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene datos del comprobante desde SUNAT consultando el XML
        /// </summary>
        private async Task<ComprobanteExtractorResult?> ObtenerDatosComprobanteSunatAsync(
            string ruc, string serie, string correlativo, string tipoComprobante)
        {
            try
            {
                _logger.LogInformation("===== INICIO: Obtención XML desde SUNAT =====");
                
                // Asegurar que el tipo de comprobante tenga 2 dígitos (01, 03, 07, etc.)
                tipoComprobante = tipoComprobante.PadLeft(2, '0');
                
                _logger.LogInformation("Parámetros: RUC={Ruc}, Serie={Serie}, Correlativo={Correlativo}, Tipo={Tipo}",
                    ruc, serie, correlativo, tipoComprobante);

                // Obtener token de autorización desde BD
                _logger.LogInformation("PASO 1: Consultando token SUNAT en BD (PARAMETROS.Id=1)...");
                var parametro = await _dbContext.Parametros
                    .FirstOrDefaultAsync(p => p.Id == 1);

                if (parametro == null || string.IsNullOrWhiteSpace(parametro.Valor))
                {
                    _logger.LogWarning("No se encontró el token SUNAT en la tabla PARAMETROS (Id=1)");
                    return null;
                }

                var token = parametro.Valor;
                _logger.LogInformation("Token SUNAT obtenido exitosamente (longitud: {Length} caracteres)", token.Length);

                // Construir URL
                _logger.LogInformation("PASO 2: Construyendo URL de consulta...");
                var url = $"https://api-cpe.sunat.gob.pe/v1/contribuyente/consultacpe/comprobantes/{ruc}-{tipoComprobante}-{serie}-{correlativo}-2/02";
                _logger.LogInformation("URL construida: {Url}", url);

                // Configurar request con reintentos
                _logger.LogInformation("PASO 3: Configurando headers y realizando petición HTTP GET...");
                const int maxRetries = 5;
                HttpResponseMessage? response = null;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Add("Accept", "application/json, text/plain, */*");
                        request.Headers.Add("Accept-Language", "es,es-ES;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6,es-PE;q=0.5");
                        request.Headers.Add("Origin", "https://e-factura.sunat.gob.pe");
                        request.Headers.Add("Referer", "https://e-factura.sunat.gob.pe/");
                        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36 Edg/144.0.0.0");
                        request.Headers.Add("Authorization", $"Bearer {token}");

                        if (attempt > 1)
                        {
                            _logger.LogInformation("Intento {Attempt} de {MaxRetries} para consultar SUNAT...", attempt, maxRetries);
                        }

                        response = await _httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Respuesta HTTP exitosa: {StatusCode}", response.StatusCode);
                            break;
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();

                            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                _logger.LogWarning("Error de autenticación en SUNAT: {StatusCode} - {Error}", response.StatusCode, errorContent);
                                return null;
                            }

                            if ((response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                                 response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                                 response.StatusCode == System.Net.HttpStatusCode.BadGateway) &&
                                attempt < maxRetries)
                            {
                                var delaySeconds = Math.Pow(2, attempt - 1);
                                _logger.LogWarning("Error temporal en SUNAT (intento {Attempt}/{MaxRetries}): {StatusCode}. Reintentando en {Delay} segundos...",
                                    attempt, maxRetries, response.StatusCode, delaySeconds);
                                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                                continue;
                            }
                            else
                            {
                                _logger.LogWarning("Error al consultar SUNAT (intento {Attempt}/{MaxRetries}): {StatusCode} - {Error}",
                                    attempt, maxRetries, response.StatusCode, errorContent);

                                if (attempt == maxRetries)
                                {
                                    return null;
                                }
                            }
                        }
                    }
                    catch (HttpRequestException httpEx)
                    {
                        if (attempt < maxRetries)
                        {
                            var delaySeconds = Math.Pow(2, attempt - 1);
                            _logger.LogWarning(httpEx, "Excepción HTTP en SUNAT (intento {Attempt}/{MaxRetries}). Reintentando en {Delay} segundos...",
                                attempt, maxRetries, delaySeconds);
                            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                            continue;
                        }
                        else
                        {
                            _logger.LogError(httpEx, "Error HTTP al consultar SUNAT después de {MaxRetries} intentos", maxRetries);
                            return null;
                        }
                    }
                    catch (TaskCanceledException tcEx)
                    {
                        if (attempt < maxRetries)
                        {
                            var delaySeconds = Math.Pow(2, attempt - 1);
                            _logger.LogWarning(tcEx, "Timeout en SUNAT (intento {Attempt}/{MaxRetries}). Reintentando en {Delay} segundos...",
                                attempt, maxRetries, delaySeconds);
                            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                            continue;
                        }
                        else
                        {
                            _logger.LogError(tcEx, "Timeout en SUNAT después de {MaxRetries} intentos", maxRetries);
                            return null;
                        }
                    }
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No se pudo obtener respuesta exitosa de SUNAT después de {MaxRetries} intentos", maxRetries);
                    return null;
                }

                _logger.LogInformation("PASO 4: Procesando respuesta JSON...");
                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("JSON recibido (longitud: {Length} caracteres)", jsonResponse.Length);

                var sunatResponse = JsonSerializer.Deserialize<SunatXmlResponse>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (sunatResponse == null || string.IsNullOrWhiteSpace(sunatResponse.ValArchivo))
                {
                    _logger.LogWarning("Respuesta de SUNAT vacía o sin valArchivo");
                    return null;
                }

                _logger.LogInformation("Archivo recibido: {NomArchivo}, ValArchivo (Base64 longitud: {Length})",
                    sunatResponse.NomArchivo, sunatResponse.ValArchivo?.Length ?? 0);

                // Decodificar Base64 y extraer XML del ZIP
                _logger.LogInformation("PASO 5: Decodificando Base64 y extrayendo XML del ZIP...");
                var xmlContent = ExtraerXmlDeZip(sunatResponse.ValArchivo!);

                if (string.IsNullOrWhiteSpace(xmlContent))
                {
                    _logger.LogWarning("No se pudo extraer XML del archivo ZIP");
                    return null;
                }

                _logger.LogInformation("XML extraído exitosamente (longitud: {Length} caracteres)", xmlContent.Length);
                _logger.LogInformation("PASO 6: Procesando XML con ExtractFromXml...");

                // Procesar XML con el método existente
                var result = ComprobanteExtractor.ExtractFromXml(xmlContent);

                _logger.LogInformation("XML procesado - Gravados: {G}, Inafectos: {I}, Exonerados: {E}, AfectacionDetectada: {AD}",
                    result.MontosGravados.Count, result.MontosInafectos.Count, result.MontosExonerados.Count, result.AfectacionIgvDetectada);

                _logger.LogInformation("===== FIN: XML obtenido y procesado exitosamente =====");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos desde SUNAT");
                return null;
            }
        }

        /// <summary>
        /// Determina el tipo de comprobante SUNAT según la serie
        /// </summary>
        private string DeterminarTipoComprobantePorSerie(string serie)
        {
            if (string.IsNullOrWhiteSpace(serie))
                return "01";

            var serieUpper = serie.ToUpper();

            if (serieUpper.StartsWith("F") || serieUpper.StartsWith("E"))
                return "01"; // Factura

            if (serieUpper.StartsWith("B"))
                return "03"; // Boleta

            return "01"; // Por defecto Factura
        }

        /// <summary>
        /// Extrae el contenido XML de un archivo ZIP codificado en Base64
        /// </summary>
        private string? ExtraerXmlDeZip(string base64Zip)
        {
            try
            {
                _logger.LogInformation("Decodificando Base64 (longitud: {Length})...", base64Zip.Length);
                var zipBytes = Convert.FromBase64String(base64Zip);
                _logger.LogInformation("ZIP decodificado: {Size} bytes", zipBytes.Length);

                using var zipStream = new MemoryStream(zipBytes);
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

                _logger.LogInformation("ZIP abierto, contiene {Count} entradas", archive.Entries.Count);

                // Excluir archivos que empiezan con "R-" (CDR de SUNAT, no el comprobante)
                var xmlEntry = archive.Entries.FirstOrDefault(e =>
                    e.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
                    !e.Name.StartsWith("R-", StringComparison.OrdinalIgnoreCase));

                if (xmlEntry == null)
                {
                    _logger.LogWarning("No se encontró archivo XML en el ZIP");
                    return null;
                }

                _logger.LogInformation("Archivo XML encontrado: {Name}, extrayendo contenido...", xmlEntry.Name);

                using var entryStream = xmlEntry.Open();
                using var reader = new StreamReader(entryStream, Encoding.UTF8);
                var xmlContent = reader.ReadToEnd();
                _logger.LogInformation("Contenido XML extraído: {Length} caracteres", xmlContent.Length);
                return xmlContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer XML del ZIP");
                return null;
            }
        }

        /// <summary>
        /// Clase para deserializar la respuesta de SUNAT
        /// </summary>
        private class SunatXmlResponse
        {
            public string? NomArchivo { get; set; }
            public string? ValArchivo { get; set; }
        }

        private void CleanLineBreaks(ComprobanteExtractorResult result)
        {
            if (result == null) return;

            // Limpiar RUCs
            for (int i = 0; i < result.Rucs.Count; i++)
            {
                result.Rucs[i] = CleanText(result.Rucs[i]);
            }

            // Limpiar Razones Sociales
            for (int i = 0; i < result.RazonesSociales.Count; i++)
            {
                result.RazonesSociales[i] = CleanText(result.RazonesSociales[i]);
            }

            // Limpiar Fechas de Emisión
            for (int i = 0; i < result.FechasEmision.Count; i++)
            {
                result.FechasEmision[i] = CleanText(result.FechasEmision[i]);
            }

            // Limpiar Montos Totales
            for (int i = 0; i < result.MontosTotales.Count; i++)
            {
                result.MontosTotales[i] = CleanText(result.MontosTotales[i]);
            }

            // Limpiar Series
            for (int i = 0; i < result.Series.Count; i++)
            {
                result.Series[i] = CleanText(result.Series[i]);
            }

            // Limpiar Correlativos
            for (int i = 0; i < result.Correlativos.Count; i++)
            {
                result.Correlativos[i] = CleanText(result.Correlativos[i]);
            }
        }

        /// <summary>
        /// Limpia un texto eliminando saltos de línea y espacios múltiples
        /// </summary>
        /// <param name="text">Texto a limpiar</param>
        /// <returns>Texto limpio sin saltos de línea</returns>
        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Reemplazar saltos de línea por espacios
            text = text.Replace("\r\n", " ")
                       .Replace("\r", " ")
                       .Replace("\n", " ")
                       .Replace("\t", " ");

            // Reemplazar múltiples espacios por uno solo
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            // Eliminar espacios al inicio y al final
            return text.Trim();
        }

        /// <summary>
        /// Sumariza los montos de cada tipo de afectación del IGV, dejando solo un total por cada array
        /// </summary>
        /// <param name="result">Resultado del extractor de comprobantes</param>
        private void SumarizarMontosAfectacion(ComprobanteExtractorResult result)
        {
            if (result == null) return;

            // Sumarizar Montos Gravados
            result.MontosGravados = SumarArray(result.MontosGravados);

            // Sumarizar Montos Inafectos
            result.MontosInafectos = SumarArray(result.MontosInafectos);

            // Sumarizar Montos Exonerados
            result.MontosExonerados = SumarArray(result.MontosExonerados);

            // Sumarizar TaxAmounts de IGV Especial (impuesto cobrado por línea)
            result.MontosIgvEspecial = SumarArray(result.MontosIgvEspecial);

            // Sumarizar bases imponibles de IGV Especial (suma de TaxableAmount por línea)
            // Este valor es el que representa el subtotal real para facturas con IGV reducido
            result.MontosBaseIgvEspecial = SumarArray(result.MontosBaseIgvEspecial);

            // Sumarizar Montos Impuesto Consumo
            result.MontosImpuestoConsumo = SumarArray(result.MontosImpuestoConsumo);
        }

        /// <summary>
        /// Suma todos los valores de un array de strings numéricos y retorna un array con un solo elemento (la suma total)
        /// </summary>
        /// <param name="valores">Array de valores numéricos en formato string</param>
        /// <returns>Array con un solo elemento que contiene la suma total, o array vacío si no hay valores</returns>
        private List<string> SumarArray(List<string> valores)
        {
            if (valores == null || valores.Count == 0)
                return new List<string>();

            decimal suma = 0;
            foreach (var valor in valores)
            {
                if (decimal.TryParse(valor, System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out decimal numero))
                {
                    suma += numero;
                }
            }

            // Si la suma es 0, retornar array vacío
            if (suma == 0)
                return new List<string>();

            // Retornar array con un solo elemento: la suma formateada con 2 decimales
            return new List<string> { suma.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) };
        }

    }
}