using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioTipoPersonaController : ControllerBase
    {
        private readonly IUsuarioTipoPersonaService _usuarioTipoPersonaService;

        public UsuarioTipoPersonaController(IUsuarioTipoPersonaService usuarioTipoPersonaService)
        {
            _usuarioTipoPersonaService = usuarioTipoPersonaService;
        }

        /// <summary>
        /// Obtiene todos los usuarios tipo persona activos
        /// </summary>
        /// <returns>Lista de usuarios tipo persona</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _usuarioTipoPersonaService.GetAllAsync();
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }

        /// <summary>
        /// Obtiene un usuario tipo persona por código
        /// </summary>
        /// <param name="code">Código del usuario</param>
        /// <returns>Usuario tipo persona</returns>
        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var response = await _usuarioTipoPersonaService.GetByCodeAsync(code);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return response.Data == null ? NotFound(response) : BadRequest(response);
        }

        /// <summary>
        /// Obtiene usuarios tipo persona con filtros y paginación
        /// </summary>
        /// <param name="filtro">Filtros de búsqueda</param>
        /// <returns>Lista paginada de usuarios tipo persona</returns>
        [HttpPost("filtrado")]
        public async Task<IActionResult> GetFiltered([FromBody] UsuarioTipoPersonaFiltroDto filtro)
        {
            var response = await _usuarioTipoPersonaService.GetFilteredAsync(filtro);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }

        /// <summary>
        /// Crear un nuevo usuario tipo persona
        /// </summary>
        /// <param name="createDto">Datos del usuario a crear</param>
        /// <returns>Usuario creado</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UsuarioTipoPersonaCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _usuarioTipoPersonaService.CreateAsync(createDto);
            
            if (response.Success)
            {
                return CreatedAtAction(
                    nameof(GetByCode), 
                    new { code = response.Data?.Code }, 
                    response);
            }
            
            return BadRequest(response);
        }

        /// <summary>
        /// Actualizar un usuario tipo persona existente
        /// </summary>
        /// <param name="code">Código del usuario</param>
        /// <param name="updateDto">Datos a actualizar</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{code}")]
        public async Task<IActionResult> Update(string code, [FromBody] UsuarioTipoPersonaUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _usuarioTipoPersonaService.UpdateAsync(code, updateDto);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return response.Data == null ? NotFound(response) : BadRequest(response);
        }

        /// <summary>
        /// Eliminar un usuario tipo persona (eliminación lógica)
        /// </summary>
        /// <param name="code">Código del usuario</param>
        /// <returns>Resultado de la eliminación</returns>
        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            var response = await _usuarioTipoPersonaService.DeleteAsync(code);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return response.Message.Contains("no encontrado") ? NotFound(response) : BadRequest(response);
        }

        /// <summary>
        /// Verificar si existe un usuario tipo persona con el código especificado
        /// </summary>
        /// <param name="code">Código del usuario</param>
        /// <returns>True si existe, False si no existe</returns>
        [HttpGet("existe/{code}")]
        public async Task<IActionResult> Exists(string code)
        {
            var response = await _usuarioTipoPersonaService.ExistsAsync(code);
            return Ok(response);
        }

        /// <summary>
        /// Obtener usuarios por tipo de persona
        /// </summary>
        /// <param name="tpId">ID del tipo de persona</param>
        /// <returns>Lista de usuarios del tipo de persona especificado</returns>
        [HttpGet("tipo-persona/{tpId}")]
        public async Task<IActionResult> GetByTipoPersona(int tpId)
        {
            var response = await _usuarioTipoPersonaService.GetByTipoPersonaAsync(tpId);
            
            if (response.Success)
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
    }
}