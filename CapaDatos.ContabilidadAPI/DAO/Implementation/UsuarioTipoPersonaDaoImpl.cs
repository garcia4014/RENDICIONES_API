using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CapaDatos.ContabilidadAPI.DAO.Implementation
{
    public class UsuarioTipoPersonaDaoImpl : IUsuarioTipoPersonaDao
    {
        private readonly SvrendicionesContext _context;

        public UsuarioTipoPersonaDaoImpl(SvrendicionesContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UsuarioTipoPersona>> GetAllAsync()
        {
            try
            {
                return await _context.UsuarioTipoPersonas
                    .Include(u => u.TipoPersona)
                    .OrderBy(u => u.Code)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<UsuarioTipoPersona>();
            }
        }

        public async Task<UsuarioTipoPersona?> GetByCodeAsync(string code)
        {
            try
            {
                return await _context.UsuarioTipoPersonas
                    .Include(u => u.TipoPersona)
                    .FirstOrDefaultAsync(u => u.Code == code);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(IEnumerable<UsuarioTipoPersona> Items, int TotalCount)> GetFilteredAsync(
            string? code = null, 
            int? tpId = null, 
            string? userSAP = null, 
            bool? activo = null,
            int pagina = 1, 
            int tamañoPagina = 10)
        {
            try
            {
                var query = _context.UsuarioTipoPersonas
                    .Include(u => u.TipoPersona)
                    .Where(u => u.Code != null);

                // Aplicar filtros
                if (!string.IsNullOrEmpty(code))
                {
                    query = query.Where(u => u.Code.Contains(code));
                }

                if (tpId.HasValue)
                {
                    query = query.Where(u => u.TpId == tpId.Value);
                }

                if (!string.IsNullOrEmpty(userSAP))
                {
                    query = query.Where(u => u.UserSAP != null && u.UserSAP.Contains(userSAP));
                }

                if (activo.HasValue)
                {
                    query = query.Where(u => u.Activo == activo.Value);
                }
                
                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(u => u.Code)
                    .Skip((pagina - 1) * tamañoPagina)
                    .Take(tamañoPagina)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception)
            {
                return (new List<UsuarioTipoPersona>(), 0);
            }
        }

        public async Task<UsuarioTipoPersona> CreateAsync(UsuarioTipoPersona usuarioTipoPersona)
        {
            try
            {
                usuarioTipoPersona.FechaCreacion = DateTime.Now;
                usuarioTipoPersona.Activo = true;

                _context.UsuarioTipoPersonas.Add(usuarioTipoPersona);
                await _context.SaveChangesAsync();

                // Recargar con navegación
                return await _context.UsuarioTipoPersonas
                    .Include(u => u.TipoPersona)
                    .FirstAsync(u => u.Code == usuarioTipoPersona.Code);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<UsuarioTipoPersona?> UpdateAsync(UsuarioTipoPersona usuarioTipoPersona)
        {
            try
            {
                var existingEntity = await _context.UsuarioTipoPersonas
                    .FirstOrDefaultAsync(u => u.Code == usuarioTipoPersona.Code);

                if (existingEntity != null)
                {
                    existingEntity.TpId = usuarioTipoPersona.TpId;
                    existingEntity.UserSAP = usuarioTipoPersona.UserSAP;
                    existingEntity.Activo = usuarioTipoPersona.Activo;
                    existingEntity.FechaModificacion = DateTime.Now;

                    await _context.SaveChangesAsync();

                    // Recargar con navegación
                    return await _context.UsuarioTipoPersonas
                        .Include(u => u.TipoPersona)
                        .FirstAsync(u => u.Code == usuarioTipoPersona.Code);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteAsync(string code)
        {
            try
            {
                var entity = await _context.UsuarioTipoPersonas
                    .FirstOrDefaultAsync(u => u.Code == code);

                if (entity != null)
                {
                    // Eliminación lógica
                    entity.Activo = false;
                    entity.FechaModificacion = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string code)
        {
            try
            {
                return await _context.UsuarioTipoPersonas
                    .AnyAsync(u => u.Code == code);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<UsuarioTipoPersona>> GetByTipoPersonaAsync(int tpId)
        {
            try
            {
                return await _context.UsuarioTipoPersonas
                    .Include(u => u.TipoPersona)
                    .Where(u => u.TpId == tpId)
                    .OrderBy(u => u.Code)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<UsuarioTipoPersona>();
            }
        }
    }
}