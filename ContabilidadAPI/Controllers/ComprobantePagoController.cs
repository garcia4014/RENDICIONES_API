using AutoMapper;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ContabilidadAPI.Controllers
{
    /// <summary>
    /// Controlador para gestión de comprobantes de pago
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ComprobantePagoController : ControllerBase
    {
        private readonly IComprobantePagoService _comprobantePagoService;
        private readonly ILogger<ComprobantePagoController> _logger;
        private readonly ISviatico _dao;
        private readonly IMapper _mapper;
        private readonly INotificacionesService _notificacionesService;

        public ComprobantePagoController(
            INotificacionesService notificacionesService,
            IMapper mapper,
            ISviatico dao,
            IComprobantePagoService comprobantePagoService,
            ILogger<ComprobantePagoController> logger)
        {
            _comprobantePagoService = comprobantePagoService;
            _logger = logger;
            _dao = dao;
            _mapper = mapper;
            _notificacionesService = notificacionesService;
        }
         
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ComprobantePagoDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ComprobantePagoDto>>), 400)]
        public async Task<IActionResult> GetAll([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            try
            {
                if (pagina <= 0 || tamanoPagina <= 0)
                {
                    return BadRequest(new ApiResponse<PagedResult<ComprobantePagoDto>>(null, "La página y el tamaño de página deben ser mayores a 0"));
                }

                if (tamanoPagina > 100)
                {
                    return BadRequest(new ApiResponse<PagedResult<ComprobantePagoDto>>(null, "El tamaño de página no puede ser mayor a 100"));
                }

                var resultado = await _comprobantePagoService.GetAllAsync(pagina, tamanoPagina);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comprobantes de pago");
                return StatusCode(500, new ApiResponse<PagedResult<ComprobantePagoDto>>(null, "Error interno del servidor"));
            }
        }
         
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ComprobantePagoDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ComprobantePagoDto>), 404)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<ComprobantePagoDto>(null, "El ID debe ser mayor a 0"));
                }

                var resultado = await _comprobantePagoService.GetByIdAsync(id);
                
                if (resultado.Data == null)
                {
                    return NotFound(resultado);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comprobante de pago por ID: {Id}", id);
                return StatusCode(500, new ApiResponse<ComprobantePagoDto>(null, "Error interno del servidor"));
            }
        }
         
        [HttpPost("buscar")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ComprobantePagoDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ComprobantePagoDto>>), 400)]
        public async Task<IActionResult> Buscar([FromBody] ComprobantePagoFiltroDto filtro)
        {
            try
            {
                if (filtro == null)
                {
                    return BadRequest(new ApiResponse<PagedResult<ComprobantePagoDto>>(null, "Los filtros son requeridos"));
                }

                if (filtro.Pagina <= 0 || filtro.TamanoPagina <= 0)
                {
                    return BadRequest(new ApiResponse<PagedResult<ComprobantePagoDto>>(null, "La página y el tamaño de página deben ser mayores a 0"));
                }

                if (filtro.TamanoPagina > 100)
                {
                    return BadRequest(new ApiResponse<PagedResult<ComprobantePagoDto>>(null, "El tamaño de página no puede ser mayor a 100"));
                }

                var resultado = await _comprobantePagoService.BuscarAsync(filtro);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar comprobantes de pago");
                return StatusCode(500, new ApiResponse<PagedResult<ComprobantePagoDto>>(null, "Error interno del servidor"));
            }
        }
         
        [HttpGet("cabecera/{cabeceraId}")]
        [ProducesResponseType(typeof(ApiResponse<List<ComprobantePagoDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<ComprobantePagoDto>>), 400)]
        public async Task<IActionResult> GetByCabeceraId([FromRoute] int cabeceraId)
        {
            try
            {
                if (cabeceraId <= 0)
                {
                    return BadRequest(new ApiResponse<List<ComprobantePagoDto>>(null, "El ID de cabecera debe ser mayor a 0"));
                }

                var resultado = await _comprobantePagoService.GetByCabeceraIdAsync(cabeceraId);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comprobantes por cabecera ID: {CabeceraId}", cabeceraId);
                return StatusCode(500, new ApiResponse<List<ComprobantePagoDto>>(null, "Error interno del servidor"));
            }
        }
         
        [HttpGet("detalle/{detalleId}")]
        [ProducesResponseType(typeof(ApiResponse<List<ComprobantePagoDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<ComprobantePagoDto>>), 400)]
        public async Task<IActionResult> GetByDetalleId([FromRoute] int detalleId)
        {
            try
            {
                if (detalleId <= 0)
                {
                    return BadRequest(new ApiResponse<List<ComprobantePagoDto>>(null, "El ID de detalle debe ser mayor a 0"));
                }

                var resultado = await _comprobantePagoService.GetByDetalleIdAsync(detalleId);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comprobantes por detalle ID: {DetalleId}", detalleId);
                return StatusCode(500, new ApiResponse<List<ComprobantePagoDto>>(null, "Error interno del servidor"));
            }
        }
         
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ComprobantePagoDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<ComprobantePagoDto>), 400)]
        public async Task<IActionResult> Create([FromBody] ComprobantePagoCreateDto createDto)
        {
            try
            {
                if (createDto == null)
                {
                    return BadRequest(new ApiResponse<ComprobantePagoDto>(null, "Los datos del comprobante son requeridos"));
                }

                if (!ModelState.IsValid)
                {
                    var errores = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new ApiResponse<ComprobantePagoDto>(null, $"Errores de validación: {string.Join(", ", errores)}"));
                }

                var resultado = await _comprobantePagoService.CreateAsync(createDto);
                
                if (resultado.Data == null)
                {
                    return BadRequest(resultado);
                }


                var cabecera = await _dao.GetSviaticosCabecera(createDto.SvIdCabecera);
                var dto = new NotificacionCreateDto()
                {
                    CodUsuReceptor = cabecera.SvEmpDni ?? string.Empty,
                    UsuarioReceptor = null,
                    CodUsuValidador = null,
                    UsuarioValidador = null,
                    Mensaje = $"Solicitud #{cabecera.SvId} - se cargo el comprobante ${createDto.Serie}-${createDto.Correlativo} en el detalle ${createDto.SvIdDetalle} ",
                    Leido = false,
                    EstadoFlujo = 7
                };
                var responseTMP = await _notificacionesService.CreateAsync(dto);


                return CreatedAtAction(nameof(GetById), new { id = resultado.Data.Id }, resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear comprobante de pago");
                return StatusCode(500, new ApiResponse<ComprobantePagoDto>(null, "Error interno del servidor"));
            }
        }
         
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ComprobantePagoDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ComprobantePagoDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<ComprobantePagoDto>), 404)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ComprobantePagoUpdateDto updateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<ComprobantePagoDto>(null, "El ID debe ser mayor a 0"));
                }

                if (updateDto == null)
                {
                    return BadRequest(new ApiResponse<ComprobantePagoDto>(null, "Los datos del comprobante son requeridos"));
                }

                if (id != updateDto.Id)
                {
                    return BadRequest(new ApiResponse<ComprobantePagoDto>(null, "El ID de la ruta no coincide con el ID del objeto"));
                }

                if (!ModelState.IsValid)
                {
                    var errores = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(new ApiResponse<ComprobantePagoDto>(null, $"Errores de validación: {string.Join(", ", errores)}"));
                }

                var resultado = await _comprobantePagoService.UpdateAsync(updateDto);
                
                if (resultado.Data == null)
                {
                    return NotFound(resultado);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar comprobante de pago ID: {Id}", id);
                return StatusCode(500, new ApiResponse<ComprobantePagoDto>(null, "Error interno del servidor"));
            }
        }
         
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<bool>(false, "El ID debe ser mayor a 0"));
                }

                var resultado = await _comprobantePagoService.DeleteAsync(id);
                
                if (!resultado.Data)
                {
                    return NotFound(resultado);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar comprobante de pago ID: {Id}", id);
                return StatusCode(500, new ApiResponse<bool>(false, "Error interno del servidor"));
            }
        } 

        [HttpGet("estadisticas")]
        [ProducesResponseType(typeof(ApiResponse<ComprobantePagoEstadisticasDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ComprobantePagoEstadisticasDto>), 400)]
        public async Task<IActionResult> GetEstadisticas(
            [FromQuery][Required] DateTime fechaInicio,
            [FromQuery][Required] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return BadRequest(new ApiResponse<ComprobantePagoEstadisticasDto>(null, "La fecha de inicio no puede ser mayor a la fecha de fin"));
                }

                if (fechaFin > DateTime.Now)
                {
                    return BadRequest(new ApiResponse<ComprobantePagoEstadisticasDto>(null, "La fecha de fin no puede ser mayor a la fecha actual"));
                }

                var resultado = await _comprobantePagoService.GetEstadisticasAsync(fechaInicio, fechaFin);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de comprobantes");
                return StatusCode(500, new ApiResponse<ComprobantePagoEstadisticasDto>(null, "Error interno del servidor"));
            }
        }
         
        [HttpGet("validar-duplicado")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
        public async Task<IActionResult> ValidarDuplicado(
            [FromQuery][Required] string serie,
            [FromQuery][Required] string correlativo,
            [FromQuery] int? idExcluir = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serie))
                {
                    return BadRequest(new ApiResponse<bool>(false, "La serie es requerida"));
                }

                if (string.IsNullOrWhiteSpace(correlativo))
                {
                    return BadRequest(new ApiResponse<bool>(false, "El correlativo es requerido"));
                }

                var existeDuplicado = await _comprobantePagoService.ExisteDuplicadoAsync(serie, correlativo, idExcluir);
                var mensaje = existeDuplicado ? "Existe un comprobante con la misma serie y correlativo" : "No existe duplicado";
                
                return Ok(new ApiResponse<bool>(existeDuplicado, mensaje));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar duplicado de comprobante");
                return StatusCode(500, new ApiResponse<bool>(false, "Error interno del servidor"));
            }
        }
        [HttpGet("dashboard")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRendicionesDashboard(
            [FromQuery] string? svEmpDni = null, 
            [FromQuery] DateTime? fechaInicio = null, 
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] string[]? estados = null)
        {
            var response = await _comprobantePagoService.GetRendicionesDashboardAsync(estados,svEmpDni, fechaInicio, fechaFin);
            return Ok(response);
        }
    }
}