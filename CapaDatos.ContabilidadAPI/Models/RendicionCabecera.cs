using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.Models
{
    public class RendicionCabecera
    {
        public Guid RendId { get; set; }
        public int? SolicitudId { get; set; }
        public string UsrCod { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaEnvio { get; set; }
        public decimal Total { get; set; }
        public string Observacion { get; set; }

        public ICollection<RendicionDetalle> Detalles { get; set; } = new List<RendicionDetalle>();
    }
}

