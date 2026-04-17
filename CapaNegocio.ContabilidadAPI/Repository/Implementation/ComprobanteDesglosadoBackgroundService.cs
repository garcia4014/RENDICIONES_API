using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public ComprobanteDesglosadoBackgroundService(
            ILogger<ComprobanteDesglosadoBackgroundService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
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
                var dbContext = scope.ServiceProvider.GetRequiredService<CapaDatos.ContabilidadAPI.SvrendicionesContext>();
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

                // Fecha límite: configurable desde appsettings (DesglosarXml:FechaLimiteDias), por defecto 2 días
                var fechaLimiteDias = _configuration.GetValue<int>("DesglosarXml:FechaLimiteDias", 2);
                var fechaLimite = DateTime.Now.AddDays(-fechaLimiteDias);

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
        /// Procesa el desglose de un único comprobante por su ID.
        /// Obtiene el XML de SUNAT y actualiza los detalles de impuestos.
        /// </summary>
        /// <returns>true si fue procesado exitosamente, false si falló o no era necesario.</returns>
        public async Task<DesglosePorIdResultado> ProcesarComprobanteDesglosadoPorIdAsync(int id)
        {
            _logger.LogInformation("Procesando desglose para comprobante ID={Id}", id);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CapaDatos.ContabilidadAPI.SvrendicionesContext>();
            var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

            var comprobante = await dbContext.ComprobantesPago
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo == true);

            if (comprobante == null)
                return new DesglosePorIdResultado { Exito = false, Mensaje = $"Comprobante ID={id} no encontrado o inactivo" };

            //if (comprobante.Desglosado == true)
            //    return new DesglosePorIdResultado { Exito = true, Mensaje = "El comprobante ya estaba desglosado", YaDesglosado = true };

            if (comprobante.Ruc == null || string.IsNullOrEmpty(comprobante.Serie) || string.IsNullOrEmpty(comprobante.Correlativo))
                return new DesglosePorIdResultado { Exito = false, Mensaje = "Faltan datos requeridos (RUC, Serie o Correlativo)" };

            try
            {
                var xmlResult = await ObtenerXmlDesdeSunatAsync(
                    dbContext,
                    httpClient,
                    comprobante.Ruc.Value.ToString(),
                    comprobante.Serie,
                    comprobante.Correlativo);

                if (xmlResult == null || !xmlResult.AfectacionIgvDetectada)
                {
                    _logger.LogWarning("No se pudo obtener XML o no se detectó afectación para comprobante ID={Id}", id);
                    return new DesglosePorIdResultado { Exito = false, Mensaje = "SUNAT no devolvió XML válido o no se detectó afectación IGV" };
                }

                ActualizarComprobanteConDatosXml(comprobante, xmlResult);
                await dbContext.SaveChangesAsync();

                // Validar en SUNAT después del procesamiento exitoso
                try
                {
                    var comprobantePagoService = scope.ServiceProvider.GetRequiredService<IComprobantePagoService>();
                    await comprobantePagoService.ValidarComprobanteEnSunatAsync(comprobante.Id);
                    _logger.LogInformation("Validación SUNAT ejecutada para comprobante ID={Id}", id);
                }
                catch (Exception validationEx)
                {
                    _logger.LogWarning(validationEx, "Error al validar comprobante ID={Id} en SUNAT, pero el desglose fue exitoso", id);
                }

                _logger.LogInformation("Comprobante ID={Id} desglosado exitosamente", id);
                return new DesglosePorIdResultado { Exito = true, Mensaje = "Comprobante desglosado exitosamente" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desglosas comprobante ID={Id}", id);
                // Descartar cambios parciales
                try { dbContext.Entry(comprobante).State = EntityState.Unchanged; } catch { }
                return new DesglosePorIdResultado { Exito = false, Mensaje = $"Error inesperado: {ex.Message}" };
            }
        }

        /// <summary>
        /// Obtiene el XML de SUNAT para el comprobante indicado, lo guarda en disco
        /// según la configuración de appsettings (DesglosarXml:PathXml) y lo devuelve como string.
        /// </summary>
        public async Task<ObtenerXmlResultado> ObtenerYGuardarXmlPorIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo XML para comprobante ID={Id}", id);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CapaDatos.ContabilidadAPI.SvrendicionesContext>();
            var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

            var comprobante = await dbContext.ComprobantesPago
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo == true);

            if (comprobante == null)
                return new ObtenerXmlResultado { Exito = false, Mensaje = $"Comprobante ID={id} no encontrado o inactivo" };

            if (comprobante.Ruc == null || string.IsNullOrEmpty(comprobante.Serie) || string.IsNullOrEmpty(comprobante.Correlativo))
                return new ObtenerXmlResultado { Exito = false, Mensaje = "Faltan datos requeridos (RUC, Serie o Correlativo)" };

            try
            {
                var ruc = comprobante.Ruc.Value.ToString();
                var serie = comprobante.Serie;
                var correlativo = comprobante.Correlativo;

                // Construir ruta destino igual que GuardarXmlEnDisco
                var path = _configuration.GetValue<string>("DesglosarXml:PathXml") ?? string.Empty;
                var fileName = $"{ruc}-{serie}-{correlativo}.xml";
                var fullPath = string.IsNullOrWhiteSpace(path) ? fileName : Path.Combine(path, fileName);

                // Si ya existe en disco, devolverlo directamente
                if (File.Exists(fullPath))
                {
                    _logger.LogInformation("XML ya existe en disco: {Path}", fullPath);
                    var xmlExistente = await File.ReadAllTextAsync(fullPath);
                    return new ObtenerXmlResultado { Exito = true, Mensaje = "XML obtenido desde caché en disco", XmlContent = xmlExistente, RutaArchivo = fullPath };
                }

                // Obtener de SUNAT y forzar guardado independientemente del flag GuardarXml
                var xmlResult = await ObtenerXmlDesdeSunatAsync(dbContext, httpClient, ruc, serie, correlativo);

                if (xmlResult == null)
                    return new ObtenerXmlResultado { Exito = false, Mensaje = "SUNAT no devolvio un XML válido" };

                // Guardar siempre (este endpoint lo pide explícitamente)
                if (!string.IsNullOrWhiteSpace(path))
                {
                    Directory.CreateDirectory(path);
                    await File.WriteAllTextAsync(fullPath, xmlResult.XmlRaw ?? string.Empty, Encoding.UTF8);
                    _logger.LogInformation("XML guardado en: {Path}", fullPath);
                }

                return new ObtenerXmlResultado
                {
                    Exito = true,
                    Mensaje = "XML obtenido desde SUNAT y guardado en disco",
                    XmlContent = xmlResult.XmlRaw ?? string.Empty,
                    RutaArchivo = fullPath
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener XML para comprobante ID={Id}", id);
                return new ObtenerXmlResultado { Exito = false, Mensaje = $"Error inesperado: {ex.Message}" };
            }
        }

        /// <summary>
        /// Actualiza el comprobante con los datos extraídos del XML
        /// </summary>
        internal void ActualizarComprobanteConDatosXml(ComprobantePago comprobante, ComprobanteExtractorResult xmlResult)
        {
            // Extraer montos de las listas
            // MontosIgvEspecial = BASE IMPONIBLE de ítems con IGV reducido (igual semántica que MontosGravados)
            // MontosBaseIgvEspecial = TaxAmount real calculado por SUNAT para esos ítems (el IGV exacto)
            var montoGravado        = SumarMontos(xmlResult.MontosGravados);
            var montoInafecto       = ExtraerPrimerMonto(xmlResult.MontosInafectos);
            var montoExonerado      = ExtraerPrimerMonto(xmlResult.MontosExonerados);
            var montoBaseIgvEspecial= SumarMontos(xmlResult.MontosIgvEspecial);     // base imponible IGV especial
            var montoTaxIgvEspecial = SumarMontos(xmlResult.MontosBaseIgvEspecial); // TaxAmount suma de líneas (fallback)
            var igvDocumento        = decimal.TryParse(xmlResult.MontoIgvDocumento,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var _igvDoc) ? _igvDoc : 0m; // TaxAmount cabecera XML (valor exacto SUNAT)
            var subtotalDocumento    = decimal.TryParse(xmlResult.MontoGravadoDocumento,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var _subtDoc) ? _subtDoc : 0m; // TaxableAmount cabecera XML (base imponible oficial SUNAT)
            var montoImpuestoConsumo= ExtraerPrimerMonto(xmlResult.MontosImpuestoConsumo);
            var montoTotal          = ExtraerPrimerMonto(xmlResult.MontosTotales);

            // Actualizar campos de montos
            comprobante.MontoGravado = montoGravado;
            comprobante.MontoInafecto = montoInafecto;
            comprobante.MontoExonerado = montoExonerado;
            comprobante.MontoIgvEspecial = montoBaseIgvEspecial; // base imponible 
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
            comprobante.IgvEspecial = montoBaseIgvEspecial > 0;
            comprobante.OtrosCargos = montoImpuestoConsumo > 0;

            // IGV directo del XML: usar TaxAmount de cabecera (exacto, sin redondeos de línea);
            // si no viene en cabecera (XML antiguo), caer al acumulado de líneas como fallback.
            var igvFinal = igvDocumento > 0 ? igvDocumento : montoTaxIgvEspecial;

            // Subtotal e IGV — sin ningún cálculo de tasas, todo viene del XML
            if (montoGravado > 0)
            {
                comprobante.Subtotal = subtotalDocumento > 0 ? subtotalDocumento : montoGravado;
                comprobante.Igv = igvFinal;
            }
            else if (montoBaseIgvEspecial > 0)
            {
                comprobante.Subtotal = subtotalDocumento > 0 ? subtotalDocumento : montoBaseIgvEspecial;
                comprobante.Igv = igvFinal;
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
            
            _logger.LogInformation("Datos actualizados - Gravado: {G}, Inafecto: {I}, Exonerado: {E}, BaseIgvEspecial: {IE}, TaxIgvEspecial: {TX}, ImpuestoConsumo: {IC}, IGV: {IGV}, Subtotal: {S}, MontoTotal: {MT}, FechaEmision: {FE}",
                comprobante.MontoGravado,
                comprobante.MontoInafecto,
                comprobante.MontoExonerado,
                comprobante.MontoIgvEspecial,
                montoTaxIgvEspecial,
                comprobante.MontoOtrosCargos,
                comprobante.Igv,
                comprobante.Subtotal,
                comprobante.Monto,
                comprobante.FechaEmision?.ToString("dd/MM/yyyy") ?? "N/A");
        }

        /// <summary>
        /// Suma todos los montos válidos de una lista de strings
        /// </summary>
        private decimal SumarMontos(List<string> montos)
        {
            if (montos == null || !montos.Any())
                return 0m;
            return montos.Sum(m => decimal.TryParse(m,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0m);
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
            CapaDatos.ContabilidadAPI.SvrendicionesContext dbContext,
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

                String tipoComprobante = dbContext.ComprobantesPago
                    .Where(c => c.Ruc == long.Parse(ruc) && c.Serie == serie && c.Correlativo == correlativo)
                    .Select(c => c.TipoComprobante)
                    .FirstOrDefault() ?? "01"; // Default a "01" si no se encuentra

                tipoComprobante = tipoComprobante.PadLeft(2, '0'); // Asegurar formato de 2 dígitos
                 _logger.LogInformation("Tipo de comprobante determinado: {TipoComprobante}", tipoComprobante);

                // Construir URL
                var url = $"https://api-cpe.sunat.gob.pe/v1/contribuyente/consultacpe/comprobantes/{ruc}-{tipoComprobante}-{serie}-{correlativo}-2/02";
                 _logger.LogInformation("Url de consulta :{Url} ",url);

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

                // Guardar XML en disco si está habilitado en appsettings
                GuardarXmlEnDisco(xmlContent, ruc, serie, correlativo);

                // Procesar XML
                var result = ComprobanteExtractor.ExtractFromXml(xmlContent);
                result.XmlRaw = xmlContent; // guardar el XML crudo en el resultado
                
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
        /// Guarda el XML en disco si DesglosarXml:GuardarXml = true en appsettings.
        /// El archivo se nombra {ruc}-{serie}-{correlativo}.xml dentro de DesglosarXml:PathXml.
        /// </summary>
        private void GuardarXmlEnDisco(string xmlContent, string ruc, string serie, string correlativo)
        {
            try
            {
                var guardar = _configuration.GetValue<bool>("DesglosarXml:GuardarXml");
                if (!guardar)
                    return;

                var path = _configuration.GetValue<string>("DesglosarXml:PathXml");
                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.LogWarning("[GuardarXml] DesglosarXml:PathXml no está configurado, no se puede guardar el XML.");
                    return;
                }

                Directory.CreateDirectory(path);

                var fileName = $"{ruc}-{serie}-{correlativo}.xml";
                var fullPath = Path.Combine(path, fileName);
                File.WriteAllText(fullPath, xmlContent, Encoding.UTF8);

                _logger.LogInformation("[GuardarXml] XML guardado en: {Path}", fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[GuardarXml] Error al guardar XML en disco (no afecta el proceso).");
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

                // Excluir archivos "R-*" (CDR de SUNAT, solo tomar el comprobante)
                var xmlEntry = archive.Entries.FirstOrDefault(e =>
                    e.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
                    !e.Name.StartsWith("R-", StringComparison.OrdinalIgnoreCase));

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

    /// <summary>
    /// Resultado del procesamiento de desglose por ID
    /// </summary>
    public class DesglosePorIdResultado
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        /// <summary>true si el comprobante ya estaba desglosado previamente (no requirió acción)</summary>
        public bool YaDesglosado { get; set; }
    }

    /// <summary>
    /// Resultado de la obtención y guardado del XML SUNAT por ID de comprobante
    /// </summary>
    public class ObtenerXmlResultado
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string XmlContent { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
    }
}
