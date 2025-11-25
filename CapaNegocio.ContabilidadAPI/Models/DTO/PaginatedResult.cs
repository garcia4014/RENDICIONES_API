namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// Resultado paginado genérico
    /// </summary>
    /// <typeparam name="T">Tipo de datos en la colección</typeparam>
    public class PaginatedResult<T>
    {
        /// <summary>
        /// Lista de datos de la página actual
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Número total de registros
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Página actual
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Tamaño de página
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Número total de páginas
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indica si hay página siguiente
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Indica si hay página anterior
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
}