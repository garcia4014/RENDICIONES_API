using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class RendicionCabeceraDto
    {
        public Guid RendId { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public List<RendicionDetalleDto> Detalles { get; set; }
    }
}

