using CapaDatos.ContabilidadAPI.Models;
using CapaDatos.ContabilidadAPI.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class SviaticosCabeceraDTO
    {
        //public int SvId { get; set; }

        //public string SvNumero { get; set; } = null!;

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

        public int SvSefId { get; set; }

        public string? SvEmpresa { get; set; }
        public string? SvPersonaEntrevistar { get; set; }

        public bool? SvPoliticas { get; set; }

        public List<SviaticosDetalleDTO> Detalles { get; set; } = new();
    }

    public class SviaticosCabeceraUpdateDTO
    {
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

        public int SvSefId { get; set; }

        public string? SvEmpresa { get; set; }
        public string? SvPersonaEntrevistar { get; set; }

        public bool? SvPoliticas { get; set; }

        public string? Observado { get; set; }
        public string? TipoOperacion { get; set; } = string.Empty;
        public List<SviaticosDetalleUpdateDTO> Detalles { get; set; } = new();
    }

    public class SviaticosCabeceraDTOResponse
    {
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
        public int SvSefId { get; set; }
        public string? SvEmpresa { get; set; }
        public string? SvPersonaEntrevistar { get; set; }
        public bool? SvPoliticas { get; set; }
        public DateTime SvFechaCreacion { get; set; }
        public string? Comentarios { get; set; }
        public SolicitudEstadoFlujoDTO Estado { get; set; } = new();
        public List<SviaticosDetalleDTOResponse> Detalles { get; set; } = new();
        public List<ComprobantePagoDto> ComprobantesPago { get; set; } = new();
    }

    public class EmpleadoDTO
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string U_MVT_APPAT { get; set; }
        public string U_MVT_APMAT { get; set; }
        public string U_MVT_NOM1 { get; set; }
        public string U_MVT_NOM2 { get; set; }
        public string U_MVT_NOM3 { get; set; }
        public string U_MVT_PUESTO { get; set; }
        public DateTime U_MVT_FECINGRESO { get; set; }
        public string U_MVT_UNIDAD { get; set; }
        public string U_MVT_CCOSTO { get; set; }
        public string U_MVT_SEDE { get; set; }
        public string U_MVT_AREA { get; set; }
        public string U_MVT_ESTADO { get; set; }
        public string? U_MVT_CORREO { get; set; }
        public string? U_MVT_TELEFONO { get; set; }
        public string? U_MVT_RANGO { get; set; }
        public string? U_MVT_RANGO_ID { get; set; }
        public string? U_MVT_CORREOPERSONAL { get; set; }
        public string? U_MVT_CELULAR { get; set; }
        public string? U_MVT_SEXO { get; set; }
        public string? U_MVT_RESPONSABLE { get; set; }
        public string? U_MVT_PASSWORD { get; set; }
    }

    public class SviaticosCabecerav2DTOResponse
    {
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

        public int SvSefId { get; set; }
        public string? SvEmpresa { get; set; }
        public string? SvPersonaEntrevistar { get; set; }

        public bool? SvPoliticas { get; set; }


        public SolicitudEstadoFlujoDTO Estado { get; set; } = new();

        public List<SviaticosDetalleDTOResponse> Detalles { get; set; } = new();

        public List<ComprobantePagoDto> ComprobantesPago { get; set; } = new();

        public EmpleadoDTO? Empleado { get; set; }
    }

    /// <summary>
    /// DTO para filtrar viáticos con múltiples criterios
    /// </summary>
    public class SviaticoFiltroDto
    {
        /// <summary>
        /// DNI del empleado para filtrar
        /// </summary>
        public string? SvEmpDni { get; set; }

        /// <summary>
        /// Fecha de inicio del rango de búsqueda (fecha de creación)
        /// </summary>
        public DateTime? FechaCreacionInicio { get; set; }

        /// <summary>
        /// Fecha de fin del rango de búsqueda (fecha de creación)
        /// </summary>
        public DateTime? FechaCreacionFin { get; set; }

        /// <summary>
        /// Array de estados (SV_SEF_ID) para filtrar
        /// </summary>
        public int[]? Estados { get; set; }

        /// <summary>
        /// Descripción del viático para búsqueda parcial
        /// </summary>
        public string? SvDescripcion { get; set; }

        /// <summary>
        /// Número de página para paginación (opcional)
        /// </summary>
        public int? Pagina { get; set; } = 1;

        /// <summary>
        /// Tamaño de página para paginación (opcional)
        /// </summary>
        public int? TamanoPagina { get; set; } = 50;
    }

    /// <summary>
    /// DTO para la respuesta de viáticos filtrados con conteo de estados
    /// </summary>
    public class ViaticosFiltradosResponseDto
    {
        public IEnumerable<SviaticosCabeceraDTOResponse> Viaticos { get; set; } = new List<SviaticosCabeceraDTOResponse>();
        public ConteoEstadosDto ConteoEstados { get; set; } = new ConteoEstadosDto();
        public PaginacionInfoDto? PaginacionInfo { get; set; }
    }

    /// <summary>
    /// DTO para el conteo de estados
    /// </summary>
    public class ConteoEstadosDto
    {
        public int Solicitado { get; set; } = 0;           // Estado 1
        public int Abierto { get; set; } = 0;              // Estado 2
        public int Aprobado { get; set; } = 0;             // Estado 3
        public int Rechazado { get; set; } = 0;            // Estado 4
        public int PagoEfectuado { get; set; } = 0;        // Estado 5
        public int Observado { get; set; } = 0;            // Estado 6
        public int Rendido { get; set; } = 0;              // Estado 7
        public int RendicionObservada { get; set; } = 0;   // Estado 8
        public int RendicionCerrada { get; set; } = 0;     // Estado 9

        /// <summary>
        /// Total de viáticos que coinciden con los filtros
        /// </summary>
        public int Total => Solicitado + Abierto + Aprobado + Rechazado + PagoEfectuado +
                           Observado + Rendido + RendicionObservada + RendicionCerrada;
    }

    /// <summary>
    /// DTO para información de paginación
    /// </summary>
    public class PaginacionInfoDto
    {
        public int PaginaActual { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
    }

    /// <summary>
    /// DTO extendido que incluye comprobantes agrupados por detalle
    /// </summary>
    public class SviaticoConComprobantesDto
    {
        public SviaticosCabeceraDTOResponse Cabecera { get; set; } = new();
        public List<DetalleConComprobantesDto> DetallesConComprobantes { get; set; } = new();
    }

    /// <summary>
    /// DTO que agrupa un detalle con sus comprobantes asociados
    /// </summary>
    public class DetalleConComprobantesDto
    {
        public SviaticosDetalleDTOResponse Detalle { get; set; } = new();
        public List<ComprobantePagoDto> Comprobantes { get; set; } = new();
    }
}
