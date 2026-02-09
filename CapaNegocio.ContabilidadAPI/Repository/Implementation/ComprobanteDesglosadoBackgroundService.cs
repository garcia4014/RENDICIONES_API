using CapaDatos.ContabilidadAPI;
using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Servicio en background para procesar comprobantes no desglosados
    /// Obtiene XML de SUNAT y actualiza los detalles de impuestos
    /// </summary>
    public class ComprobanteDesglosadoBackgroundService
    {
        private readonly ILogger<ComprobanteDesglosadoBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ComprobanteDesglosadoBackgroundService(
            ILogger<ComprobanteDesglosadoBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Procesa comprobantes con DESGLOSADO = 0 (false)
        /// </summary>
        public async Task ProcesarComprobantesNoDesglosados()
        {
            _logger.LogInformation("===== INICIO: Procesamiento de comprobantes no desglosados =====");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SvrendicionesContext>();
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

                // Fecha límite: últimos 2 días
                var fechaLimite = DateTime.Now.AddDays(-2);

                // Obtener comprobantes con DESGLOSADO = 0 o NULL de los últimos 2 días
                var comprobantesNoDesglosados = await dbContext.ComprobantesPago
                    .Where(c => c.Activo == true && 
                           (c.Desglosado == false || c.Desglosado == null) &&
                           c.Ruc != null &&
                           !string.IsNullOrEmpty(c.Serie) &&
                           !string.IsNullOrEmpty(c.Correlativo) &&
                           c.FechaCarga >= fechaLimite)
                    .Take(50) // Procesar máximo 50 por vez para no sobrecargar
                    .ToListAsync();

                if (!comprobantesNoDesglosados.Any())
                {
                    _logger.LogInformation("No hay comprobantes pendientes de desglosa (DESGLOSADO=0)");
                    return;
                }

                _logger.LogInformation("Encontrados {Cantidad} comprobantes para procesar", comprobantesNoDesglosados.Count);

                int exitosos = 0;
                int fallidos = 0;

                foreach (var comprobante in comprobantesNoDesglosados)
                {
                    try
                    {
                        _logger.LogInformation("Procesando comprobante ID={Id}, RUC={Ruc}, Serie={Serie}, Correlativo={Correlativo}",
                            comprobante.Id, comprobante.Ruc, comprobante.Serie, comprobante.Correlativo);

                        // Obtener XML desde SUNAT
                        var xmlResult = await ObtenerXmlDesdeSunatAsync(
                            dbContext, 
                            httpClient,
                            comprobante.Ruc!.Value.ToString(),
                            comprobante.Serie!,
                            comprobante.Correlativo!);

                        if (xmlResult != null && xmlResult.AfectacionIgvDetectada)
                        {
                            // Actualizar comprobante con datos del XML
                            ActualizarComprobanteConDatosXml(comprobante, xmlResult);
                            
                            _logger.LogInformation("Comprobante ID={Id} actualizado exitosamente - Gravado:{G}, Inafecto:{I}, Exonerado:{E}",
                                comprobante.Id,
                                comprobante.MontoGravado,
                                comprobante.MontoInafecto,
                                comprobante.MontoExonerado);
                            
                            // Guardar cambios solo si fue exitoso
                            await dbContext.SaveChangesAsync();
                            exitosos++;

                            // Validar comprobante en SUNAT después de procesamiento exitoso
                            try
                            {
                                var comprobantePagoService = scope.ServiceProvider.GetRequiredService<IComprobantePagoService>();
                                await comprobantePagoService.ValidarComprobanteEnSunatAsync(comprobante.Id);
                                _logger.LogInformation("Validación SUNAT ejecutada para comprobante ID={Id}", comprobante.Id);
                            }
                            catch (Exception validationEx)
                            {
                                _logger.LogWarning(validationEx, "Error al validar comprobante ID={Id} en SUNAT, pero el procesamiento fue exitoso", comprobante.Id);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No se pudo obtener XML o no se detectó afectación para comprobante ID={Id}. Se reintentará en la próxima ejecución.", comprobante.Id);
                            fallidos++;
                            // NO marcar como desglosado para que se reintente en la próxima ejecución
                            // Descartar cualquier cambio trackeado en este comprobante
                            dbContext.Entry(comprobante).State = EntityState.Unchanged;
                        }
                        
                        // Pequeño delay para no saturar la API de SUNAT
                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar comprobante ID={Id}. Se reintentará en la próxima ejecución.", comprobante.Id);
                        fallidos++;
                        // NO marcar como desglosado para que se reintente en la próxima ejecución
                        // Descartar cualquier cambio trackeado en este comprobante
                        try
                        {
                            dbContext.Entry(comprobante).State = EntityState.Unchanged;
                        }
                        catch
                        {
                            // Ignorar si ya fue detached
                        }
                    }
                }

                _logger.LogInformation("===== FIN: Procesamiento completado - Exitosos: {Exitosos}, Fallidos: {Fallidos} =====",
                    exitosos, fallidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general en ProcesarComprobantesNoDesglosados");
            }
        }

        /// <summary>
        /// Actualiza el comprobante con los datos extraídos del XML
        /// </summary>
        private void ActualizarComprobanteConDatosXml(ComprobantePago comprobante, ComprobanteExtractorResult xmlResult)
        {
            // Extraer montos de las listas
            var montoGravado = ExtraerPrimerMonto(xmlResult.MontosGravados);
            var montoInafecto = ExtraerPrimerMonto(xmlResult.MontosInafectos);
            var montoExonerado = ExtraerPrimerMonto(xmlResult.MontosExonerados);
            var montoIgvEspecial = ExtraerPrimerMonto(xmlResult.MontosIgvEspecial);
            var montoImpuestoConsumo = ExtraerPrimerMonto(xmlResult.MontosImpuestoConsumo);
            var montoTotal = ExtraerPrimerMonto(xmlResult.MontosTotales);

            // Actualizar campos de montos
            comprobante.MontoGravado = montoGravado;
            comprobante.MontoInafecto = montoInafecto;
            comprobante.MontoExonerado = montoExonerado;
            comprobante.MontoIgvEspecial = montoIgvEspecial;
            comprobante.MontoOtrosCargos = montoImpuestoConsumo;
            
            // Actualizar monto total desde el XML si está disponible y el comprobante no tiene monto
            if (montoTotal > 0 && (comprobante.Monto == null || comprobante.Monto == 0))
            {
                comprobante.Monto = montoTotal;
                _logger.LogInformation("Monto total actualizado desde XML: {MontoTotal}", montoTotal);
            }

            // Actualizar flags booleanos
            comprobante.Gravado = montoGravado > 0;
            comprobante.Inafecto = montoInafecto > 0;
            comprobante.Exonerado = montoExonerado > 0;
            comprobante.IgvEspecial = montoIgvEspecial > 0;
            comprobante.OtrosCargos = montoImpuestoConsumo > 0;

            // Calcular IGV y Subtotal si hay monto gravado
            if (montoGravado > 0)
            {
                var porcentajeIgv = comprobante.IgvPorcentaje ?? 18;
                comprobante.Igv = Math.Round(montoGravado * porcentajeIgv / 100, 2);
                comprobante.Subtotal = montoGravado;
            }
            else if (montoInafecto > 0)
            {
                comprobante.Subtotal = montoInafecto;
                comprobante.Igv = 0;
            }
            else if (montoExonerado > 0)
            {
                comprobante.Subtotal = montoExonerado;
                comprobante.Igv = 0;
            }

            // Actualizar Fecha de Emisión desde el XML si está disponible
            if (xmlResult.FechasEmision != null && xmlResult.FechasEmision.Any())
            {
                var fechaString = xmlResult.FechasEmision.First();
                _logger.LogInformation("Intentando parsear fecha desde XML. Valor crudo: '{FechaString}'", fechaString);
                
                if (DateTime.TryParse(fechaString, out var fechaEmision))
                {
                    comprobante.FechaEmision = fechaEmision;
                    _logger.LogInformation("Fecha de emisión actualizada desde XML: {FechaEmision}", fechaEmision.ToString("dd/MM/yyyy"));
                }
                else
                {
                    _logger.LogWarning("No se pudo parsear la fecha desde XML. Valor: '{FechaString}'", fechaString);
                }
            }
            else
            {
                _logger.LogWarning("XML no contiene fechas de emisión. FechasEmision Count: {Count}", 
                    xmlResult.FechasEmision?.Count ?? 0);
            }

            // Marcar como desglosado
            comprobante.Desglosado = true;
            
            _logger.LogInformation("Datos actualizados - Gravado: {G}, Inafecto: {I}, Exonerado: {E}, IgvEspecial: {IE}, ImpuestoConsumo: {IC}, IGV: {IGV}, Subtotal: {S}, MontoTotal: {MT}, FechaEmision: {FE}",
                comprobante.MontoGravado,
                comprobante.MontoInafecto,
                comprobante.MontoExonerado,
                comprobante.MontoIgvEspecial,
                comprobante.MontoOtrosCargos,
                comprobante.Igv,
                comprobante.Subtotal,
                comprobante.Monto,
                comprobante.FechaEmision?.ToString("dd/MM/yyyy") ?? "N/A");
        }

        /// <summary>
        /// Extrae el primer monto válido de una lista de strings
        /// </summary>
        private decimal ExtraerPrimerMonto(List<string> montos)
        {
            if (montos == null || !montos.Any())
                return 0m;

            foreach (var monto in montos)
            {
                if (decimal.TryParse(monto, out var valor))
                {
                    return valor;
                }
            }

            return 0m;
        }

        /// <summary>
        /// Obtiene el XML del comprobante desde la API de SUNAT
        /// (Adaptado del método en AzureDocumentIntelligenceService)
        /// </summary>
        private async Task<ComprobanteExtractorResult?> ObtenerXmlDesdeSunatAsync(
            SvrendicionesContext dbContext,
            HttpClient httpClient,
            string ruc, 
            string serie, 
            string correlativo)
        {
            try
            {
                _logger.LogInformation("Obteniendo XML desde SUNAT - RUC={Ruc}, Serie={Serie}, Correlativo={Correlativo}", 
                    ruc, serie, correlativo);

                // Obtener token de autorización desde BD
                var parametro = await dbContext.Parametros
                    .FirstOrDefaultAsync(p => p.Id == 1);

                if (parametro == null || string.IsNullOrWhiteSpace(parametro.Valor))
                {
                    _logger.LogWarning("No se encontró el token SUNAT en la tabla PARAMETROS (Id=1)");
                    return null;
                }

                var token = parametro.Valor;

                // Determinar tipo de comprobante por la serie y asegurar 2 dígitos (01, 03, 07, etc.)
                string tipoComprobante = DeterminarTipoComprobante(serie).PadLeft(2, '0');

                // Construir URL
                var url = $"https://api-cpe.sunat.gob.pe/v1/contribuyente/consultacpe/comprobantes/{ruc}-{tipoComprobante}-{serie}-{correlativo}-2/02";

                // Configurar request con reintentos
                const int maxRetries = 3; // Menos reintentos que en el proceso principal
                HttpResponseMessage? response = null;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Add("Accept", "application/json, text/plain, */*");
                        request.Headers.Add("Accept-Language", "es,es-ES;q=0.9,en;q=0.8");
                        request.Headers.Add("Origin", "https://e-factura.sunat.gob.pe");
                        request.Headers.Add("Referer", "https://e-factura.sunat.gob.pe/");
                        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                        request.Headers.Add("Authorization", $"Bearer {token}");

                        response = await httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            break;
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            _logger.LogWarning("Token SUNAT no autorizado para RUC={Ruc}, Serie={Serie}", ruc, serie);
                            return null;
                        }
                        else if (attempt < maxRetries)
                        {
                            await Task.Delay(1000 * attempt); // 1s, 2s, 3s
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error en intento {Attempt} de {MaxRetries}", attempt, maxRetries);
                        if (attempt == maxRetries)
                            return null;
                        await Task.Delay(1000 * attempt);
                    }
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var sunatResponse = JsonSerializer.Deserialize<SunatXmlResponse>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (sunatResponse == null || string.IsNullOrWhiteSpace(sunatResponse.ValArchivo))
                {
                    _logger.LogWarning("Respuesta de SUNAT vacía");
                    return null;
                }

                // Decodificar Base64 y extraer XML del ZIP
                var xmlContent = ExtraerXmlDeZip(sunatResponse.ValArchivo);

                if (string.IsNullOrWhiteSpace(xmlContent))
                {
                    _logger.LogWarning("No se pudo extraer XML del archivo ZIP");
                    return null;
                }

                // Procesar XML
                var result = ComprobanteExtractor.ExtractFromXml(xmlContent);
                
                _logger.LogInformation("XML procesado - AfectacionDetectada: {AD}, Gravados: {G}, Inafectos: {I}, Exonerados: {E}, FechasEmision: {FE}",
                    result.AfectacionIgvDetectada,
                    result.MontosGravados.Count,
                    result.MontosInafectos.Count,
                    result.MontosExonerados.Count,
                    result.FechasEmision.Count);
                
                if (result.FechasEmision.Any())
                {
                    _logger.LogInformation("Fecha extraída del XML: {Fecha}", string.Join(", ", result.FechasEmision));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener XML desde SUNAT");
                return null;
            }
        }

        /// <summary>
        /// Determina el tipo de comprobante SUNAT según la serie
        /// </summary>
        private string DeterminarTipoComprobante(string serie)
        {
            if (string.IsNullOrWhiteSpace(serie))
                return "01";

            var serieUpper = serie.ToUpper();

            if (serieUpper.StartsWith("F") || serieUpper.StartsWith("E"))
                return "01"; // Factura

            if (serieUpper.StartsWith("B"))
                return "03"; // Boleta

            return "01"; // Por defecto, Factura
        }

        /// <summary>
        /// Extrae el contenido XML de un archivo ZIP codificado en Base64
        /// </summary>
        private string? ExtraerXmlDeZip(string base64Zip)
        {
            try
            {
                var zipBytes = Convert.FromBase64String(base64Zip);

                using var zipStream = new MemoryStream(zipBytes);
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

                var xmlEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

                if (xmlEntry == null)
                {
                    _logger.LogWarning("No se encontró archivo XML en el ZIP");
                    return null;
                }

                using var entryStream = xmlEntry.Open();
                using var reader = new StreamReader(entryStream, Encoding.UTF8);
                return reader.ReadToEnd();
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
    }
}
