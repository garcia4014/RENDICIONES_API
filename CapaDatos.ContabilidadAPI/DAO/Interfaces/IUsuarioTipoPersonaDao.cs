using CapaDatos.ContabilidadAPI.Models;

namespace CapaDatos.ContabilidadAPI.DAO.Interfaces
{
    public interface IUsuarioTipoPersonaDao
    {
        Task<IEnumerable<UsuarioTipoPersona>> GetAllAsync();
        Task<UsuarioTipoPersona?> GetByCodeAsync(string code);
        Task<(IEnumerable<UsuarioTipoPersona> Items, int TotalCount)> GetFilteredAsync(
            string? code = null, 
            int? tpId = null, 
            string? userSAP = null, 
            bool? activo = null,
            int pagina = 1, 
            int tamañoPagina = 10);
        Task<UsuarioTipoPersona> CreateAsync(UsuarioTipoPersona usuarioTipoPersona);
        Task<UsuarioTipoPersona?> UpdateAsync(UsuarioTipoPersona usuarioTipoPersona);
        Task<bool> DeleteAsync(string code);
        Task<bool> ExistsAsync(string code);
        Task<IEnumerable<UsuarioTipoPersona>> GetByTipoPersonaAsync(int tpId);
    }
}