using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class RendicionDetalleDto
    {
        public Guid DetId { get; set; }
        public string Descripcion { get; set; }
        public decimal Importe { get; set; }
        public string ComprobanteUrl { get; set; }
    }
}

