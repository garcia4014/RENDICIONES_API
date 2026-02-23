using CapaDatos.ContabilidadAPI;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Servicio en background para verificar datos de empresas emisoras en SUNAT
    /// Se ejecuta cada 5 minutos para actualizar información de RUC
    /// </summary>
    public class ComprobanteEmpresaVerificadorBackgroundService
    {
        private readonly ILogger<ComprobanteEmpresaVerificadorBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ComprobanteEmpresaVerificadorBackgroundService(
            ILogger<ComprobanteEmpresaVerificadorBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Verifica y actualiza datos de empresas emisoras consultando a SUNAT
        /// </summary>
        public async Task VerificarEmpresasEmisoras()
        {
            _logger.LogInformation("===== INICIO: Verificación de empresas emisoras en SUNAT =====");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SvrendicionesContext>();
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

                // Obtener token de SUNAT desde parámetros
                var parametro = await dbContext.Parametros.FirstOrDefaultAsync(p => p.Id == 1);
                if (parametro == null || string.IsNullOrEmpty(parametro.Valor))
                {
                    _logger.LogError("No se encontró el token de SUNAT en la tabla PARAMETROS (Id=1, Columna Valor)");
                    return;
                }

                var token = parametro.Valor;

                // Obtener RUCs únicos de comprobantes activos que necesitan verificación
                // (comprobantes recientes sin valores en los campos de empresa)
                var rucsParaVerificar = await dbContext.ComprobantesPago
                    .Where(c => c.Activo == true &&
                           c.Ruc != null &&
                           (c.ActivoEmp == null || c.HabidoEmp == null || c.RusEmp == null))
                    .Select(c => c.Ruc!.Value)
                    .Distinct()
                    .Take(20) // Procesar máximo 20 RUCs diferentes por vez
                    .ToListAsync();

                if (!rucsParaVerificar.Any())
                {
                    _logger.LogInformation("No hay RUCs pendientes de verificación");
                    return;
                }

                _logger.LogInformation("Encontrados {Cantidad} RUCs únicos para verificar en SUNAT", rucsParaVerificar.Count);

                int exitosos = 0;
                int fallidos = 0;

                foreach (var ruc in rucsParaVerificar)
                {
                    try
                    {
                        _logger.LogInformation("Verificando RUC: {Ruc}", ruc);

                        var resultado = await ConsultarRucEnSunat(httpClient, token, ruc);

                        if (resultado.Exitoso)
                        {
                            // Actualizar todos los comprobantes con este RUC
                            var comprobantesActualizar = await dbContext.ComprobantesPago
                                .Where(c => c.Activo == true && c.Ruc == ruc)
                                .ToListAsync();

                            foreach (var comprobante in comprobantesActualizar)
                            {
                                comprobante.RazonSocial = resultado.RazonSocial ?? comprobante.RazonSocial;
                                comprobante.ActivoEmp = resultado.ActivoEmp;
                                comprobante.HabidoEmp = resultado.HabidoEmp;
                                comprobante.RusEmp = resultado.RusEmp;

                                // Si el tipo de comprobante NO es 01 (Factura) ni 03 (Boleta)
                                // entonces marcar como INAFECTO y ajustar montos
                                if (!string.IsNullOrEmpty(comprobante.TipoComprobante) &&
                                    comprobante.TipoComprobante != "01" &&
                                    comprobante.TipoComprobante != "03" &&
                                    resultado.RusEmp.GetValueOrDefault(false) &&
                                    resultado.ActivoEmp.GetValueOrDefault(false) &&
                                    resultado.HabidoEmp.GetValueOrDefault(false)
                                    )
                                {
                                    // Copiar Monto a Subtotal y MontoInafecto
                                    comprobante.Subtotal = comprobante.Monto;
                                    comprobante.MontoInafecto = comprobante.Monto;

                                    // Marcar como Inafecto
                                    comprobante.Inafecto = true;

                                    // Desmarcar otros tipos
                                    comprobante.Exonerado = false;
                                    comprobante.Gravado = false;
                                    comprobante.IgvEspecial = false;
                                    comprobante.OtrosCargos = false;

                                    // Limpiar otros montos
                                    comprobante.MontoExonerado = 0m;
                                    comprobante.MontoGravado = 0m;
                                    comprobante.MontoIgvEspecial = 0m;
                                    comprobante.MontoOtrosCargos = 0m;
                                    comprobante.Igv = 0m;

                                    comprobante.Desglosado = true; // Marcar como desglosado para indicar que se ajustó el desglose

                                    _logger.LogInformation(
                                        "Comprobante ID={Id} Tipo={Tipo} (distinto a 01/03) marcado como INAFECTO. Monto={Monto}, Subtotal={Subtotal}",
                                        comprobante.Id, comprobante.TipoComprobante, comprobante.Monto, comprobante.Subtotal);
                                }
                            }

                            await dbContext.SaveChangesAsync();

                            _logger.LogInformation(
                                "RUC {Ruc} verificado exitosamente. Actualizados {Cantidad} comprobantes. " +
                                "RazonSocial={RazonSocial}, Activo={Activo}, Habido={Habido}, RUS={Rus}",
                                ruc, comprobantesActualizar.Count, resultado.RazonSocial,
                                resultado.ActivoEmp, resultado.HabidoEmp, resultado.RusEmp);

                            exitosos++;
                        }
                        else
                        {
                            _logger.LogWarning("No se pudo verificar RUC {Ruc}: {Mensaje}", ruc, resultado.MensajeError);
                            fallidos++;
                        }

                        // Pequeña pausa entre llamadas para no saturar la API
                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al verificar RUC {Ruc}", ruc);
                        fallidos++;
                    }
                }

                _logger.LogInformation(
                    "===== FIN: Verificación de empresas emisoras - Exitosos: {Exitosos}, Fallidos: {Fallidos} =====",
                    exitosos, fallidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general en el proceso de verificación de empresas emisoras");
            }
        }

        /// <summary>
        /// Consulta datos de un RUC en la API de SUNAT
        /// </summary>
        private async Task<ResultadoVerificacion> ConsultarRucEnSunat(HttpClient httpClient, string token, long ruc)
        {
            const int maxReintentos = 3;
            int intentoActual = 0;

            while (intentoActual < maxReintentos)
            {
                intentoActual++;
                try
                {
                    var url = $"https://api-cpe.sunat.gob.pe/v1/contribuyente/parametros/contribuyentes/{ruc}";

                    _logger.LogInformation("Consultando SUNAT API (intento {Intento}/{Max}): {Url}", intentoActual, maxReintentos, url);

                    // Configurar headers (simular navegador/Postman)
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "es,es-419;q=0.9");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                    var response = await httpClient.GetAsync(url);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // SIEMPRE logear la respuesta de la API para monitoreo
                    _logger.LogInformation(
                        "Respuesta SUNAT para RUC {Ruc} - Status: {StatusCode}, Content: {Content}",
                        ruc, (int)response.StatusCode, responseContent);

                    // Si es 200, procesar la respuesta exitosa
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var sunatResponse = JsonSerializer.Deserialize<SunatContribuyenteResponse>(
                            responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (sunatResponse?.DatosContribuyente != null)
                        {
                            var datos = sunatResponse.DatosContribuyente;

                            // Determinar RUS: verificar si tiene tributo 041000
                            bool tieneRus = false;
                            if (datos.Tributos != null && datos.Tributos.Any())
                            {
                                tieneRus = datos.Tributos.Any(t => t.CodTributo == "041000");
                            }

                            // Determinar si está activo: codEstado = "00"
                            bool estaActivo = datos.CodEstado == "00";

                            // Determinar si es habido: codDomHabido = "00"
                            bool esHabido = datos.CodDomHabido == "00";

                            return new ResultadoVerificacion
                            {
                                Exitoso = true,
                                RazonSocial = datos.DesRazonSocial,
                                ActivoEmp = estaActivo,
                                HabidoEmp = esHabido,
                                RusEmp = tieneRus
                            };
                        }
                    }

                    // Si no es 200 o hay error, intentar deserializar como error
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<SunatErrorResponse>(
                            responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (errorResponse?.Errors != null && errorResponse.Errors.Any())
                        {
                            var mensajesError = string.Join(", ", errorResponse.Errors.Select(e => $"[{e.Cod}] {e.Msg}"));
                            return new ResultadoVerificacion
                            {
                                Exitoso = false,
                                MensajeError = $"Error {errorResponse.Cod}: {errorResponse.Msg} - {mensajesError}"
                            };
                        }
                    }
                    catch
                    {
                        // Si no se puede deserializar como error, usar el contenido crudo
                    }

                    // Si no es 200, no reintentar, devolver error inmediatamente
                    return new ResultadoVerificacion
                    {
                        Exitoso = false,
                        MensajeError = $"HTTP {(int)response.StatusCode}: {responseContent}"
                    };
                }
                catch (HttpRequestException ex) when (intentoActual < maxReintentos)
                {
                    // Errores de red (DNS, conexión) - reintentar
                    _logger.LogWarning("Error de red al consultar RUC {Ruc} en SUNAT (intento {Intento}/{Max}): {Error}. Reintentando en 3 segundos...", 
                        ruc, intentoActual, maxReintentos, ex.Message);
                    await Task.Delay(3000); // Esperar 3 segundos antes de reintentar
                    continue;
                }
                catch (TaskCanceledException ex) when (intentoActual < maxReintentos)
                {
                    // Timeout - reintentar
                    _logger.LogWarning("Timeout al consultar RUC {Ruc} en SUNAT (intento {Intento}/{Max}): {Error}. Reintentando en 3 segundos...", 
                        ruc, intentoActual, maxReintentos, ex.Message);
                    await Task.Delay(3000);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error al consultar RUC {Ruc} en SUNAT (intento {Intento}/{Max}): {Error}", 
                        ruc, intentoActual, maxReintentos, ex.Message);
                    
                    if (intentoActual < maxReintentos)
                    {
                        await Task.Delay(3000);
                        continue;
                    }
                    
                    return new ResultadoVerificacion
                    {
                        Exitoso = false,
                        MensajeError = $"Excepción después de {maxReintentos} intentos: {ex.Message}"
                    };
                }
            }

            // Si llegamos aquí, todos los reintentos fallaron
            return new ResultadoVerificacion
            {
                Exitoso = false,
                MensajeError = $"No se pudo conectar a SUNAT después de {maxReintentos} intentos"
            };
        }

        #region Modelos de respuesta de SUNAT

        /// <summary>
        /// Modelo para respuesta exitosa de SUNAT
        /// </summary>
        private class SunatContribuyenteResponse
        {
            [JsonPropertyName("datosContribuyente")]
            public DatosContribuyente? DatosContribuyente { get; set; }
        }

        private class DatosContribuyente
        {
            [JsonPropertyName("desRazonSocial")]
            public string? DesRazonSocial { get; set; }

            [JsonPropertyName("desNomApe")]
            public string? DesNomApe { get; set; }

            [JsonPropertyName("codCorreo1")]
            public string? CodCorreo1 { get; set; }

            [JsonPropertyName("codCorreo2")]
            public string? CodCorreo2 { get; set; }

            [JsonPropertyName("ubigeo")]
            public Ubigeo? Ubigeo { get; set; }

            [JsonPropertyName("contacto")]
            public Contacto? Contacto { get; set; }

            [JsonPropertyName("tributos")]
            public List<Tributo>? Tributos { get; set; }

            [JsonPropertyName("desDireccion")]
            public string? DesDireccion { get; set; }

            [JsonPropertyName("codEstado")]
            public string? CodEstado { get; set; }

            [JsonPropertyName("codDomHabido")]
            public string? CodDomHabido { get; set; }
        }

        private class Ubigeo
        {
            [JsonPropertyName("codUbigeo")]
            public string? CodUbigeo { get; set; }

            [JsonPropertyName("desDepartamento")]
            public string? DesDepartamento { get; set; }

            [JsonPropertyName("desProvincia")]
            public string? DesProvincia { get; set; }

            [JsonPropertyName("desDistrito")]
            public string? DesDistrito { get; set; }
        }

        private class Contacto
        {
            [JsonPropertyName("numTelefono1")]
            public string? NumTelefono1 { get; set; }

            [JsonPropertyName("numTelefono2")]
            public string? NumTelefono2 { get; set; }

            [JsonPropertyName("numTelefono3")]
            public string? NumTelefono3 { get; set; }
        }

        private class Tributo
        {
            [JsonPropertyName("codTributo")]
            public string? CodTributo { get; set; }

            [JsonPropertyName("fecVigencia")]
            public DateTime? FecVigencia { get; set; }

            [JsonPropertyName("fecAlta")]
            public DateTime? FecAlta { get; set; }
        }

        /// <summary>
        /// Modelo para respuesta de error de SUNAT
        /// </summary>
        private class SunatErrorResponse
        {
            [JsonPropertyName("cod")]
            public int Cod { get; set; }

            [JsonPropertyName("msg")]
            public string? Msg { get; set; }

            [JsonPropertyName("errors")]
            public List<ErrorDetalle>? Errors { get; set; }
        }

        private class ErrorDetalle
        {
            [JsonPropertyName("cod")]
            public int Cod { get; set; }

            [JsonPropertyName("msg")]
            public string? Msg { get; set; }
        }

        /// <summary>
        /// Resultado de la verificación de un RUC
        /// </summary>
        private class ResultadoVerificacion
        {
            public bool Exitoso { get; set; }
            public string? RazonSocial { get; set; }
            public bool? ActivoEmp { get; set; }
            public bool? HabidoEmp { get; set; }
            public bool? RusEmp { get; set; }
            public string? MensajeError { get; set; }
        }

        #endregion
    }
}
