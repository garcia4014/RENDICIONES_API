using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace CapaDatos.ContabilidadAPI.Models;

public  class SviaticosDetalle
{
    [Key]
    public int SvdId { get; set; }

    public string SvdNumeroCabecera { get; set; } = null!;

    public int SvdIdCabecera { get; set; }

    public int? SvdTgId { get; set; }

    [NotMapped] 
    public string? SvTipoGasto { get; set; } //CAMPO TEMPORAL PARA OBTENER DETALLE

    public decimal? SvdPrecioUnitario { get; set; }

    public decimal? SvdImporteSolicitado { get; set; }

    public decimal? SvdSubtotal { get; set; }

    public string? SvdDescripcion { get; set; }

    public int? SvdCantEmpleado { get; set; }

    public DateTime? SvdFechaInicio { get; set; }

    public DateTime? SvdFechaFin { get; set; }
    public int? SvdNumeroDias { get; set; }

    public decimal? SvdKilometraje { get; set; }    

    public bool? Observado { get; set; }
    public bool? Aprobado { get; set; }
    public string? Observacion { get; set; }
    
    // Relación con Cabecera
    [ForeignKey("SvdIdCabecera")]
    public SviaticosCabecera Cabecera { get; set; } // = null!;

    // Relación con Comprobantes de Pago - COMENTADA: Ahora ComprobantePago se relaciona directamente con Cabecera
    // public ICollection<ComprobantePago> ComprobantesPago { get; set; } = new List<ComprobantePago>();
}
