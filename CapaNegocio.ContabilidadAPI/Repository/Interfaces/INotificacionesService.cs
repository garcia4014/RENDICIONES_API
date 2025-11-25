using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de Notificaciones
    /// </summary>
    public interface INotificacionesService
    {
        /// <summary>
        /// Obtiene todas las notificaciones activas
        /// </summary>
        /// <returns>Respuesta con lista de notificaciones</returns>
        Task<ApiResponse<List<NotificacionDto>>> GetAllAsync();

        /// <summary>
        /// Obtiene una notificación por su ID
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <returns>Respuesta con la notificación encontrada</returns>
        Task<ApiResponse<NotificacionDto>> GetByIdAsync(int id);

        /// <summary>
        /// Crea una nueva notificación
        /// </summary>
        /// <param name="createDto">Datos para crear la notificación</param>
        /// <returns>Respuesta con la notificación creada</returns>
        Task<ApiResponse<NotificacionDto>> CreateAsync(NotificacionCreateDto createDto);

        /// <summary>
        /// Actualiza una notificación existente
        /// </summary>
        /// <param name="id">ID de la notificación a actualizar</param>
        /// <param name="updateDto">Datos actualizados</param>
        /// <returns>Respuesta con la notificación actualizada</returns>
        Task<ApiResponse<NotificacionDto>> UpdateAsync(int id, NotificacionUpdateDto updateDto);

        /// <summary>
        /// Elimina lógicamente una notificación
        /// </summary>
        /// <param name="id">ID de la notificación a eliminar</param>
        /// <returns>Respuesta de confirmación</returns>
        Task<ApiResponse<bool>> DeleteAsync(int id);

        /// <summary>
        /// Obtiene notificaciones por código de usuario receptor
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Respuesta con lista de notificaciones del usuario</returns>
        Task<ApiResponse<List<NotificacionDto>>> GetByCodUsuReceptorAsync(string codUsuReceptor);

        /// <summary>
        /// Obtiene notificaciones por código de usuario validador
        /// </summary>
        /// <param name="codUsuValidador">Código del usuario validador</param>
        /// <returns>Respuesta con lista de notificaciones del validador</returns>
        Task<ApiResponse<List<NotificacionDto>>> GetByCodUsuValidadorAsync(string codUsuValidador);

        /// <summary>
        /// Obtiene notificaciones no leídas por usuario receptor
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Respuesta con lista de notificaciones no leídas</returns>
        Task<ApiResponse<List<NotificacionDto>>> GetNoLeidasByCodUsuReceptorAsync(string codUsuReceptor);

        /// <summary>
        /// Obtiene notificaciones filtradas con paginación
        /// </summary>
        /// <param name="filtro">Filtros a aplicar</param>
        /// <returns>Respuesta con lista paginada de notificaciones y metadatos</returns>
        Task<ApiResponse<object>> GetFilteredAsync(NotificacionFiltroDto filtro);

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <returns>Respuesta de confirmación</returns>
        Task<ApiResponse<bool>> MarcarComoLeidaAsync(int id);

        /// <summary>
        /// Marca múltiples notificaciones como leídas
        /// </summary>
        /// <param name="marcarLeidoDto">IDs de notificaciones a marcar</param>
        /// <returns>Respuesta con número de notificaciones actualizadas</returns>
        Task<ApiResponse<int>> MarcarVariasComoLeidasAsync(MarcarLeidoDto marcarLeidoDto);

        /// <summary>
        /// Obtiene el número de notificaciones no leídas por usuario
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Respuesta con el conteo de notificaciones no leídas</returns>
        Task<ApiResponse<int>> GetCountNoLeidasAsync(string codUsuReceptor);

        /// <summary>
        /// Obtiene el resumen de notificaciones para un usuario
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Respuesta con resumen (total, no leídas, recientes)</returns>
        Task<ApiResponse<object>> GetResumenNotificacionesAsync(string codUsuReceptor);
    }
}
