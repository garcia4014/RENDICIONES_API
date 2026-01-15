using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Servicio para la autenticación con SUNAT
    /// </summary>
    public class SunatTokenService : ISunatTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SunatTokenService> _logger;
        private readonly string _tokenBaseUrl = "https://api-seguridad.sunat.gob.pe/v1/clientesextranet";

        public SunatTokenService(HttpClient httpClient, ILogger<SunatTokenService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiResponse<SunatTokenResponseDto>> ObtenerTokenAsync(string clientId, string clientSecret)
        {
            try
            {
                _logger.LogInformation("Iniciando solicitud de token SUNAT para client_id: {ClientId}", clientId);

                // Construir URL con el client_id
                var url = $"{_tokenBaseUrl}/{clientId}/oauth2/token/";

                // Preparar los datos del formulario
                var formData = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "client_credentials"),
                    new("scope", "https://api.sunat.gob.pe/v1/contribuyente/contribuyentes"),
                    new("client_id", clientId),
                    new("client_secret", clientSecret)
                };

                var content = new FormUrlEncodedContent(formData);

                // Realizar la petición
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = JsonSerializer.Deserialize<SunatTokenResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogInformation("Token obtenido exitosamente. Expira en: {ExpiresIn} segundos", tokenResponse.expires_in);

                    return new ApiResponse<SunatTokenResponseDto>(tokenResponse, "Token obtenido exitosamente");
                }
                else
                {
                    _logger.LogError("Error al obtener token SUNAT. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);

                    return new ApiResponse<SunatTokenResponseDto>($"Error al obtener token: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener token SUNAT");
                return new ApiResponse<SunatTokenResponseDto>("Error de conexión con SUNAT");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar respuesta de token SUNAT");
                return new ApiResponse<SunatTokenResponseDto>("Error al procesar respuesta de SUNAT");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener token SUNAT");
                return new ApiResponse<SunatTokenResponseDto>("Error inesperado al obtener token");
            }
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return false;

                // Aquí podrías implementar validación adicional del token
                // Por ejemplo, verificar su formato JWT si es aplicable
                // Por ahora, solo verificamos que no esté vacío
                return !string.IsNullOrWhiteSpace(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar token");
                return false;
            }
        }
    }
}