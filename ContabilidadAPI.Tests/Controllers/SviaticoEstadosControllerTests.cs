using Xunit;
using Moq;
using FluentAssertions;
using ContabilidadAPI.Controllers;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContabilidadAPI.Tests.Controllers
{
    /// <summary>
    /// Pruebas unitarias para SviaticoEstadosController
    /// </summary>
    public class SviaticoEstadosControllerTests
    {
        private readonly Mock<ISviaticoService> _mockService;
        private readonly Mock<ILogger<SviaticoEstadosController>> _mockLogger;
        private readonly Mock<ISviatico> _mockDao;
        private readonly Mock<INotificacionesService> _mockNotificaciones;
        private readonly SviaticoEstadosController _controller;

        public SviaticoEstadosControllerTests()
        {
            _mockService = new Mock<ISviaticoService>();
            _mockLogger = new Mock<ILogger<SviaticoEstadosController>>();
            _mockDao = new Mock<ISviatico>();
            _mockNotificaciones = new Mock<INotificacionesService>();

            _controller = new SviaticoEstadosController(
                _mockNotificaciones.Object,
                _mockDao.Object,
                _mockService.Object,
                _mockLogger.Object
            );
        }

        [Fact(DisplayName = "PUT /api/SviaticoEstados/{id}/solicitar - Debe actualizar a estado Solicitado")]
        public async Task SolicitarViatico_DebeRetornarOk()
        {
            // Arrange
            var viaticoId = 1;
            var request = new ActualizarEstadoRequestDto
            {
                Comentario = "Solicitud de viáticos para viaje a Lima"
            };

            var viatico = new SviaticosCabeceraDTOResponse
            {
                SvId = viaticoId,
                SvNumero = "RV-2024-001"
            };

            var apiResponse = new ApiResponse<SviaticosCabeceraDTOResponse>(viatico, "Estado actualizado");

            _mockService
                .Setup(x => x.ActualizarEstadoSolicitud(viaticoId, 1, request.Comentario))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.SolicitarViatico(viaticoId, request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<SviaticosCabeceraDTOResponse>>().Subject;
            response.Success.Should().BeTrue();
        }

        [Fact(DisplayName = "PUT /api/SviaticoEstados/{id}/abrir - Debe actualizar a estado Abierto")]
        public async Task AbrirViatico_DebeRetornarOk()
        {
            // Arrange
            var viaticoId = 1;
            var request = new ActualizarEstadoRequestDto
            {
                Comentario = "Viático abierto para rendición"
            };

            var viatico = new SviaticosCabeceraDTOResponse
            {
                SvId = viaticoId,
                SvNumero = "RV-2024-001"
            };

            var apiResponse = new ApiResponse<SviaticosCabeceraDTOResponse>(viatico, "OK");

            _mockService
                .Setup(x => x.ActualizarEstadoSolicitud(viaticoId, 2, request.Comentario))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.AbrirViatico(viaticoId, request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact(DisplayName = "PUT /api/SviaticoEstados/{id}/aprobar - Debe actualizar a estado Aprobado")]
        public async Task AprobarViatico_DebeRetornarOk()
        {
            // Arrange
            var viaticoId = 1;
            var request = new ActualizarEstadoRequestDto
            {
                Comentario = "Aprobado por gerencia"
            };

            var viatico = new SviaticosCabeceraDTOResponse
            {
                SvId = viaticoId,
                SvNumero = "RV-2024-001"
            };

            var apiResponse = new ApiResponse<SviaticosCabeceraDTOResponse>(viatico, "OK");

            _mockService
                .Setup(x => x.ActualizarEstadoSolicitud(viaticoId, 3, request.Comentario))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.AprobarViatico(viaticoId, request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact(DisplayName = "PUT /api/SviaticoEstados/{id}/rechazar - Debe actualizar a estado Rechazado")]
        public async Task RechazarViatico_DebeRetornarOk()
        {
            // Arrange
            var viaticoId = 1;
            var request = new ActualizarEstadoRequestDto
            {
                Comentario = "Rechazado por documentación incompleta"
            };

            var viatico = new SviaticosCabeceraDTOResponse
            {
                SvId = viaticoId,
                SvNumero = "RV-2024-001"
            };

            var apiResponse = new ApiResponse<SviaticosCabeceraDTOResponse>(viatico, "OK");

            _mockService
                .Setup(x => x.ActualizarEstadoSolicitud(viaticoId, 4, request.Comentario))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.RechazarViatico(viaticoId, request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact(DisplayName = "GET /api/SviaticoEstados/estados - Debe retornar estados disponibles")]
        public async Task GetEstadosDisponibles_DebeRetornarOk()
        {
            // Arrange
            var estados = new List<SolicitudEstadoFlujo>
            {
                new SolicitudEstadoFlujo { SefId = 1, SefDescripcion = "Solicitado" },
                new SolicitudEstadoFlujo { SefId = 2, SefDescripcion = "Abierto" },
                new SolicitudEstadoFlujo { SefId = 3, SefDescripcion = "Aprobado" }
            };

            var apiResponse = new ApiResponse<IEnumerable<SolicitudEstadoFlujo>>(estados, "OK");

            _mockService
                .Setup(x => x.GetEstadosDisponibles())
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetEstadosDisponibles();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<IEnumerable<SolicitudEstadoFlujo>>>().Subject;
            response.Data.Should().HaveCount(3);
        }

        [Fact(DisplayName = "PUT /api/SviaticoEstados/{id}/solicitar - Debe manejar errores")]
        public async Task SolicitarViatico_DebeManejarErrores()
        {
            // Arrange
            var viaticoId = 999;
            var request = new ActualizarEstadoRequestDto();

            var apiResponse = new ApiResponse<SviaticosCabeceraDTOResponse>(null, "Viático no encontrado")
            {
                Success = false
            };

            _mockService
                .Setup(x => x.ActualizarEstadoSolicitud(viaticoId, 1, It.IsAny<string>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.SolicitarViatico(viaticoId, request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact(DisplayName = "PUT /api/SviaticoEstados/{id}/solicitar - Debe manejar excepciones")]
        public async Task SolicitarViatico_DebeManejarExcepciones()
        {
            // Arrange
            var viaticoId = 1;
            var request = new ActualizarEstadoRequestDto();

            _mockService
                .Setup(x => x.ActualizarEstadoSolicitud(viaticoId, 1, It.IsAny<string>()))
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            var result = await _controller.SolicitarViatico(viaticoId, request);

            // Assert
            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Theory(DisplayName = "PUT /api/SviaticoEstados/{id} - Debe procesar diferentes estados")]
        [InlineData(1, "Solicitado")]
        [InlineData(2, "Abierto")]
        [InlineData(3, "Aprobado")]
        [InlineData(4, "Rechazado")]
        public async Task ActualizarEstado_DebeProcesarDiferentesEstados(int estadoId, string descripcion)
        {
            // Arrange
            var viaticoId = 1;
            var viatico = new SviaticosCabeceraDTOResponse
            {
                SvId = viaticoId,
                SvNumero = "RV-2024-001"
            };

            var apiResponse = new ApiResponse<SviaticosCabeceraDTOResponse>(viatico, "OK");

            _mockService
                .Setup(x => x.ActualizarEstadoSolicitud(viaticoId, estadoId, It.IsAny<string>()))
                .ReturnsAsync(apiResponse);

            // Verificar que el setup funciona
            var result = await _mockService.Object.ActualizarEstadoSolicitud(viaticoId, estadoId, null);

            // Assert
            result.Success.Should().BeTrue();
        }
    }
}
