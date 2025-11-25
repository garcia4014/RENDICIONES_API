using CapaDatos.ContabilidadAPI.DAO.Interfaces.Access;
using CapaDatos.ContabilidadAPI.Models.Access;
using Microsoft.EntityFrameworkCore;

namespace CapaDatos.ContabilidadAPI.DAO.Implementation.Access
{
    /// <summary>
    /// Implementación del DAO para Personal
    /// </summary>
    public class PersonalDaoImpl : IPersonalDao
    {
        private readonly AccessDBContext _context;

        public PersonalDaoImpl(AccessDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene personal por documento de identidad
        /// </summary>
        public async Task<Personal?> GetByIdDocumentoAsync(string idDocumento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idDocumento))
                    return null;

                return await _context.Personal
                    .FirstOrDefaultAsync(p => p.IdDocumento == idDocumento);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Obtiene personal filtrado con paginación
        /// </summary>
        public async Task<(List<Personal> personal, int totalRegistros)> GetPersonalFiltradoAsync(
            string? nombres = null,
            string? idDocumento = null,
            string? empresa = null,
            string? usrSidige = null,
            int pagina = 1,
            int tamanoPagina = 10)
        {
            try
            {
                var query = _context.Personal.AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrWhiteSpace(idDocumento))
                {
                    query = query.Where(p => p.IdDocumento.Contains(idDocumento));
                }

                if (!string.IsNullOrWhiteSpace(nombres))
                {
                    query = query.Where(p => p.Nombres.Contains(nombres));
                }
                 

                // Obtener total de registros antes de la paginación
                var totalRegistros = await query.CountAsync();

                // Aplicar paginación
                var personal = await query
                    .OrderBy(p => p.Nombres)
                    .Skip((pagina - 1) * tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();

                return (personal, totalRegistros);
            }
            catch (Exception)
            {
                return (new List<Personal>(), 0);
            }
        }

        /// <summary>
        /// Obtiene personal que coincida con los nombres (búsqueda parcial)
        /// </summary>
        public async Task<List<Personal>> GetByNombresAsync(string nombres)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombres))
                    return new List<Personal>();

                return await _context.Personal
                    .Where(p => p.Nombres.Contains(nombres))
                    .OrderBy(p => p.Nombres)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Personal>();
            }
        }

        /// <summary>
        /// Verifica si existe personal con el documento especificado
        /// </summary>
        public async Task<bool> ExistePersonalAsync(string idDocumento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idDocumento))
                    return false;

                return await _context.Personal
                    .AnyAsync(p => p.IdDocumento == idDocumento);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}