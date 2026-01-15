using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CapaDatos.ContabilidadAPI.DAO.Implementation
{
    /// <summary>
    /// Implementación del DAO para Notificaciones
    /// </summary>
    public class NotificacionDaoImpl : INotificacionDao
    {
        private readonly SvrendicionesContext _context;

        public NotificacionDaoImpl(SvrendicionesContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todas las notificaciones activas
        /// </summary>
        public async Task<List<Notificacion>> GetAllAsync()
        {
            try
            {
                return await _context.Notificaciones
                    .Where(n => n.Activo == true)
                    .OrderByDescending(n => n.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener todas las notificaciones: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene una notificación por su ID
        /// </summary>
        public async Task<Notificacion?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Notificaciones
                    .Where(n => n.Id == id && n.Activo == true)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener notificación por ID {id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea una nueva notificación
        /// </summary>
        public async Task<Notificacion> CreateAsync(Notificacion notificacion)
        {
            try
            {
                // Configurar valores por defecto
                notificacion.FechaCreacion = DateTime.Now;
                notificacion.Activo = true;
                notificacion.Leido = notificacion.Leido ?? false;

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                return notificacion;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear notificación: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza una notificación existente
        /// </summary>
        public async Task<Notificacion> UpdateAsync(Notificacion notificacion)
        {
            try
            {
                _context.Entry(notificacion).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return notificacion;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar notificación: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina lógicamente una notificación
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var notificacion = await _context.Notificaciones
                    .Where(n => n.Id == id && n.Activo == true)
                    .FirstOrDefaultAsync();

                if (notificacion == null)
                    return false;

                notificacion.Activo = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar notificación {id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene notificaciones por código de usuario receptor
        /// </summary>
        public async Task<List<Notificacion>> GetByCodUsuReceptorAsync(string codUsuReceptor)
        {
            try
            {
                return await _context.Notificaciones
                    .Where(n => n.CodUsuReceptor == codUsuReceptor && n.Activo == true)
                    .OrderByDescending(n => n.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener notificaciones por usuario receptor {codUsuReceptor}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene notificaciones por código de usuario validador
        /// </summary>
        public async Task<List<Notificacion>> GetByCodUsuValidadorAsync(string codUsuValidador)
        {
            try
            {
                return await _context.Notificaciones
                    .Where(n => n.CodUsuValidador == codUsuValidador && n.Activo == true)
                    .OrderByDescending(n => n.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener notificaciones por usuario validador {codUsuValidador}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene notificaciones no leídas por usuario receptor
        /// </summary>
        public async Task<List<Notificacion>> GetNoLeidasByCodUsuReceptorAsync(string codUsuReceptor)
        {
            try
            {
                return await _context.Notificaciones
                    .Where(n => n.CodUsuReceptor == codUsuReceptor && 
                               n.Activo == true && 
                               n.Leido == false)
                    .OrderByDescending(n => n.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener notificaciones no leídas para usuario {codUsuReceptor}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene notificaciones filtradas con paginación
        /// </summary>
        public async Task<(List<Notificacion> notificaciones, int total)> GetFilteredAsync(
            string? codUsuReceptor = null,
            string? codUsuValidador = null,
            bool? leido = null,
            bool? activo = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string? textoBusqueda = null,
            int pagina = 1,
            int tamanoPagina = 10)
        {
            try
            {
                var query = _context.Notificaciones.AsQueryable();

                // Aplicar filtros
                if (activo.HasValue)
                    query = query.Where(n => n.Activo == activo.Value);
                else
                    query = query.Where(n => n.Activo == true); // Por defecto solo activas

                if (!string.IsNullOrWhiteSpace(codUsuReceptor))
                    query = query.Where(n => n.CodUsuReceptor == codUsuReceptor);

                if (!string.IsNullOrWhiteSpace(codUsuValidador))
                    query = query.Where(n => n.CodUsuValidador == codUsuValidador);

                if (leido.HasValue)
                    query = query.Where(n => n.Leido == leido.Value);

                if (fechaDesde.HasValue)
                    query = query.Where(n => n.FechaCreacion >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                    query = query.Where(n => n.FechaCreacion <= fechaHasta.Value);

                if (!string.IsNullOrWhiteSpace(textoBusqueda))
                {
                    query = query.Where(n => n.Mensaje!.Contains(textoBusqueda) ||
                                           n.UsuarioReceptor!.Contains(textoBusqueda) ||
                                           n.UsuarioValidador!.Contains(textoBusqueda));
                }

                // Obtener total antes de paginar
                var total = await query.CountAsync();

                // Aplicar paginación y ordenamiento
                var notificaciones = await query
                    .OrderByDescending(n => n.FechaCreacion)
                    .Skip((pagina - 1) * tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();

                return (notificaciones, total);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener notificaciones filtradas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        public async Task<bool> MarcarComoLeidaAsync(int id)
        {
            try
            {
                var notificacion = await _context.Notificaciones
                    .Where(n => n.Id == id && n.Activo == true)
                    .FirstOrDefaultAsync();

                if (notificacion == null)
                    return false;

                notificacion.Leido = true;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al marcar notificación {id} como leída: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Marca múltiples notificaciones como leídas
        /// </summary>
        public async Task<int> MarcarVariasComoLeidasAsync(List<int> ids)
        {
            try
            {
                var notificaciones = await _context.Notificaciones
                    .Where(n => ids.Contains(n.Id) && n.Activo == true)
                    .ToListAsync();

                foreach (var notificacion in notificaciones)
                {
                    notificacion.Leido = true;
                }

                await _context.SaveChangesAsync();
                return notificaciones.Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al marcar notificaciones como leídas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el número de notificaciones no leídas por usuario
        /// </summary>
        public async Task<int> GetCountNoLeidasAsync(string codUsuReceptor)
        {
            try
            {
                return await _context.Notificaciones
                    .Where(n => n.CodUsuReceptor == codUsuReceptor && 
                               n.Activo == true && 
                               n.Leido == false)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener conteo de notificaciones no leídas para usuario {codUsuReceptor}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si una notificación existe y está activa
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _context.Notificaciones
                    .AnyAsync(n => n.Id == id && n.Activo == true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar existencia de notificación {id}: {ex.Message}", ex);
            }
        }
    }
}