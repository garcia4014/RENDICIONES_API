using CapaDatos.ContabilidadAPI.Models.Access;

namespace CapaDatos.ContabilidadAPI.DAO.Interfaces.Access
{
    /// <summary>
    /// Interfaz para operaciones de datos del Personal
    /// </summary>
    public interface IPersonalDao
    {
        /// <summary>
        /// Obtiene personal por documento de identidad
        /// </summary>
        /// <param name="idDocumento">Documento de identidad</param>
        /// <returns>Información del personal o null si no existe</returns>
        Task<Personal?> GetByIdDocumentoAsync(string idDocumento);

        /// <summary>
        /// Obtiene personal filtrado con paginación
        /// </summary>
        /// <param name="nombres">Filtro por nombres (opcional)</param>
        /// <param name="idDocumento">Filtro por documento (opcional)</param>
        /// <param name="empresa">Filtro por empresa (opcional)</param>
        /// <param name="usrSidige">Filtro por usuario SIDIGE (opcional)</param>
        /// <param name="pagina">Número de página</param>
        /// <param name="tamanoPagina">Tamaño de página</param>
        /// <returns>Lista paginada de personal y total de registros</returns>
        Task<(List<Personal> personal, int totalRegistros)> GetPersonalFiltradoAsync(
            string? nombres = null,
            string? idDocumento = null,
            string? empresa = null,
            string? usrSidige = null,
            int pagina = 1,
            int tamanoPagina = 10);

        /// <summary>
        /// Obtiene personal que coincida con los nombres (búsqueda parcial)
        /// </summary>
        /// <param name="nombres">Nombres a buscar</param>
        /// <returns>Lista de personal que coincide</returns>
        Task<List<Personal>> GetByNombresAsync(string nombres);

        /// <summary>
        /// Verifica si existe personal con el documento especificado
        /// </summary>
        /// <param name="idDocumento">Documento de identidad</param>
        /// <returns>True si existe, False si no existe</returns>
        Task<bool> ExistePersonalAsync(string idDocumento);
    }
}