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
    /// Pruebas unitarias para NotificacionController
    /// </summary>
    public class NotificacionControllerTests
    {
        private readonly Mock<INotificacionService> _mockService;
        private readonly NotificacionController _controller;

        public NotificacionControllerTests()
        {
            _mockService = new Mock<INotificacionService>();
            _controller = new NotificacionController(_mockService.Object);
        }

        [Fact(DisplayName = "GET /api/Notificacion/{id} - Debe enviar notificación exitosamente")]
        public async Task SendNotificacion_DebeRetornarOk_CuandoEnviaExitosamente()
        {
            // Arrange
            var notificacionId = 1;

            _mockService
                .Setup(x => x.SendMail(notificacionId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SendNotificacion(notificacionId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(true);

            _mockService.Verify(x => x.SendMail(notificacionId), Times.Once);
        }

        [Fact(DisplayName = "GET /api/Notificacion/{id} - Debe retornar NotFound cuando falla el envío")]
        public async Task SendNotificacion_DebeRetornarNotFound_CuandoFallaEnvio()
        {
            // Arrange
            var notificacionId = 999;

            _mockService
                .Setup(x => x.SendMail(notificacionId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SendNotificacion(notificacionId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact(DisplayName = "GET /api/Notificacion/{id} - Debe manejar excepciones")]
        public async Task SendNotificacion_DebeManejarExcepciones()
        {
            // Arrange
            var notificacionId = 1;

            _mockService
                .Setup(x => x.SendMail(notificacionId))
                .ThrowsAsync(new Exception("Error al enviar correo"));

            // Act
            var result = await _controller.SendNotificacion(notificacionId);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Theory(DisplayName = "GET /api/Notificacion/{id} - Debe procesar diferentes IDs")]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task SendNotificacion_DebeProcesarDiferentesIds(int id)
        {
            // Arrange
            _mockService
                .Setup(x => x.SendMail(id))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SendNotificacion(id);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
