using AutoMapper;
using CapaDatos.ContabilidadAPI;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Implementación del servicio para ComprobantePago
    /// </summary>
    public class ComprobantePagoServiceImpl : IComprobantePagoService
    {
        private readonly IComprobantePago _comprobantePagoDao;
        private readonly IMapper _mapper;
        private readonly SvrendicionesContext _context;
        private readonly INotificacionesService _notificacionesService;

        public ComprobantePagoServiceImpl(INotificacionesService notificacionesService,SvrendicionesContext context,IComprobantePago comprobantePagoDao, IMapper mapper)
        {
            _comprobantePagoDao = comprobantePagoDao;
            _mapper = mapper;
            _context = context;
            _notificacionesService = notificacionesService;
        }

        /// <summary>
        /// Obtiene todos los comprobantes de pago con paginación
        /// </summary>
        public async Task<ApiResponse<PagedResult<ComprobantePagoDto>>> GetAllAsync(int pagina = 1, int tamanoPagina = 10)
        {
            try
            {
                var comprobantes = await _comprobantePagoDao.GetAllAsync();
                var totalItems = comprobantes.Count;

                var comprobantesPaginados = comprobantes
                    .Skip((pagina - 1) * tamanoPagina)
                    .Take(tamanoPagina)
                    .ToList();

                var comprobantesDto = _mapper.Map<List<ComprobantePagoDto>>(comprobantesPaginados);

                // Agregar descripciones de tipo de comprobante
                foreach (var dto in comprobantesDto)
                {
                    dto.TipoComprobanteDescripcion = GetTipoComprobanteDescripcion(dto.TipoComprobante);
                }

                var result = new PagedResult<ComprobantePagoDto>
                {
                    Items = comprobantesDto,
                    TotalItems = totalItems,
                    CurrentPage = pagina,
                    PageSize = tamanoPagina
                };

                return new ApiResponse<PagedResult<ComprobantePagoDto>>(result, "Comprobantes obtenidos correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResult<ComprobantePagoDto>>(null, $"Error al obtener comprobantes: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene un comprobante de pago por su ID
        /// </summary>
        public async Task<ApiResponse<ComprobantePagoDto>> GetByIdAsync(int id)
        {
            try
            {
                var comprobante = await _comprobantePagoDao.GetByIdAsync(id);

                if (comprobante == null)
                {
                    return new ApiResponse<ComprobantePagoDto>(null, "Comprobante no encontrado");
                }

                var comprobanteDto = _mapper.Map<ComprobantePagoDto>(comprobante);
                comprobanteDto.TipoComprobanteDescripcion = GetTipoComprobanteDescripcion(comprobanteDto.TipoComprobante);

                return new ApiResponse<ComprobantePagoDto>(comprobanteDto, "Comprobante obtenido correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<ComprobantePagoDto>(null, $"Error al obtener comprobante: {ex.Message}");
            }
        }

        /// <summary>
        /// Busca comprobantes con filtros aplicados
        /// </summary>
        public async Task<ApiResponse<PagedResult<ComprobantePagoDto>>> BuscarAsync(ComprobantePagoFiltroDto filtro)
        {
            try
            {
                var comprobantes = await _comprobantePagoDao.GetAllAsync();

                // Aplicar filtros
                if (filtro.SvIdCabecera.HasValue)
                {
                    comprobantes = comprobantes.Where(c => c.SvIdCabecera == filtro.SvIdCabecera.Value).ToList();
                }

                if (filtro.SvIdDetalle.HasValue)
                {
                    comprobantes = comprobantes.Where(c => c.SvIdDetalle == filtro.SvIdDetalle.Value).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.Serie))
                {
                    comprobantes = comprobantes.Where(c => c.Serie != null && c.Serie.Contains(filtro.Serie)).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.Correlativo))
                {
                    comprobantes = comprobantes.Where(c => c.Correlativo != null && c.Correlativo.Contains(filtro.Correlativo)).ToList();
                }

                if (filtro.Ruc.HasValue)
                {
                    comprobantes = comprobantes.Where(c => c.Ruc == filtro.Ruc.Value).ToList();
                }

                if (filtro.FechaEmisionDesde.HasValue)
                {
                    comprobantes = comprobantes.Where(c => c.FechaEmision >= filtro.FechaEmisionDesde.Value).ToList();
                }

                if (filtro.FechaEmisionHasta.HasValue)
                {
                    comprobantes = comprobantes.Where(c => c.FechaEmision <= filtro.FechaEmisionHasta.Value).ToList();
                }

                if (filtro.ValidoSunat.HasValue)
                {
                    comprobantes = comprobantes.Where(c => c.ValidoSunat == filtro.ValidoSunat.Value).ToList();
                }

                var totalItems = comprobantes.Count;

                var comprobantesPaginados = comprobantes
                    .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
                    .Take(filtro.TamanoPagina)
                    .ToList();

                var comprobantesDto = _mapper.Map<List<ComprobantePagoDto>>(comprobantesPaginados);

                foreach (var dto in comprobantesDto)
                {
                    dto.TipoComprobanteDescripcion = GetTipoComprobanteDescripcion(dto.TipoComprobante);
                }

                var result = new PagedResult<ComprobantePagoDto>
                {
                    Items = comprobantesDto,
                    TotalItems = totalItems,
                    CurrentPage = filtro.Pagina,
                    PageSize = filtro.TamanoPagina
                };

                return new ApiResponse<PagedResult<ComprobantePagoDto>>(result, "Búsqueda completada correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResult<ComprobantePagoDto>>(null, $"Error en la búsqueda: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene comprobantes por ID de cabecera de viáticos
        /// </summary>
        public async Task<ApiResponse<List<ComprobantePagoDto>>> GetByCabeceraIdAsync(int svIdCabecera)
        {
            try
            {
                var comprobantes = await _comprobantePagoDao.GetByCabeceraIdAsync(svIdCabecera);
                var comprobantesDto = _mapper.Map<List<ComprobantePagoDto>>(comprobantes);

                foreach (var dto in comprobantesDto)
                {
                    dto.TipoComprobanteDescripcion = GetTipoComprobanteDescripcion(dto.TipoComprobante);
                }

                return new ApiResponse<List<ComprobantePagoDto>>(comprobantesDto, "Comprobantes obtenidos correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ComprobantePagoDto>>(null, $"Error al obtener comprobantes: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene comprobantes por ID de detalle de viáticos
        /// </summary>
        public async Task<ApiResponse<List<ComprobantePagoDto>>> GetByDetalleIdAsync(int svIdDetalle)
        {
            try
            {
                var comprobantes = await _comprobantePagoDao.GetByDetalleIdAsync(svIdDetalle);
                var comprobantesDto = _mapper.Map<List<ComprobantePagoDto>>(comprobantes);

                foreach (var dto in comprobantesDto)
                {
                    dto.TipoComprobanteDescripcion = GetTipoComprobanteDescripcion(dto.TipoComprobante);
                }

                return new ApiResponse<List<ComprobantePagoDto>>(comprobantesDto, "Comprobantes obtenidos correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ComprobantePagoDto>>(null, $"Error al obtener comprobantes: {ex.Message}");
            }
        }
         
        public async Task<ApiResponse<ComprobantePagoDto>> CreateAsync(ComprobantePagoCreateDto createDto)
        {
            try
            {
                // Validar duplicidad
                if (await ExisteDuplicadoAsync(createDto.Serie, createDto.Correlativo))
                {
                    return new ApiResponse<ComprobantePagoDto>(null, "Ya existe un comprobante con la misma serie y correlativo");
                }

                await _comprobantePagoDao.InactiveVoucherPrevius(createDto.SvIdCabecera, createDto.SvIdDetalle);

                var comprobante = _mapper.Map<ComprobantePago>(createDto);
                var comprobanteCreado = await _comprobantePagoDao.CreateAsync(comprobante);
                var comprobanteDto = _mapper.Map<ComprobantePagoDto>(comprobanteCreado);
                comprobanteDto.TipoComprobanteDescripcion = GetTipoComprobanteDescripcion(comprobanteDto.TipoComprobante);

                if (createDto.Observado.HasValue && createDto.Observado == 1)
                {
                    var detalle = await _context.SviaticosDetalles.FirstOrDefaultAsync(x=>x.SvdId == createDto.SvIdDetalle);
                    if (detalle != null)
                    {
                        detalle.Observado = false;
                        var cabecera = await _context.SviaticosCabeceras.FirstOrDefaultAsync(x => x.SvId == detalle.SvdIdCabecera);
                        //cabecera.SvSefId = 7;
                        var dto = new NotificacionCreateDto()
                        {
                            CodUsuReceptor = cabecera.SvEmpDni ?? string.Empty,
                            UsuarioReceptor = null,
                            CodUsuValidador = null,
                            UsuarioValidador = null,
                            Mensaje = $"Solicitud #{cabecera.SvId} - se envió el detalle de la rendición a subsanar",
                            Leido = false,
                            EstadoFlujo = 7
                        };
                        var responseTMP = await _notificacionesService.CreateAsync(dto);
                        _context.SaveChanges();
                    }
                   
                }

                return new ApiResponse<ComprobantePagoDto>(comprobanteDto, "Comprobante creado correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<ComprobantePagoDto>(null, $"Error al crear comprobante: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza un comprobante de pago existente
        /// </summary>
        public async Task<ApiResponse<ComprobantePagoDto>> UpdateAsync(ComprobantePagoUpdateDto updateDto)
        {
            try
            {
                // Validar que existe
                var comprobanteExistente = await _comprobantePagoDao.GetByIdAsync(updateDto.Id);
                if (comprobanteExistente == null)
                {
                    return new ApiResponse<ComprobantePagoDto>(null, "Comprobante no encontrado");
                }

                // Validar duplicidad excluyendo el actual
                if (!string.IsNullOrEmpty(updateDto.Serie) && !string.IsNullOrEmpty(updateDto.Correlativo))
                {
                    if (await ExisteDuplicadoAsync(updateDto.Serie, updateDto.Correlativo, updateDto.Id))
                    {
                        return new ApiResponse<ComprobantePagoDto>(null, "Ya existe otro comprobante con la misma serie y correlativo");
                    }
                }

                var comprobante = _mapper.Map<ComprobantePago>(updateDto);
                var comprobanteActualizado = await _comprobantePagoDao.UpdateAsync(comprobante);

                var comprobanteDto = _mapper.Map<ComprobantePagoDto>(comprobanteActualizado);
                comprobanteDto.TipoComprobanteDescripcion = GetTipoComprobanteDescripcion(comprobanteDto.TipoComprobante);

                return new ApiResponse<ComprobantePagoDto>(comprobanteDto, "Comprobante actualizado correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<ComprobantePagoDto>(null, $"Error al actualizar comprobante: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina (borrado lógico) un comprobante de pago
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var resultado = await _comprobantePagoDao.DeleteAsync(id);

                if (!resultado)
                {
                    return new ApiResponse<bool>(false, "Comprobante no encontrado o ya eliminado");
                }

                return new ApiResponse<bool>(true, "Comprobante eliminado correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, $"Error al eliminar comprobante: {ex.Message}");
            }
        }

        /// <summary>
        /// Valida la duplicidad de un comprobante por serie y correlativo
        /// </summary>
        public async Task<bool> ExisteDuplicadoAsync(string serie, string correlativo, int? idExcluir = null)
        {
            try
            {
                var comprobantes = await _comprobantePagoDao.GetBySerieCorrelattivoAsync(serie, correlativo);

                if (idExcluir.HasValue)
                {
                    comprobantes = comprobantes.Where(c => c.Id != idExcluir.Value).ToList();
                }

                return comprobantes.Any();
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de comprobantes por período
        /// </summary>
        public async Task<ApiResponse<ComprobantePagoEstadisticasDto>> GetEstadisticasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var comprobantes = await _comprobantePagoDao.GetByFechaEmisionAsync(fechaInicio, fechaFin);

                var estadisticas = new ComprobantePagoEstadisticasDto
                {
                    TotalComprobantes = comprobantes.Count,
                    MontoTotal = comprobantes.Sum(c => c.Monto ?? 0),
                    ComprobantesPendientes = comprobantes.Count(c => c.ValidoSunat != true),
                    ComprobantesValidados = comprobantes.Count(c => c.ValidoSunat == true),
                    ComprobantesSunat = comprobantes.Count(c => c.ValidoSunat == true)
                };

                return new ApiResponse<ComprobantePagoEstadisticasDto>(estadisticas, "Estadísticas obtenidas correctamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<ComprobantePagoEstadisticasDto>(null, $"Error al obtener estadísticas: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene dashboard de rendiciones para un empleado específico
        /// </summary>
        public async Task<ApiResponse<RendicionesDashboardDto>> GetRendicionesDashboardAsync(string[] estados,string svEmpDni, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                int[] estadosFormateado ;
                if (estados.Length == 0) {
                    estadosFormateado = [5] ;
                }
                else
                {
                    estadosFormateado = Array.ConvertAll(estados, int.Parse);
                }

                // Aplicar fechas por defecto si no se proporcionan
                var fechaInicioFinal = fechaInicio ?? DateTime.Now.AddMonths(-6).Date;
                var fechaFinFinal = fechaFin ?? DateTime.Now.Date.AddDays(1);

                if (fechaInicioFinal > fechaFinFinal)
                {
                    return new ApiResponse<RendicionesDashboardDto>(null, "La fecha de inicio no puede ser mayor a la fecha de fin");
                }

                var (rendicionesPendientes, comprobantesCargados, validadosSunat, pendientesValidacion) =
                    await _comprobantePagoDao.GetRendicionesDashboardAsync(svEmpDni, fechaInicioFinal, fechaFinFinal, estadosFormateado);

                var dashboard = new RendicionesDashboardDto
                {
                    RendicionesPendientes = rendicionesPendientes,
                    ComprobantesCargados = comprobantesCargados,
                    ValidadosSunat = validadosSunat,
                    PendientesValidacion = pendientesValidacion,
                    SvEmpDni = svEmpDni,
                    FechaInicio = fechaInicioFinal,
                    FechaFin = fechaFinFinal,
                    FechaGeneracion = DateTime.Now
                };

                var mensaje = $"Dashboard generado para empleado {svEmpDni} del {fechaInicioFinal:dd/MM/yyyy} al {fechaFinFinal:dd/MM/yyyy}";
                return new ApiResponse<RendicionesDashboardDto>(dashboard, mensaje);
            }
            catch (Exception ex)
            {
                return new ApiResponse<RendicionesDashboardDto>(null, $"Error al obtener dashboard: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene la descripción del tipo de comprobante
        /// </summary>
        private string GetTipoComprobanteDescripcion(int? tipoComprobante)
        {
            return tipoComprobante switch
            {
                1 => "Factura",
                2 => "Boleta de Venta",
                3 => "Recibo por Honorarios",
                4 => "Nota de Crédito",
                5 => "Nota de Débito",
                6 => "Guía de Remisión",
                7 => "Comprobante de Retención",
                8 => "Comprobante de Percepción",
                9 => "Ticket",
                10 => "Otros",
                _ => "No especificado"
            };
        }
    }
}