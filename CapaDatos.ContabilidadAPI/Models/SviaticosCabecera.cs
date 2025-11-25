using System.ComponentModel.DataAnnotations;

namespace CapaDatos.ContabilidadAPI.Models;

public partial class SviaticosCabecera
{
    [Key]
    public int SvId { get; set; }

    public string SvNumero { get; set; } = null!;

    public string SvEmpCodigo { get; set; } = null!;

    public string? SvEmpDni { get; set; }

    public DateTime? SvFechaInicio { get; set; }

    public DateTime? SvFechaRetorno { get; set; }

    public int? SvEmpCantidad { get; set; }

    public int? SvNumeroDias { get; set; }
    public string? SvOrdenVenta { get; set; }

    public string? SvDescripcion { get; set; }

    public string? SvRuc { get; set; }

    public string? SvContacto { get; set; }

    public string? SvObjetivoVisita { get; set; }

    public string SvLocalidad { get; set; } = null!;

    public decimal? SvTotalSolicitado { get; set; }

    public int SvSefId { get; set; } = 1;
    public string? SvEmpresa { get; set; }
    public string? SvPersonaEntrevistar { get; set; }

    public bool? SvPoliticas { get; set; }
    public DateTime? SvFechaCreacion { get; set; } = DateTime.Now;
    public string? Comentarios { get; set; }
    public string? ComentariosRendicion { get; set; }
    // Relación con Detalles
    public ICollection<SviaticosDetalle> Detalles { get; set; } = new List<SviaticosDetalle>();
    public virtual SolicitudEstadoFlujo? SolicitudEstadoFlujo { get; set; }

    // Relación con Comprobantes de Pago
    public ICollection<ComprobantePago> ComprobantesPago { get; set; } = new List<ComprobantePago>();

}
