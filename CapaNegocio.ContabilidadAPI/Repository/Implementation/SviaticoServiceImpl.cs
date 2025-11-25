using AutoMapper;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    public class SviaticoServiceImpl : ISviaticoService
    {
        private readonly ISviatico _dao;
        private readonly IMapper _mapper;
        private readonly INotificacionesService _notificacionesService;

        public SviaticoServiceImpl(ISviatico sviatico, IMapper mapper, INotificacionesService notificacionesService)
        {
            _dao = sviatico;
            _mapper = mapper;
            _notificacionesService = notificacionesService;
        }


        public async Task<ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>> GetListSviaticosCabecera()
        {
            try
            {
                var list = await _dao.GetListSviaticosCabecera();
                if (list == null)
                    return new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>("No se encontre el viatico con el siguiente codigo");


                var listCabeceraMapper = _mapper.Map<List<SviaticosCabeceraDTOResponse>>(list);
                return new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>(listCabeceraMapper);
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>($"Error: {ex.Message}");
            }

        }
        public async Task<ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>> GetListSviaticosCabeceraDNI(string idDocumento)
        {
            try
            {
                var list = await _dao.GetListSviaticosCabeceraDNI(idDocumento);
                if (list == null)
                    return new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>("No se encontre el viatico con el siguiente codigo");


                var listCabeceraMapper = _mapper.Map<List<SviaticosCabeceraDTOResponse>>(list);
                return new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>(listCabeceraMapper);
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>($"Error: {ex.Message}");
            }

        }

        public async Task<ApiResponse<SviaticosCabeceraDTOResponse>> GetSviaticoCabecera(int id)
        {
            try
            {
                var viatico = await _dao.GetSviaticosCabecera(id);
                if (viatico == null)
                    return new ApiResponse<SviaticosCabeceraDTOResponse>("No se encontre el viatico con el siguiente codigo");


                var cabeceraMapper = _mapper.Map<SviaticosCabeceraDTOResponse>(viatico);
                return new ApiResponse<SviaticosCabeceraDTOResponse>(cabeceraMapper);
            }
            catch (Exception ex)
            {
                return new ApiResponse<SviaticosCabeceraDTOResponse>($"Error: {ex.Message}");
            }

        }


        public async Task<ApiResponse<SviaticosCabecerav2DTOResponse>> GetSviaticoCabeceraV2(int id)
        {
            try
            {
                var (viatico, empleado) = await _dao.GetSviaticosCabeceraV2(id);
                if (viatico == null)
                    return new ApiResponse<SviaticosCabecerav2DTOResponse>("No se encontre el viatico con el siguiente codigo");


                var cabeceraMapper = _mapper.Map<SviaticosCabecerav2DTOResponse>(viatico);
                cabeceraMapper.Empleado = empleado != null ? _mapper.Map<EmpleadoDTO>(empleado) : null;
                return new ApiResponse<SviaticosCabecerav2DTOResponse>(cabeceraMapper);
            }
            catch (Exception ex)
            {
                return new ApiResponse<SviaticosCabecerav2DTOResponse>($"Error: {ex.Message}");
            }

        }

        public async Task<ApiResponse<SviaticosCabeceraDTOResponse>> SaveCabecera(SviaticosCabeceraDTO dto)
        {
            try
            {
                int ultimoId = (await _dao.GetListSviaticosCabecera()).Count + 1;
                string numeroCorrelativo = $"SV-{ultimoId:D5}";

                var cabecera = new SviaticosCabecera
                {
                    SvNumero = numeroCorrelativo,
                    SvEmpCodigo = dto.SvEmpCodigo,
                    SvEmpDni = dto.SvEmpDni,
                    SvFechaInicio = dto.SvFechaInicio,
                    SvFechaRetorno = dto.SvFechaRetorno,
                    SvEmpCantidad = dto.SvEmpCantidad,
                    SvNumeroDias = dto.SvNumeroDias,
                    SvOrdenVenta = dto.SvOrdenVenta,
                    SvDescripcion = dto.SvDescripcion,
                    SvRuc = dto.SvRuc,
                    SvContacto = dto.SvContacto,
                    SvObjetivoVisita = dto.SvObjetivoVisita,
                    SvLocalidad = dto.SvLocalidad,
                    SvTotalSolicitado = dto.SvTotalSolicitado,
                    SvSefId = 1,
                    SvEmpresa = dto.SvEmpresa,
                    SvPersonaEntrevistar = dto.SvPersonaEntrevistar,
                    SvPoliticas = dto.SvPoliticas,

                    Detalles = dto.Detalles.Select(d => new SviaticosDetalle
                    {
                        SvdNumeroCabecera = numeroCorrelativo,
                        SvdTgId = d.SvdTgId,
                        SvdPrecioUnitario = d.SvdPrecioUnitario,
                        SvdImporteSolicitado = d.SvdImporteSolicitado,
                        SvdSubtotal = d.SvdSubtotal,
                        SvdDescripcion = d.SvdDescripcion,
                        SvdCantEmpleado = d.SvdCantEmpleado,
                        SvdFechaInicio = d.SvdFechaInicio,
                        SvdFechaFin = d.SvdFechaFin,
                        SvdNumeroDias = d.SvdNumeroDias,
                        SvdKilometraje = d.SvdKilometraje,
                        Observado = false,
                        Aprobado = false,
                    }).ToList()
                };

                bool resultado = await _dao.SaveCabecera(cabecera);

                if (!resultado)
                    return new ApiResponse<SviaticosCabeceraDTOResponse>("Error al guardar");

                var createDto = new NotificacionCreateDto()
                {
                    CodUsuReceptor = dto.SvEmpDni ?? string.Empty,
                    UsuarioReceptor = null,
                    CodUsuValidador = null,
                    UsuarioValidador = null,
                    Mensaje = $"Solicitud #{numeroCorrelativo} -{dto.SvDescripcion} ",
                    Leido = false,
                    EstadoFlujo = 1
                };
                 
                var response = await _notificacionesService.CreateAsync(createDto);
                 
                var cabeceraMapper = _mapper.Map<SviaticosCabeceraDTOResponse>(cabecera);
    
                return new ApiResponse<SviaticosCabeceraDTOResponse>(cabeceraMapper);
            }
            catch (Exception e)
            {
                return new ApiResponse<SviaticosCabeceraDTOResponse>($"Error: {e.Message}");
            }

        }

        public async Task<ApiResponse<SviaticosCabeceraDTOResponse>> UpdateCabecera(SviaticosCabeceraUpdateDTO dto)
        {
            try
            {
                var cabecera = await _dao.GetSviaticosCabecera(dto.SvId);
                if (cabecera == null)
                    return new ApiResponse<SviaticosCabeceraDTOResponse>("Cabecera no encontrada");

                if (dto.Observado == "1")
                {
                    dto.SvSefId = 1; 
                }

                // Mapear los cambios de la cabecera
                cabecera.SvEmpCodigo = dto.SvEmpCodigo;
                cabecera.SvEmpDni = dto.SvEmpDni;
                cabecera.SvFechaInicio = dto.SvFechaInicio;
                cabecera.SvFechaRetorno = dto.SvFechaRetorno;
                cabecera.SvEmpCantidad = dto.SvEmpCantidad;
                cabecera.SvNumeroDias = dto.SvNumeroDias;
                cabecera.SvOrdenVenta = dto.SvOrdenVenta;
                cabecera.SvDescripcion = dto.SvDescripcion;
                cabecera.SvRuc = dto.SvRuc;
                cabecera.SvContacto = dto.SvContacto;
                cabecera.SvObjetivoVisita = dto.SvObjetivoVisita;
                cabecera.SvLocalidad = dto.SvLocalidad;
                cabecera.SvTotalSolicitado = dto.SvTotalSolicitado;
                cabecera.SvSefId = dto.SvSefId;
                cabecera.SvEmpresa = dto.SvEmpresa;
                cabecera.SvPersonaEntrevistar = dto.SvPersonaEntrevistar;
                cabecera.SvPoliticas = dto.SvPoliticas;

                var detalles = dto.Detalles.Select(d => new SviaticosDetalle
                {
                    SvdId = d.SvdId,
                    SvdIdCabecera = cabecera.SvId,
                    SvdNumeroCabecera = cabecera.SvNumero,
                    SvdTgId = d.SvdTgId,
                    SvdPrecioUnitario = d.SvdPrecioUnitario,
                    SvdImporteSolicitado = d.SvdImporteSolicitado,
                    SvdSubtotal = d.SvdSubtotal,
                    SvdDescripcion = d.SvdDescripcion,
                    SvdCantEmpleado = d.SvdCantEmpleado,
                    SvdFechaInicio = d.SvdFechaInicio,
                    SvdFechaFin = d.SvdFechaFin,
                    SvdNumeroDias = d.SvdNumeroDias,
                    SvdKilometraje = d.SvdKilometraje,

                }).ToList();

                bool result = await _dao.UpdateCabecera(cabecera, detalles);

                if (!result)
                    return new ApiResponse<SviaticosCabeceraDTOResponse>("Error al actualizar");

                var cabeceraMapper = _mapper.Map<SviaticosCabeceraDTOResponse>(cabecera);


                var createDto = new NotificacionCreateDto()
                {
                    CodUsuReceptor = cabecera.SvEmpDni ?? string.Empty,
                    UsuarioReceptor = null,
                    CodUsuValidador = null,
                    UsuarioValidador = null,
                    Mensaje = $"Solicitud #{cabecera.SvId} - enviado como subsanado",
                    Leido = false,
                    EstadoFlujo = 1
                };

                var responseTMP = await _notificacionesService.CreateAsync(createDto);


                return new ApiResponse<SviaticosCabeceraDTOResponse>(cabeceraMapper);
            }
            catch (Exception e)
            {
                return new ApiResponse<SviaticosCabeceraDTOResponse>($"Error: {e.Message}");
            }
        }

        //Detalle
        public Task<IEnumerable<SviaticosDetalle>> GetListSviaticosDetalle()
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<SviaticosDetalleDTOResponse>> GetSviaticosDetalle(int id)
        {
            try
            {
                var detalle = await _dao.GetSviaticosDetalle(id);
                if (detalle == null)
                    return new ApiResponse<SviaticosDetalleDTOResponse>("No se encontre el detalle con el siguiente codigo");


                var detalleMapper = _mapper.Map<SviaticosDetalleDTOResponse>(detalle);
                return new ApiResponse<SviaticosDetalleDTOResponse>(detalleMapper);
            }
            catch (Exception ex)
            {
                return new ApiResponse<SviaticosDetalleDTOResponse>($"Error: {ex.Message}");
            }
        }

        // Método para obtener viáticos filtrados
        public async Task<ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>> GetViaticosFiltrados(SviaticoFiltroDto filtro)
        {
            try
            {
                var viaticos = await _dao.GetViaticosFiltrados(
                    filtro.SvEmpDni,
                    filtro.FechaCreacionInicio,
                    filtro.FechaCreacionFin,
                    filtro.Estados,
                    filtro.SvDescripcion,
                    filtro.Pagina,
                    filtro.TamanoPagina
                );

                if (!viaticos.Any())
                {
                    return new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>(
                        new List<SviaticosCabeceraDTOResponse>(),
                        "No se encontraron viáticos que coincidan con los filtros especificados");
                }

                var viaticosMapper = _mapper.Map<IEnumerable<SviaticosCabeceraDTOResponse>>(viaticos);
                return new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>(viaticosMapper, 
                    $"Se encontraron {viaticos.Count} viáticos que coinciden con los filtros");
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>($"Error: {ex.Message}");
            }
        }

        // Método para obtener viáticos filtrados con conteo de estados
        public async Task<ApiResponse<ViaticosFiltradosResponseDto>> GetViaticosFiltradosConConteo(SviaticoFiltroDto filtro)
        {
            try
            {
                var (viaticos, conteoEstados) = await _dao.GetViaticosFiltradosConConteo(
                    filtro.SvEmpDni,
                    filtro.FechaCreacionInicio,
                    filtro.FechaCreacionFin,
                    filtro.Estados,
                    filtro.SvDescripcion,
                    filtro.Pagina,
                    filtro.TamanoPagina
                );

                var viaticosMapper = _mapper.Map<IEnumerable<SviaticosCabeceraDTOResponse>>(viaticos);

                var conteoEstadosDto = new ConteoEstadosDto
                {
                    Solicitado = conteoEstados.GetValueOrDefault(1, 0),
                    Abierto = conteoEstados.GetValueOrDefault(2, 0),
                    Aprobado = conteoEstados.GetValueOrDefault(3, 0),
                    Rechazado = conteoEstados.GetValueOrDefault(4, 0),
                    PagoEfectuado = conteoEstados.GetValueOrDefault(5, 0),
                    Observado = conteoEstados.GetValueOrDefault(6, 0),
                    Rendido = conteoEstados.GetValueOrDefault(7, 0),
                    RendicionObservada = conteoEstados.GetValueOrDefault(8, 0),
                    RendicionCerrada = conteoEstados.GetValueOrDefault(9, 0)
                };

                // Crear información de paginación
                PaginacionInfoDto? paginacionInfo = null;
                if (filtro.Pagina.HasValue && filtro.TamanoPagina.HasValue)
                {
                    var totalRegistros = conteoEstadosDto.Total;
                    var totalPaginas = (int)Math.Ceiling((double)totalRegistros / filtro.TamanoPagina.Value);
                    
                    paginacionInfo = new PaginacionInfoDto
                    {
                        PaginaActual = filtro.Pagina.Value,
                        TamanoPagina = filtro.TamanoPagina.Value,
                        TotalRegistros = totalRegistros,
                        TotalPaginas = totalPaginas,
                        TienePaginaAnterior = filtro.Pagina.Value > 1,
                        TienePaginaSiguiente = filtro.Pagina.Value < totalPaginas
                    };
                }

                // Crear respuesta
                var respuesta = new ViaticosFiltradosResponseDto
                {
                    Viaticos = viaticosMapper,
                    ConteoEstados = conteoEstadosDto,
                    PaginacionInfo = paginacionInfo
                };

                var mensaje = $"Se encontraron {viaticos.Count} viáticos en la página actual. Total de registros que coinciden con los filtros: {conteoEstadosDto.Total}";

                return new ApiResponse<ViaticosFiltradosResponseDto>(respuesta, mensaje);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ViaticosFiltradosResponseDto>($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SviaticosCabeceraDTOResponse>> ActualizarEstadoSolicitud(int sviaticoId, int nuevoEstadoId, string? comentario = null)
        {
            try
            {
                var sviatico = await _dao.GetSviaticosCabecera(sviaticoId);
                if (sviatico == null)
                    return new ApiResponse<SviaticosCabeceraDTOResponse>("No se encontró el viático con el ID especificado");

                // Actualizar el estado
                sviatico.SvSefId = nuevoEstadoId;
                sviatico.ComentariosRendicion = comentario;
                bool resultado = await _dao.UpdateEstadoSolicitud(sviaticoId, nuevoEstadoId);

                if (!resultado)
                    return new ApiResponse<SviaticosCabeceraDTOResponse>("Error al actualizar el estado del viático");
                 
                var sviaticoActualizado = await _dao.GetSviaticosCabecera(sviaticoId);
                var sviaticoMapper = _mapper.Map<SviaticosCabeceraDTOResponse>(sviaticoActualizado);

                var createDto = new NotificacionCreateDto()
                {
                    CodUsuReceptor = sviatico.SvEmpDni ?? string.Empty,
                    UsuarioReceptor = null,
                    CodUsuValidador = null,
                    UsuarioValidador = null,
                    Mensaje = $"Solicitud #{sviatico.SvId} - tuvo un cambio de estado ",
                    Leido = false,
                    EstadoFlujo = nuevoEstadoId
                };

                var response = await _notificacionesService.CreateAsync(createDto);
                  
                return new ApiResponse<SviaticosCabeceraDTOResponse>(sviaticoMapper, $"Estado actualizado exitosamente{(string.IsNullOrEmpty(comentario) ? "" : $". Comentario: {comentario}")}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<SviaticosCabeceraDTOResponse>($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<SolicitudEstadoFlujo>>> GetEstadosDisponibles()
        {
            try
            {
                var estados = await _dao.GetEstadosDisponibles();
                return new ApiResponse<IEnumerable<SolicitudEstadoFlujo>>(estados);
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<SolicitudEstadoFlujo>>($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SolicitudEstadoFlujo>> GetEstadoActual(int sviaticoId)
        {
            try
            {
                var sviatico = await _dao.GetSviaticosCabecera(sviaticoId);
                if (sviatico == null)
                    return new ApiResponse<SolicitudEstadoFlujo>("No se encontró el viático con el ID especificado");

                var estado = await _dao.GetEstadoPorId(sviatico.SvSefId);
                if (estado == null)
                    return new ApiResponse<SolicitudEstadoFlujo>("No se encontró el estado asociado");

                return new ApiResponse<SolicitudEstadoFlujo>(estado);
            }
            catch (Exception ex)
            {
                return new ApiResponse<SolicitudEstadoFlujo>($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ViaticoDashboardDto>> GetDashboardEstadisticas(string codigoUsuario)
        {
            try
            {
                if (string.IsNullOrEmpty(codigoUsuario))
                    return new ApiResponse<ViaticoDashboardDto>("El código de usuario es requerido");

                var estadisticas = await _dao.GetDashboardEstadisticas(codigoUsuario);

                var dashboard = new ViaticoDashboardDto
                {
                    ViaticosPendientes = estadisticas.pendientes,
                    RendicionesAprobadas = estadisticas.aprobadas,
                    EnRevision = estadisticas.enRevision,
                    TotalDelMes = estadisticas.totalMes,
                    CodigoUsuario = codigoUsuario,
                    FechaGeneracion = DateTime.Now,
                    PeriodoReporte = DateTime.Now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"))
                };

                return new ApiResponse<ViaticoDashboardDto>(dashboard, "Dashboard generado exitosamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<ViaticoDashboardDto>($"Error al generar dashboard: {ex.Message}");
            }
        }
         
        public async Task<ApiResponse<bool>> ActualizarDetalleObservado(int svdId, bool observado,string? comentario)
        {
            try
            {
                var resultado = await _dao.ActualizarDetalleObservado(svdId, observado, comentario);
                if (resultado)
                {
                    var tmp = await _dao.GetSviaticosDetalle(svdId);
                    var tmp2 = await _dao.GetSviaticosCabecera(tmp.SvdIdCabecera);
                    var createDto = new NotificacionCreateDto()
                    {
                        CodUsuReceptor = tmp2.SvEmpDni ?? string.Empty,
                        UsuarioReceptor = null,
                        CodUsuValidador = null,
                        UsuarioValidador = null,
                        Mensaje = $"Solicitud #{tmp2.SvId} - el item {tmp.SvdDescripcion} ha sido observado ",
                        Leido = false,
                        EstadoFlujo = 8
                    };

                    var response = await _notificacionesService.CreateAsync(createDto);

                    return new ApiResponse<bool>(true, $"Detalle {(observado ? "marcado como observado" : "desmarcado como observado")} exitosamente");
                }
                else
                {
                    return new ApiResponse<bool>(false, "No se encontró el detalle con el ID especificado");
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ActualizarDetalleAprobado(int svdId, bool aprobado)
        {
            try
            {
                var resultado = await _dao.ActualizarDetalleAprobado(svdId, aprobado);
                if (resultado)
                {
                    var tmp = await _dao.GetSviaticosDetalle(svdId);
                    var tmp2 = await _dao.GetSviaticosCabecera(tmp.SvdIdCabecera);
                    var createDto = new NotificacionCreateDto()
                    {
                        CodUsuReceptor = tmp2.SvEmpDni ?? string.Empty,
                        UsuarioReceptor = null,
                        CodUsuValidador = null,
                        UsuarioValidador = null,
                        Mensaje = $"Solicitud #{tmp2.SvId} - el detalle ${tmp.SvdDescripcion} ha sido aprobado ",
                        Leido = false,
                        EstadoFlujo = 3
                    };

                    var response = await _notificacionesService.CreateAsync(createDto);
                     
                    return new ApiResponse<bool>(true, $"Detalle {(aprobado ? "aprobado" : "desaprobado")} exitosamente");
                }
                else
                {
                    return new ApiResponse<bool>(false, "No se encontró el detalle con el ID especificado");
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, $"Error: {ex.Message}");
            }
        }

    }
}
