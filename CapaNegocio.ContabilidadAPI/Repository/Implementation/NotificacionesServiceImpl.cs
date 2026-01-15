using AutoMapper;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Implementación del servicio de Notificaciones
    /// </summary>
    public class NotificacionesServiceImpl : INotificacionesService
    {
        private readonly INotificacionDao _notificacionDao;
        private readonly IMapper _mapper;

        public NotificacionesServiceImpl(INotificacionDao notificacionDao, IMapper mapper)
        {
            _notificacionDao = notificacionDao;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene todas las notificaciones activas
        /// </summary>
        public async Task<ApiResponse<List<NotificacionDto>>> GetAllAsync()
        {
            try
            {
                var notificaciones = await _notificacionDao.GetAllAsync();
                var notificacionesDto = _mapper.Map<List<NotificacionDto>>(notificaciones);

                return new ApiResponse<List<NotificacionDto>>(notificacionesDto, "Notificaciones obtenidas correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<NotificacionDto>>(null, $"Error al obtener notificaciones: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene una notificación por su ID
        /// </summary>
        public async Task<ApiResponse<NotificacionDto>> GetByIdAsync(int id)
        {
            try
            {
                var notificacion = await _notificacionDao.GetByIdAsync(id);
                if (notificacion == null)
                {
                    return new ApiResponse<NotificacionDto>(null, "Notificación no encontrada");
                }

                var notificacionDto = _mapper.Map<NotificacionDto>(notificacion);
                return new ApiResponse<NotificacionDto>(notificacionDto, "Notificación obtenida correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<NotificacionDto>(null, $"Error al obtener notificación: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea una nueva notificación
        /// </summary>
        public async Task<ApiResponse<NotificacionDto>> CreateAsync(NotificacionCreateDto createDto)
        {
            try
            {
                var notificacion = _mapper.Map<Notificacion>(createDto);
                var notificacionCreada = await _notificacionDao.CreateAsync(notificacion);

                var notificacionDto = _mapper.Map<NotificacionDto>(notificacionCreada);
                return new ApiResponse<NotificacionDto>(notificacionDto, "Notificación creada correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<NotificacionDto>(null, $"Error al crear notificación: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza una notificación existente
        /// </summary>
        public async Task<ApiResponse<NotificacionDto>> UpdateAsync(int id, NotificacionUpdateDto updateDto)
        {
            try
            {
                var notificacionExistente = await _notificacionDao.GetByIdAsync(id);
                if (notificacionExistente == null)
                {
                    return new ApiResponse<NotificacionDto>(null, "Notificación no encontrada");
                }

                // Mapear solo los campos que no son null en el DTO
                if (!string.IsNullOrWhiteSpace(updateDto.CodUsuReceptor))
                    notificacionExistente.CodUsuReceptor = updateDto.CodUsuReceptor;

                if (!string.IsNullOrWhiteSpace(updateDto.UsuarioReceptor))
                    notificacionExistente.UsuarioReceptor = updateDto.UsuarioReceptor;

                if (!string.IsNullOrWhiteSpace(updateDto.CodUsuValidador))
                    notificacionExistente.CodUsuValidador = updateDto.CodUsuValidador;

                if (!string.IsNullOrWhiteSpace(updateDto.UsuarioValidador))
                    notificacionExistente.UsuarioValidador = updateDto.UsuarioValidador;

                if (!string.IsNullOrWhiteSpace(updateDto.Mensaje))
                    notificacionExistente.Mensaje = updateDto.Mensaje;

                if (updateDto.Leido.HasValue)
                    notificacionExistente.Leido = updateDto.Leido.Value;

                var notificacionActualizada = await _notificacionDao.UpdateAsync(notificacionExistente);
                var notificacionDto = _mapper.Map<NotificacionDto>(notificacionActualizada);

                return new ApiResponse<NotificacionDto>(notificacionDto, "Notificación actualizada correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<NotificacionDto>(null, $"Error al actualizar notificación: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina lógicamente una notificación
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var eliminada = await _notificacionDao.DeleteAsync(id);
                if (!eliminada)
                {
                    return new ApiResponse<bool>(false, "Notificación no encontrada");
                }

                return new ApiResponse<bool>(true, "Notificación eliminada correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, $"Error al eliminar notificación: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene notificaciones por código de usuario receptor
        /// </summary>
        public async Task<ApiResponse<List<NotificacionDto>>> GetByCodUsuReceptorAsync(string codUsuReceptor)
        {
            try
            {
                var notificaciones = await _notificacionDao.GetByCodUsuReceptorAsync(codUsuReceptor);
                var notificacionesDto = _mapper.Map<List<NotificacionDto>>(notificaciones);

                return new ApiResponse<List<NotificacionDto>>(notificacionesDto, 
                    $"Notificaciones obtenidas para usuario {codUsuReceptor}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<NotificacionDto>>(null, 
                    $"Error al obtener notificaciones del usuario: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene notificaciones por código de usuario validador
        /// </summary>
        public async Task<ApiResponse<List<NotificacionDto>>> GetByCodUsuValidadorAsync(string codUsuValidador)
        {
            try
            {
                var notificaciones = await _notificacionDao.GetByCodUsuValidadorAsync(codUsuValidador);
                var notificacionesDto = _mapper.Map<List<NotificacionDto>>(notificaciones);

                return new ApiResponse<List<NotificacionDto>>(notificacionesDto,
                    $"Notificaciones obtenidas para validador {codUsuValidador}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<NotificacionDto>>(null,
                    $"Error al obtener notificaciones del validador: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene notificaciones no leídas por usuario receptor
        /// </summary>
        public async Task<ApiResponse<List<NotificacionDto>>> GetNoLeidasByCodUsuReceptorAsync(string codUsuReceptor)
        {
            try
            {
                var notificaciones = await _notificacionDao.GetNoLeidasByCodUsuReceptorAsync(codUsuReceptor);
                var notificacionesDto = _mapper.Map<List<NotificacionDto>>(notificaciones);

                return new ApiResponse<List<NotificacionDto>>(notificacionesDto,
                    $"Notificaciones no leídas obtenidas para usuario {codUsuReceptor}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<NotificacionDto>>(null,
                    $"Error al obtener notificaciones no leídas: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene notificaciones filtradas con paginación
        /// </summary>
        public async Task<ApiResponse<object>> GetFilteredAsync(NotificacionFiltroDto filtro)
        {
            try
            {
                var (notificaciones, total) = await _notificacionDao.GetFilteredAsync(
                    filtro.CodUsuReceptor,
                    filtro.CodUsuValidador,
                    filtro.Leido,
                    filtro.Activo,
                    filtro.FechaDesde,
                    filtro.FechaHasta,
                    filtro.TextoBusqueda,
                    filtro.Pagina,
                    filtro.TamanoPagina);

                var notificacionesDto = _mapper.Map<List<NotificacionDto>>(notificaciones);

                var resultado = new
                {
                    Notificaciones = notificacionesDto,
                    TotalRegistros = total,
                    PaginaActual = filtro.Pagina,
                    TamanoPagina = filtro.TamanoPagina,
                    TotalPaginas = (int)Math.Ceiling((double)total / filtro.TamanoPagina),
                    TieneAnterior = filtro.Pagina > 1,
                    TieneSiguiente = filtro.Pagina < Math.Ceiling((double)total / filtro.TamanoPagina)
                };

                return new ApiResponse<object>(resultado, "Notificaciones filtradas obtenidas correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>(null, $"Error al filtrar notificaciones: {ex.Message}");
            }
        }

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        public async Task<ApiResponse<bool>> MarcarComoLeidaAsync(int id)
        {
            try
            {
                var marcada = await _notificacionDao.MarcarComoLeidaAsync(id);
                if (!marcada)
                {
                    return new ApiResponse<bool>(false, "Notificación no encontrada");
                }

                return new ApiResponse<bool>(true, "Notificación marcada como leída");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, $"Error al marcar notificación como leída: {ex.Message}");
            }
        }

        /// <summary>
        /// Marca múltiples notificaciones como leídas
        /// </summary>
        public async Task<ApiResponse<int>> MarcarVariasComoLeidasAsync(MarcarLeidoDto marcarLeidoDto)
        {
            try
            {
                var actualizadas = await _notificacionDao.MarcarVariasComoLeidasAsync(marcarLeidoDto.NotificacionIds);

                return new ApiResponse<int>(actualizadas, 
                    $"{actualizadas} notificaciones marcadas como leídas");
            }
            catch (Exception ex)
            {
                return new ApiResponse<int>(0, $"Error al marcar notificaciones como leídas: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el número de notificaciones no leídas por usuario
        /// </summary>
        public async Task<ApiResponse<int>> GetCountNoLeidasAsync(string codUsuReceptor)
        {
            try
            {
                var count = await _notificacionDao.GetCountNoLeidasAsync(codUsuReceptor);

                return new ApiResponse<int>(count, $"Conteo de notificaciones no leídas para usuario {codUsuReceptor}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<int>(0, $"Error al obtener conteo de notificaciones: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el resumen de notificaciones para un usuario
        /// </summary>
        public async Task<ApiResponse<object>> GetResumenNotificacionesAsync(string codUsuReceptor)
        {
            try
            {
                var todasLasNotificaciones = await _notificacionDao.GetByCodUsuReceptorAsync(codUsuReceptor);
                var noLeidas = await _notificacionDao.GetCountNoLeidasAsync(codUsuReceptor);
                var recientes = todasLasNotificaciones.Take(5).ToList(); // Últimas 5

                var recientesDto = _mapper.Map<List<NotificacionDto>>(recientes);

                var resumen = new
                {
                    TotalNotificaciones = todasLasNotificaciones.Count,
                    NotificacionesNoLeidas = noLeidas,
                    NotificacionesRecientes = recientesDto
                };

                return new ApiResponse<object>(resumen, $"Resumen de notificaciones para usuario {codUsuReceptor}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>(null, $"Error al obtener resumen de notificaciones: {ex.Message}");
            }
        }
    }
}
