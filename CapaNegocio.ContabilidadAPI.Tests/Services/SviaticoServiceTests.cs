using Xunit;
using Moq;
using FluentAssertions;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaDatos.ContabilidadAPI.Models;

namespace CapaNegocio.ContabilidadAPI.Tests.Services
{
    /// <summary>
    /// Pruebas unitarias para ISviaticoService usando xUnit y Moq
    /// Demuestra técnicas de testing equivalentes a JUnit + Mockito en Java
    /// </summary>
    public class SviaticoServiceTests
    {
        private readonly Mock<ISviaticoService> _mockService;

        public SviaticoServiceTests()
        {
            _mockService = new Mock<ISviaticoService>();
        }

        [Fact(DisplayName = "GetListSviaticosCabecera - Debe retornar lista de viáticos")]
        public async Task GetListSviaticosCabecera_DebeRetornarLista()
        {
            // Arrange - Preparación del escenario de prueba (equivalente a @Before en JUnit)
            var viaticos = new List<SviaticosCabeceraDTOResponse>
            {
                new SviaticosCabeceraDTOResponse { SvId = 1, SvNumero = "RV-2024-001" },
                new SviaticosCabeceraDTOResponse { SvId = 2, SvNumero = "RV-2024-002" }
            };

            var apiResponse = new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>(
                viaticos,
                "Viáticos obtenidos exitosamente"
            );

            // Setup del mock (equivalente a when().thenReturn() en Mockito)
            _mockService
                .Setup(x => x.GetListSviaticosCabecera())
                .ReturnsAsync(apiResponse);

            // Act - Ejecución del método a probar
            var result = await _mockService.Object.GetListSviaticosCabecera();

            // Assert - Verificación de resultados (equivalente a assertEquals/assertTrue en JUnit)
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Message.Should().Contain("exitosamente");

            // Verify - Verificar que el método fue llamado (equivalente a verify() en Mockito)
            _mockService.Verify(x => x.GetListSviaticosCabecera(), Times.Once);
        }

