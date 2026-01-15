using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaDatos.ContabilidadAPI.Models
{
    /// <summary>
    /// Entidad para el manejo de notificaciones del sistema
    /// </summary>
    [Table("NOTIFICACIONES")]
    public class Notificacion
    {
        /// <summary>
        /// Identificador único de la notificación
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Código del usuario receptor de la notificación
        /// </summary>
        [Column("CodUsuReceptor")]
        [StringLength(10)]
        public string? CodUsuReceptor { get; set; }

        /// <summary>
        /// Nombre del usuario receptor
        /// </summary>
        [Column("UsuarioReceptor")]
        [StringLength(100)]
        public string? UsuarioReceptor { get; set; }

        /// <summary>
        /// Código del usuario validador
        /// </summary>
        [Column("CodUsuValidador")]
        [StringLength(10)]
        public string? CodUsuValidador { get; set; }

        /// <summary>
        /// Nombre del usuario validador
        /// </summary>
        [Column("UsuarioValidador")]
        [StringLength(100)]
        public string? UsuarioValidador { get; set; }

        /// <summary>
        /// Mensaje de la notificación
        /// </summary>
        [Column("Mensaje")]
        [StringLength(100)]
        public string? Mensaje { get; set; }

        /// <summary>
        /// Fecha de creación de la notificación
        /// </summary>
        [Column("FechaCreacion")]
        public DateTime? FechaCreacion { get; set; }

        /// <summary>
        /// Indica si la notificación ha sido leída
        /// </summary>
        [Column("Leido")]
        public bool? Leido { get; set; }

        /// <summary>
        /// Indica si la notificación está activa (para eliminación lógica)
        /// </summary>
        [Column("Activo")]
        public bool? Activo { get; set; }

        public int EstadoFlujo { get; set; } = 1;
    }
}