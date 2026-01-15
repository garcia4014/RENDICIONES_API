using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ContabilidadAPI.Controllers
{
    /// <summary>
    /// Controlador para gestión de consultas de Personal
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class PersonalController : ControllerBase
    {
        private readonly IPersonalService _personalService;
        private readonly ILogger<PersonalController> _logger;

        public PersonalController(
            IPersonalService personalService,
            ILogger<PersonalController> logger)
        {
            _personalService = personalService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene personal por documento de identidad
        /// </summary>
        /// <param name="idDocumento">Documento de identidad del personal</param>
        /// <returns>Información del personal</returns>
        /// <response code="200">Personal encontrado exitosamente</response>
        /// <response code="400">Documento de identidad inválido</response>
        /// <response code="404">Personal no encontrado</response>
        [HttpGet("{idDocumento}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 404)]
        public async Task<IActionResult> GetByIdDocumento(
            [Required] string idDocumento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idDocumento))
                {
                    return BadRequest(new { message = "El documento de identidad es requerido" });
                }

                var response = await _personalService.GetByIdDocumentoAsync(idDocumento);
                
                if (!response.Success || response.Data == null)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener personal por documento: {IdDocumento}", idDocumento);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene personal filtrado con paginación
        /// </summary>
        /// <param name="nombres">Filtro por nombres (opcional)</param>
        /// <param name="idDocumento">Filtro por documento (opcional)</param>
        /// <param name="empresa">Filtro por empresa (opcional)</param>
        /// <param name="usrSidige">Filtro por usuario SIDIGE (opcional)</param>
        /// <param name="pagina">Número de página (por defecto: 1)</param>
        /// <param name="tamanoPagina">Tamaño de página (por defecto: 10, máximo: 100)</param>
        /// <returns>Lista paginada de personal</returns>
        /// <response code="200">Lista obtenida exitosamente</response>
        /// <response code="400">Parámetros de paginación inválidos</response>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> GetPersonalFiltrado(
            [FromQuery] string? nombres = null,
            [FromQuery] string? idDocumento = null,
            [FromQuery] string? empresa = null,
            [FromQuery] string? usrSidige = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 1000)
        {
            try
            {
                if (pagina <= 0)
                {
                    return BadRequest(new { message = "El número de página debe ser mayor a 0" });
                }

                var filtro = new PersonalFiltroDto
                {
                    Nombres = nombres,
                    IdDocumento = idDocumento,
                    Empresa = empresa,
                    UsrSidige = usrSidige,
                    Pagina = pagina,
                    TamanoPagina = tamanoPagina
                };

                var response = await _personalService.GetPersonalFiltradoAsync(filtro);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener personal filtrado");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca personal por nombres (búsqueda parcial)
        /// </summary>
        /// <param name="nombres">Nombres a buscar (mínimo 3 caracteres)</param>
        /// <returns>Lista de personal que coincide con los nombres</returns>
        /// <response code="200">Búsqueda realizada exitosamente</response>
        /// <response code="400">Parámetro de búsqueda inválido</response>
        [HttpGet("buscar-nombres")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> BuscarPorNombres(
            [FromQuery][Required] string nombres)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombres))
                {
                    return BadRequest(new { message = "El parámetro 'nombres' es requerido" });
                }

                if (nombres.Length < 3)
                {
                    return BadRequest(new { message = "El nombre debe tener al menos 3 caracteres" });
                }

                var response = await _personalService.BuscarPorNombresAsync(nombres);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar personal por nombres: {Nombres}", nombres);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Verifica si existe personal con el documento especificado
        /// </summary>
        /// <param name="idDocumento">Documento de identidad a verificar</param>
        /// <returns>True si existe, False si no existe</returns>
        /// <response code="200">Verificación realizada exitosamente</response>
        /// <response code="400">Documento de identidad inválido</response>
        [HttpGet("existe/{idDocumento}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> ExistePersonal(
            [Required] string idDocumento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idDocumento))
                {
                    return BadRequest(new { message = "El documento de identidad es requerido" });
                }

                var response = await _personalService.ExistePersonalAsync(idDocumento);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia del personal: {IdDocumento}", idDocumento);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}