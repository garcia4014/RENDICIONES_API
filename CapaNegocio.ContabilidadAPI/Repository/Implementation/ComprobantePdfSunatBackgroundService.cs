using CapaDatos.ContabilidadAPI;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.Json;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Servicio en background para descargar PDFs desde SUNAT
    /// Se ejecuta cada 5 minutos para comprobantes del último día sin PDF
    /// </summary>
    public class ComprobantePdfSunatBackgroundService
    {
        private readonly ILogger<ComprobantePdfSunatBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const int MAX_REINTENTOS_POR_DIA = 10;

        public ComprobantePdfSunatBackgroundService(
            ILogger<ComprobantePdfSunatBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Procesa comprobantes sin PDF de SUNAT (PdfSunat = false)
        /// Solo del último día y máximo 10 reintentos
        /// </summary>
        public async Task ProcesarComprobantesParaPdfSunat()
        {
            _logger.LogInformation("===== INICIO: Descarga de PDFs desde SUNAT =====");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SvrendicionesContext>();
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

                // Fecha límite: último día (24 horas)
                var fechaLimite = DateTime.Now.AddDays(-1);

                // Obtener comprobantes que cumplan los criterios:
                // 1. PDF_SUNAT = false (no se ha descargado el PDF)
                // 2. FechaCarga del último día
                // 3. Tienen RUC, Serie y Correlativo
                // 4. Reintentos < 10
                var comprobantesSinPdf = await dbContext.ComprobantesPago
                    .Where(c => c.Activo == true &&
                           (c.PdfSunat == false || c.PdfSunat == null) &&
                           c.Ruc != null &&
                           !string.IsNullOrEmpty(c.Serie) &&
                           !string.IsNullOrEmpty(c.Correlativo) &&
                           c.FechaCarga >= fechaLimite &&
                           (c.ReintentosPdfSunat == null || c.ReintentosPdfSunat < MAX_REINTENTOS_POR_DIA))
                    .Take(50) // Procesar máximo 50 por vez
                    .ToListAsync();

                if (!comprobantesSinPdf.Any())
                {
                    _logger.LogInformation("No hay comprobantes pendientes de descarga de PDF desde SUNAT");
                    return;
                }

                _logger.LogInformation("Encontrados {Cantidad} comprobantes para descargar PDF", comprobantesSinPdf.Count);

                int exitosos = 0;
                int fallidos = 0;

                foreach (var comprobante in comprobantesSinPdf)
                {
                    try
                    {
                        _logger.LogInformation("Descargando PDF para comprobante ID={Id}, RUC={Ruc}, Serie={Serie}, Correlativo={Correlativo}, Reintento={Reintento}",
                            comprobante.Id, comprobante.Ruc, comprobante.Serie, comprobante.Correlativo, comprobante.ReintentosPdfSunat ?? 0);

                        // Incrementar contador de reintentos
                        comprobante.ReintentosPdfSunat = (comprobante.ReintentosPdfSunat ?? 0) + 1;

                        // Descargar PDF desde SUNAT
                        var pdfDescargado = await DescargarPdfDesdeSunatAsync(
                            dbContext,
                            httpClient,
                            comprobante);

                        if (pdfDescargado)
                        {
                            _logger.LogInformation("PDF descargado exitosamente para comprobante ID={Id}", comprobante.Id);
                            exitosos++;
                        }
                        else
                        {
                            _logger.LogWarning("No se pudo descargar PDF para comprobante ID={Id}. Reintento {Reintento}/{Max}",
                                comprobante.Id, comprobante.ReintentosPdfSunat, MAX_REINTENTOS_POR_DIA);
                            fallidos++;
                        }

                        // Guardar cambios después de cada comprobante
                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar comprobante ID={Id}", comprobante.Id);
                        fallidos++;

                        // Guardar el incremento de reintentos incluso si falla
                        try
                        {
                            await dbContext.SaveChangesAsync();
                        }
                        catch (Exception saveEx)
                        {
                            _logger.LogError(saveEx, "Error al guardar reintentos para comprobante ID={Id}", comprobante.Id);
                        }
                    }
                }

                _logger.LogInformation("===== FIN: Descarga PDFs - Exitosos: {Exitosos}, Fallidos: {Fallidos} =====", exitosos, fallidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general en el procesamiento de PDFs de SUNAT");
            }
        }

        /// <summary>
        /// Descarga PDFs desde SUNAT bajo demanda para un arreglo de IDs.
        /// Valida primero cuáles ya existen en disco y omite los que ya están.
        /// Se ejecuta de forma síncrona durante la petición HTTP.
        /// </summary>
        public async Task<DescargaPdfMasivaResultado> DescargarPdfsMasivoAsync(List<int> ids)
        {
            var resultado = new DescargaPdfMasivaResultado();
            _logger.LogInformation("Inicio descarga masiva bajo demanda para {Cantidad} IDs", ids.Count);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SvrendicionesContext>();
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

                // Obtener los comprobantes solicitados de la BD
                var comprobantes = await dbContext.ComprobantesPago
                    .Where(c => ids.Contains(c.Id) && c.Activo == true)
                    .ToListAsync();

                // IDs no encontrados en BD
                var idsEncontrados = comprobantes.Select(c => c.Id).ToHashSet();
                resultado.NoEncontrados.AddRange(ids.Where(id => !idsEncontrados.Contains(id)));

                var pdfDirectory = Path.Combine(Directory.GetCurrentDirectory(), "PDF");
                if (!Directory.Exists(pdfDirectory))
                    Directory.CreateDirectory(pdfDirectory);

                foreach (var comprobante in comprobantes)
                {
                    try
                    {
                        // 1. Verificar si ya existe por la Ruta guardada en BD
                        if (!string.IsNullOrEmpty(comprobante.Ruta))
                        {
                            var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(),
                                comprobante.Ruta.Replace("/", Path.DirectorySeparatorChar.ToString()));
                            if (System.IO.File.Exists(rutaCompleta))
                            {
                                _logger.LogInformation("PDF ya existe (por Ruta BD) para comprobante ID={Id}, omitiendo", comprobante.Id);
                                resultado.Omitidos.Add(comprobante.Id);
                                continue;
                            }
                        }

                        // 2. Verificar si existe por nombre esperado (la BD puede no tener Ruta actualizada)
                        if (comprobante.Ruc != null && !string.IsNullOrEmpty(comprobante.Serie) && !string.IsNullOrEmpty(comprobante.Correlativo))
                        {
                            var tipoComp = DeterminarTipoComprobantePorSerie(comprobante.Serie, comprobante.TipoComprobante).PadLeft(2, '0');
                            var corrPadded = comprobante.Correlativo.PadLeft(7, '0');
                            var expectedFileName = $"{comprobante.Ruc}_{tipoComp}_{comprobante.Serie}_{corrPadded}.pdf";
                            var expectedPath = Path.Combine(pdfDirectory, expectedFileName);
                            if (System.IO.File.Exists(expectedPath))
                            {
                                _logger.LogInformation("PDF encontrado por nombre esperado para comprobante ID={Id}, actualizando BD si es necesario", comprobante.Id);
                                if (string.IsNullOrEmpty(comprobante.Ruta) || comprobante.PdfSunat != true)
                                {
                                    comprobante.Ruta = $"PDF/{expectedFileName}";
                                    comprobante.PdfSunat = true;
                                    comprobante.Extension = ".pdf";
                                    await dbContext.SaveChangesAsync();
                                }
                                resultado.Omitidos.Add(comprobante.Id);
                                continue;
                            }
                        }

                        // 3. Validar campos requeridos para descargar de SUNAT
                        if (comprobante.Ruc == null || string.IsNullOrEmpty(comprobante.Serie) || string.IsNullOrEmpty(comprobante.Correlativo))
                        {
                            _logger.LogWarning("Comprobante ID={Id} no tiene RUC, Serie o Correlativo requeridos", comprobante.Id);
                            resultado.Fallidos.Add(new DescargaPdfItemFallido
                            {
                                Id = comprobante.Id,
                                Razon = "Faltan datos requeridos (RUC, Serie o Correlativo)"
                            });
                            continue;
                        }

                        // 4. Descargar desde SUNAT
                        comprobante.ReintentosPdfSunat = (comprobante.ReintentosPdfSunat ?? 0) + 1;
                        var descargado = await DescargarPdfDesdeSunatAsync(dbContext, httpClient, comprobante);
                        await dbContext.SaveChangesAsync();

                        if (descargado)
                        {
                            _logger.LogInformation("PDF descargado exitosamente para comprobante ID={Id}", comprobante.Id);
                            resultado.Descargados.Add(comprobante.Id);
                        }
                        else
                        {
                            _logger.LogWarning("No se pudo descargar PDF de SUNAT para comprobante ID={Id}", comprobante.Id);
                            resultado.Fallidos.Add(new DescargaPdfItemFallido
                            {
                                Id = comprobante.Id,
                                Razon = "SUNAT no devolvió el PDF (token inválido, comprobante inexistente o datos incorrectos)"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar comprobante ID={Id} en descarga masiva", comprobante.Id);
                        resultado.Fallidos.Add(new DescargaPdfItemFallido
                        {
                            Id = comprobante.Id,
                            Razon = $"Error inesperado: {ex.Message}"
                        });
                    }
                }

                _logger.LogInformation(
                    "Fin descarga masiva bajo demanda - Descargados={D}, Omitidos={O}, Fallidos={F}, NoEncontrados={N}",
                    resultado.Descargados.Count, resultado.Omitidos.Count,
                    resultado.Fallidos.Count, resultado.NoEncontrados.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general en descarga masiva de PDFs");
                throw;
            }

            return resultado;
        }

        /// <summary>
        /// Descarga el PDF desde SUNAT y lo guarda en disco
        /// </summary>
        private async Task<bool> DescargarPdfDesdeSunatAsync(
            SvrendicionesContext dbContext,
            HttpClient httpClient,
            ComprobantePago comprobante)
        {
            try
            {
                var ruc = comprobante.Ruc!.Value.ToString();
                var serie = comprobante.Serie!;
                var correlativo = comprobante.Correlativo!.PadLeft(7, '0'); // Asegurar 7 dígitos con ceros a la izquierda

                // Determinar tipo de comprobante y asegurar 2 dígitos (01, 03, 07, etc.)
                var tipoComprobante = DeterminarTipoComprobantePorSerie(serie, comprobante.TipoComprobante).PadLeft(2, '0');

                _logger.LogInformation("Obteniendo PDF desde SUNAT - RUC={Ruc}, Serie={Serie}, Correlativo={Correlativo}, Tipo={Tipo}",
                    ruc, serie, correlativo, tipoComprobante);

                // Obtener token de SUNAT desde la base de datos
                var parametro = await dbContext.Parametros.FirstOrDefaultAsync(p => p.Id == 1);
                if (parametro == null || string.IsNullOrEmpty(parametro.Valor))
                {
                    _logger.LogError("No se encontró el token de SUNAT en la base de datos");
                    return false;
                }

                var token = parametro.Valor;

                // URL para obtener el PDF (endpoint /01 en lugar de /02 que es para XML)
                var url = $"https://api-cpe.sunat.gob.pe/v1/contribuyente/consultacpe/comprobantes/{ruc}-{tipoComprobante}-{serie}-{correlativo}-2/01";

                _logger.LogInformation("Llamando a SUNAT para PDF: {Url}", url);

                // Configurar headers para simular navegador (headers exactos que funcionan en navegador)
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "es,es-419;q=0.9,en;q=0.8,ja;q=0.7");
                httpClient.DefaultRequestHeaders.Add("Origin", "https://e-factura.sunat.gob.pe");
                httpClient.DefaultRequestHeaders.Add("Referer", "https://e-factura.sunat.gob.pe/");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-site");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"144\", \"Google Chrome\";v=\"144\"");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                // Realizar petición con reintentos
                HttpResponseMessage? response = null;
                int intentos = 0;
                int maxIntentosApi = 3;

                while (intentos < maxIntentosApi)
                {
                    intentos++;
                    try
                    {
                        response = await httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            break;
                        }

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            _logger.LogWarning("Token SUNAT no autorizado para RUC={Ruc}, Serie={Serie}", ruc, serie);
                            return false;
                        }

                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            _logger.LogWarning("PDF no encontrado en SUNAT para RUC={Ruc}, Serie={Serie}, Correlativo={Correlativo}",
                                ruc, serie, correlativo);
                            return false;
                        }

                        _logger.LogWarning("Intento {Intento}/{Max} - Status: {Status}", intentos, maxIntentosApi, response.StatusCode);

                        if (intentos < maxIntentosApi)
                        {
                            await Task.Delay(1000 * intentos); // Espera incremental
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error en intento {Intento}/{Max} al llamar a SUNAT", intentos, maxIntentosApi);
                        if (intentos < maxIntentosApi)
                        {
                            await Task.Delay(1000 * intentos);
                        }
                    }
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    _logger.LogError("No se pudo obtener respuesta exitosa de SUNAT después de {Intentos} intentos", intentos);
                    return false;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta de SUNAT recibida para PDF: {Response}", jsonResponse.Length > 200 ? jsonResponse.Substring(0, 200) + "..." : jsonResponse);

                // Deserializar respuesta - El PDF viene como ZIP en base64
                var sunatResponse = JsonSerializer.Deserialize<SunatPdfResponse>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (string.IsNullOrEmpty(sunatResponse?.ValArchivo))
                {
                    _logger.LogWarning("La respuesta de SUNAT no contiene el archivo ZIP codificado. Response: {Json}", jsonResponse);
                    return false;
                }

                _logger.LogInformation("Archivo recibido: {NomArchivo}", sunatResponse.NomArchivo);

                // Decodificar el ZIP desde base64
                var zipBytes = Convert.FromBase64String(sunatResponse.ValArchivo);
                _logger.LogInformation("ZIP decodificado: {Size} bytes", zipBytes.Length);

                // Extraer el PDF del ZIP
                byte[] pdfBytes;
                using (var zipStream = new MemoryStream(zipBytes))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    var pdfEntry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
                    if (pdfEntry == null)
                    {
                        _logger.LogWarning("No se encontró archivo PDF dentro del ZIP. Archivos: {Files}",
                            string.Join(", ", archive.Entries.Select(e => e.Name)));
                        return false;
                    }

                    using (var pdfStream = pdfEntry.Open())
                    using (var memoryStream = new MemoryStream())
                    {
                        await pdfStream.CopyToAsync(memoryStream);
                        pdfBytes = memoryStream.ToArray();
                    }

                    _logger.LogInformation("PDF extraído del ZIP: {PdfName}, {Size} bytes", pdfEntry.Name, pdfBytes.Length);
                }

                // Crear carpeta PDF si no existe
                var pdfDirectory = Path.Combine(Directory.GetCurrentDirectory(), "PDF");
                if (!Directory.Exists(pdfDirectory))
                {
                    Directory.CreateDirectory(pdfDirectory);
                    _logger.LogInformation("Carpeta PDF creada en: {Path}", pdfDirectory);
                }

                // Nombre del archivo: {ruc}_{tipoComprobante}_{serie}_{correlativo}.pdf
                var fileName = $"{ruc}_{tipoComprobante}_{serie}_{correlativo}.pdf";
                var filePath = Path.Combine(pdfDirectory, fileName);

                // Guardar archivo
                await File.WriteAllBytesAsync(filePath, pdfBytes);
                _logger.LogInformation("PDF guardado en: {FilePath}, Tamaño: {Size} bytes", filePath, pdfBytes.Length);

                // Actualizar comprobante
                comprobante.Ruta = $"PDF/{fileName}"; // Ruta relativa
                comprobante.PdfSunat = true;
                comprobante.Extension = ".pdf";

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar PDF desde SUNAT para comprobante ID={Id}", comprobante.Id);
                return false;
            }
        }

        /// <summary>
        /// Determina el tipo de comprobante basándose en la serie
        /// </summary>
        private string DeterminarTipoComprobantePorSerie(string serie, string? tipoComprobanteExistente)
        {
            // Si ya tiene tipo de comprobante definido, usarlo
            if (!string.IsNullOrEmpty(tipoComprobanteExistente))
            {
                return tipoComprobanteExistente;
            }

            // Determinar por serie
            if (serie.StartsWith("F", StringComparison.OrdinalIgnoreCase))
            {
                return "01"; // Factura
            }
            else if (serie.StartsWith("B", StringComparison.OrdinalIgnoreCase))
            {
                return "03"; // Boleta
            }
            else if (serie.StartsWith("E", StringComparison.OrdinalIgnoreCase))
            {
                return "07"; // Nota de Crédito
            }

            // Por defecto, asumir factura
            return "01";
        }

        /// <summary>
        /// Clase para deserializar la respuesta de SUNAT para PDF
        /// La respuesta contiene un ZIP en base64 con el PDF dentro
        /// </summary>
        private class SunatPdfResponse
        {
            public string? NomArchivo { get; set; }  // Nombre del archivo ZIP
            public string? ValArchivo { get; set; }  // Contenido del ZIP en base64
        }
    }

    /// <summary>
    /// Resultado de la descarga masiva de PDFs bajo demanda
    /// </summary>
    public class DescargaPdfMasivaResultado
    {
        /// <summary>IDs cuyos PDFs fueron descargados exitosamente desde SUNAT</summary>
        public List<int> Descargados { get; set; } = new();

        /// <summary>IDs que ya tenían PDF en disco y fueron omitidos</summary>
        public List<int> Omitidos { get; set; } = new();

        /// <summary>IDs que fallaron al intentar la descarga</summary>
        public List<DescargaPdfItemFallido> Fallidos { get; set; } = new();

        /// <summary>IDs que no existen en la base de datos</summary>
        public List<int> NoEncontrados { get; set; } = new();

        public int Total => Descargados.Count + Omitidos.Count + Fallidos.Count + NoEncontrados.Count;
    }

    /// <summary>
    /// Detalle de un comprobante que falló durante la descarga masiva
    /// </summary>
    public class DescargaPdfItemFallido
    {
        public int Id { get; set; }
        public string Razon { get; set; } = string.Empty;
    }
}
