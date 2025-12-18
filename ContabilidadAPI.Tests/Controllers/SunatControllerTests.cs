using Xunit;
using Moq;
using FluentAssertions;
using ContabilidadAPI.Controllers;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContabilidadAPI.Tests.Controllers
{
    /// <summary>
    /// Pruebas unitarias para SunatController
    /// </summary>
    public class SunatControllerTests
    {
        private readonly Mock<ISunatService> _mockSunatService;
        private readonly Mock<ISunatTokenService> _mockTokenService;
        private readonly Mock<ISunatComprobanteService> _mockComprobanteService;
        private readonly Mock<ILogger<SunatController>> _mockLogger;
        private readonly Mock<IOptions<SunatConfigurationDto>> _mockConfig;
        private readonly Mock<ISviatico> _mockSviatico;
        private readonly Mock<IComprobantePago> _mockComprobante;
        private readonly Mock<IComprobantePagoService> _mockComprobanteRepo;
        private readonly SunatController _controller;

        public SunatControllerTests()
        {
            _mockSunatService = new Mock<ISunatService>();
            _mockTokenService = new Mock<ISunatTokenService>();
            _mockComprobanteService = new Mock<ISunatComprobanteService>();
            _mockLogger = new Mock<ILogger<SunatController>>();
            _mockConfig = new Mock<IOptions<SunatConfigurationDto>>();
            _mockSviatico = new Mock<ISviatico>();
            _mockComprobante = new Mock<IComprobantePago>();
            _mockComprobanteRepo = new Mock<IComprobantePagoService>();

            var sunatConfig = new SunatConfigurationDto
            {
                BaseUrl = "https://api.sunat.gob.pe",
                ClientId = "test_client",
                ClientSecret = "test_secret"
            };

            _mockConfig.Setup(x => x.Value).Returns(sunatConfig);

            _controller = new SunatController(
                _mockSunatService.Object,
                _mockTokenService.Object,
                _mockComprobanteService.Object,
                _mockLogger.Object,
                _mockConfig.Object,
                _mockSviatico.Object,
                _mockComprobante.Object,
                _mockComprobanteRepo.Object
            );
        }

        [Fact(DisplayName = "POST /api/Sunat/token - Debe obtener token SUNAT")]
        public async Task ObtenerToken_DebeRetornarOk_ConToken()
        {
            // Arrange
            var tokenRequest = new SunatTokenRequestDto
            {
                client_id = "test_client",
                client_secret = "test_secret"
            };

            var tokenResponse = new SunatTokenResponseDto
            {
                access_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                token_type = "Bearer",
                expires_in = 3600
            };

            var apiResponse = new ApiResponse<SunatTokenResponseDto>(tokenResponse, "Token obtenido");

            _mockTokenService
                .Setup(x => x.ObtenerTokenAsync(tokenRequest.client_id, tokenRequest.client_secret))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ObtenerToken(tokenRequest);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<SunatTokenResponseDto>>().Subject;
            response.Data.access_token.Should().NotBeNullOrEmpty();
            response.Data.token_type.Should().Be("Bearer");
        }

        [Fact(DisplayName = "POST /api/Sunat/token - Debe retornar BadRequest con credenciales inválidas")]
        public async Task ObtenerToken_DebeRetornarBadRequest_ConCredencialesInvalidas()
        {
            // Arrange
            var tokenRequest = new SunatTokenRequestDto
            {
                client_id = "invalid_client",
                client_secret = "invalid_secret"
            };

            var apiResponse = new ApiResponse<SunatTokenResponseDto>(null, "Credenciales inválidas")
            {
                Success = false
            };

            _mockTokenService
                .Setup(x => x.ObtenerTokenAsync(tokenRequest.client_id, tokenRequest.client_secret))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ObtenerToken(tokenRequest);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact(DisplayName = "POST /api/Sunat/validar-comprobante - Debe validar comprobante en SUNAT")]
        public async Task ValidarComprobante_DebeRetornarOk()
        {
            // Arrange
            var request = new SunatValidacionRequestDto
            {
                RucEmisor = "20123456789",
                TipoComprobante = "01",
                Serie = "F001",
                Numero = "00001234"
            };

            var validacionResponse = new SunatValidacionResponseDto
            {
                EstadoComprobante = "1",
                EstadoRuc = "ACTIVO",
                CondicionDomicilio = "HABIDO"
            };

            var apiResponse = new ApiResponse<SunatValidacionResponseDto>(validacionResponse, "Validado");

            _mockComprobanteService
                .Setup(x => x.ValidarComprobanteAsync(It.IsAny<string>(), It.IsAny<SunatValidacionRequestDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ValidarComprobante(request);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<SunatValidacionResponseDto>>().Subject;
            response.Data.EstadoRuc.Should().Be("ACTIVO");
        }

        [Fact(DisplayName = "POST /api/Sunat/validar-comprobante - Debe manejar errores de validación")]
        public async Task ValidarComprobante_DebeManejarErrores()
        {
            // Arrange
            var request = new SunatValidacionRequestDto
            {
                RucEmisor = "20123456789",
                TipoComprobante = "01",
                Serie = "F001",
                Numero = "00001234"
            };

            var apiResponse = new ApiResponse<SunatValidacionResponseDto>(null, "Error en SUNAT")
            {
                Success = false,
                Errors = new List<string> { "Comprobante no encontrado en SUNAT" }
            };

            _mockComprobanteService
                .Setup(x => x.ValidarComprobanteAsync(It.IsAny<string>(), It.IsAny<SunatValidacionRequestDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ValidarComprobante(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory(DisplayName = "POST /api/Sunat/token - Debe validar campos requeridos")]
        [InlineData("", "secret")]
        [InlineData("client", "")]
        public async Task ObtenerToken_DebeValidarCamposRequeridos(string clientId, string clientSecret)
        {
            // Arrange
            var tokenRequest = new SunatTokenRequestDto
            {
                client_id = clientId,
                client_secret = clientSecret
            };

            // Simular ModelState inválido
            _controller.ModelState.AddModelError("client_id", "Required");

            // Act
            var result = await _controller.ObtenerToken(tokenRequest);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact(DisplayName = "POST /api/Sunat/validar-comprobante - Debe llamar al servicio con token")]
        public async Task ValidarComprobante_DebeUsarToken()
        {
            // Arrange
            var request = new SunatValidacionRequestDto
            {
                RucEmisor = "20123456789",
                TipoComprobante = "01",
                Serie = "F001",
                Numero = "00001234"
            };

            var validacionResponse = new SunatValidacionResponseDto
            {
                EstadoComprobante = "1"
            };

            var apiResponse = new ApiResponse<SunatValidacionResponseDto>(validacionResponse, "OK");

            _mockComprobanteService
                .Setup(x => x.ValidarComprobanteAsync(It.IsAny<string>(), request))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ValidarComprobante(request);

            // Assert
            _mockComprobanteService.Verify(
                x => x.ValidarComprobanteAsync(It.IsAny<string>(), It.IsAny<SunatValidacionRequestDto>()),
                Times.Once
            );
        }
    }
}
