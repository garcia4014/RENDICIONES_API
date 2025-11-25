using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificacionController : ControllerBase
    {
        private readonly INotificacionService _notificacionService;

        public NotificacionController(INotificacionService notificacionService)
        {
            _notificacionService = notificacionService;
        }

        // GET api/<SviaticoController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> SendNotificacion(int id)
        {
            try
            {
                var response = await _notificacionService.SendMail(id);

                if (!response)
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
