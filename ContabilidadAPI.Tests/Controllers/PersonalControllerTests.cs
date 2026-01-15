using Xunit;
using Moq;
using FluentAssertions;
using ContabilidadAPI.Controllers;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces.Access;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContabilidadAPI.Tests.Controllers
{
    /// <summary>
    /// Pruebas unitarias para PersonalController
    /// </summary>
    public class PersonalControllerTests
    {
        private readonly Mock<IPersonalService> _mockService;
        private readonly Mock<ILogger<PersonalController>> _mockLogger;
        private readonly PersonalController _controller;

        public PersonalControllerTests()
        {
            _mockService = new Mock<IPersonalService>();
            _mockLogger = new Mock<ILogger<PersonalController>>();
            _controller = new PersonalController(_mockService.Object, _mockLogger.Object);
        }

        [Fact(DisplayName = "GET /api/Personal/{idDocumento} - Debe retornar personal por DNI")]
        public async Task GetByIdDocumento_DebeRetornarOk_CuandoExiste()
        {
            // Arrange
            var dni = "12345678";
            var personal = new PersonalDTO
            {
                idDocumento = dni,
                Nombres = "Juan Pérez",
                Correo = "juan@example.com"
            };

            var apiResponse = new ApiResponse<PersonalDTO>(personal, "Personal encontrado");

            _mockService
                .Setup(x => x.GetByIdDocumentoAsync(dni))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetByIdDocumento(dni);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<PersonalDTO>>().Subject;
            response.Data.idDocumento.Should().Be(dni);
        }

        [Fact(DisplayName = "GET /api/Personal/{idDocumento} - Debe retornar NotFound cuando no existe")]
        public async Task GetByIdDocumento_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var dni = "99999999";
            var apiResponse = new ApiResponse<PersonalDTO>(null, "Personal no encontrado")
            {
                Success = false
            };

            _mockService
                .Setup(x => x.GetByIdDocumentoAsync(dni))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetByIdDocumento(dni);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact(DisplayName = "GET /api/Personal/{idDocumento} - Debe retornar BadRequest con DNI vacío")]
        public async Task GetByIdDocumento_DebeRetornarBadRequest_ConDniVacio()
        {
            // Act
            var result = await _controller.GetByIdDocumento("");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact(DisplayName = "GET /api/Personal - Debe retornar lista filtrada de personal")]
        public async Task Get_DebeRetornarOk_ConListaFiltrada()
        {
            // Arrange
            var personalList = new List<PersonalDTO>
            {
                new PersonalDTO { idDocumento = "12345678", Nombres = "Juan Pérez" },
                new PersonalDTO { idDocumento = "87654321", Nombres = "María García" }
            };

            var pagedResult = new PagedResult<PersonalDTO>
            {
                Items = personalList,
                TotalItems = 2,
                CurrentPage = 1,
                PageSize = 10
            };

            var apiResponse = new ApiResponse<PagedResult<PersonalDTO>>(pagedResult, "OK");

            _mockService
                .Setup(x => x.GetFilteredAsync(null, null, null, null, 1, 10))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<PagedResult<PersonalDTO>>>().Subject;
            response.Data.Items.Should().HaveCount(2);
        }

        [Fact(DisplayName = "GET /api/Personal - Debe manejar excepciones")]
        public async Task GetByIdDocumento_DebeManejarExcepciones()
        {
            // Arrange
            var dni = "12345678";

            _mockService
                .Setup(x => x.GetByIdDocumentoAsync(dni))
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            var result = await _controller.GetByIdDocumento(dni);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Theory(DisplayName = "GET /api/Personal/{idDocumento} - Debe validar diferentes DNIs")]
        [InlineData("12345678")]
        [InlineData("87654321")]
        [InlineData("11223344")]
        public async Task GetByIdDocumento_DebeAceptarDiferentesDnis(string dni)
        {
            // Arrange
            var personal = new PersonalDTO { idDocumento = dni, Nombres = "Test" };
            var apiResponse = new ApiResponse<PersonalDTO>(personal, "OK");

            _mockService
                .Setup(x => x.GetByIdDocumentoAsync(dni))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetByIdDocumento(dni);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
