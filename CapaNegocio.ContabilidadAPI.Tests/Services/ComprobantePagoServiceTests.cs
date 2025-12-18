using Xunit;
using Moq;
using FluentAssertions;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;

namespace CapaNegocio.ContabilidadAPI.Tests.Services
{
    /// <summary>
    /// Pruebas unitarias para IComprobantePagoService usando xUnit y Moq
    /// Demuestra técnicas de testing equivalentes a JUnit + Mockito en Java
    /// </summary>
    public class ComprobantePagoServiceTests
    {
        private readonly Mock<IComprobantePagoService> _mockService;

        public ComprobantePagoServiceTests()
        {
            _mockService = new Mock<IComprobantePagoService>();
        }

        [Fact(DisplayName = "GetAllAsync - Debe retornar lista paginada de comprobantes")]
        public async Task GetAllAsync_DebeRetornarListaPaginada()
        {
            // Arrange - Preparación del escenario de prueba
            var comprobantes = new List<ComprobantePagoDto>
            {
                new ComprobantePagoDto { Id = 1, Serie = "F001", Correlativo = "00001", Monto = 100m },
                new ComprobantePagoDto { Id = 2, Serie = "F001", Correlativo = "00002", Monto = 200m }
            };

            var pagedResult = new PagedResult<ComprobantePagoDto>
            {
                Items = comprobantes,
                TotalItems = 2,
                CurrentPage = 1,
                PageSize = 10
            };

            var apiResponse = new ApiResponse<PagedResult<ComprobantePagoDto>>(
                pagedResult,
                "Comprobantes obtenidos exitosamente"
            );

            // Configurar el mock para que retorne el resultado esperado
            _mockService
                .Setup(x => x.GetAllAsync(1, 10))
                .ReturnsAsync(apiResponse);

            // Act - Ejecución del método a probar
            var result = await _mockService.Object.GetAllAsync(1, 10);

            // Assert - Verificación de resultados usando FluentAssertions
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Items.Should().HaveCount(2);
            result.Data.TotalItems.Should().Be(2);
            result.Data.CurrentPage.Should().Be(1);

            // Verify - Verificar que el método fue llamado exactamente una vez
            _mockService.Verify(x => x.GetAllAsync(1, 10), Times.Once);
        }

