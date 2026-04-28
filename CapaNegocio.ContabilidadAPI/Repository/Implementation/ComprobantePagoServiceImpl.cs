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
                
                // Validar duplicidad por RUC + serie + correlativo
                if (createDto.Ruc.HasValue && await ExisteDuplicadoPorRucAsync(createDto.Ruc.Value, createDto.Serie, createDto.Correlativo))
                {
                    return new ApiResponse<ComprobantePagoDto>(null, $"Ya existe un comprobante del RUC {createDto.Ruc} con la misma serie y correlativo");
                }

                //await _comprobantePagoDao.InactiveVoucherPrevius(createDto.SvIdCabecera, createDto.SvIdDetalle);

                var comprobante = _mapper.Map<ComprobantePago>(createDto);
                
                // Desglosado viene desde el frontend basado en la respuesta del OCR (afectacionIgvDetectada)
                // No se recalcula, solo se usa el valor recibido
                comprobante.Desglosado = createDto.Desglosado ?? false;
                
                // Calcular IGV total basándose en los montos específicos
                // MontoGravado  = base imponible gravado 18%
                // MontoIgvEspecial = base imponible IGV especial (tasa reducida ≈ 10%)
                decimal igvGravado = (comprobante.MontoGravado ?? 0) * 0.18m;
                decimal igvEspecial = (comprobante.MontoIgvEspecial ?? 0) * 0.10m;
                decimal igvTotal = igvGravado + igvEspecial;
                
                comprobante.Igv = igvTotal;
                
                // Calcular IgvPorcentaje según los tipos de afectación presentes
                bool tieneGravado = (comprobante.MontoGravado ?? 0) > 0;
                bool tieneEspecial = (comprobante.MontoIgvEspecial ?? 0) > 0;
                bool tieneExonerado = (comprobante.MontoExonerado ?? 0) > 0;
                bool tieneInafecto = (comprobante.MontoInafecto ?? 0) > 0;
                bool tieneOtrosCargos = (comprobante.MontoOtrosCargos ?? 0) > 0;
                
                // Determinar porcentaje efectivo sobre la base total imponible
                if (tieneGravado && tieneEspecial)
                {
                    decimal baseTotal = (comprobante.MontoGravado ?? 0) + (comprobante.MontoIgvEspecial ?? 0);
                    comprobante.IgvPorcentaje = baseTotal > 0 ? Math.Round((igvTotal / baseTotal) * 100, 2) : 0;
                }
                else if (tieneEspecial)
                {
                    comprobante.IgvPorcentaje = 10;
                    // Subtotal = base imponible IGV especial (el background job lo corregirá con el valor exacto de SUNAT)
                    comprobante.Subtotal = comprobante.MontoIgvEspecial;
                }
                else if (tieneExonerado || tieneInafecto || tieneOtrosCargos)
                {
                    comprobante.IgvPorcentaje = 0;
                }
                else if (tieneGravado)
                {
                    comprobante.IgvPorcentaje = 18;
                    comprobante.Subtotal = comprobante.MontoGravado;
                }
                else
                {
                    // Por defecto (caso legacy sin montos específicos)
                    if (comprobante.IgvEspecial == true)
                        comprobante.IgvPorcentaje = 10;
                    else if (comprobante.Exonerado == true || comprobante.Inafecto == true)
                        comprobante.IgvPorcentaje = 0;
                    else
                        comprobante.IgvPorcentaje = 18;
                }

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
                    if (updateDto.Ruc.HasValue && await ExisteDuplicadoPorRucAsync(updateDto.Ruc.Value, updateDto.Serie, updateDto.Correlativo, updateDto.Id))
                    {
                        return new ApiResponse<ComprobantePagoDto>(null, $"Ya existe otro comprobante del RUC {updateDto.Ruc} con la misma serie y correlativo");
                    }
                }

                var comprobante = _mapper.Map<ComprobantePago>(updateDto);
                
                // Preservar campos que no deben sobrescribirse con NULL si no fueron enviados
                if (string.IsNullOrEmpty(updateDto.TipoComprobante))
                {
                    comprobante.TipoComprobante = comprobanteExistente.TipoComprobante;
                }
                
                if (string.IsNullOrEmpty(updateDto.Descripcion))
                {
                    comprobante.Descripcion = comprobanteExistente.Descripcion;
                }
                
                // Preservar SvIdDetalle si no se envió (no debe sobrescribirse con NULL)
                if (updateDto.SvIdDetalle == null)
                {
                    comprobante.SvIdDetalle = comprobanteExistente.SvIdDetalle;
                }
                
                // Preservar SvTgId si no se envió (no debe sobrescribirse con NULL)
                if (updateDto.SvTgId == null)
                {
                    comprobante.SvTgId = comprobanteExistente.SvTgId;
                }
                
                // Verificar si cambiaron los datos que identifican el comprobante en SUNAT
                // Solo Serie, Correlativo y RUC; el Monto puede cambiar sin afectar la identidad del comprobante
                bool datosIdentidadCambiaron = 
                    comprobanteExistente.Serie != updateDto.Serie ||
                    comprobanteExistente.Correlativo != updateDto.Correlativo ||
                    comprobanteExistente.Ruc != updateDto.Ruc;
                
                // Verificar si el tipo de comprobante cambió
                bool tipoComprobanteCambio = comprobanteExistente.TipoComprobante != updateDto.TipoComprobante;
                
                // La ruta solo se limpia si:
                // 1. Se envió una nueva ruta (archivo nuevo subido)
                // 2. O cambiaron los datos de identidad Y el tipo es validado por SUNAT 
                //    Y el PDF actual fue descargado de SUNAT (PdfSunat = true)
                //    (necesitamos descargar el PDF correcto con los nuevos datos)
                // 
                // IMPORTANTE: Si el PDF fue subido MANUALMENTE (PdfSunat = false),
                // se PRESERVA siempre, porque sigue siendo válido aunque cambien
                // el tipo de comprobante o los datos (Serie, Correlativo, RUC)
                if (!string.IsNullOrEmpty(updateDto.Ruta) && updateDto.Ruta.Length > 10)
                {
                    // Se subió un nuevo archivo, actualizar la ruta
                    comprobante.Ruta = updateDto.Ruta;
                    comprobante.PdfSunat = false; // Es un PDF subido manualmente
                    comprobante.ReintentosPdfSunat = 0;
                }
                else if (datosIdentidadCambiaron && 
                         EsTipoValidadoPorSunat(updateDto.TipoComprobante) && 
                         comprobanteExistente.PdfSunat == true)
                {
                    // Si cambiaron los datos de identidad Y es un tipo validado por SUNAT
                    // Y el PDF actual fue descargado de SUNAT,
                    // limpiar para buscar el PDF correcto desde SUNAT con los nuevos datos
                    comprobante.Ruta = null;
                    comprobante.PdfSunat = false;
                    comprobante.ReintentosPdfSunat = 0;
                }
                else
                {
                    // En cualquier otro caso, preservar la ruta existente:
                    // - El PDF fue subido manualmente (se preserva siempre)
                    // - No cambiaron datos de identidad
                    // - Es un tipo manual que no se valida por SUNAT
                    comprobante.Ruta = comprobanteExistente.Ruta;
                    comprobante.PdfSunat = comprobanteExistente.PdfSunat;
                    comprobante.ReintentosPdfSunat = comprobanteExistente.ReintentosPdfSunat;
                }
                
                comprobante.ValidoSunat = false;
                
                // Desglosado viene desde el frontend basado en la respuesta del OCR (afectacionIgvDetectada)
                // No se recalcula, solo se usa el valor recibido
                comprobante.Desglosado = updateDto.Desglosado ?? false;
                
                // Calcular IGV total basándose en los montos específicos
                // MontoGravado  = base imponible gravado 18%
                // MontoIgvEspecial = base imponible IGV especial (tasa reducida ≈ 10%)
                decimal igvGravado = (comprobante.MontoGravado ?? 0) * 0.18m;
                decimal igvEspecial = (comprobante.MontoIgvEspecial ?? 0) * 0.10m;
                decimal igvTotal = igvGravado + igvEspecial;
                
                comprobante.Igv = igvTotal;
                
                // Calcular IgvPorcentaje según los tipos de afectación presentes
                bool tieneGravado = (comprobante.MontoGravado ?? 0) > 0;
                bool tieneEspecial = (comprobante.MontoIgvEspecial ?? 0) > 0;
                bool tieneExonerado = (comprobante.MontoExonerado ?? 0) > 0;
                bool tieneInafecto = (comprobante.MontoInafecto ?? 0) > 0;
                bool tieneOtrosCargos = (comprobante.MontoOtrosCargos ?? 0) > 0;
                
                // Determinar porcentaje efectivo sobre la base total imponible
                if (tieneGravado && tieneEspecial)
                {
                    decimal baseTotal = (comprobante.MontoGravado ?? 0) + (comprobante.MontoIgvEspecial ?? 0);
                    comprobante.IgvPorcentaje = baseTotal > 0 ? Math.Round((igvTotal / baseTotal) * 100, 2) : 0;
                }
                else if (tieneEspecial)
                {
                    comprobante.IgvPorcentaje = 10;
                    comprobante.Subtotal = comprobante.MontoIgvEspecial;
                }
                else if (tieneExonerado || tieneInafecto || tieneOtrosCargos)
                {
                    comprobante.IgvPorcentaje = 0;
                }
                else if (tieneGravado)
                {
                    comprobante.IgvPorcentaje = 18;
                    comprobante.Subtotal = comprobante.MontoGravado;
                }
                else
                {
                    // Por defecto (caso legacy sin montos específicos)
                    if (comprobante.IgvEspecial == true)
                        comprobante.IgvPorcentaje = 10;
                    else if (comprobante.Exonerado == true || comprobante.Inafecto == true)
                        comprobante.IgvPorcentaje = 0;
                    else
                        comprobante.IgvPorcentaje = 18;
                }

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
                if (serie == null && correlativo == null) return false;

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
        /// Valida la duplicidad de un comprobante por RUC, serie y correlativo
        /// </summary>
        public async Task<bool> ExisteDuplicadoPorRucAsync(long ruc, string serie, string correlativo, int? idExcluir = null)
        {
            try
            {
                if (string.IsNullOrEmpty(serie) && string.IsNullOrEmpty(correlativo)) return false;

                var comprobantes = await _comprobantePagoDao.GetByRucSerieCorrelattivoAsync(ruc, serie, correlativo);

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
                "01F" => "Factura Física",
                "03" => "Boleta de Venta",
                "03F" => "Boleta Física",
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
        /// Determina si un tipo de comprobante es validado por SUNAT
        /// </summary>
        private bool EsTipoValidadoPorSunat(string? tipoComprobante)
        {
            if (string.IsNullOrEmpty(tipoComprobante))
                return false;

            // Tipos ELECTRÓNICOS validados por SUNAT
            var tiposValidadosPorSunat = new HashSet<string>
            {
                "01",  // Factura Electrónica
                "03",  // Boleta Electrónica
                "RH",  // Recibo por Honorarios
                "07",  // Nota de Crédito
                "08",  // Nota de Débito
                "09",  // Guía de Remisión
                "20",  // Comprobante de Retención
                "40"   // Comprobante de Percepción
            };

            return tiposValidadosPorSunat.Contains(tipoComprobante.ToUpper());
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