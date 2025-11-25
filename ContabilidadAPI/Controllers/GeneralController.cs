using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ContabilidadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GeneralController : ControllerBase
    {

        private readonly IGeneralService _generalService;

        public GeneralController(IGeneralService generalService)
        {
            _generalService = generalService;
        }

        
        //POST api/<GeneralController>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post(GeneralRequest request)
        {
            try
            {
                var item = await _generalService.GetGeneralData(request.idDocumento);
                if (item == null)
                    return NotFound(item);
                return Ok(item);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>("Error interno del servidor: " + ex.Message));
            }
        }
    }

    public class GeneralRequest
    {
        public string idDocumento { get; set; }
    }
}


