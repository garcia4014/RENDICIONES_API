using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ContabilidadAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar los estados de los viáticos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class SviaticoEstadosController : ControllerBase
    {
        private readonly ISviaticoService _sviaticoService;
        private readonly ILogger<SviaticoEstadosController> _logger;
        private readonly ISviatico _dao;
        private readonly INotificacionesService _notificacionesService;

        public SviaticoEstadosController(
            INotificacionesService notificacionesService,
            ISviatico dao,
            ISviaticoService sviaticoService,
            ILogger<SviaticoEstadosController> logger)
        {
            _sviaticoService = sviaticoService;
            _logger = logger;
            _dao = dao;
            _notificacionesService = notificacionesService;
        }

        /// <summary>
        /// Actualiza el estado de un viático a "Solicitado" (ID: 1)
        /// </summary>
        /// <param name="sviaticoId">ID del viático</param>
        /// <param name="request">Datos adicionales de la solicitud</param>
        /// <returns>Viático con estado actualizado</returns>
        [HttpPut("{sviaticoId}/solicitar")]
        [ProducesResponseType(typeof(ApiResponse<SviaticosCabeceraDTOResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<SviaticosCabeceraDTOResponse>>> SolicitarViatico(
            [FromRoute] int sviaticoId,
            [FromBody] ActualizarEstadoRequestDto request)
        {
            try
            {
                _logger.LogInformation("Solicitando viático ID: {SviaticoId}", sviaticoId);

                var result = await _sviaticoService.ActualizarEstadoSolicitud(sviaticoId, 1, request.Comentario);

                if (result.Success)
                {
                    _logger.LogInformation("Viático {SviaticoId} actualizado a estado 'Solicitado' exitosamente", sviaticoId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error al solicitar viático {SviaticoId}: {Message}", sviaticoId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al solicitar viático {SviaticoId}", sviaticoId);
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Actualiza el estado de un viático a "Abierto" (ID: 2)
        /// </summary>
        /// <param name="sviaticoId">ID del viático</param>
        /// <param name="request">Datos adicionales de la solicitud</param>
        /// <returns>Viático con estado actualizado</returns>
        [HttpPut("{sviaticoId}/abrir")]
        [ProducesResponseType(typeof(ApiResponse<SviaticosCabeceraDTOResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<SviaticosCabeceraDTOResponse>>> AbrirViatico(
            [FromRoute] int sviaticoId,
            [FromBody] ActualizarEstadoRequestDto request)
        {
            try
            {
                _logger.LogInformation("Abriendo viático ID: {SviaticoId}", sviaticoId);

                var result = await _sviaticoService.ActualizarEstadoSolicitud(sviaticoId, 2, request.Comentario);

                if (result.Success)
                {
                    _logger.LogInformation("Viático {SviaticoId} actualizado a estado 'Abierto' exitosamente", sviaticoId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error al abrir viático {SviaticoId}: {Message}", sviaticoId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al abrir viático {SviaticoId}", sviaticoId);
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Actualiza el estado de un viático a "Aprobado" (ID: 3)
        /// </summary>
        /// <param name="sviaticoId">ID del viático</param>
        /// <param name="request">Datos adicionales de la aprobación</param>
        /// <returns>Viático con estado actualizado</returns>
        [HttpPut("{sviaticoId}/aprobar")]
        [ProducesResponseType(typeof(ApiResponse<SviaticosCabeceraDTOResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<SviaticosCabeceraDTOResponse>>> AprobarViatico(
            [FromRoute] int sviaticoId,
            [FromBody] ActualizarEstadoRequestDto request)
        {
            try
            {
                _logger.LogInformation("Aprobando viático ID: {SviaticoId}", sviaticoId);

                var result = await _sviaticoService.ActualizarEstadoSolicitud(sviaticoId, 3, request.Comentario);

                if (result.Success)
                {
                    _logger.LogInformation("Viático {SviaticoId} aprobado exitosamente", sviaticoId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error al aprobar viático {SviaticoId}: {Message}", sviaticoId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al aprobar viático {SviaticoId}", sviaticoId);
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Actualiza el estado de un viático a "Rechazado" (ID: 4)
        /// </summary>
        /// <param name="sviaticoId">ID del viático</param>
        /// <param name="request">Datos del rechazo (comentario requerido)</param>
        /// <returns>Viático con estado actualizado</returns>
        [HttpPut("{sviaticoId}/rechazar")]
        [ProducesResponseType(typeof(ApiResponse<SviaticosCabeceraDTOResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<SviaticosCabeceraDTOResponse>>> RechazarViatico(
            [FromRoute] int sviaticoId,
            [FromBody] RechazarViaticoRequestDto request)
        {
            try
            {
                _logger.LogInformation("Rechazando viático ID: {SviaticoId}", sviaticoId);

                if (string.IsNullOrWhiteSpace(request.MotivoRechazo))
                {
                    return BadRequest(new ApiResponse<object>("El motivo del rechazo es requerido"));
                }

                var result = await _sviaticoService.ActualizarEstadoSolicitud(sviaticoId, 4, request.MotivoRechazo);

                if (result.Success)
                {
                    _logger.LogInformation("Viático {SviaticoId} rechazado exitosamente. Motivo: {Motivo}", 
                        sviaticoId, request.MotivoRechazo);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error al rechazar viático {SviaticoId}: {Message}", sviaticoId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al rechazar viático {SviaticoId}", sviaticoId);
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        [HttpPut("{sviaticoId}/EnviarRendicion")]
        [ProducesResponseType(typeof(ApiResponse<SviaticosCabeceraDTOResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<SviaticosCabeceraDTOResponse>>> RendirViatico(
          [FromRoute] int sviaticoId,
          [FromBody] PagarViaticoRequestDto request)
        {
            try
            {
                _logger.LogInformation("Enviando viático ID: {SviaticoId} a rendir", sviaticoId);

                var cabecera = await _dao.GetSviaticosCabecera(sviaticoId);

                var comentario = $"Enviado a rendir";
                if (!string.IsNullOrWhiteSpace(cabecera.SvNumero))
                {
                    comentario += $" - Operación: {cabecera.SvNumero}";
                }
                if (!string.IsNullOrWhiteSpace(request.Comentario))
                {
                    comentario += $" - {request.Comentario}";
                }

                var result = await _sviaticoService.ActualizarEstadoSolicitud(sviaticoId, 7 , comentario);

                if (result.Success)
                {
                    _logger.LogInformation("Viático {SviaticoId} enviado a Rendir", sviaticoId);
                     
                    var createDto = new NotificacionCreateDto()
                    {
                        CodUsuReceptor = cabecera.SvEmpDni ?? string.Empty,
                        UsuarioReceptor = null,
                        CodUsuValidador = null,
                        UsuarioValidador = null,
                        Mensaje = $"Solicitud #{cabecera.SvId} - la rendición ha sido enviada a aprobar ",
                        Leido = false,
                        EstadoFlujo = 7
                    };

                    var responseTMP = await _notificacionesService.CreateAsync(createDto);


                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error al enviar el viático {SviaticoId}: {Message} a rendir", sviaticoId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al enviar a rendir el viático {SviaticoId}", sviaticoId);
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }



        [HttpPut("{sviaticoId}/pagar")]
        [ProducesResponseType(typeof(ApiResponse<SviaticosCabeceraDTOResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<SviaticosCabeceraDTOResponse>>> PagarViatico(
            [FromRoute] int sviaticoId,
            [FromBody] PagarViaticoRequestDto request)
        {
            try
            {
                _logger.LogInformation("Marcando pago efectuado para viático ID: {SviaticoId}", sviaticoId);

                var comentario = $"Pago efectuado";
                if (!string.IsNullOrWhiteSpace(request.NumeroOperacion))
                {
                    comentario += $" - Operación: {request.NumeroOperacion}";
                }
                if (!string.IsNullOrWhiteSpace(request.Comentario))
                {
                    comentario += $" - {request.Comentario}";
                }

                var result = await _sviaticoService.ActualizarEstadoSolicitud(sviaticoId, 5, comentario);

                if (result.Success)
                {
                    _logger.LogInformation("Viático {SviaticoId} marcado como pagado exitosamente", sviaticoId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error al marcar pago para viático {SviaticoId}: {Message}", sviaticoId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al marcar pago para viático {SviaticoId}", sviaticoId);
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene el estado actual de un viático
        /// </summary>
        /// <param name="sviaticoId">ID del viático</param>
        /// <returns>Estado actual del viático</returns>
        [HttpGet("{sviaticoId}/estado-actual")]
        [ProducesResponseType(typeof(ApiResponse<SolicitudEstadoFlujo>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<SolicitudEstadoFlujo>>> ObtenerEstadoActual(
            [FromRoute] int sviaticoId)
        {
            try
            {
                _logger.LogInformation("Obteniendo estado actual del viático ID: {SviaticoId}", sviaticoId);

                var result = await _sviaticoService.GetEstadoActual(sviaticoId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error al obtener estado del viático {SviaticoId}: {Message}", sviaticoId, result.Message);
                    return NotFound(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener estado del viático {SviaticoId}", sviaticoId);
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene todos los estados disponibles para viáticos
        /// </summary>
        /// <returns>Lista de estados disponibles</returns>
        [HttpGet("estados-disponibles")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SolicitudEstadoFlujo>>), 200)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SolicitudEstadoFlujo>>>> ObtenerEstadosDisponibles()
        {
            try
            {
                _logger.LogInformation("Obteniendo estados disponibles para viáticos");

                var result = await _sviaticoService.GetEstadosDisponibles();

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Error al obtener estados disponibles: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener estados disponibles");
                return StatusCode(500, new ApiResponse<object>("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene el historial de cambios de estado de un viático (endpoint futuro)
        /// </summary>
        /// <param name="sviaticoId">ID del viático</param>
        /// <returns>Historial de cambios de estado</returns>
        [HttpGet("{sviaticoId}/historial-estados")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 501)]
        public ActionResult<ApiResponse<object>> ObtenerHistorialEstados([FromRoute] int sviaticoId)
        {
            // Endpoint para implementación futura del historial de cambios
            return StatusCode(501, new ApiResponse<object>("Funcionalidad pendiente de implementación"));
        }
    }

    /// <summary>
    /// DTO para actualizar estado de viático
    /// </summary>
    public class ActualizarEstadoRequestDto
    {
        /// <summary>
        /// Comentario opcional sobre el cambio de estado
        /// </summary>
        public string? Comentario { get; set; }
    }

    /// <summary>
    /// DTO para rechazar un viático
    /// </summary>
    public class RechazarViaticoRequestDto
    {
        /// <summary>
        /// Motivo del rechazo (requerido)
        /// </summary>
        [Required(ErrorMessage = "El motivo del rechazo es requerido")]
        [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        public string MotivoRechazo { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para marcar pago efectuado
    /// </summary>
    public class PagarViaticoRequestDto
    {
        /// <summary>
        /// Número de operación bancaria o referencia del pago
        /// </summary>
        [StringLength(100, ErrorMessage = "El número de operación no puede exceder 100 caracteres")]
        public string? NumeroOperacion { get; set; }

        /// <summary>
        /// Comentario adicional sobre el pago
        /// </summary>
        [StringLength(300, ErrorMessage = "El comentario no puede exceder 300 caracteres")]
        public string? Comentario { get; set; }
    }
}