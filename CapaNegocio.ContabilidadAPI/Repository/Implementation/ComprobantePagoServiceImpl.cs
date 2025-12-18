using AutoMapper;
using CapaDatos.ContabilidadAPI;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ISunatTokenService _sunatTokenService;
        private readonly ISunatComprobanteService _sunatComprobanteService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ComprobantePagoServiceImpl> _logger;

        public ComprobantePagoServiceImpl(
            INotificacionesService notificacionesService,
            SvrendicionesContext context,
            IComprobantePago comprobantePagoDao, 
            IMapper mapper,
            ISunatTokenService sunatTokenService,
            ISunatComprobanteService sunatComprobanteService,
            IConfiguration configuration,
            ILogger<ComprobantePagoServiceImpl> logger)
        {
            _comprobantePagoDao = comprobantePagoDao;
            _mapper = mapper;
            _context = context;
            _notificacionesService = notificacionesService;
            _sunatTokenService = sunatTokenService;
            _sunatComprobanteService = sunatComprobanteService;
            _configuration = configuration;
            _logger = logger;
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

        public async Task<ComprobantePago> GetById(int id)
        {
            try
            {
                var comprobante =  await _comprobantePagoDao.GetByIdAsync(id);

                if (comprobante == null)
                {
                    return new ComprobantePago();
                }

               
                return comprobante;
            }
            catch (Exception ex)
            {
                return new ComprobantePago();
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

                //await _comprobantePagoDao.InactiveVoucherPrevius(createDto.SvIdCabecera, createDto.SvIdDetalle);

                var comprobante = _mapper.Map<ComprobantePago>(createDto);
                
                // Calcular IGVPorcentaje automáticamente
                if (comprobante.IgvEspecial == true)
                    comprobante.IgvPorcentaje = 10;
                else if (comprobante.Exonerado == true || comprobante.Inafecto == true)
                    comprobante.IgvPorcentaje = 0;
                else
                    comprobante.IgvPorcentaje = 18;

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
                comprobante.Ruta = comprobanteExistente.Ruta;
                comprobante.ValidoSunat = false;
                
                // Calcular IGVPorcentaje automáticamente
                if (comprobante.IgvEspecial == true)
                    comprobante.IgvPorcentaje = 10;
                else if (comprobante.Exonerado == true || comprobante.Inafecto == true)
                    comprobante.IgvPorcentaje = 0;
                else
                    comprobante.IgvPorcentaje = 18;

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
        public async Task<ApiResponse<RendicionesDashboardDto>> GetRendicionesDashboardAsync(string[] estados  ,string svEmpDni, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                int[] estadosFormateado ;
                if (estados == null) {
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
        /// Actualiza el estado de observado de un comprobante
        /// </summary>
        public async Task<ApiResponse<bool>> ActualizarComprobanteObservado(int comprobanteId, bool observado, string? comentario)
        {
            try
            {
                var resultado = await _comprobantePagoDao.ActualizarComprobanteObservado(comprobanteId, observado, comentario ?? string.Empty);
                
                if (resultado)
                {
                    // Obtener el comprobante para la notificación
                    var comprobante = await _comprobantePagoDao.GetByIdAsync(comprobanteId);
                    if (comprobante != null && comprobante.SviaticosCabecera != null)
                    {
                        // Cambiar el estado de la cabecera a 8 (Observado)
                        comprobante.SviaticosCabecera.SvSefId = 8;
                        await _context.SaveChangesAsync();

                        // Crear notificación
                        var createDto = new NotificacionCreateDto()
                        {
                            CodUsuReceptor = comprobante.SviaticosCabecera.SvEmpDni ?? string.Empty,
                            UsuarioReceptor = null,
                            CodUsuValidador = null,
                            UsuarioValidador = null,
                            Mensaje = $"Solicitud #{comprobante.SviaticosCabecera.SvId} - el comprobante {comprobante.Serie}-{comprobante.Correlativo} ha sido observado: {comentario}",
                            Leido = false,
                            EstadoFlujo = 8
                        };

                        await _notificacionesService.CreateAsync(createDto);
                    }

                    return new ApiResponse<bool>(true, "Comprobante marcado como observado correctamente");
                }

                return new ApiResponse<bool>(false, "No se pudo actualizar el comprobante");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, $"Error al actualizar comprobante observado: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza el estado de aprobado de un comprobante
        /// </summary>
        public async Task<ApiResponse<bool>> ActualizarComprobanteAprobado(int comprobanteId, bool aprobado)
        {
            try
            {
                var resultado = await _comprobantePagoDao.ActualizarComprobanteAprobado(comprobanteId, aprobado);
                
                if (resultado)
                {
                    // Obtener el comprobante para la notificación
                    var comprobante = await _comprobantePagoDao.GetByIdAsync(comprobanteId);
                    if (comprobante != null && comprobante.SviaticosCabecera != null)
                    {
                        // Crear notificación
                        var createDto = new NotificacionCreateDto()
                        {
                            CodUsuReceptor = comprobante.SviaticosCabecera.SvEmpDni ?? string.Empty,
                            UsuarioReceptor = null,
                            CodUsuValidador = null,
                            UsuarioValidador = null,
                            Mensaje = $"Solicitud #{comprobante.SviaticosCabecera.SvId} - el comprobante {comprobante.Serie}-{comprobante.Correlativo} ha sido aprobado",
                            Leido = false,
                            EstadoFlujo = comprobante.SviaticosCabecera.SvSefId
                        };

                        await _notificacionesService.CreateAsync(createDto);
                    }

                    return new ApiResponse<bool>(true, "Comprobante marcado como aprobado correctamente");
                }

                return new ApiResponse<bool>(false, "No se pudo actualizar el comprobante");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, $"Error al actualizar comprobante aprobado: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene la descripción del tipo de comprobante
        /// </summary>
        private string GetTipoComprobanteDescripcion(string? tipoComprobante)
        {
            return tipoComprobante switch
            {
                "01" => "Factura",
                "03" => "Boleta de Venta",
                "RH" => "Recibo por Honorarios",
                "07" => "Nota de Crédito",
                "08" => "Nota de Débito",
                "09" => "Guía de Remisión",
                "20" => "Comprobante de Retención",
                "40" => "Comprobante de Percepción",
                "TK" => "Ticket",
                "OT" => "Otros",
                // Compatibilidad con números antiguos
                "1" => "Factura",
                "2" => "Boleta de Venta",
                "3" => "Recibo por Honorarios",
                "4" => "Nota de Crédito",
                "5" => "Nota de Débito",
                "6" => "Guía de Remisión",
                "7" => "Comprobante de Retención",
                "8" => "Comprobante de Percepción",
                "9" => "Ticket",
                "10" => "Otros",
                _ => "No especificado"
            };
        }

        /// <summary>
        /// Valida un comprobante en SUNAT de manera asíncrona (ejecutado por Hangfire)
        /// </summary>
        public async Task ValidarComprobanteEnSunatAsync(int comprobanteId)
        {
            try
            {
                _logger.LogInformation("Iniciando validación SUNAT para comprobante {ComprobanteId}", comprobanteId);

                // Obtener comprobante
                var comprobante = await _context.ComprobantesPago.FindAsync(comprobanteId);
                if (comprobante == null)
                {
                    _logger.LogWarning("Comprobante {ComprobanteId} no encontrado", comprobanteId);
                    return;
                }

                // Obtener configuración de SUNAT
                var sunatConfig = _configuration.GetSection("SunatConfiguration").Get<SunatConfigurationDto>();
                if (sunatConfig == null)
                {
                    _logger.LogError("Configuración de SUNAT no encontrada");
                    return;
                }

                // Obtener token de SUNAT
                var tokenResponse = await _sunatTokenService.ObtenerTokenAsync(sunatConfig.ClientId, sunatConfig.ClientSecret);
                if (!tokenResponse.Success || string.IsNullOrEmpty(tokenResponse.Data?.access_token))
                {
                    _logger.LogError("No se pudo obtener token de SUNAT: {Message}", tokenResponse.Message);
                    comprobante.ResultadoSunat = $"Error de autenticación: {tokenResponse.Message}";
                    await _context.SaveChangesAsync();
                    return;
                }

                var token = tokenResponse.Data.access_token;

                // Preparar request de validación
                var request = new SunatComprobanteRequestDto
                {
                    numRuc = comprobante.Ruc?.ToString() ?? string.Empty,
                    codComp = comprobante.TipoComprobante?.PadLeft(2, '0') ?? "01",
                    numeroSerie = comprobante.Serie ?? string.Empty,
                    numero = comprobante.Correlativo ?? string.Empty,
                    fechaEmision = comprobante.FechaEmision?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy"),
                    monto = comprobante.Monto?.ToString("F2") ?? "0.00"
                };

                _logger.LogInformation("Validando en SUNAT - RUC: {RUC}, Serie: {Serie}, Número: {Numero}", 
                    request.numRuc, request.numeroSerie, request.numero);

                // Validar en SUNAT
                var result = await _sunatComprobanteService.ValidarComprobanteAsync(sunatConfig.RUC, token, request);

                // Actualizar comprobante con resultado
                if (result.Success && result.Data != null)
                {
                    comprobante.ValidoSunat = result.Data.data.estadoCp == "1";
                    comprobante.ResultadoSunat = result.Data.data.estadoCp == "1" 
                        ? "VÁLIDO" 
                        : $"NO VÁLIDO - Estado: {result.Data.data.estadoCp}, Observaciones: {string.Join(", ", result.Data.data.observaciones ?? new string[]{})}";

                    _logger.LogInformation("Comprobante {ComprobanteId} validado: {Resultado}", 
                        comprobanteId, comprobante.ResultadoSunat);
                }
                else
                {
                    comprobante.ValidoSunat = false;
                    comprobante.ResultadoSunat = $"Error en validación: {result.Message}";
                    _logger.LogWarning("Error al validar comprobante {ComprobanteId}: {Message}", 
                        comprobanteId, result.Message);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Validación SUNAT completada para comprobante {ComprobanteId}", comprobanteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar comprobante {ComprobanteId} en SUNAT", comprobanteId);
                
                // Actualizar comprobante con error
                try
                {
                    var comprobante = await _context.ComprobantesPago.FindAsync(comprobanteId);
                    if (comprobante != null)
                    {
                        comprobante.ValidoSunat = false;
                        comprobante.ResultadoSunat = $"Error de sistema: {ex.Message}";
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Error al actualizar comprobante {ComprobanteId} con error", comprobanteId);
                }
            }
        }
    }
}