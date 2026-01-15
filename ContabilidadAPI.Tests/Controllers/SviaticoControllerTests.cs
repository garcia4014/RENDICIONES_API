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
    /// Pruebas unitarias para SviaticoController
    /// </summary>
    public class SviaticoControllerTests
    {
        private readonly Mock<ISviaticoService> _mockService;
        private readonly SviaticoController _controller;

        public SviaticoControllerTests()
        {
            _mockService = new Mock<ISviaticoService>();
            _controller = new SviaticoController(_mockService.Object);
        }

        [Fact(DisplayName = "GET /api/Sviatico/{id} - Debe retornar viático por ID")]
        public async Task GetSviaticoById_DebeRetornarOk()
        {
            // Arrange
            var viaticoId = 1;
            var viatico = new SviaticoDto
            {
                Id = viaticoId,
                NumeroRendicion = "RV-2024-001",
                Estado = "Pendiente",
                MontoTotal = 500m
            };

            var apiResponse = new ApiResponse<SviaticoDto>(viatico, "OK");

            _mockService
                .Setup(x => x.GetSviaticoById(viaticoId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetSviaticoById(viaticoId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<SviaticoDto>>().Subject;
            response.Success.Should().BeTrue();
            response.Data.Id.Should().Be(viaticoId);
        }

        [Fact(DisplayName = "POST /api/Sviatico/cabecera - Debe crear viático con número correlativo")]
        public async Task SaveCabecera_DebeCrearViatico()
        {
            // Arrange
            var createDto = new SviaticoCabeceraCreateDto
            {
                UsuarioId = 1,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddDays(3),
                MontoTotal = 500m,
                Motivo = "Viaje de trabajo"
            };

            var viaticoCreado = new SviaticoDto
            {
                Id = 1,
                NumeroRendicion = "RV-2024-001",
                MontoTotal = 500m
            };

            var apiResponse = new ApiResponse<SviaticoDto>(
                viaticoCreado,
                "Viático creado exitosamente"
            );

            _mockService
                .Setup(x => x.SaveCabecera(createDto))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.SaveCabecera(createDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(_controller.GetSviaticoById));
            
            var response = createdResult.Value.Should().BeAssignableTo<ApiResponse<SviaticoDto>>().Subject;
            response.Data.NumeroRendicion.Should().StartWith("RV-");

            _mockService.Verify(x => x.SaveCabecera(createDto), Times.Once);
        }

        [Fact(DisplayName = "POST /api/Sviatico/detalle - Debe agregar detalle a viático")]
        public async Task SaveDetalle_DebeAgregarDetalle()
        {
            // Arrange
            var detalleDto = new SviaticoDetalleCreateDto
            {
                SvIdCabecera = 1,
                TipoGastoId = 1,
                Descripcion = "Hospedaje",
                Monto = 200m,
                FechaGasto = DateTime.Now
            };

            var apiResponse = new ApiResponse<bool>(true, "Detalle agregado");

            _mockService
                .Setup(x => x.SaveDetalle(detalleDto))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.SaveDetalle(detalleDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<bool>>().Subject;
            response.Success.Should().BeTrue();

            _mockService.Verify(x => x.SaveDetalle(detalleDto), Times.Once);
        }

        [Fact(DisplayName = "PUT /api/Sviatico/detalle/{id}/observado - Debe actualizar estado observado")]
        public async Task ActualizarDetalleObservado_DebeActualizar()
        {
            // Arrange
            var detalleId = 1;
            var observacion = "Falta sustento";
            var apiResponse = new ApiResponse<bool>(true, "Actualizado");

            _mockService
                .Setup(x => x.ActualizarDetalleObservado(detalleId, true, observacion))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ActualizarDetalleObservado(detalleId, true, observacion);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<bool>>().Subject;
            response.Success.Should().BeTrue();
        }

        [Fact(DisplayName = "GET /api/Sviatico/usuario/{usuarioId} - Debe retornar viáticos por usuario")]
        public async Task GetByUsuario_DebeRetornarListaPorUsuario()
        {
            // Arrange
            var usuarioId = 1;
            var viaticos = new List<SviaticoDto>
            {
                new SviaticoDto { Id = 1, UsuarioId = usuarioId, NumeroRendicion = "RV-2024-001" },
                new SviaticoDto { Id = 2, UsuarioId = usuarioId, NumeroRendicion = "RV-2024-002" }
            };

            var apiResponse = new ApiResponse<List<SviaticoDto>>(viaticos, "OK");

            _mockService
                .Setup(x => x.GetByUsuario(usuarioId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetByUsuario(usuarioId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<List<SviaticoDto>>>().Subject;
            response.Data.Should().HaveCount(2);
            response.Data.All(v => v.UsuarioId == usuarioId).Should().BeTrue();
        }

        [Theory(DisplayName = "Debe validar montos negativos")]
        [InlineData(-100)]
        [InlineData(0)]
        public async Task SaveCabecera_DebeValidarMontos(decimal montoInvalido)
        {
            // Arrange
            var createDto = new SviaticoCabeceraCreateDto
            {
                UsuarioId = 1,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddDays(3),
                MontoTotal = montoInvalido,
                Motivo = "Viaje"
            };

            var apiResponse = new ApiResponse<SviaticoDto>(null, "Monto inválido")
            {
                Success = false,
                Errors = new List<string> { "El monto debe ser mayor a cero" }
            };

            _mockService
                .Setup(x => x.SaveCabecera(createDto))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.SaveCabecera(createDto);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = badRequestResult.Value.Should().BeAssignableTo<ApiResponse<SviaticoDto>>().Subject;
            response.Success.Should().BeFalse();
        }

        [Fact(DisplayName = "DELETE /api/Sviatico/{id} - Debe eliminar viático")]
        public async Task Delete_DebeEliminarViatico()
        {
            // Arrange
            var viaticoId = 1;
            var apiResponse = new ApiResponse<bool>(true, "Eliminado correctamente");

            _mockService
                .Setup(x => x.Delete(viaticoId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Delete(viaticoId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<bool>>().Subject;
            response.Success.Should().BeTrue();

            _mockService.Verify(x => x.Delete(viaticoId), Times.Once);
        }

        [Fact(DisplayName = "Debe manejar excepciones del servicio")]
        public async Task GetSviaticoById_DebeManejExcepciones()
        {
            // Arrange
            var viaticoId = 999;
            var apiResponse = new ApiResponse<SviaticoDto>(null, "Error interno")
            {
                Success = false,
                Errors = new List<string> { "Error al consultar la base de datos" }
            };

            _mockService
                .Setup(x => x.GetSviaticoById(viaticoId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetSviaticoById(viaticoId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var response = notFoundResult.Value.Should().BeAssignableTo<ApiResponse<SviaticoDto>>().Subject;
            response.Success.Should().BeFalse();
        }
    }
}
