using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaDatos.ContabilidadAPI.Models
{
    [Table("PARAMETROS")]
    public class Parametros
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Descripcion")]
        [StringLength(200)]
        public string? Descripcion { get; set; }

        [Column("Valor")]
        [StringLength(2000)]
        public string? Valor { get; set; }

        [Column("Estado")]
        public bool Estado { get; set; }
    }
}
