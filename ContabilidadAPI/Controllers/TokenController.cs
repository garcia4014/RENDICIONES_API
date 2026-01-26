using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IParametrosDao _parametrosDao;
        private readonly ILogger<TokenController> _logger;

        public TokenController(IParametrosDao parametrosDao, ILogger<TokenController> logger)
        {
            _parametrosDao = parametrosDao;
            _logger = logger;
        }

        /// <summary>
        /// Actualiza el token de consulta XML/PDF Sunat (sin autorización)
        /// </summary>
        /// <param name="request">Objeto con el nuevo valor del token</param>
        /// <returns>Respuesta indicando si la actualización fue exitosa</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ActualizarToken([FromBody] ActualizarTokenRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Valor))
                {
                    return BadRequest(new ActualizarTokenResponseDto
                    {
                        Success = false,
                        Message = "El valor del token no puede estar vacío"
                    });
                }

                var resultado = await _parametrosDao.ActualizarTokenAsync(request.Valor);

                if (resultado)
                {
                    _logger.LogInformation($"Token actualizado exitosamente a las {DateTime.Now}");
                    
                    return Ok(new ActualizarTokenResponseDto
                    {
                        Success = true,
                        Message = "Token actualizado exitosamente"
                    });
                }
                else
                {
                    _logger.LogWarning("No se pudo actualizar el token - Registro no encontrado");
                    
                    return NotFound(new ActualizarTokenResponseDto
                    {
                        Success = false,
                        Message = "No se encontró el parámetro para actualizar"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el token");
                
                return StatusCode(500, new ActualizarTokenResponseDto
                {
                    Success = false,
                    Message = $"Error interno al actualizar el token: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obtiene el token actual (sin autorización, solo para verificación)
        /// </summary>
        /// <returns>El parámetro con el token actual</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerToken()
        {
            try
            {
                var parametro = await _parametrosDao.ObtenerParametroPorIdAsync(1);

                if (parametro == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Parámetro no encontrado"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = parametro.Id,
                        descripcion = parametro.Descripcion,
                        valor = parametro.Valor,
                        estado = parametro.Estado
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el token");
                
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno: {ex.Message}"
                });
            }
        }
    }
}
