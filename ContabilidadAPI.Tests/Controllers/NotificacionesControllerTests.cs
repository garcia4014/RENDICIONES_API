using ContabilidadAPI.Controllers;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace ContabilidadAPI.Tests.Controllers
{
    public class NotificacionesControllerTests
    {
        private readonly Mock<INotificacionesService> _mockService;
        private readonly Mock<ILogger<NotificacionesController>> _mockLogger;
        private readonly NotificacionesController _controller;

        public NotificacionesControllerTests()
        {
            _mockService = new Mock<INotificacionesService>();
            _mockLogger = new Mock<ILogger<NotificacionesController>>();
            _controller = new NotificacionesController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkWithNotifications_WhenSuccessful()
        {
            // Arrange
            var notificaciones = new List<NotificacionDto>
            {
                new NotificacionDto { Id = 1, Mensaje = "Notificacion 1" },
                new NotificacionDto { Id = 2, Mensaje = "Notificacion 2" }
            };
            var response = new ApiResponse<List<NotificacionDto>>(notificaciones, "Success");
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(response);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(response);
            _mockService.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturn500_WhenServiceFails()
        {
            // Arrange
            var response = new ApiResponse<List<NotificacionDto>>(null, "Error de servicio");
            response.Success = false;
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(response);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetAll_ShouldReturn500_WhenExceptionThrown()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllAsync()).ThrowsAsync(new Exception("Error inesperado"));

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenNotificacionExists()
        {
            // Arrange
            var notificacion = new NotificacionDto { Id = 1, Mensaje = "Notificacion 1" };
            var response = new ApiResponse<NotificacionDto>(notificacion, "Success");
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNotificacionDoesNotExist()
        {
            // Arrange
            var response = new ApiResponse<NotificacionDto>(null, "Not found");
            response.Success = false;
            _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var createDto = new NotificacionCreateDto { CodUsuReceptor = "USER123", Mensaje = "Test" };
            var notificacion = new NotificacionDto { Id = 1, Mensaje = "Test" };
            var response = new ApiResponse<NotificacionDto>(notificacion, "Success");
            _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.ActionName.Should().Be(nameof(_controller.GetById));
            _mockService.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldReturn500_WhenServiceFails()
        {
            // Arrange
            var createDto = new NotificacionCreateDto { CodUsuReceptor = "USER123", Mensaje = "Test" };
            var response = new ApiResponse<NotificacionDto>(null, "Error al crear");
            response.Success = false;
            _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>();
            var objectResult = result.Result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var updateDto = new NotificacionUpdateDto { Mensaje = "Updated" };
            var notificacion = new NotificacionDto { Id = 1, Mensaje = "Updated" };
            var response = new ApiResponse<NotificacionDto>(notificacion, "Success");
            _mockService.Setup(s => s.UpdateAsync(1, updateDto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Update(1, updateDto);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.UpdateAsync(1, updateDto), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenNotificacionDoesNotExist()
        {
            // Arrange
            var updateDto = new NotificacionUpdateDto { Mensaje = "Updated" };
            var response = new ApiResponse<NotificacionDto>(null, "Not found");
            response.Success = false;
            _mockService.Setup(s => s.UpdateAsync(999, updateDto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Update(999, updateDto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task GetById_ShouldWorkWithDifferentIds(int id)
        {
            // Arrange
            var notificacion = new NotificacionDto { Id = id, Mensaje = $"Notificacion {id}" };
            var response = new ApiResponse<NotificacionDto>(notificacion, "Success");
            _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.GetByIdAsync(id), Times.Once);
        }
    }
}
