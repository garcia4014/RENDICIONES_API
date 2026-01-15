using Xunit;
using Moq;
using FluentAssertions;
using ContabilidadAPI.Controllers;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ContabilidadAPI.Tests.Controllers
{
    /// <summary>
    /// Pruebas unitarias para ComprobantePagoController
    /// Simula el comportamiento del servicio con Moq
    /// </summary>
    public class ComprobantePagoControllerTests
    {
        private readonly Mock<IComprobantePagoService> _mockService;
        private readonly ComprobantePagoController _controller;

        public ComprobantePagoControllerTests()
        {
            _mockService = new Mock<IComprobantePagoService>();
            _controller = new ComprobantePagoController(_mockService.Object);
        }

        [Fact(DisplayName = "GET /api/ComprobantePago - Debe retornar OK con lista de comprobantes")]
        public async Task GetAll_DebeRetornarOkConLista()
        {
            // Arrange
            var comprobantes = new List<ComprobantePagoDto>
            {
                new ComprobantePagoDto
                {
                    Id = 1,
                    Serie = "F001",
                    Correlativo = "00001",
                    Monto = 100m
                }
            };

            var apiResponse = new ApiResponse<List<ComprobantePagoDto>>(
                comprobantes,
                "Comprobantes obtenidos"
            );

            _mockService
                .Setup(x => x.GetAll())
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().NotBeNull();
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<List<ComprobantePagoDto>>>().Subject;
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(1);

            _mockService.Verify(x => x.GetAll(), Times.Once);
        }

        [Fact(DisplayName = "GET /api/ComprobantePago/{id} - Debe retornar OK cuando encuentra el comprobante")]
        public async Task GetById_DebeRetornarOk_CuandoExiste()
        {
            // Arrange
            var comprobanteId = 1;
            var comprobante = new ComprobantePagoDto
            {
                Id = comprobanteId,
                Serie = "F001",
                Correlativo = "00001"
            };

            var apiResponse = new ApiResponse<ComprobantePagoDto>(comprobante, "OK");

            _mockService
                .Setup(x => x.GetById(comprobanteId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetById(comprobanteId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<ComprobantePagoDto>>().Subject;
            response.Success.Should().BeTrue();
            response.Data.Id.Should().Be(comprobanteId);
        }

        [Fact(DisplayName = "GET /api/ComprobantePago/{id} - Debe retornar NotFound cuando no existe")]
        public async Task GetById_DebeRetornarNotFound_CuandoNoExiste()
        {
            // Arrange
            var comprobanteId = 999;
            var apiResponse = new ApiResponse<ComprobantePagoDto>(null, "No encontrado")
            {
                Success = false
            };

            _mockService
                .Setup(x => x.GetById(comprobanteId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetById(comprobanteId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact(DisplayName = "POST /api/ComprobantePago - Debe crear comprobante y retornar Created")]
        public async Task Create_DebeRetornarCreated()
        {
            // Arrange
            var createDto = new ComprobantePagoCreateDto
            {
                SvIdCabecera = 1,
                SvIdDetalle = 1,
                TipoComprobante = "01",
                Serie = "F001",
                Correlativo = "00001",
                FechaEmision = DateTime.Now,
                Monto = 100m
            };

            var comprobanteCreado = new ComprobantePagoDto
            {
                Id = 1,
                Serie = "F001",
                Correlativo = "00001"
            };

            var apiResponse = new ApiResponse<ComprobantePagoDto>(
                comprobanteCreado,
                "Comprobante creado exitosamente"
            );

            _mockService
                .Setup(x => x.Create(createDto))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(_controller.GetById));
            createdResult.RouteValues["id"].Should().Be(1);

            _mockService.Verify(x => x.Create(createDto), Times.Once);
        }

        [Fact(DisplayName = "PUT /api/ComprobantePago/{id}/observado - Debe marcar como observado")]
        public async Task ActualizarObservado_DebRetornarOk()
        {
            // Arrange
            var comprobanteId = 1;
            var comentario = "Documento incompleto";
            var apiResponse = new ApiResponse<bool>(true, "Actualizado correctamente");

            _mockService
                .Setup(x => x.ActualizarComprobanteObservado(comprobanteId, true, comentario))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ActualizarComprobanteObservado(comprobanteId, true, comentario);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<bool>>().Subject;
            response.Success.Should().BeTrue();

            _mockService.Verify(
                x => x.ActualizarComprobanteObservado(comprobanteId, true, comentario),
                Times.Once
            );
        }

        [Fact(DisplayName = "DELETE /api/ComprobantePago/{id} - Debe eliminar comprobante")]
        public async Task Delete_DebeRetornarOk()
        {
            // Arrange
            var comprobanteId = 1;
            var apiResponse = new ApiResponse<bool>(true, "Eliminado correctamente");

            _mockService
                .Setup(x => x.Delete(comprobanteId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Delete(comprobanteId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<bool>>().Subject;
            response.Success.Should().BeTrue();

            _mockService.Verify(x => x.Delete(comprobanteId), Times.Once);
        }

        [Fact(DisplayName = "GET /api/ComprobantePago/detalle/{svIdDetalle} - Debe retornar comprobantes por detalle")]
        public async Task GetBySvIdDetalle_DebeRetornarListaPorDetalle()
        {
            // Arrange
            var svIdDetalle = 5;
            var comprobantes = new List<ComprobantePagoDto>
            {
                new ComprobantePagoDto { Id = 1, SvIdDetalle = svIdDetalle },
                new ComprobantePagoDto { Id = 2, SvIdDetalle = svIdDetalle }
            };

            var apiResponse = new ApiResponse<List<ComprobantePagoDto>>(comprobantes, "OK");

            _mockService
                .Setup(x => x.GetBySvIdDetalle(svIdDetalle))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetBySvIdDetalle(svIdDetalle);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<List<ComprobantePagoDto>>>().Subject;
            response.Data.Should().HaveCount(2);
            response.Data.All(c => c.SvIdDetalle == svIdDetalle).Should().BeTrue();
        }

        [Theory(DisplayName = "GET - Debe validar parámetros de entrada")]
        [InlineData(0)] // ID inválido
        [InlineData(-1)] // ID negativo
        public async Task GetById_DebeValidarParametros(int idInvalido)
        {
            // Arrange
            var apiResponse = new ApiResponse<ComprobantePagoDto>(null, "ID inválido")
            {
                Success = false
            };

            _mockService
                .Setup(x => x.GetById(idInvalido))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetById(idInvalido);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact(DisplayName = "POST - Debe manejar errores del servicio")]
        public async Task Create_DebeManejExcepcionDelServicio()
        {
            // Arrange
            var createDto = new ComprobantePagoCreateDto
            {
                Serie = "F001",
                Correlativo = "00001",
                FechaEmision = DateTime.Now,
                Monto = 100m
            };

            var apiResponse = new ApiResponse<ComprobantePagoDto>(null, "Error al crear")
            {
                Success = false,
                Errors = new List<string> { "Error de base de datos" }
            };

            _mockService
                .Setup(x => x.Create(createDto))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeAssignableTo<ApiResponse<ComprobantePagoDto>>().Subject;
            response.Success.Should().BeFalse();
            response.Errors.Should().Contain("Error de base de datos");
        }
    }
}
