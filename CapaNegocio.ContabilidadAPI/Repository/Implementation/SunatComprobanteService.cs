using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Servicio para la consulta de comprobantes de SUNAT
    /// </summary>
    public class SunatComprobanteService : ISunatComprobanteService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SunatComprobanteService> _logger;
        private readonly string _baseUrl = "https://api.sunat.gob.pe/v1/contribuyente/contribuyentes";

        public SunatComprobanteService(HttpClient httpClient, ILogger<SunatComprobanteService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiResponse<SunatComprobanteResponseDto>> ValidarComprobanteAsync(
            string rucConsultante,
            string token,
            SunatComprobanteRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando validación de comprobante para RUC: {RUC}, Serie: {Serie}, Número: {Numero}",
                    request.numRuc, request.numeroSerie, request.numero);

                // Validar parámetros
                if (string.IsNullOrEmpty(rucConsultante) || string.IsNullOrEmpty(token) || request == null)
                {
                    return new ApiResponse<SunatComprobanteResponseDto>("Parámetros inválidos para la consulta");
                }

                // Construir URL
                var url = $"{_baseUrl}/{rucConsultante}/validarcomprobante";

                // Configurar headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                // Serializar el request
                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Realizar la petición
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var respuestaDecodificada = JsonSerializer.Deserialize<responseSunatStatus>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (response.IsSuccessStatusCode && respuestaDecodificada.data.estadoCp == "1")
                {
                    var comprobanteResponse = JsonSerializer.Deserialize<SunatComprobanteResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Consulta de comprobante exitosa. Estado: {Estado}",
                        comprobanteResponse?.data?.estadoCp);

                    return new ApiResponse<SunatComprobanteResponseDto>(comprobanteResponse, "Validación completada exitosamente");
                }
                else
                {
                    _logger.LogError("Error al consultar comprobante. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseContent);

                    return new ApiResponse<SunatComprobanteResponseDto>($"Los datos del comprobante no se lograron validar en SUNAT, revise la información nuevamente.");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al consultar comprobante");
                return new ApiResponse<SunatComprobanteResponseDto>("Error de conexión con SUNAT");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al procesar respuesta de SUNAT");
                return new ApiResponse<SunatComprobanteResponseDto>("Error al procesar respuesta de SUNAT");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al consultar comprobante");
                return new ApiResponse<SunatComprobanteResponseDto>("Error inesperado en la consulta");
            }
        } 
        public string ObtenerDescripcionEstadoComprobante(int estadoCp)
        {
            return estadoCp switch
            {
                0 => "NO EXISTE (Comprobante no informado)",
                1 => "ACEPTADO (Comprobante aceptado)",
                2 => "ANULADO (Comunicado en una baja)",
                3 => "AUTORIZADO (con autorización de imprenta)",
                4 => "NO AUTORIZADO (no autorizado por imprenta)",
                _ => $"Estado desconocido ({estadoCp})"
            };
        }

        public string ObtenerDescripcionEstadoRuc(string estadoRuc)
        {
            return estadoRuc switch
            {
                "00" => "ACTIVO",
                "01" => "BAJA PROVISIONAL",
                "02" => "BAJA PROV. POR OFICIO",
                "03" => "SUSPENSION TEMPORAL",
                "10" => "BAJA DEFINITIVA",
                "11" => "BAJA DE OFICIO",
                "22" => "INHABILITADO-VENT.UNICA",
                _ => $"Estado desconocido ({estadoRuc})"
            };
        }

        public string ObtenerDescripcionCondicionDomiciliaria(string condDomiRuc)
        {
            return condDomiRuc switch
            {
                "00" => "HABIDO",
                "09" => "PENDIENTE",
                "11" => "POR VERIFICAR",
                "12" => "NO HABIDO",
                "20" => "NO HALLADO",
                _ => $"Condición desconocida ({condDomiRuc})"
            };
        } 
        //{"success":true,"message":"Operation Success! ","data":{"estadoCp":"0"}}


        public class responseSunatStatus
        {
            public bool? success { get; set; }
            public string? message { get; set; }
            public DataResponseSunatStatus data { get; set; }

        }

        public class DataResponseSunatStatus
        {
            public string? estadoCp { get; set; }
        }


    }
}