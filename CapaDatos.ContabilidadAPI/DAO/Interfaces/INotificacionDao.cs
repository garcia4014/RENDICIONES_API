using CapaDatos.ContabilidadAPI.Models;

namespace CapaDatos.ContabilidadAPI.DAO.Interfaces
{
    /// <summary>
    /// Interfaz para el acceso a datos de Notificaciones
    /// </summary>
    public interface INotificacionDao
    {
        /// <summary>
        /// Obtiene todas las notificaciones activas
        /// </summary>
        /// <returns>Lista de notificaciones activas</returns>
        Task<List<Notificacion>> GetAllAsync();

        /// <summary>
        /// Obtiene una notificación por su ID
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <returns>Notificación encontrada o null</returns>
        Task<Notificacion?> GetByIdAsync(int id);

        /// <summary>
        /// Crea una nueva notificación
        /// </summary>
        /// <param name="notificacion">Datos de la notificación a crear</param>
        /// <returns>Notificación creada</returns>
        Task<Notificacion> CreateAsync(Notificacion notificacion);

        /// <summary>
        /// Actualiza una notificación existente
        /// </summary>
        /// <param name="notificacion">Datos actualizados de la notificación</param>
        /// <returns>Notificación actualizada</returns>
        Task<Notificacion> UpdateAsync(Notificacion notificacion);

        /// <summary>
        /// Elimina lógicamente una notificación (marca Activo = false)
        /// </summary>
        /// <param name="id">ID de la notificación a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Obtiene notificaciones por código de usuario receptor
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Lista de notificaciones del usuario</returns>
        Task<List<Notificacion>> GetByCodUsuReceptorAsync(string codUsuReceptor);

        /// <summary>
        /// Obtiene notificaciones por código de usuario validador
        /// </summary>
        /// <param name="codUsuValidador">Código del usuario validador</param>
        /// <returns>Lista de notificaciones del validador</returns>
        Task<List<Notificacion>> GetByCodUsuValidadorAsync(string codUsuValidador);

        /// <summary>
        /// Obtiene notificaciones no leídas por usuario receptor
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Lista de notificaciones no leídas</returns>
        Task<List<Notificacion>> GetNoLeidasByCodUsuReceptorAsync(string codUsuReceptor);

        /// <summary>
        /// Obtiene notificaciones filtradas con paginación
        /// </summary>
        /// <param name="codUsuReceptor">Filtro por usuario receptor</param>
        /// <param name="codUsuValidador">Filtro por usuario validador</param>
        /// <param name="leido">Filtro por estado de lectura</param>
        /// <param name="activo">Filtro por estado activo</param>
        /// <param name="fechaDesde">Filtro fecha desde</param>
        /// <param name="fechaHasta">Filtro fecha hasta</param>
        /// <param name="textoBusqueda">Búsqueda en mensaje</param>
        /// <param name="pagina">Número de página</param>
        /// <param name="tamanoPagina">Tamaño de página</param>
        /// <returns>Tupla con lista de notificaciones y total de registros</returns>
        Task<(List<Notificacion> notificaciones, int total)> GetFilteredAsync(
            string? codUsuReceptor = null,
            string? codUsuValidador = null,
            bool? leido = null,
            bool? activo = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string? textoBusqueda = null,
            int pagina = 1,
            int tamanoPagina = 10);

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <returns>True si se marcó correctamente</returns>
        Task<bool> MarcarComoLeidaAsync(int id);

        /// <summary>
        /// Marca múltiples notificaciones como leídas
        /// </summary>
        /// <param name="ids">Lista de IDs de notificaciones</param>
        /// <returns>Número de notificaciones actualizadas</returns>
        Task<int> MarcarVariasComoLeidasAsync(List<int> ids);

        /// <summary>
        /// Obtiene el número de notificaciones no leídas por usuario
        /// </summary>
        /// <param name="codUsuReceptor">Código del usuario receptor</param>
        /// <returns>Número de notificaciones no leídas</returns>
        Task<int> GetCountNoLeidasAsync(string codUsuReceptor);

        /// <summary>
        /// Verifica si una notificación existe y está activa
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <returns>True si existe y está activa</returns>
        Task<bool> ExistsAsync(int id);
    }
}