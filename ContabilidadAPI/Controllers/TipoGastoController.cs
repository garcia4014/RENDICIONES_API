using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ContabilidadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TipoGastoController : ControllerBase
    {
        private readonly ITipoGastoServices _tipoGastoServices;
        public TipoGastoController(ITipoGastoServices tipoGastoServices)
        {
            _tipoGastoServices = tipoGastoServices;
        }

        // GET: api/<TipoGastoController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var list = await _tipoGastoServices.GetListTipoGasto();

                if (list == null)
                    return NotFound(list);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }

        // GET api/<TipoGastoController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var response = await _tipoGastoServices.GetTipoGastoById(id);

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
    }
}
