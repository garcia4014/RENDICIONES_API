using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaDatos.ContabilidadAPI
{
    [Table("RENDICION_CABECERA")]
    public class RendicionCabecera
    {
        [Column("REND_ID")]
        public Guid RendId { get; set; }

        [Column("SOLICITUD_ID")]
        public int? SolicitudId { get; set; }

        [Column("USR_COD")]
        public string UsrCod { get; set; }

        [Column("ESTADO")]
        public string Estado { get; set; }

        [Column("FECHA_CREACION")]
        public DateTime FechaCreacion { get; set; }

        [Column("FECHA_ENVIO")]
        public DateTime? FechaEnvio { get; set; }

        [Column("TOTAL")]
        public decimal Total { get; set; }

        [Column("OBSERVACION")]
        public string Observacion { get; set; }

        public ICollection<RendicionDetalle> Detalles { get; set; } = new List<RendicionDetalle>();
    }
}