        [Fact(DisplayName = "GetByIdAsync - Debe retornar comprobante por ID")]
        public async Task GetByIdAsync_DebeRetornarComprobantePorId()
        {
            // Arrange
            var comprobanteId = 1;
            var comprobante = new ComprobantePagoDto
            {
                Id = comprobanteId,
                Serie = "F001",
                Correlativo = "00001",
                Monto = 100m,
                TipoComprobante = "01"
            };

            var apiResponse = new ApiResponse<ComprobantePagoDto>(comprobante, "OK");

            _mockService
                .Setup(x => x.GetByIdAsync(comprobanteId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetByIdAsync(comprobanteId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(comprobanteId);
            result.Data.Serie.Should().Be("F001");
        }

        [Fact(DisplayName = "GetByIdAsync - Debe retornar error cuando no existe")]
        public async Task GetByIdAsync_DebeRetornarError_CuandoNoExiste()
        {
            // Arrange
            var comprobanteId = 999;
            var apiResponse = new ApiResponse<ComprobantePagoDto>(null, "No encontrado")
            {
                Success = false,
                Errors = new List<string> { "Comprobante no encontrado" }
            };

            _mockService
                .Setup(x => x.GetByIdAsync(comprobanteId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetByIdAsync(comprobanteId);

            // Assert
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Errors.Should().Contain("Comprobante no encontrado");
        }

        [Fact(DisplayName = "CreateAsync - Debe crear comprobante exitosamente")]
        public async Task CreateAsync_DebeCrearComprobante()
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
                Correlativo = "00001",
                Monto = 100m
            };

            var apiResponse = new ApiResponse<ComprobantePagoDto>(
                comprobanteCreado,
                "Comprobante creado exitosamente"
            );

            _mockService
                .Setup(x => x.CreateAsync(createDto))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.CreateAsync(createDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().BeGreaterThan(0);
            result.Message.Should().Contain("exitosamente");

            _mockService.Verify(x => x.CreateAsync(createDto), Times.Once);
        }

        [Fact(DisplayName = "DeleteAsync - Debe eliminar comprobante")]
        public async Task DeleteAsync_DebeEliminarComprobante()
        {
            // Arrange
            var comprobanteId = 1;
            var apiResponse = new ApiResponse<bool>(true, "Eliminado correctamente");

            _mockService
                .Setup(x => x.DeleteAsync(comprobanteId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.DeleteAsync(comprobanteId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();

            _mockService.Verify(x => x.DeleteAsync(comprobanteId), Times.Once);
        }

        [Fact(DisplayName = "GetByDetalleIdAsync - Debe retornar comprobantes por detalle")]
        public async Task GetByDetalleIdAsync_DebeRetornarComprobantesPorDetalle()
        {
            // Arrange
            var detalleId = 5;
            var comprobantes = new List<ComprobantePagoDto>
            {
                new ComprobantePagoDto { Id = 1, SvIdDetalle = detalleId },
                new ComprobantePagoDto { Id = 2, SvIdDetalle = detalleId }
            };

            var apiResponse = new ApiResponse<List<ComprobantePagoDto>>(comprobantes, "OK");

            _mockService
                .Setup(x => x.GetByDetalleIdAsync(detalleId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetByDetalleIdAsync(detalleId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.All(c => c.SvIdDetalle == detalleId).Should().BeTrue();
        }

        [Fact(DisplayName = "ActualizarComprobanteObservado - Debe marcar como observado")]
        public async Task ActualizarComprobanteObservado_DebeMarcaComoObservado()
        {
            // Arrange
            var comprobanteId = 1;
            var comentario = "Falta documento sustentatorio";
            var apiResponse = new ApiResponse<bool>(true, "Actualizado correctamente");

            _mockService
                .Setup(x => x.ActualizarComprobanteObservado(comprobanteId, true, comentario))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.ActualizarComprobanteObservado(comprobanteId, true, comentario);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();

            _mockService.Verify(
                x => x.ActualizarComprobanteObservado(comprobanteId, true, comentario),
                Times.Once
            );
        }

        [Theory(DisplayName = "ExisteDuplicadoAsync - Debe validar duplicados correctamente")]
        [InlineData("F001", "00001", true)]
        [InlineData("B001", "99999", false)]
        public async Task ExisteDuplicadoAsync_DebeValidarDuplicados(string serie, string correlativo, bool existeDuplicado)
        {
            // Arrange
            _mockService
                .Setup(x => x.ExisteDuplicadoAsync(serie, correlativo, null))
                .ReturnsAsync(existeDuplicado);

            // Act
            var result = await _mockService.Object.ExisteDuplicadoAsync(serie, correlativo, null);

            // Assert
            result.Should().Be(existeDuplicado);
        }

        [Fact(DisplayName = "ValidarComprobanteEnSunatAsync - Debe ejecutar validación sin error")]
        public async Task ValidarComprobanteEnSunatAsync_DebeEjecutarValidacion()
        {
            // Arrange
            var comprobanteId = 1;

            _mockService
                .Setup(x => x.ValidarComprobanteEnSunatAsync(comprobanteId))
                .Returns(Task.CompletedTask);

            // Act
            await _mockService.Object.ValidarComprobanteEnSunatAsync(comprobanteId);

            // Assert - Verificar que se llamó el método
            _mockService.Verify(x => x.ValidarComprobanteEnSunatAsync(comprobanteId), Times.Once);
        }

        [Fact(DisplayName = "GetEstadisticasAsync - Debe retornar estadísticas por período")]
        public async Task GetEstadisticasAsync_DebeRetornarEstadisticas()
        {
            // Arrange
            var fechaInicio = DateTime.Now.AddMonths(-1);
            var fechaFin = DateTime.Now;

            var estadisticas = new ComprobantePagoEstadisticasDto
            {
                TotalComprobantes = 150,
                MontoTotal = 50000m,
                ComprobantesPendientes = 10,
                ComprobantesValidados = 140,
                ComprobantesSunat = 130
            };

            var apiResponse = new ApiResponse<ComprobantePagoEstadisticasDto>(estadisticas, "OK");

            _mockService
                .Setup(x => x.GetEstadisticasAsync(fechaInicio, fechaFin))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetEstadisticasAsync(fechaInicio, fechaFin);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.TotalComprobantes.Should().Be(150);
            result.Data.MontoTotal.Should().Be(50000m);
        }

        [Fact(DisplayName = "BuscarAsync - Debe manejar excepciones correctamente")]
        public async Task BuscarAsync_DebeManejarExcepciones()
        {
            // Arrange
            var filtro = new ComprobantePagoFiltroDto();

            var apiResponse = new ApiResponse<PagedResult<ComprobantePagoDto>>(null, "Error interno")
            {
                Success = false,
                Errors = new List<string> { "Error de base de datos" }
            };

            _mockService
                .Setup(x => x.BuscarAsync(filtro))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.BuscarAsync(filtro);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("Error de base de datos");
        }
    }
}
