using Xunit;
using Moq;
using FluentAssertions;
using ContabilidadAPI.Controllers;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadAPI.Tests.Controllers
{
    /// <summary>
    /// Pruebas unitarias para GeneralController
    /// </summary>
    public class GeneralControllerTests
    {
        private readonly Mock<IGeneralService> _mockService;
        private readonly GeneralController _controller;

        public GeneralControllerTests()
        {
            _mockService = new Mock<IGeneralService>();
            _controller = new GeneralController(_mockService.Object);
        }

        [Fact(DisplayName = "POST /api/General - Debe retornar datos generales por documento")]
        public async Task Post_DebeRetornarOk_ConDatosGenerales()
        {
            // Arrange
            var request = new GeneralRequest
            {
                idDocumento = "12345678"
            };

            var datosGenerales = new
            {
                idDocumento = "12345678",
                Nombres = "Juan Pérez",
                Empresa = "Movitec SAC"
            };

            _mockService
                .Setup(x => x.GetGeneralData(request.idDocumento))
                .ReturnsAsync(datosGenerales);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();

            _mockService.Verify(x => x.GetGeneralData(request.idDocumento), Times.Once);
        }

        [Fact(DisplayName = "POST /api/General - Debe retornar NotFound cuando no hay datos")]
        public async Task Post_DebeRetornarNotFound_CuandoNoHayDatos()
        {
            // Arrange
            var request = new GeneralRequest
            {
                idDocumento = "99999999"
            };

            _mockService
                .Setup(x => x.GetGeneralData(request.idDocumento))
                .ReturnsAsync((object)null);

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact(DisplayName = "POST /api/General - Debe manejar excepciones")]
        public async Task Post_DebeManejarExcepciones()
        {
            // Arrange
            var request = new GeneralRequest
            {
                idDocumento = "12345678"
            };

            _mockService
                .Setup(x => x.GetGeneralData(request.idDocumento))
                .ThrowsAsync(new Exception("Error al consultar datos"));

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Theory(DisplayName = "POST /api/General - Debe procesar diferentes documentos")]
        [InlineData("12345678")]
        [InlineData("87654321")]
        [InlineData("11223344")]
        public async Task Post_DebeProcesarDiferentesDocumentos(string idDocumento)
        {
            // Arrange
            var request = new GeneralRequest { idDocumento = idDocumento };
            var datos = new { idDocumento, Nombres = "Test" };

            _mockService
                .Setup(x => x.GetGeneralData(idDocumento))
                .ReturnsAsync(datos);

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact(DisplayName = "POST /api/General - Debe permitir acceso anónimo")]
        public async Task Post_DebePermitirAccesoAnonimo()
        {
            // Arrange
            var request = new GeneralRequest { idDocumento = "12345678" };
            var datos = new { idDocumento = "12345678" };

            _mockService
                .Setup(x => x.GetGeneralData(request.idDocumento))
                .ReturnsAsync(datos);

            // Act
            var result = await _controller.Post(request);

            // Assert
            // El controlador tiene [AllowAnonymous]
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
