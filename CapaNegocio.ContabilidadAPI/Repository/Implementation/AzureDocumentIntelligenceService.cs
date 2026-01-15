using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Implementación del servicio de Azure Document Intelligence (Form Recognizer)
    /// </summary>
    public class AzureDocumentIntelligenceService : IAzureDocumentIntelligenceService
    {
        private readonly ILogger<AzureDocumentIntelligenceService> _logger;
        private readonly AzureDocumentIntelligenceConfigurationDto _configuration;
        private readonly HttpClient _httpClient;

        public AzureDocumentIntelligenceService(
            ILogger<AzureDocumentIntelligenceService> logger,
            IOptions<AzureDocumentIntelligenceConfigurationDto> configuration,
            HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.TimeoutSeconds);
        }

        public bool IsEnabled()
        {
            return _configuration.Enabled &&
                   !string.IsNullOrWhiteSpace(_configuration.Endpoint) &&
                   !string.IsNullOrWhiteSpace(_configuration.SubscriptionKey);
        }

        public async Task<ApiResponse<AzureDocumentIntelligenceResponseDto>> AnalyzeDocumentFromUrlAsync(
            string urlSource,
            List<string>? queryFields = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!IsEnabled())
                {
                    return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                        "Azure Document Intelligence no está habilitado o configurado correctamente");
                }

                _logger.LogInformation("Iniciando análisis de documento desde URL: {Url}", urlSource);

                // Construir la URL del endpoint de análisis
                var analyzeUrl = $"{_configuration.Endpoint}/formrecognizer/documentModels/{_configuration.ModelId}:analyze?api-version={_configuration.ApiVersion}";

                // Crear el request body
                var requestBody = new AzureDocumentIntelligenceRequestDto
                {
                    UrlSource = urlSource,
                    QueryFields = queryFields
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Configurar headers
                var request = new HttpRequestMessage(HttpMethod.Post, analyzeUrl)
                {
                    Content = content
                };
                request.Headers.Add("Ocp-Apim-Subscription-Key", _configuration.SubscriptionKey);

                // Enviar solicitud POST
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error en la solicitud a Azure: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                        $"Error al iniciar el análisis: {response.StatusCode}");
                }

                // Obtener la URL de operación desde el header Operation-Location
                if (!response.Headers.TryGetValues("Operation-Location", out var operationLocations))
                {
                    _logger.LogError("No se encontró el header Operation-Location en la respuesta");
                    return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                        "No se pudo obtener la URL de operación");
                }

                var operationLocation = operationLocations.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(operationLocation))
                {
                    return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                        "URL de operación inválida");
                }

                _logger.LogInformation("Documento enviado para análisis. Operation-Location: {Location}", operationLocation);

                // Consultar el resultado
                var result = await PollForResultAsync(operationLocation);

                stopwatch.Stop();
                _logger.LogInformation("Análisis completado en {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error inesperado en AnalyzeDocumentFromUrlAsync");
                return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                    $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AzureDocumentIntelligenceResponseDto>> AnalyzeDocumentFromBytesAsync(
            byte[] documentBytes,
            List<string>? queryFields = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!IsEnabled())
                {
                    return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                        "Azure Document Intelligence no está habilitado o configurado correctamente");
                }

                _logger.LogInformation("Iniciando análisis de documento desde bytes ({Size} bytes)", documentBytes.Length);

                // Construir la URL del endpoint de análisis
                var analyzeUrl = $"{_configuration.Endpoint}/formrecognizer/documentModels/{_configuration.ModelId}:analyze?api-version={_configuration.ApiVersion}";

                // Detectar el tipo de contenido basado en los magic bytes
                string contentType = DetectContentType(documentBytes);
                _logger.LogInformation("Tipo de contenido detectado: {ContentType}", contentType);

                // Crear el contenido con el documento
                var content = new ByteArrayContent(documentBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                // Configurar headers
                var request = new HttpRequestMessage(HttpMethod.Post, analyzeUrl)
                {
                    Content = content
                };
                request.Headers.Add("Ocp-Apim-Subscription-Key", _configuration.SubscriptionKey);

                // Si hay queryFields, agregarlos como parámetro de consulta
                if (queryFields != null && queryFields.Any())
                {
                    var fieldsParam = string.Join(",", queryFields);
                    request.RequestUri = new Uri($"{analyzeUrl}&queryFields={Uri.EscapeDataString(fieldsParam)}");
                }

                // Enviar solicitud POST
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error en la solicitud a Azure: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                        $"Error al iniciar el análisis: {response.StatusCode}");
                }

                // Obtener la URL de operación desde el header Operation-Location
                if (!response.Headers.TryGetValues("Operation-Location", out var operationLocations))
                {
                    _logger.LogError("No se encontró el header Operation-Location en la respuesta");
                    return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                        "No se pudo obtener la URL de operación");
                }

                var operationLocation = operationLocations.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(operationLocation))
                {
                    return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                        "URL de operación inválida");
                }

                _logger.LogInformation("Documento enviado para análisis. Operation-Location: {Location}", operationLocation);

                // Consultar el resultado
                var result = await PollForResultAsync(operationLocation);

                stopwatch.Stop();
                _logger.LogInformation("Análisis completado en {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error inesperado en AnalyzeDocumentFromBytesAsync");
                return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                    $"Error inesperado: {ex.Message}");
            }
        }

        private async Task<ApiResponse<AzureDocumentIntelligenceResponseDto>> PollForResultAsync(string operationLocation)
        {
            var attempts = 0;
            var maxAttempts = _configuration.MaxPollingAttempts;
            var pollingInterval = TimeSpan.FromMilliseconds(_configuration.PollingIntervalMs);

            while (attempts < maxAttempts)
            {
                attempts++;

                try
                {
                    // Crear solicitud GET
                    var request = new HttpRequestMessage(HttpMethod.Get, operationLocation);
                    request.Headers.Add("Ocp-Apim-Subscription-Key", _configuration.SubscriptionKey);

                    var response = await _httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Intento {Attempt}/{MaxAttempts} - Error al consultar resultado: {StatusCode}",
                            attempts, maxAttempts, response.StatusCode);
                        await Task.Delay(pollingInterval);
                        continue;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var azureResponse = JsonSerializer.Deserialize<AzureDocumentIntelligenceResponseDto>(
                        jsonResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (azureResponse == null)
                    {
                        _logger.LogWarning("Intento {Attempt}/{MaxAttempts} - Respuesta nula o inválida",
                            attempts, maxAttempts);
                        await Task.Delay(pollingInterval);
                        continue;
                    }

                    _logger.LogInformation("Intento {Attempt}/{MaxAttempts} - Estado: {Status}",
                        attempts, maxAttempts, azureResponse.Status);

                    // Verificar el estado
                    if (azureResponse.Status.Equals("succeeded", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Análisis completado exitosamente");
                        return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                            azureResponse,
                            "Análisis completado exitosamente");
                    }

                    if (azureResponse.Status.Equals("failed", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogError("El análisis falló en Azure");
                        return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                            "El análisis del documento falló");
                    }

                    // Estado "running" o "notStarted" - continuar esperando
                    await Task.Delay(pollingInterval);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error en intento {Attempt}/{MaxAttempts} al consultar resultado",
                        attempts, maxAttempts);
                    await Task.Delay(pollingInterval);
                }
            }

            _logger.LogError("Se alcanzó el número máximo de intentos ({MaxAttempts}) sin obtener resultado",
                maxAttempts);
            return new ApiResponse<AzureDocumentIntelligenceResponseDto>(
                "Tiempo de espera agotado para obtener el resultado del análisis");
        }

        public OcrResponseDto ConvertToOcrResponse(AzureDocumentIntelligenceResponseDto azureResponse)
        {
            var ocrResponse = new OcrResponseDto
            {
                ProcessedAt = DateTime.UtcNow,
                Language = "auto",
                FileType = OcrFileType.PDF
            };

            if (azureResponse?.AnalyzeResult == null)
            {
                return ocrResponse;
            }

            // Extraer el contenido completo
            ocrResponse.ExtractedText = azureResponse.AnalyzeResult.Content ?? string.Empty;

            // Procesar páginas
            if (azureResponse.AnalyzeResult.Pages != null)
            {
                ocrResponse.ProcessedPages = azureResponse.AnalyzeResult.Pages.Count;

                foreach (var page in azureResponse.AnalyzeResult.Pages)
                {
                    var pageResult = new OcrPageResultDto
                    {
                        PageNumber = page.PageNumber,
                        Text = string.Join(" ", page.Lines?.Select(l => l.Content) ?? new List<string>()),
                        WordCount = page.Words?.Count ?? 0,
                        Confidence = page.Words?.Average(w => w.Confidence ?? 0) ?? 0
                    };

                    ocrResponse.PageResults.Add(pageResult);
                }
            }

            // Calcular confianza promedio
            if (azureResponse.AnalyzeResult.Documents != null && azureResponse.AnalyzeResult.Documents.Any())
            {
                var avgConfidence = azureResponse.AnalyzeResult.Documents
                    .Where(d => d.Confidence.HasValue)
                    .Select(d => d.Confidence!.Value)
                    .DefaultIfEmpty(0)
                    .Average();

                ocrResponse.Confidence = avgConfidence;
            }

            return ocrResponse;
        }

        public CapaDatos.ContabilidadAPI.Models.ComprobanteExtractorResult ConvertToComprobanteExtractorResult(AzureDocumentIntelligenceResponseDto azureResponse)
        {
            var result = new CapaDatos.ContabilidadAPI.Models.ComprobanteExtractorResult();

            if (azureResponse?.AnalyzeResult?.Documents == null || !azureResponse.AnalyzeResult.Documents.Any())
            {
                return result;
            }

            var document = azureResponse.AnalyzeResult.Documents.First();
            var fields = document.Fields;

            if (fields == null)
            {
                return result;
            }

            try
            {
                // Extraer RUC (VendorTaxId)
                if (fields.TryGetValue("VendorTaxId", out var vendorTaxId) && 
                    !string.IsNullOrWhiteSpace(vendorTaxId?.ValueString))
                {
                    result.Rucs.Add(vendorTaxId.ValueString);
                }

                // Extraer Razón Social (VendorName o VendorAddressRecipient)
                if (fields.TryGetValue("VendorAddressRecipient", out var vendorAddressRecipient) && 
                    !string.IsNullOrWhiteSpace(vendorAddressRecipient?.ValueString))
                {
                    result.RazonesSociales.Add(vendorAddressRecipient.ValueString);
                }
                else if (fields.TryGetValue("VendorName", out var vendorName) && 
                         !string.IsNullOrWhiteSpace(vendorName?.ValueString))
                {
                    result.RazonesSociales.Add(vendorName.ValueString);
                }

                // Extraer Fecha de Emisión (InvoiceDate)
                if (fields.TryGetValue("InvoiceDate", out var invoiceDate))
                {
                    if (!string.IsNullOrWhiteSpace(invoiceDate?.ValueDate))
                    {
                        result.FechasEmision.Add(invoiceDate.ValueDate);
                    }
                    else if (!string.IsNullOrWhiteSpace(invoiceDate?.Content))
                    {
                        result.FechasEmision.Add(invoiceDate.Content);
                    }
                }

                // Extraer Monto Total (InvoiceTotal)
                if (fields.TryGetValue("InvoiceTotal", out var invoiceTotal) && 
                    invoiceTotal?.ValueCurrency?.Amount != null)
                {
                    var amount = invoiceTotal.ValueCurrency.Amount.Value.ToString("F2");
                    result.MontosTotales.Add(amount);
                }

                // Extraer Serie y Correlativo (InvoiceId)
                if (fields.TryGetValue("InvoiceId", out var invoiceId))
                {
                    string? invoiceIdValue = null;
                    
                    // Intentar obtener el valor de diferentes propiedades
                    if (!string.IsNullOrWhiteSpace(invoiceId?.ValueString))
                    {
                        invoiceIdValue = invoiceId.ValueString.Trim();
                    }
                    else if (!string.IsNullOrWhiteSpace(invoiceId?.Content))
                    {
                        invoiceIdValue = invoiceId.Content.Trim();
                    }
                    
                    if (!string.IsNullOrWhiteSpace(invoiceIdValue))
                    {
                        _logger.LogInformation("InvoiceId encontrado: {InvoiceId}", invoiceIdValue);
                        
                        // Formato peruano: SERIE-CORRELATIVO o SERIE CORRELATIVO
                        // Serie: hasta 4 caracteres alfanuméricos (ej: B206, F001, EB01, E001)
                        // Correlativo: hasta 8 dígitos
                        var match = System.Text.RegularExpressions.Regex.Match(
                            invoiceIdValue, 
                            @"^([A-Z]+[0-9]{0,3})[\s\-:\.]*(\d{1,8})$",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        
                        if (match.Success && match.Groups.Count >= 3)
                        {
                            var serie = match.Groups[1].Value.ToUpper();
                            var correlativo = match.Groups[2].Value;
                            
                            if (!string.IsNullOrWhiteSpace(serie))
                            {
                                result.Series.Add(serie);
                                _logger.LogInformation("Serie extraída: {Serie}", serie);
                            }
                            
                            if (!string.IsNullOrWhiteSpace(correlativo))
                            {
                                result.Correlativos.Add(correlativo);
                                _logger.LogInformation("Correlativo extraído: {Correlativo}", correlativo);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No se pudo parsear InvoiceId '{InvoiceId}' con el patrón esperado", invoiceIdValue);
                            
                            // Intentar encontrar serie y correlativo por separado en el texto
                            var serieMatch = System.Text.RegularExpressions.Regex.Match(invoiceIdValue, @"([A-Z]+[0-9]{0,3})");
                            var corrMatch = System.Text.RegularExpressions.Regex.Match(invoiceIdValue, @"(\d{1,8})");
                            
                            if (serieMatch.Success)
                            {
                                result.Series.Add(serieMatch.Groups[1].Value.ToUpper());
                            }
                            
                            if (corrMatch.Success)
                            {
                                result.Correlativos.Add(corrMatch.Groups[1].Value);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("InvoiceId está presente pero sin valor");
                    }
                }

                _logger.LogInformation("Conversión completada: RUCs={RucCount}, RazonesSociales={RsCount}, " +
                                     "Fechas={FechaCount}, Montos={MontoCount}, Series={SerieCount}, Correlativos={CorrCount}",
                    result.Rucs.Count, result.RazonesSociales.Count, result.FechasEmision.Count,
                    result.MontosTotales.Count, result.Series.Count, result.Correlativos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir respuesta de Azure a ComprobanteExtractorResult");
            }

            return result;
        }

        /// <summary>
        /// Detecta el tipo de contenido del archivo basado en los magic bytes
        /// </summary>
        private string DetectContentType(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length < 4)
            {
                return "application/octet-stream";
            }

            // PDF: %PDF (0x25 0x50 0x44 0x46)
            if (fileBytes.Length >= 4 && 
                fileBytes[0] == 0x25 && fileBytes[1] == 0x50 && 
                fileBytes[2] == 0x44 && fileBytes[3] == 0x46)
            {
                return "application/pdf";
            }

            // JPEG: FF D8 FF
            if (fileBytes.Length >= 3 && 
                fileBytes[0] == 0xFF && fileBytes[1] == 0xD8 && fileBytes[2] == 0xFF)
            {
                return "image/jpeg";
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (fileBytes.Length >= 8 && 
                fileBytes[0] == 0x89 && fileBytes[1] == 0x50 && 
                fileBytes[2] == 0x4E && fileBytes[3] == 0x47 &&
                fileBytes[4] == 0x0D && fileBytes[5] == 0x0A && 
                fileBytes[6] == 0x1A && fileBytes[7] == 0x0A)
            {
                return "image/png";
            }

            // TIFF: 49 49 2A 00 (little-endian) o 4D 4D 00 2A (big-endian)
            if (fileBytes.Length >= 4)
            {
                if ((fileBytes[0] == 0x49 && fileBytes[1] == 0x49 && 
                     fileBytes[2] == 0x2A && fileBytes[3] == 0x00) ||
                    (fileBytes[0] == 0x4D && fileBytes[1] == 0x4D && 
                     fileBytes[2] == 0x00 && fileBytes[3] == 0x2A))
                {
                    return "image/tiff";
                }
            }

            // BMP: 42 4D
            if (fileBytes.Length >= 2 && 
                fileBytes[0] == 0x42 && fileBytes[1] == 0x4D)
            {
                return "image/bmp";
            }

            // Por defecto, usar application/octet-stream
            _logger.LogWarning("No se pudo detectar el tipo de contenido, usando application/octet-stream");
            return "application/octet-stream";
        }
    }
}