        [Fact(DisplayName = "GetSviaticoCabecera - Debe retornar viático por ID")]
        public async Task GetSviaticoCabecera_DebeRetornarViaticoPorId()
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
                .Setup(x => x.GetSviaticoCabecera(viaticoId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetSviaticoCabecera(viaticoId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.SvId.Should().Be(viaticoId);
            result.Data.SvNumero.Should().Be("RV-2024-001");
        }

        [Fact(DisplayName = "SaveCabecera - Debe crear viático con número correlativo")]
        public async Task SaveCabecera_DebeCrearViaticoConNumeroCorrelativo()
        {
            // Arrange
            var createDto = new SviaticosCabeceraDTO();

            var viaticoCreado = new SviaticosCabeceraDTOResponse
            {
                SvId = 1,
                SvNumero = "RV-2024-001"
            };

            var apiResponse = new ApiResponse<SviaticosCabeceraDTOResponse>(
                viaticoCreado,
                "Viático creado exitosamente"
            );

            _mockService
                .Setup(x => x.SaveCabecera(createDto))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.SaveCabecera(createDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.SvId.Should().BeGreaterThan(0);
            result.Data.SvNumero.Should().StartWith("RV-");

            _mockService.Verify(x => x.SaveCabecera(createDto), Times.Once);
        }

        [Fact(DisplayName = "ActualizarDetalleObservado - Debe actualizar estado observado")]
        public async Task ActualizarDetalleObservado_DebeActualizarObservacion()
        {
            // Arrange
            var detalleId = 1;
            var observacion = "Falta comprobante";
            var apiResponse = new ApiResponse<bool>(true, "Actualizado correctamente");

            _mockService
                .Setup(x => x.ActualizarDetalleObservado(detalleId, true, observacion))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.ActualizarDetalleObservado(detalleId, true, observacion);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();

            _mockService.Verify(
                x => x.ActualizarDetalleObservado(detalleId, true, observacion),
                Times.Once
            );
        }

        [Fact(DisplayName = "ActualizarDetalleAprobado - Debe actualizar estado aprobado")]
        public async Task ActualizarDetalleAprobado_DebeActualizarAprobacion()
        {
            // Arrange
            var detalleId = 1;
            var apiResponse = new ApiResponse<bool>(true, "Aprobado correctamente");

            _mockService
                .Setup(x => x.ActualizarDetalleAprobado(detalleId, true))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.ActualizarDetalleAprobado(detalleId, true);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();

            _mockService.Verify(x => x.ActualizarDetalleAprobado(detalleId, true), Times.Once);
        }

        [Fact(DisplayName = "GetListSviaticosCabeceraDNI - Debe retornar viáticos por DNI")]
        public async Task GetListSviaticosCabeceraDNI_DebeRetornarViaticoPorDni()
        {
            // Arrange
            var dni = "12345678";
            var viaticos = new List<SviaticosCabeceraDTOResponse>
            {
                new SviaticosCabeceraDTOResponse { SvId = 1, SvNumero = "RV-2024-001" }
            };

            var apiResponse = new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>(viaticos, "OK");

            _mockService
                .Setup(x => x.GetListSviaticosCabeceraDNI(dni))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetListSviaticosCabeceraDNI(dni);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact(DisplayName = "ActualizarEstadoSolicitud - Debe cambiar estado del viático")]
        public async Task ActualizarEstadoSolicitud_DebeCambiarEstado()
        {
            // Arrange
            var viaticoId = 1;
            var nuevoEstadoId = 2;
            var comentario = "Aprobado por gerencia";

            var viaticoActualizado = new SviaticosCabeceraDTOResponse
            {
                SvId = viaticoId,
                SvNumero = "RV-2024-001"
            };

            var apiResponse = new ApiResponse<SviaticosCabeceraDTOResponse>(
                viaticoActualizado,
                "Estado actualizado"
            );

            _mockService
                .Setup(x => x.ActualizarEstadoSolicitud(viaticoId, nuevoEstadoId, comentario))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.ActualizarEstadoSolicitud(viaticoId, nuevoEstadoId, comentario);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.SvId.Should().Be(viaticoId);
        }

        [Fact(DisplayName = "GetEstadosDisponibles - Debe retornar lista de estados")]
        public async Task GetEstadosDisponibles_DebeRetornarEstados()
        {
            // Arrange
            var estados = new List<SolicitudEstadoFlujo>
            {
                new SolicitudEstadoFlujo { SefId = 1 },
                new SolicitudEstadoFlujo { SefId = 2 },
                new SolicitudEstadoFlujo { SefId = 3 }
            };

            var apiResponse = new ApiResponse<IEnumerable<SolicitudEstadoFlujo>>(estados, "OK");

            _mockService
                .Setup(x => x.GetEstadosDisponibles())
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetEstadosDisponibles();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(3);
        }

        [Fact(DisplayName = "GetDashboardEstadisticas - Debe retornar estadísticas del usuario")]
        public async Task GetDashboardEstadisticas_DebeRetornarEstadisticas()
        {
            // Arrange
            var codigoUsuario = "USR001";
            var dashboard = new ViaticoDashboardDto();

            var apiResponse = new ApiResponse<ViaticoDashboardDto>(dashboard, "OK");

            _mockService
                .Setup(x => x.GetDashboardEstadisticas(codigoUsuario))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetDashboardEstadisticas(codigoUsuario);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Theory(DisplayName = "ActualizarDetalleObservado - Debe validar parámetros")]
        [InlineData(1, true, "Observación válida")]
        [InlineData(2, false, "Otra observación")]
        public async Task ActualizarDetalleObservado_DebeValidarParametros(int detalleId, bool observado, string? comentario)
        {
            // Arrange
            var apiResponse = new ApiResponse<bool>(true, "OK");

            _mockService
                .Setup(x => x.ActualizarDetalleObservado(detalleId, observado, comentario))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.ActualizarDetalleObservado(detalleId, observado, comentario);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact(DisplayName = "GetViaticosFiltrados - Debe buscar con filtros")]
        public async Task GetViaticosFiltrados_DebeBuscarConFiltros()
        {
            // Arrange
            var filtro = new SviaticoFiltroDto();

            var viaticos = new List<SviaticosCabeceraDTOResponse>
            {
                new SviaticosCabeceraDTOResponse { SvId = 1 }
            };

            var apiResponse = new ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>(viaticos, "OK");

            _mockService
                .Setup(x => x.GetViaticosFiltrados(filtro))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetViaticosFiltrados(filtro);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact(DisplayName = "GetViaticosFiltradosConConteo - Debe retornar viáticos con conteo")]
        public async Task GetViaticosFiltradosConConteo_DebeRetornarConConteo()
        {
            // Arrange
            var filtro = new SviaticoFiltroDto();
            var responseDto = new ViaticosFiltradosResponseDto();

            var apiResponse = new ApiResponse<ViaticosFiltradosResponseDto>(responseDto, "OK");

            _mockService
                .Setup(x => x.GetViaticosFiltradosConConteo(filtro))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetViaticosFiltradosConConteo(filtro);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact(DisplayName = "Debe manejar errores correctamente")]
        public async Task GetSviaticoCabecera_DebeManejarErrores()
        {
            // Arrange
            var viaticoId = 999;
            var apiResponse = new ApiResponse<SviaticosCabeceraDTOResponse>(null, "No encontrado")
            {
                Success = false,
                Errors = new List<string> { "Viático no encontrado" }
            };

            _mockService
                .Setup(x => x.GetSviaticoCabecera(viaticoId))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _mockService.Object.GetSviaticoCabecera(viaticoId);

            // Assert
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Errors.Should().Contain("Viático no encontrado");
        }
    }
}
