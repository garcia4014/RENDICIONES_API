using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.Models
{
    public class SolicitudEstadoFlujo
    {
        [Key]
        public int SefId { get; set; }
        public string? SefDescripcion { get; set; }
        public string? SefAbreviatura { get; set; }
        public string? SefProceso { get; set; }
        public bool SefEstado { get; set; }

        public virtual ICollection<SviaticosCabecera> SviaticosCabecera { get; set; } = new List<SviaticosCabecera>();

    }
}
