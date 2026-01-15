using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.Models
{
    public class RendicionDetalle
    {
        public Guid DetId { get; set; }
        public Guid RendId { get; set; }

        public int? TipoGastoId { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaGasto { get; set; }
        public decimal Importe { get; set; }
        public string Moneda { get; set; }
        public string EstadoValidacion { get; set; }
        public string Observacion { get; set; }
        public string ComprobanteUrl { get; set; }
        public string Ruc { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public DateTime? FechaEmitida { get; set; }

        public RendicionCabecera Rendicion { get; set; }
    }
}

