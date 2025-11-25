using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    public interface IUsuarioTipoPersonaService
    {
        Task<ApiResponse<IEnumerable<UsuarioTipoPersonaDto>>> GetAllAsync();
        Task<ApiResponse<UsuarioTipoPersonaDto>> GetByCodeAsync(string code);
        Task<ApiResponse<PaginatedResult<UsuarioTipoPersonaDto>>> GetFilteredAsync(UsuarioTipoPersonaFiltroDto filtro);
        Task<ApiResponse<UsuarioTipoPersonaDto>> CreateAsync(UsuarioTipoPersonaCreateDto createDto);
        Task<ApiResponse<UsuarioTipoPersonaDto>> UpdateAsync(string code, UsuarioTipoPersonaUpdateDto updateDto);
        Task<ApiResponse<bool>> DeleteAsync(string code);
        Task<ApiResponse<bool>> ExistsAsync(string code);
        Task<ApiResponse<IEnumerable<UsuarioTipoPersonaDto>>> GetByTipoPersonaAsync(int tpId);
    }
}