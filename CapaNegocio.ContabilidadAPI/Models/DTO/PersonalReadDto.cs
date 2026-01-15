namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO para el modelo Personal
    /// </summary>
    public class PersonalReadDto
    {
        /// <summary>
        /// Identificador del documento de identidad
        /// </summary>
        public string IdDocumento { get; set; } = string.Empty;

        /// <summary>
        /// Nombres completos de la persona
        /// </summary>
        public string Nombres { get; set; } = string.Empty;

        /// <summary>
        ///// Huella dactilar (opcional)
        ///// </summary>
        //public string? Huella { get; set; }

        ///// <summary>
        ///// Empresa a la que pertenece
        ///// </summary>
        //public string Empresa { get; set; } = string.Empty;

        ///// <summary>
        ///// Fecha de registro en el sistema
        ///// </summary>
        //public DateTime? FechaRegistro { get; set; }

        /// <summary>
        /// Usuario SIDIGE
        /// </summary>
        public string UsrSidige { get; set; } = string.Empty;

        /// <summary>
        /// Email SIDIGE (opcional)
        /// </summary>
        public string? EmailSidige { get; set; }
    }

    /// <summary>
    /// DTO para filtros de búsqueda de Personal
    /// </summary>
    public class PersonalFiltroDto
    {
        /// <summary>
        /// Filtro por documento de identidad (búsqueda exacta)
        /// </summary>
        public string? IdDocumento { get; set; }

        /// <summary>
        /// Filtro por nombres (búsqueda parcial)
        /// </summary>
        public string? Nombres { get; set; }

        /// <summary>
        /// Filtro por empresa
        /// </summary>
        public string? Empresa { get; set; }

        /// <summary>
        /// Filtro por usuario SIDIGE
        /// </summary>
        public string? UsrSidige { get; set; }

        /// <summary>
        /// Número de página para paginación (por defecto: 1)
        /// </summary>
        public int Pagina { get; set; } = 1;

        /// <summary>
        /// Tamaño de página para paginación (por defecto: 10)
        /// </summary>
        public int TamanoPagina { get; set; } = 10;
    }
}