using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces.Access
{
    /// <summary>
    /// Interfaz del servicio para Personal
    /// </summary>
    public interface IPersonalService
    {
        /// <summary>
        /// Obtiene personal por documento de identidad
        /// </summary>
        /// <param name="idDocumento">Documento de identidad</param>
        /// <returns>Información del personal</returns>
        Task<ApiResponse<PersonalReadDto>> GetByIdDocumentoAsync(string idDocumento);

        /// <summary>
        /// Obtiene personal filtrado con paginación
        /// </summary>
        /// <param name="filtro">Filtros de búsqueda</param>
        /// <returns>Lista paginada de personal</returns>
        Task<ApiResponse<PaginatedResult<PersonalReadDto>>> GetPersonalFiltradoAsync(PersonalFiltroDto filtro);

        /// <summary>
        /// Busca personal por nombres (búsqueda parcial)
        /// </summary>
        /// <param name="nombres">Nombres a buscar</param>
        /// <returns>Lista de personal que coincide</returns>
        Task<ApiResponse<List<PersonalReadDto>>> BuscarPorNombresAsync(string nombres);

        /// <summary>
        /// Verifica si existe personal con el documento especificado
        /// </summary>
        /// <param name="idDocumento">Documento de identidad</param>
        /// <returns>True si existe, False si no existe</returns>
        Task<ApiResponse<bool>> ExistePersonalAsync(string idDocumento);
    }
}