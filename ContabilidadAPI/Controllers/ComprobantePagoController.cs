using AutoMapper;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Implementation;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
 
 
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
        private readonly ISviaticoService _SviaticoService;
        private readonly IComprobantePago _comprobanteService;
        private readonly ComprobantePdfSunatBackgroundService _pdfSunatService;
        private readonly ComprobanteDesglosadoBackgroundService _desglosadoService;

        public ComprobantePagoController(
            ISviaticoService sviaticoService,
            INotificacionesService notificacionesService,
            IMapper mapper,
            ISviatico dao,
            IComprobantePagoService comprobantePagoService,
            ILogger<ComprobantePagoController> logger,
            IComprobantePago comprobanteService,
            ComprobantePdfSunatBackgroundService pdfSunatService,
            ComprobanteDesglosadoBackgroundService desglosadoService)
        {
            _comprobanteService = comprobanteService;
            _SviaticoService = sviaticoService;
            _comprobantePagoService = comprobantePagoService;
            _logger = logger;
            _dao = dao;
            _mapper = mapper;
            _notificacionesService = notificacionesService;
            _pdfSunatService = pdfSunatService;
            _desglosadoService = desglosadoService;
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
                    Mensaje = $"Solicitud #{cabecera.SvId} - se cargó el comprobante {createDto.Serie}-{createDto.Correlativo}",
                    Leido = false,
                    EstadoFlujo = 7
                };
                var responseTMP = await _notificacionesService.CreateAsync(dto);

                // Encolar validación SUNAT en background usando Hangfire
                var jobId = BackgroundJob.Enqueue<IComprobantePagoService>(
                    service => service.ValidarComprobanteEnSunatAsync(resultado.Data.Id));
                _logger.LogInformation("Validación SUNAT encolada en Hangfire con Job ID: {JobId} para comprobante {ComprobanteId}", 
                    jobId, resultado.Data.Id);

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
            var response = await _comprobantePagoService.GetRendicionesDashboardAsync(estados, svEmpDni, fechaInicio, fechaFin);
            return Ok(response);
        }

        /// <summary>
        /// Marca un comprobante como observado
        /// </summary>
        /// <param name="id">ID del comprobante</param>
        /// <param name="comentarios">Comentarios de observación</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut("comprobante/{id}/observado")]
        [AllowAnonymous]
        public async Task<IActionResult> MarcarComprobanteObservado(int id, [FromBody] string comentarios)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<bool>(false, "ID de comprobante inválido"));
                }

                var response = await _comprobantePagoService.ActualizarComprobanteObservado(id, true, comentarios);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>(false, $"Error interno del servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Marca un comprobante como aprobado
        /// </summary>
        /// <param name="id">ID del comprobante</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut("comprobante/{id}/aprobado")]
        [AllowAnonymous]
        public async Task<IActionResult> MarcarComprobanteAprobado(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<bool>(false, "ID de comprobante inválido"));
                }

                var response = await _comprobantePagoService.ActualizarComprobanteAprobado(id, true);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>(false, $"Error interno del servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtener el PDF de un comprobante (endpoint público)
        /// </summary>
        /// <param name="id">ID del comprobante</param>
        /// <returns>Archivo PDF del comprobante</returns>
        [HttpGet("{id}/pdf")]
        [AllowAnonymous] // Endpoint público
        [EnableCors("PublicFiles")] // Permitir cualquier origen para archivos
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetComprobantePdf(int id, [FromQuery] bool inline = false)
        {
            try
            {
                _logger.LogInformation("Solicitando PDF para comprobante ID={Id}, inline={Inline}", id, inline);

                if (id <= 0)
                {
                    return BadRequest(new { message = "ID de comprobante inválido" });
                }

                // Obtener el comprobante
                var comprobante = await _comprobanteService.GetByIdAsync(id);

                if (comprobante == null)
                {
                    _logger.LogWarning("Comprobante ID={Id} no encontrado", id);
                    return NotFound(new { message = $"Comprobante con ID {id} no encontrado" });
                }

                // Verificar que tenga ruta de archivo
                if (string.IsNullOrEmpty(comprobante.Ruta))
                {
                    _logger.LogWarning("Comprobante ID={Id} no tiene archivo adjunto", id);
                    return NotFound(new { message = "El comprobante no tiene archivo adjunto" });
                }

                // Construir ruta completa del archivo
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), comprobante.Ruta.Replace("/", Path.DirectorySeparatorChar.ToString()));

                _logger.LogInformation("Buscando archivo en: {FilePath}", filePath);

                // Verificar que el archivo exista
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Archivo no encontrado en ruta: {FilePath}", filePath);
                    return NotFound(new { message = "El archivo del comprobante no se encuentra en el servidor" });
                }

                // Obtener extension del archivo
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                string contentType;

                switch (extension)
                {
                    case ".pdf":
                        contentType = "application/pdf";
                        break;
                    case ".jpg":
                    case ".jpeg":
                        contentType = "image/jpeg";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                    case ".xml":
                        contentType = "application/xml";
                        break;
                    default:
                        contentType = "application/octet-stream";
                        break;
                }

                // Leer el archivo
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                // Generar nombre de archivo
                var fileName = Path.GetFileName(filePath);

                _logger.LogInformation("Sirviendo archivo {FileName} ({Size} bytes) para comprobante ID={Id}",
                    fileName, fileBytes.Length, id);

                // Si inline=true, mostrar en navegador; sino, forzar descarga
                if (inline)
                {
                    Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");
                    return File(fileBytes, contentType);
                }
                else
                {
                    return File(fileBytes, contentType, fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener PDF del comprobante ID={Id}", id);
                return StatusCode(500, new { message = $"Error al obtener el archivo: {ex.Message}" });
            }
        }

        /// <summary>
        /// Descarga masiva de PDFs desde SUNAT bajo demanda.
        /// Primero valida qué PDFs ya existen en disco y los omite;
        /// luego descarga desde SUNAT solo los faltantes.
        /// Todo ocurre de forma síncrona durante la petición.
        /// </summary>
        /// <param name="request">Arreglo de IDs de comprobantes a procesar</param>
        /// <summary>
        /// Procesa el desglose (extrae impuestos desde XML de SUNAT) de un único comprobante por su ID.
        /// Si el comprobante ya estaba desglosado, lo indica sin reprocesar.
        /// </summary>
        [HttpPost("{id}/desglosa")]
        [ProducesResponseType(typeof(ApiResponse<DesglosePorIdResultado>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DesglosaComprobante([FromRoute] int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<string>("ID de comprobante inválido"));

            try
            {
                _logger.LogInformation("Solicitud de desglose para comprobante ID={Id}", id);
                var resultado = await _desglosadoService.ProcesarComprobanteDesglosadoPorIdAsync(id);

                if (!resultado.Exito && resultado.Mensaje.Contains("no encontrado"))
                    return NotFound(new ApiResponse<string>(resultado.Mensaje));

                return Ok(new ApiResponse<DesglosePorIdResultado>(resultado, resultado.Mensaje));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en desglose de comprobante ID={Id}", id);
                return StatusCode(500, new ApiResponse<string>($"Error interno del servidor: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el XML de SUNAT para el comprobante indicado y lo guarda en la ruta
        /// configurada en appsettings (DesglosarXml:PathXml).
        /// Si ya existe en disco, lo devuelve desde caché sin llamar a SUNAT.
        /// </summary>
        [HttpGet("{id}/xml")]
        [ProducesResponseType(typeof(ApiResponse<ObtenerXmlResultado>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ObtenerXmlComprobante([FromRoute] int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<string>("ID de comprobante inválido"));

            try
            {
                _logger.LogInformation("Solicitud de XML para comprobante ID={Id}", id);
                var resultado = await _desglosadoService.ObtenerYGuardarXmlPorIdAsync(id);

                if (!resultado.Exito && resultado.Mensaje.Contains("no encontrado"))
                    return NotFound(new ApiResponse<string>(resultado.Mensaje));

                if (!resultado.Exito)
                    return BadRequest(new ApiResponse<string>(resultado.Mensaje));

                return Ok(new ApiResponse<ObtenerXmlResultado>(resultado, resultado.Mensaje));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener XML de comprobante ID={Id}", id);
                return StatusCode(500, new ApiResponse<string>($"Error interno del servidor: {ex.Message}"));
            }
        }

        [HttpPost("descargar-pdfs-masivo")]
        [ProducesResponseType(typeof(ApiResponse<DescargaPdfMasivaResultado>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DescargarPdfsMasivo([FromBody] DescargarPdfsMasivoRequest request)
        {
            if (request?.Ids == null || !request.Ids.Any())
                return BadRequest(new ApiResponse<string>("Debe proporcionar al menos un ID de comprobante"));

            if (request.Ids.Count > 200)
                return BadRequest(new ApiResponse<string>("El arreglo no puede superar los 200 IDs por petición"));

            try
            {
                _logger.LogInformation("Solicitud de descarga masiva de PDFs para {Cantidad} comprobantes", request.Ids.Count);
                var resultado = await _pdfSunatService.DescargarPdfsMasivoAsync(request.Ids);
                return Ok(new ApiResponse<DescargaPdfMasivaResultado>(resultado, "Proceso completado"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en descarga masiva de PDFs");
                return StatusCode(500, new ApiResponse<string>($"Error interno del servidor: {ex.Message}"));
            }
        }
    }

    /// <summary>
    /// Request para el endpoint de descarga masiva de PDFs
    /// </summary>
    public class DescargarPdfsMasivoRequest
    {
        /// <summary>Lista de IDs de comprobantes a procesar (máximo 200)</summary>
        public List<int> Ids { get; set; } = new();
    }  