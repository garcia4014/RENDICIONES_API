using CapaDatos.ContabilidadAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class SviaticosDetalleDTO
    {

        public int? SvdTgId { get; set; }

        public decimal? SvdPrecioUnitario { get; set; }

        public decimal? SvdImporteSolicitado { get; set; }

        public decimal? SvdSubtotal { get; set; }

        public string? SvdDescripcion { get; set; }

        public int? SvdCantEmpleado { get; set; }

        public DateTime? SvdFechaInicio { get; set; }

        public DateTime? SvdFechaFin { get; set; }
        public int? SvdNumeroDias { get; set; }
        public decimal? SvdKilometraje { get; set; }


    }
    public class SviaticosDetalleUpdateDTO
    {
        public int SvdId { get; set; }
        public int? SvdTgId { get; set; }

        public decimal? SvdPrecioUnitario { get; set; }

        public decimal? SvdImporteSolicitado { get; set; }

        public decimal? SvdSubtotal { get; set; }

        public string? SvdDescripcion { get; set; }

        public int? SvdCantEmpleado { get; set; }

        public DateTime? SvdFechaInicio { get; set; }

        public DateTime? SvdFechaFin { get; set; }
        public int? SvdNumeroDias { get; set; }
        public decimal? SvdKilometraje { get; set; }

    } 
    public class SviaticosDetalleDTOResponse
    {
        public int SvdId { get; set; }

        public string SvdNumeroCabecera { get; set; } = null!;

        public int SvdIdCabecera { get; set; }

        public int? SvdTgId { get; set; }

        public decimal? SvdPrecioUnitario { get; set; }

        public decimal? SvdImporteSolicitado { get; set; }

        public decimal? SvdSubtotal { get; set; }

        public string? SvdDescripcion { get; set; }

        public int? SvdCantEmpleado { get; set; }

        public DateTime? SvdFechaInicio { get; set; }

        public DateTime? SvdFechaFin { get; set; }

        public int? SvdNumeroDias { get; set; }
        public decimal? SvdKilometraje { get; set; } 
        public bool Observado { get; set; }
        public bool Aprobado { get; set; }
        public string? Observacion { get; set; }
        public List<ComprobantePago> ComprobantesPago { get; set; } = new();

    }
}
