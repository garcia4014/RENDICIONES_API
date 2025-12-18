using Azure.Core;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Implementation;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Hangfire;

namespace ContabilidadAPI.Controllers
{
    /// <summary>
    /// Controlador para servicios de SUNAT - Validación de comprobantes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SunatController : ControllerBase
    {
        private readonly ISunatService _sunatService;
        private readonly ISunatTokenService _tokenService;
        private readonly ISunatComprobanteService _comprobanteService;
        private readonly ILogger<SunatController> _logger;
        private readonly SunatConfigurationDto _sunatConfig;
        private readonly ISviatico _sviatico;
        private readonly IComprobantePago _comprobanteImp;
        private readonly IComprobantePagoService _comprobanteRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public SunatController(
            ISunatService sunatService,
            ISunatTokenService tokenService,
            ISunatComprobanteService comprobanteService,
            ILogger<SunatController> logger,
            IOptions<SunatConfigurationDto> sunatConfig,
            ISviatico sviatico,
            IComprobantePago comprobanteImp,
            IComprobantePagoService comprobanteRepository,
            IBackgroundJobClient backgroundJobClient
            )
        {
            _sunatService = sunatService;
            _tokenService = tokenService;
            _comprobanteService = comprobanteService;
            _logger = logger;
            _sunatConfig = sunatConfig.Value;
            _sviatico = sviatico;
            _backgroundJobClient = backgroundJobClient;
            _comprobanteImp = comprobanteImp;
            _comprobanteRepository = comprobanteRepository;
        }

        [HttpPost("token")]
        [ProducesResponseType(typeof(ApiResponse<SunatTokenResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<SunatTokenResponseDto>>> ObtenerToken(
            [FromBody] SunatTokenRequestDto request)
        {
            try
            {
                _logger.LogInformation("Solicitando token SUNAT para cliente: {ClientId}", request.client_id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>("Datos de solicitud inválidos"));
                }

                var result = await _tokenService.ObtenerTokenAsync(request.client_id, request.client_secret);

                if (result.Success)
                {
                    _logger.LogInformation("Token SUNAT obtenido exitosamente");
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error obteniendo token SUNAT: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado obteniendo token SUNAT");
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        [HttpPost("validar-comprobante")]
        [ProducesResponseType(typeof(ApiResponse<SunatComprobanteResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<SunatComprobanteResponseDto>>> ValidarComprobante(
            [FromQuery, Required] string rucConsultante,
            [FromHeader(Name = "Authorization")] string authorization,
            [FromBody] SunatComprobanteRequestDto request)
        {
            try
            {
                _logger.LogInformation("Validando comprobante para RUC: {RucConsultante}, Serie: {Serie}, Número: {Numero}", 
                    rucConsultante, request.numeroSerie, request.numero);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>("Datos de solicitud inválidos"));
                }

                var token = await _tokenService.ObtenerTokenAsync(_sunatConfig.ClientId, _sunatConfig.ClientSecret);
                  
                // Validar que el token no esté vacío
                if (string.IsNullOrWhiteSpace(token.Data.access_token))
                {
                    return Unauthorized(new ApiResponse<object>("Token de autorización inválido"));
                }

                var result = await _comprobanteService.ValidarComprobanteAsync(rucConsultante, token.Data.access_token, request);

                if (result.Success)
                {
                    _logger.LogInformation("Comprobante validado exitosamente");
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error validando comprobante: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado validando comprobante");
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        [HttpPost("validar-comprobante-auto")]
        [ProducesResponseType(typeof(ApiResponse<SunatComprobanteResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<SunatComprobanteResponseDto>>> ValidarComprobanteAuto(
              [FromQuery, Required] int detalleComprobante
            )
        {
            try
            { 
                var comprobante = await _comprobanteRepository.GetById(detalleComprobante); 

                if (comprobante == null || !comprobante.Activo)
                {
                    return BadRequest(new ApiResponse<object>(null, "No se encontró un comprobante activo para este detalle"));
                }

                var responseAuthSunat = await _tokenService.ObtenerTokenAsync(_sunatConfig.ClientId, _sunatConfig.ClientSecret);
                var token = responseAuthSunat.Data.access_token;

                _logger.LogInformation("Validando comprobante para RUC: {RucConsultante}, Serie: {Serie}, Número: {Numero}",
                    comprobante.Ruc, comprobante.Serie, comprobante.Correlativo);

                SunatComprobanteRequestDto request = new();
                request.numRuc = comprobante.Ruc.ToString() ?? string.Empty;
                request.codComp = comprobante.TipoComprobante?.PadLeft(2,'0') ?? string.Empty;
                request.numeroSerie = comprobante.Serie ?? string.Empty;
                request.numero = comprobante.Correlativo ?? string.Empty;
                request.fechaEmision = comprobante.FechaEmision.GetValueOrDefault().ToString("dd/MM/yyyy");
                request.monto = comprobante.Monto.ToString() ?? string.Empty;

                var result = await _comprobanteService.ValidarComprobanteAsync(_sunatConfig.RUC, token, request);

                if (result.Success)
                {
                    _logger.LogInformation("Comprobante validado exitosamente");
                    comprobante.ValidoSunat = true;
                    comprobante.ResultadoSunat = JsonSerializer.Serialize(result);

                    await _comprobanteImp.UpdateAsync(comprobante);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error validando comprobante: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado validando comprobante");
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Valida múltiples comprobantes de forma masiva en segundo plano usando Hangfire
        /// </summary>
        /// <param name="request">Lista de IDs de comprobantes a validar</param>
        /// <returns>Confirmación de que el proceso se inició</returns>
        [HttpPost("validar-comprobantes-masivo")]
        [ProducesResponseType(typeof(ApiResponse<ValidacionMasivaResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public ActionResult<ApiResponse<ValidacionMasivaResponseDto>> ValidarComprobantesMasivo(
            [FromBody] ValidacionMasivaRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>("Datos de solicitud inválidos"));
                }

                if (request.IdsComprobantes == null || !request.IdsComprobantes.Any())
                {
                    return BadRequest(new ApiResponse<object>("Debe proporcionar al menos un ID de comprobante"));
                }

                if (request.IdsComprobantes.Count > 1000)
                {
                    return BadRequest(new ApiResponse<object>("El límite máximo es 1000 comprobantes por solicitud"));
                }

                _logger.LogInformation("Iniciando validación masiva de {Count} comprobantes", request.IdsComprobantes.Count);

                // Encolar trabajos en Hangfire
                var jobIds = new List<string>();
                foreach (var comprobanteId in request.IdsComprobantes)
                {
                    var jobId = _backgroundJobClient.Enqueue<IComprobantePagoService>(
                        service => service.ValidarComprobanteEnSunatAsync(comprobanteId));
                    
                    jobIds.Add(jobId);
                }

                var response = new ValidacionMasivaResponseDto
                {
                    TotalComprobantes = request.IdsComprobantes.Count,
                    JobIds = jobIds,
                    FechaInicio = DateTime.UtcNow,
                    Estado = "Procesando"
                };

                _logger.LogInformation("Validación masiva iniciada. Total: {Total}, Jobs creados: {Jobs}", 
                    response.TotalComprobantes, jobIds.Count);

                return Ok(new ApiResponse<ValidacionMasivaResponseDto>(
                    response, 
                    $"Validación masiva iniciada correctamente. {response.TotalComprobantes} comprobantes en cola."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error iniciando validación masiva");
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor al iniciar validación masiva"));
            }
        }

        /// <summary>
        /// Consulta el estado de una validación masiva
        /// </summary>
        /// <param name="jobIds">Lista de IDs de trabajos Hangfire</param>
        /// <returns>Estado de los trabajos</returns>
        [HttpPost("estado-validacion-masiva")]
        [ProducesResponseType(typeof(ApiResponse<EstadoValidacionMasivaDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public ActionResult<ApiResponse<EstadoValidacionMasivaDto>> ObtenerEstadoValidacionMasiva(
            [FromBody] EstadoValidacionMasivaRequestDto request)
        {
            try
            {
                if (request.JobIds == null || !request.JobIds.Any())
                {
                    return BadRequest(new ApiResponse<object>("Debe proporcionar al menos un Job ID"));
                }

                var monitoringApi = JobStorage.Current.GetMonitoringApi();
                
                int completados = 0;
                int enProceso = 0;
                int fallidos = 0;
                int pendientes = 0;

                foreach (var jobId in request.JobIds)
                {
                    var jobDetails = monitoringApi.JobDetails(jobId);
                    
                    if (jobDetails != null && jobDetails.History != null)
                    {
                        var lastState = jobDetails.History.FirstOrDefault()?.StateName;
                        
                        switch (lastState)
                        {
                            case "Succeeded":
                                completados++;
                                break;
                            case "Processing":
                                enProceso++;
                                break;
                            case "Failed":
                                fallidos++;
                                break;
                            case "Enqueued":
                            case "Scheduled":
                                pendientes++;
                                break;
                        }
                    }
                }

                var total = request.JobIds.Count;
                var porcentajeCompletado = total > 0 ? (completados * 100.0) / total : 0;

                var response = new EstadoValidacionMasivaDto
                {
                    TotalJobs = total,
                    Completados = completados,
                    EnProceso = enProceso,
                    Fallidos = fallidos,
                    Pendientes = pendientes,
                    PorcentajeCompletado = Math.Round(porcentajeCompletado, 2)
                };

                return Ok(new ApiResponse<EstadoValidacionMasivaDto>(
                    response,
                    $"Estado: {completados}/{total} completados"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando estado de validación masiva");
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        } 

        /// <summary>
        /// Valida un comprobante de pago de forma completa (obtiene token automáticamente)
        /// </summary>
        /// <param name="request">Solicitud completa con credenciales y datos del comprobante</param>
        /// <returns>Resultado detallado de la validación</returns>
        [HttpPost("validar-comprobante-completo")]
        [ProducesResponseType(typeof(ApiResponse<SunatComprobanteValidationResultDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<SunatComprobanteValidationResultDto>>> ValidarComprobanteCompleto(
            [FromBody] SunatValidacionCompletaRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando validación completa de comprobante para RUC: {RucConsultante}", 
                    request.RucConsultante);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>("Datos de solicitud inválidos"));
                }

                var result = await _sunatService.ValidarComprobanteCompletoAsync(
                    request.ClientId,
                    request.ClientSecret,
                    request.RucConsultante,
                    request.ComprobanteRequest);

                if (result.Success)
                {
                    _logger.LogInformation("Validación completa exitosa para comprobante {Serie}-{Numero}", 
                        request.ComprobanteRequest.numeroSerie, request.ComprobanteRequest.numero);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error en validación completa: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en validación completa");
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Verifica el estado de un token de SUNAT
        /// </summary>
        /// <param name="token">Token a verificar</param>
        /// <returns>Estado del token</returns>
        [HttpGet("verificar-token")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<ActionResult<ApiResponse<bool>>> VerificarToken(
            [FromQuery, Required] string token)
        {
            try
            {
                _logger.LogInformation("Verificando validez del token SUNAT");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new ApiResponse<object>("Token requerido"));
                }

                var esValido = await _tokenService.ValidarTokenAsync(token);

                return Ok(new ApiResponse<bool>(esValido, 
                    esValido ? "Token válido" : "Token inválido o expirado"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando token");
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene información sobre los códigos de estado de comprobantes
        /// </summary>
        /// <returns>Información de códigos de estado</returns>
        [HttpGet("estados-comprobante")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public ActionResult<ApiResponse<object>> ObtenerEstadosComprobante()
        {
            try
            {
                var estados = new
                {
                    EstadosComprobante = new[]
                    {
                        new { Codigo = 0, Descripcion = "No autorizado" },
                        new { Codigo = 1, Descripcion = "Autorizado" },
                        new { Codigo = 2, Descripcion = "Anulado" },
                        new { Codigo = 3, Descripcion = "Autorizado con observaciones" }
                    },
                    EstadosRuc = new[]
                    {
                        new { Codigo = "00", Descripcion = "Activo" },
                        new { Codigo = "01", Descripcion = "Suspendido temporalmente" },
                        new { Codigo = "02", Descripcion = "Suspendido definitivamente" },
                        new { Codigo = "03", Descripcion = "No autorizado" }
                    },
                    CondicionesDomiciliarias = new[]
                    {
                        new { Codigo = "00", Descripcion = "No habido" },
                        new { Codigo = "09", Descripcion = "Pendiente" },
                        new { Codigo = "11", Descripcion = "Por verificar" },
                        new { Codigo = "12", Descripcion = "No hallado" },
                        new { Codigo = "20", Descripcion = "Habido" }
                    },
                    TiposComprobante = new[]
                    {
                        new { Codigo = "01", Descripcion = "Factura" },
                        new { Codigo = "03", Descripcion = "Boleta de venta" },
                        new { Codigo = "07", Descripcion = "Nota de crédito" },
                        new { Codigo = "08", Descripcion = "Nota de débito" }
                    }
                };

                return Ok(new ApiResponse<object>(estados, "Información de códigos de estado obtenida"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estados");
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }
    }

    /// <summary>
    /// DTO para la validación completa de comprobante
    /// </summary>
    public class SunatValidacionCompletaRequestDto
    {
        [Required]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        public string ClientSecret { get; set; } = string.Empty;

        [Required]
        [StringLength(11, MinimumLength = 11)]
        public string RucConsultante { get; set; } = string.Empty;

        [Required]
        public SunatComprobanteRequestDto ComprobanteRequest { get; set; } = new();
    }

    /// <summary>
    /// DTO para solicitud de validación masiva
    /// </summary>
    public class ValidacionMasivaRequestDto
    {
        [Required]
        public List<int> IdsComprobantes { get; set; } = new();
    }

    /// <summary>
    /// DTO para respuesta de validación masiva
    /// </summary>
    public class ValidacionMasivaResponseDto
    {
        public int TotalComprobantes { get; set; }
        public List<string> JobIds { get; set; } = new();
        public DateTime FechaInicio { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para solicitud de estado de validación masiva
    /// </summary>
    public class EstadoValidacionMasivaRequestDto
    {
        [Required]
        public List<string> JobIds { get; set; } = new();
    }

    /// <summary>
    /// DTO para respuesta de estado de validación masiva
    /// </summary>
    public class EstadoValidacionMasivaDto
    {
        public int TotalJobs { get; set; }
        public int Completados { get; set; }
        public int EnProceso { get; set; }
        public int Fallidos { get; set; }
        public int Pendientes { get; set; }
        public double PorcentajeCompletado { get; set; }
    }
}