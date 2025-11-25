using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class SolicitudEstadoFlujoDTO
    {
        public int SefId { get; set; }
        public string? SefDescripcion { get; set; }
        public string? SefAbreviatura { get; set; }
        public string? SefProceso { get; set; }
        public bool SefEstado { get; set; }
    }
}
