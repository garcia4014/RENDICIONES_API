using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
 
namespace ContabilidadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SviaticoController : ControllerBase
    {
        private readonly ISviaticoService _SviaticoService;
        private readonly INotificacionesService _notificacionesService;
        private readonly ISviatico _dao;
        public SviaticoController(ISviatico dao,ISviaticoService sviaticoService, INotificacionesService notificacionesService)
        {
            _SviaticoService = sviaticoService;
            _notificacionesService = notificacionesService;
            _dao = dao;
        }
        // GET: api/<SviaticoController>
        [HttpGet]
        public async Task<IActionResult> GetViaticos()
        {
            try
            {
                var response = await _SviaticoService.GetListSviaticosCabecera();

                if (response.Data == null)
                    return NotFound(response);

                return Ok(response);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }

        [HttpGet("codeUsuario/{idDocumento}")]
        public async Task<IActionResult> GetViaticosDNI(string idDocumento)
        {
            try
            {
                var response = await _SviaticoService.GetListSviaticosCabeceraDNI(idDocumento);

                if (response.Data == null)
                    return NotFound(response);

                return Ok(response);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }

        // GET api/<SviaticoController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetViatico(string idDocumento)
        {
            try
            {
                var response = await _SviaticoService.GetListSviaticosCabeceraDNI(idDocumento);

                if (response.Data == null)
                    return NotFound(response);

                return Ok(response);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }

        [HttpGet("V2/{id}")]
        public async Task<IActionResult> GetViaticoV2(int id)
        {
            try
            {
                var response = await _SviaticoService.GetSviaticoCabeceraV2(id);

                if (response.Data == null)
                    return NotFound(response);

                return Ok(response);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(SviaticosCabeceraDTO request)
        {
            var response = await _SviaticoService.SaveCabecera(request);

            if (response == null)
            {
                return BadRequest();
            }

            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetViatico),
                new
                {
                    id = response.Data?.SvId,
                    nroSerie = response.Data?.SvNumero
                }, response);
        }

        [HttpGet("Detalle/{id}")]
        public async Task<IActionResult> GetViaticoDetalle(int id)
        {
            try
            {
                var response = await _SviaticoService.GetSviaticosDetalle(id);

                if (response.Data == null)
                    return NotFound(response);

                return Ok(response);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCabecera(SviaticosCabeceraUpdateDTO request)
        {


            if (request == null)
                return BadRequest("Id invalido o datos requeridos");

            var response = await _SviaticoService.UpdateCabecera(request);

            if (!response.Success)
                return NotFound(response);

            return Ok(
                new
                {
                    message = "Viatico actualizado correctamente",
                    id = response.Data?.SvId,
                    nroSerie = response.Data?.SvNumero,
                    data = response.Data
                });
        }
         
        [HttpGet("dashboard/{codigoUsuario}")]
        [ProducesResponseType(typeof(ApiResponse<ViaticoDashboardDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> GetDashboardEstadisticas(string codigoUsuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigoUsuario))
                {
                    return BadRequest(new ApiResponse<string>("El código de usuario es requerido"));
                }

                var response = await _SviaticoService.GetDashboardEstadisticas(codigoUsuario);

                if (!response.Success)
                    return BadRequest(response);

                if (response.Data == null)
                    return NotFound(new ApiResponse<string>("No se encontraron datos para el usuario especificado"));

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }
         
        [HttpGet("filtrar")]
        [ProducesResponseType(typeof(ApiResponse<ViaticosFiltradosResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> GetViaticosFiltrados(
            [FromQuery] string? svEmpDni = null,
            [FromQuery] DateTime? fechaCreacionInicio = null,
            [FromQuery] DateTime? fechaCreacionFin = null,
            [FromQuery] string? estados = null,
            [FromQuery] string? svDescripcion = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 50)
        {
            try
            {
                if (pagina < 1)
                {
                    return BadRequest(new ApiResponse<string>("La página debe ser mayor a 0"));
                }

                if (tamanoPagina < 1 || tamanoPagina > 100)
                {
                    return BadRequest(new ApiResponse<string>("El tamaño de página debe estar entre 1 y 100"));
                }

                if (!fechaCreacionInicio.HasValue)
                {
                    fechaCreacionInicio = DateTime.Now.AddMonths(-6).Date; // Hace 6 meses
                }

                if (!fechaCreacionFin.HasValue)
                {
                    fechaCreacionFin = DateTime.Now.Date; // Hoy
                }

                if (fechaCreacionInicio.HasValue && fechaCreacionFin.HasValue && fechaCreacionInicio > fechaCreacionFin)
                {
                    return BadRequest(new ApiResponse<string>("La fecha de inicio no puede ser mayor a la fecha de fin"));
                }

                int[]? estadosArray = null;
                if (!string.IsNullOrEmpty(estados))
                {
                    try
                    {
                        estadosArray = estados.Split(',')
                            .Where(e => !string.IsNullOrWhiteSpace(e))
                            .Select(e => int.Parse(e.Trim()))
                            .ToArray();
                    }
                    catch (FormatException)
                    {
                        return BadRequest(new ApiResponse<string>("Los estados deben ser números enteros separados por coma"));
                    }
                }

                var filtro = new SviaticoFiltroDto
                {
                    SvEmpDni = svEmpDni,
                    FechaCreacionInicio = fechaCreacionInicio,
                    FechaCreacionFin = fechaCreacionFin,
                    Estados = estadosArray,
                    SvDescripcion = svDescripcion,
                    Pagina = pagina,
                    TamanoPagina = tamanoPagina
                };

                var response = await _SviaticoService.GetViaticosFiltradosConConteo(filtro);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }

        [HttpPut("{sviaticoId}/rendicion-cerrada")]
        [ProducesResponseType(typeof(ApiResponse<SviaticosCabeceraDTOResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> RendicionCerrada(
            [FromRoute] int sviaticoId,
            [FromBody] ActualizarEstadoRendicionDto? request = null)
        {
            try
            {
                if (sviaticoId <= 0)
                {
                    return BadRequest(new ApiResponse<string>("ID de viático inválido"));
                }

                string comentario = string.Empty;
                if (request != null && !string.IsNullOrWhiteSpace(request.Comentario))
                {
                    comentario += request.Comentario;
                }

                var response = await _SviaticoService.ActualizarEstadoSolicitud(sviaticoId, 9, comentario);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                if (response.Data == null)
                {
                    return NotFound(new ApiResponse<string>("Viático no encontrado"));
                }

                var cabecera = await _dao.GetSviaticosCabecera(sviaticoId);
                var createDto = new NotificacionCreateDto()
                {
                    CodUsuReceptor = cabecera.SvEmpDni ?? string.Empty,
                    UsuarioReceptor = null,
                    CodUsuValidador = null,
                    UsuarioValidador = null,
                    Mensaje = $"Solicitud #{cabecera.SvId} - la rendición ha sido aprobada y cerrada ",
                    Leido = false,
                    EstadoFlujo = 9
                };

                var responseTMP = await _notificacionesService.CreateAsync(createDto);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }

        [HttpPut("{sviaticoId}/rendicion-observada")]
        [ProducesResponseType(typeof(ApiResponse<SviaticosCabeceraDTOResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> RendicionObservada(
            [FromRoute] int sviaticoId,
            [FromBody] ActualizarEstadoObservacionDto? request = null)
        {
            try
            {
                if (sviaticoId <= 0)
                {
                    return BadRequest(new ApiResponse<string>("ID de viático inválido"));
                }

                string comentario = string.Empty;
                if (request != null && !string.IsNullOrWhiteSpace(request.Comentario))
                {
                    comentario = request.Comentario;
                }

                var response = await _SviaticoService.ActualizarEstadoSolicitud(sviaticoId, 9, comentario);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                if (response.Data == null)
                {
                    return NotFound(new ApiResponse<string>("Viático no encontrado"));
                }

                var cabecera = await _dao.GetSviaticosCabecera(sviaticoId);
                var createDto = new NotificacionCreateDto()
                {
                    CodUsuReceptor = cabecera.SvEmpDni ?? string.Empty,
                    UsuarioReceptor = null,
                    CodUsuValidador = null,
                    UsuarioValidador = null,
                    Mensaje = $"Solicitud #{cabecera.SvId} - la rendición ha sido observada - ${comentario} ",
                    Leido = false,
                    EstadoFlujo = 8
                };

                var responseTMP = await _notificacionesService.CreateAsync(createDto);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }
         
        [HttpPut("detalle/{svdId}/observado")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> MarcarDetalleObservado(
            [FromRoute] int svdId,
            [FromBody] ActualizarEstadoObservacionDto comentarios)
        {
            try
            {
                ActualizarCampoDetalleDto request = new() { Valor = true };
                if (svdId <= 0)
                {
                    return BadRequest(new ApiResponse<string>("ID de detalle inválido"));
                }

                if (request == null)
                {
                    return BadRequest(new ApiResponse<string>("Los datos de la solicitud son requeridos"));
                }

                var response = await _SviaticoService.ActualizarDetalleObservado(svdId, request.Valor, comentarios.Comentario);

                if (!response.Success)
                {
                    if (response.Message.Contains("No se encontró"))
                    {
                        return NotFound(response);
                    }
                    return BadRequest(response);
                }
                var detalle = await _dao.GetSviaticosDetalle(svdId);
                var cabecera = await _dao.GetSviaticosCabecera(detalle.SvdIdCabecera);
                var createDto = new NotificacionCreateDto()
                {
                    CodUsuReceptor = cabecera.SvEmpDni ?? string.Empty,
                    UsuarioReceptor = null,
                    CodUsuValidador = null,
                    UsuarioValidador = null,
                    Mensaje = $"Solicitud #{cabecera.SvId} - el detalle ${detalle.SvdDescripcion} ha sido observado ",
                    Leido = false,
                    EstadoFlujo = 8
                };

                var responseTMP = await _notificacionesService.CreateAsync(createDto);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }

        [HttpPut("detalle/{svdId}/aprobado")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        public async Task<IActionResult> MarcarDetalleAprobado(
            [FromRoute] int svdId)
        {
            try
            {
                ActualizarCampoDetalleDto request = new() { Valor = true }; 
                if (svdId <= 0)
                {
                    return BadRequest(new ApiResponse<string>("ID de detalle inválido"));
                }

                if (request == null)
                {
                    return BadRequest(new ApiResponse<string>("Los datos de la solicitud son requeridos"));
                }

                var response = await _SviaticoService.ActualizarDetalleAprobado(svdId, request.Valor);

                if (!response.Success)
                {
                    if (response.Message.Contains("No se encontró"))
                    {
                        return NotFound(response);
                    }
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }
    }


    public class ActualizarEstadoRendicionDto
    {
        public string? Comentario { get; set; }
    }

    public class ActualizarEstadoObservacionDto
    {
        public string? Comentario { get; set; }
    }

    public class ActualizarCampoDetalleDto
    {
        [Required(ErrorMessage = "El valor es requerido")]
        public bool Valor { get; set; }
    }
}
