using Xunit;
using Moq;
using FluentAssertions;
using ContabilidadAPI.Controllers;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadAPI.Tests.Controllers
{
    /// <summary>
    /// Pruebas unitarias para TipoGastoController
    /// </summary>
    public class TipoGastoControllerTests
    {
        private readonly Mock<ITipoGastoServices> _mockService;
        private readonly TipoGastoController _controller;

        public TipoGastoControllerTests()
        {
            _mockService = new Mock<ITipoGastoServices>();
            _controller = new TipoGastoController(_mockService.Object);
        }

        [Fact(DisplayName = "GET /api/TipoGasto - Debe retornar lista de tipos de gasto")]
        public async Task Get_DebeRetornarOkConLista()
        {
            // Arrange
            var tiposGasto = new List<TipoGasto>
            {
                new TipoGasto { TgasId = 1, TgasDescripcion = "Hospedaje" },
                new TipoGasto { TgasId = 2, TgasDescripcion = "Alimentación" },
                new TipoGasto { TgasId = 3, TgasDescripcion = "Transporte" }
            };

            var apiResponse = new ApiResponse<List<TipoGasto>>(tiposGasto, "Tipos de gasto obtenidos");

            _mockService
                .Setup(x => x.GetListTipoGasto())
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<List<TipoGasto>>>().Subject;
            response.Data.Should().HaveCount(3);
        }

        [Fact(DisplayName = "GET /api/TipoGasto/{id} - Debe retornar tipo de gasto por ID")]
        public async Task GetById_DebeRetornarOk_CuandoExiste()
        {
            // Arrange
            var tipoGastoId = 1;
            var tipoGasto = new TipoGasto
            {
                TgasId = tipoGastoId,
                TgasDescripcion = "Hospedaje"
            };

            var apiResponse = new ApiResponse<TipoGasto>(tipoGasto, "Tipo de gasto encontrado");

            _mockService
                .Setup(x => x.GetTipoGastoById(tipoGastoId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Get(tipoGastoId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<TipoGasto>>().Subject;
            response.Data.TgasId.Should().Be(tipoGastoId);
        }

        [Fact(DisplayName = "GET /api/TipoGasto/{id} - Debe retornar NotFound cuando no existe")]
        public async Task GetById_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var tipoGastoId = 999;
            var apiResponse = new ApiResponse<TipoGasto>(null, "Tipo de gasto no encontrado");

            _mockService
                .Setup(x => x.GetTipoGastoById(tipoGastoId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Get(tipoGastoId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact(DisplayName = "GET /api/TipoGasto - Debe retornar NotFound cuando lista es null")]
        public async Task Get_DebeRetornarNotFound_CuandoListaEsNull()
        {
            // Arrange
            _mockService
                .Setup(x => x.GetListTipoGasto())
                .ReturnsAsync((ApiResponse<List<TipoGasto>>)null);

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact(DisplayName = "GET /api/TipoGasto - Debe manejar excepciones")]
        public async Task Get_DebeManejarExcepciones()
        {
            // Arrange
            _mockService
                .Setup(x => x.GetListTipoGasto())
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            var result = await _controller.Get();

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Fact(DisplayName = "GET /api/TipoGasto/{id} - Debe manejar excepciones")]
        public async Task GetById_DebeManejarExcepciones()
        {
            // Arrange
            _mockService
                .Setup(x => x.GetTipoGastoById(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            var result = await _controller.Get(1);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Theory(DisplayName = "GET /api/TipoGasto/{id} - Debe validar IDs")]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task GetById_DebeAceptarDiferentesIds(int id)
        {
            // Arrange
            var tipoGasto = new TipoGasto { TgasId = id, TgasDescripcion = $"Tipo {id}" };
            var apiResponse = new ApiResponse<TipoGasto>(tipoGasto, "OK");

            _mockService
                .Setup(x => x.GetTipoGastoById(id))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Get(id);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
