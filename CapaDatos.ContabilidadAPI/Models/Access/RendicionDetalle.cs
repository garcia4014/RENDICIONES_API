using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapaDatos.ContabilidadAPI
{
    [Table("RENDICION_DETALLE")]
    public class RendicionDetalle
    {
        [Column("DET_ID")]
        public Guid DetId { get; set; }

        [Column("REND_ID")]
        public Guid RendId { get; set; }

        [Column("TIPO_GASTO_ID")]
        public int? TipoGastoId { get; set; }

        [Column("DESCRIPCION")]
        public string Descripcion { get; set; }

        [Column("FECHA_GASTO")]
        public DateTime? FechaGasto { get; set; }

        [Column("IMPORTE")]
        public decimal Importe { get; set; }

        [Column("MONEDA")]
        public string Moneda { get; set; }

        [Column("ESTADO_VALIDACION")]
        public string EstadoValidacion { get; set; }

        [Column("OBSERVACION")]
        public string Observacion { get; set; }

        [Column("COMPROBANTE_URL")]
        public string ComprobanteUrl { get; set; }

        [Column("RUC")]
        public string Ruc { get; set; }

        [Column("SERIE")]
        public string Serie { get; set; }

        [Column("NUMERO")]
        public string Numero { get; set; }

        [Column("FECHA_EMITIDA")]
        public DateTime? FechaEmitida { get; set; }

        [ForeignKey(nameof(RendId))]
        public RendicionCabecera Rendicion { get; set; }
    }
}
