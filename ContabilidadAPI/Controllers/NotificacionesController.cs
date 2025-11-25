using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ContabilidadAPI.Controllers
{
    /// <summary>
    /// Controlador para el manejo de notificaciones del sistema
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class NotificacionesController : ControllerBase
    {
        private readonly INotificacionesService _notificacionesService;
        private readonly ILogger<NotificacionesController> _logger;

        public NotificacionesController(
            INotificacionesService notificacionesService,
            ILogger<NotificacionesController> logger)
        {
            _notificacionesService = notificacionesService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las notificaciones activas
        /// </summary>
        /// <returns>Lista de notificaciones activas</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<NotificacionDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<NotificacionDto>>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las notificaciones activas");

                var response = await _notificacionesService.GetAllAsync();

                if (response.Success)
                {
                    _logger.LogInformation($"Se obtuvieron {response.Data?.Count ?? 0} notificaciones");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al obtener notificaciones: {response.Message}");
                    return StatusCode(500, new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener notificaciones");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene una notificación específica por ID
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <returns>Notificación encontrada</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<NotificacionDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<NotificacionDto>>> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"Obteniendo notificación con ID: {id}");

                var response = await _notificacionesService.GetByIdAsync(id);

                if (response.Success && response.Data != null)
                {
                    _logger.LogInformation($"Notificación {id} obtenida correctamente");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Notificación {id} no encontrada");
                    return NotFound(new ApiResponse<object>(null, "Notificación no encontrada"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener notificación {id}");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Crea una nueva notificación
        /// </summary>
        /// <param name="createDto">Datos de la notificación a crear</param>
        /// <returns>Notificación creada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NotificacionDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<NotificacionDto>>> Create([FromBody] NotificacionCreateDto createDto)
        {
            try
            {
                _logger.LogInformation($"Creando notificación para usuario: {createDto.CodUsuReceptor}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(null, "Datos de solicitud inválidos"));
                }

                var response = await _notificacionesService.CreateAsync(createDto);

                if (response.Success)
                {
                    _logger.LogInformation($"Notificación creada con ID: {response.Data?.Id}");
                    return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
                }
                else
                {
                    _logger.LogWarning($"Error al crear notificación: {response.Message}");
                    return StatusCode(500, new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear notificación");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Actualiza una notificación existente
        /// </summary>
        /// <param name="id">ID de la notificación a actualizar</param>
        /// <param name="updateDto">Datos actualizados</param>
        /// <returns>Notificación actualizada</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<NotificacionDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<NotificacionDto>>> Update(int id, [FromBody] NotificacionUpdateDto updateDto)
        {
            try
            {
                _logger.LogInformation($"Actualizando notificación con ID: {id}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(null, "Datos de solicitud inválidos"));
                }

                var response = await _notificacionesService.UpdateAsync(id, updateDto);

                if (response.Success && response.Data != null)
                {
                    _logger.LogInformation($"Notificación {id} actualizada correctamente");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al actualizar notificación {id}: {response.Message}");
                    return NotFound(new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al actualizar notificación {id}");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Elimina lógicamente una notificación
        /// </summary>
        /// <param name="id">ID de la notificación a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando notificación con ID: {id}");

                var response = await _notificacionesService.DeleteAsync(id);

                if (response.Success)
                {
                    _logger.LogInformation($"Notificación {id} eliminada correctamente");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al eliminar notificación {id}: {response.Message}");
                    return NotFound(new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al eliminar notificación {id}");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene notificaciones por usuario receptor
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Lista de notificaciones del usuario</returns>
        [HttpGet("usuario/{codUsuReceptor}")]
        [ProducesResponseType(typeof(ApiResponse<List<NotificacionDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<NotificacionDto>>>> GetByUsuarioReceptor(string codUsuReceptor)
        {
            try
            {
                _logger.LogInformation($"Obteniendo notificaciones para usuario: {codUsuReceptor}");

                var response = await _notificacionesService.GetByCodUsuReceptorAsync(codUsuReceptor);

                if (response.Success)
                {
                    _logger.LogInformation($"Se obtuvieron {response.Data?.Count ?? 0} notificaciones para usuario {codUsuReceptor}");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al obtener notificaciones para usuario {codUsuReceptor}: {response.Message}");
                    return StatusCode(500, new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener notificaciones para usuario {codUsuReceptor}");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene notificaciones por usuario validador
        /// </summary>
        /// <param name="codUsuValidador">Código del usuario validador</param>
        /// <returns>Lista de notificaciones del validador</returns>
        [HttpGet("validador/{codUsuValidador}")]
        [ProducesResponseType(typeof(ApiResponse<List<NotificacionDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<NotificacionDto>>>> GetByUsuarioValidador(string codUsuValidador)
        {
            try
            {
                _logger.LogInformation($"Obteniendo notificaciones para validador: {codUsuValidador}");

                var response = await _notificacionesService.GetByCodUsuValidadorAsync(codUsuValidador);

                if (response.Success)
                {
                    _logger.LogInformation($"Se obtuvieron {response.Data?.Count ?? 0} notificaciones para validador {codUsuValidador}");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al obtener notificaciones para validador {codUsuValidador}: {response.Message}");
                    return StatusCode(500, new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener notificaciones para validador {codUsuValidador}");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene notificaciones no leídas por usuario
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Lista de notificaciones no leídas</returns>
        [HttpGet("no-leidas/{codUsuReceptor}")]
        [ProducesResponseType(typeof(ApiResponse<List<NotificacionDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<NotificacionDto>>>> GetNoLeidasByUsuario(string codUsuReceptor)
        {
            try
            {
                _logger.LogInformation($"Obteniendo notificaciones no leídas para usuario: {codUsuReceptor}");

                var response = await _notificacionesService.GetNoLeidasByCodUsuReceptorAsync(codUsuReceptor);

                if (response.Success)
                {
                    _logger.LogInformation($"Se obtuvieron {response.Data?.Count ?? 0} notificaciones no leídas para usuario {codUsuReceptor}");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al obtener notificaciones no leídas para usuario {codUsuReceptor}: {response.Message}");
                    return StatusCode(500, new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener notificaciones no leídas para usuario {codUsuReceptor}");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene notificaciones filtradas con paginación
        /// </summary>
        /// <param name="filtro">Filtros a aplicar</param>
        /// <returns>Lista paginada de notificaciones</returns>
        [HttpPost("filtrar")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> GetFiltered([FromBody] NotificacionFiltroDto filtro)
        {
            try
            {
                _logger.LogInformation($"Filtrando notificaciones - Página: {filtro.Pagina}, Tamaño: {filtro.TamanoPagina}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(null, "Datos de filtro inválidos"));
                }

                var response = await _notificacionesService.GetFilteredAsync(filtro);

                if (response.Success)
                {
                    _logger.LogInformation("Notificaciones filtradas obtenidas correctamente");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al filtrar notificaciones: {response.Message}");
                    return StatusCode(500, new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al filtrar notificaciones");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <returns>Confirmación de actualización</returns>
        [HttpPatch("{id:int}/marcar-leida")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<bool>>> MarcarComoLeida(int id)
        {
            try
            {
                _logger.LogInformation($"Marcando notificación {id} como leída");

                var response = await _notificacionesService.MarcarComoLeidaAsync(id);

                if (response.Success)
                {
                    _logger.LogInformation($"Notificación {id} marcada como leída");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al marcar notificación {id} como leída: {response.Message}");
                    return NotFound(new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al marcar notificación {id} como leída");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Marca múltiples notificaciones como leídas
        /// </summary>
        /// <param name="marcarLeidoDto">IDs de notificaciones a marcar</param>
        /// <returns>Número de notificaciones actualizadas</returns>
        [HttpPatch("marcar-varias-leidas")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<int>>> MarcarVariasComoLeidas([FromBody] MarcarLeidoDto marcarLeidoDto)
        {
            try
            {
                _logger.LogInformation($"Marcando {marcarLeidoDto.NotificacionIds.Count} notificaciones como leídas");

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(null, "Datos de solicitud inválidos"));
                }

                var response = await _notificacionesService.MarcarVariasComoLeidasAsync(marcarLeidoDto);

                if (response.Success)
                {
                    _logger.LogInformation($"{response.Data} notificaciones marcadas como leídas");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al marcar notificaciones como leídas: {response.Message}");
                    return StatusCode(500, new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al marcar notificaciones como leídas");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene el conteo de notificaciones no leídas por usuario
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Número de notificaciones no leídas</returns>
        [HttpGet("count-no-leidas/{codUsuReceptor}")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<int>>> GetCountNoLeidas(string codUsuReceptor)
        {
            try
            {
                _logger.LogInformation($"Obteniendo conteo de notificaciones no leídas para usuario: {codUsuReceptor}");

                var response = await _notificacionesService.GetCountNoLeidasAsync(codUsuReceptor);

                if (response.Success)
                {
                    _logger.LogInformation($"Usuario {codUsuReceptor} tiene {response.Data} notificaciones no leídas");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al obtener conteo para usuario {codUsuReceptor}: {response.Message}");
                    return StatusCode(500, new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener conteo para usuario {codUsuReceptor}");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtiene el resumen de notificaciones para un usuario
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Resumen de notificaciones (total, no leídas, recientes)</returns>
        [HttpGet("resumen/{codUsuReceptor}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> GetResumenNotificaciones(string codUsuReceptor)
        {
            try
            {
                _logger.LogInformation($"Obteniendo resumen de notificaciones para usuario: {codUsuReceptor}");

                var response = await _notificacionesService.GetResumenNotificacionesAsync(codUsuReceptor);

                if (response.Success)
                {
                    _logger.LogInformation($"Resumen de notificaciones obtenido para usuario {codUsuReceptor}");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Error al obtener resumen para usuario {codUsuReceptor}: {response.Message}");
                    return StatusCode(500, new ApiResponse<object>(null, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener resumen para usuario {codUsuReceptor}");
                return StatusCode(500, new ApiResponse<object>(null, "Error interno del servidor"));
            }
        }
    }
}