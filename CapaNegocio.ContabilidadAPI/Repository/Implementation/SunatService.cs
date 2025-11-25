using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Servicio coordinador para las operaciones con SUNAT
    /// </summary>
    public class SunatService : ISunatService
    {
        private readonly ISunatTokenService _tokenService;
        private readonly ISunatComprobanteService _comprobanteService;
        private readonly ILogger<SunatService> _logger;

        public SunatService(
            ISunatTokenService tokenService, 
            ISunatComprobanteService comprobanteService,
            ILogger<SunatService> logger)
        {
            _tokenService = tokenService;
            _comprobanteService = comprobanteService;
            _logger = logger;
        }

        public async Task<ApiResponse<SunatComprobanteValidationResultDto>> ValidarComprobanteCompletoAsync(
            string clientId, 
            string clientSecret, 
            string rucConsultante, 
            SunatComprobanteRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando validación completa de comprobante. RUC Emisor: {RucEmisor}, Serie: {Serie}, Número: {Numero}",
                    request.numRuc, request.numeroSerie, request.numero);

                // Paso 1: Obtener token
                var tokenResponse = await _tokenService.ObtenerTokenAsync(clientId, clientSecret);
                if (!tokenResponse.Success || tokenResponse.Data == null)
                {
                    _logger.LogError("Error al obtener token: {Message}", tokenResponse.Message);
                    return new ApiResponse<SunatComprobanteValidationResultDto>($"Error en autenticación: {tokenResponse.Message}");
                }

                // Paso 2: Validar comprobante
                var validacionResponse = await _comprobanteService.ValidarComprobanteAsync(
                    rucConsultante, 
                    tokenResponse.Data.access_token, 
                    request);

                if (!validacionResponse.Success || validacionResponse.Data == null)
                {
                    _logger.LogError("Error al validar comprobante: {Message}", validacionResponse.Message);
                    return new ApiResponse<SunatComprobanteValidationResultDto>($"Error en validación: {validacionResponse.Message}");
                }

                // Paso 3: Procesar y enriquecer la respuesta
                var resultado = new SunatComprobanteValidationResultDto
                {
                    Success = validacionResponse.Data.success,
                    Message = validacionResponse.Data.message,
                    ErrorCode = validacionResponse.Data.errorCode,
                    FechaConsulta = DateTime.Now,
                    RucConsultante = rucConsultante,
                    ComprobanteConsultado = $"{request.numeroSerie}-{request.numero}"
                };

                if (validacionResponse.Data.data != null)
                {
                    var data = validacionResponse.Data.data;
                    
                    resultado.EstadoComprobante = int.Parse(data.estadoCp) ;
                    resultado.DescripcionEstadoComprobante = _comprobanteService.ObtenerDescripcionEstadoComprobante(int.Parse(data.estadoCp));
                    
                    resultado.EstadoRuc = data.estadoRuc;
                    resultado.DescripcionEstadoRuc = _comprobanteService.ObtenerDescripcionEstadoRuc(data.estadoRuc);
                    
                    resultado.CondicionDomiciliaria = data.condDomiRuc;
                    resultado.DescripcionCondicionDomiciliaria = _comprobanteService.ObtenerDescripcionCondicionDomiciliaria(data.condDomiRuc);
                    
                    resultado.Observaciones = data.observaciones ?? new string[0];
                }

                _logger.LogInformation("Validación de comprobante completada exitosamente. Estado: {Estado}", 
                    resultado.DescripcionEstadoComprobante);

                return new ApiResponse<SunatComprobanteValidationResultDto>(resultado, "Validación completada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en validación completa de comprobante");
                return new ApiResponse<SunatComprobanteValidationResultDto>("Error inesperado en la validación");
            }
        }
    }
}