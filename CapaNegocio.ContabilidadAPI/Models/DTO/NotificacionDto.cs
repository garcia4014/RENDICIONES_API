using System.ComponentModel.DataAnnotations;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO principal para mostrar información de notificaciones
    /// </summary>
    public class NotificacionDto
    {
        /// <summary>
        /// Identificador único de la notificación
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Código del usuario receptor
        /// </summary>
        public string? CodUsuReceptor { get; set; }

        /// <summary>
        /// Nombre del usuario receptor
        /// </summary>
        public string? UsuarioReceptor { get; set; }

        /// <summary>
        /// Código del usuario validador
        /// </summary>
        public string? CodUsuValidador { get; set; }

        /// <summary>
        /// Nombre del usuario validador
        /// </summary>
        public string? UsuarioValidador { get; set; }

        /// <summary>
        /// Mensaje de la notificación
        /// </summary>
        public string? Mensaje { get; set; }

        /// <summary>
        /// Fecha de creación de la notificación
        /// </summary>
        public DateTime? FechaCreacion { get; set; }

        /// <summary>
        /// Indica si la notificación ha sido leída
        /// </summary>
        public bool? Leido { get; set; }

        /// <summary>
        /// Indica si la notificación está activa
        /// </summary>
        public bool? Activo { get; set; }
        public int EstadoFlujo { get; set; }
    }

    /// <summary>
    /// DTO para crear una nueva notificación
    /// </summary>
    public class NotificacionCreateDto
    {
        /// <summary>
        /// Código del usuario receptor (requerido)
        /// </summary>
        [Required(ErrorMessage = "El código del usuario receptor es requerido")]
        [StringLength(10, ErrorMessage = "El código del usuario receptor no puede exceder 10 caracteres")]
        public string CodUsuReceptor { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del usuario receptor
        /// </summary>
        [StringLength(100, ErrorMessage = "El nombre del usuario receptor no puede exceder 100 caracteres")]
        public string? UsuarioReceptor { get; set; }

        /// <summary>
        /// Código del usuario validador
        /// </summary>
        [StringLength(10, ErrorMessage = "El código del usuario validador no puede exceder 10 caracteres")]
        public string? CodUsuValidador { get; set; }

        /// <summary>
        /// Nombre del usuario validador
        /// </summary>
        [StringLength(100, ErrorMessage = "El nombre del usuario validador no puede exceder 100 caracteres")]
        public string? UsuarioValidador { get; set; }

        /// <summary>
        /// Mensaje de la notificación (requerido)
        /// </summary>
        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(100, ErrorMessage = "El mensaje no puede exceder 100 caracteres")]
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la notificación ha sido leída (por defecto false)
        /// </summary>
        public bool Leido { get; set; } = false;
        public int EstadoFlujo { get; set; } = 1;
    }

    /// <summary>
    /// DTO para actualizar una notificación existente
    /// </summary>
    public class NotificacionUpdateDto
    {
        /// <summary>
        /// Código del usuario receptor
        /// </summary>
        [StringLength(10, ErrorMessage = "El código del usuario receptor no puede exceder 10 caracteres")]
        public string? CodUsuReceptor { get; set; }

        /// <summary>
        /// Nombre del usuario receptor
        /// </summary>
        [StringLength(100, ErrorMessage = "El nombre del usuario receptor no puede exceder 100 caracteres")]
        public string? UsuarioReceptor { get; set; }

        /// <summary>
        /// Código del usuario validador
        /// </summary>
        [StringLength(10, ErrorMessage = "El código del usuario validador no puede exceder 10 caracteres")]
        public string? CodUsuValidador { get; set; }

        /// <summary>
        /// Nombre del usuario validador
        /// </summary>
        [StringLength(100, ErrorMessage = "El nombre del usuario validador no puede exceder 100 caracteres")]
        public string? UsuarioValidador { get; set; }

        /// <summary>
        /// Mensaje de la notificación
        /// </summary>
        [StringLength(100, ErrorMessage = "El mensaje no puede exceder 100 caracteres")]
        public string? Mensaje { get; set; }

        /// <summary>
        /// Indica si la notificación ha sido leída
        /// </summary>
        public bool? Leido { get; set; }
    }

    /// <summary>
    /// DTO para filtrar notificaciones en consultas
    /// </summary>
    public class NotificacionFiltroDto
    {
        /// <summary>
        /// Filtrar por código del usuario receptor
        /// </summary>
        public string? CodUsuReceptor { get; set; }

        /// <summary>
        /// Filtrar por código del usuario validador
        /// </summary>
        public string? CodUsuValidador { get; set; }

        /// <summary>
        /// Filtrar por estado de lectura
        /// </summary>
        public bool? Leido { get; set; }

        /// <summary>
        /// Filtrar por estado activo
        /// </summary>
        public bool? Activo { get; set; }

        /// <summary>
        /// Filtrar por fecha de creación desde
        /// </summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>
        /// Filtrar por fecha de creación hasta
        /// </summary>
        public DateTime? FechaHasta { get; set; }

        /// <summary>
        /// Búsqueda en el mensaje
        /// </summary>
        public string? TextoBusqueda { get; set; }

        /// <summary>
        /// Número de página (para paginación)
        /// </summary>
        public int Pagina { get; set; } = 1;

        /// <summary>
        /// Tamaño de página (para paginación)
        /// </summary>
        public int TamanoPagina { get; set; } = 10;
    }

    /// <summary>
    /// DTO para marcar notificaciones como leídas
    /// </summary>
    public class MarcarLeidoDto
    {
        /// <summary>
        /// Lista de IDs de notificaciones a marcar como leídas
        /// </summary>
        [Required(ErrorMessage = "Debe proporcionar al menos una notificación")]
        public List<int> NotificacionIds { get; set; } = new List<int>();
    }
}