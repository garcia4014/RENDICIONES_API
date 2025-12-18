using Xunit;
using Moq;
using FluentAssertions;
using ContabilidadAPI.Controllers;
using ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces.Access;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ContabilidadAPI.Tests.Controllers
{
    /// <summary>
    /// Pruebas unitarias para AuthController
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IAccessService> _mockAccessService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAccessService = new Mock<IAccessService>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Configurar JWT mock
            _mockConfiguration.Setup(x => x["JwtSecurityToken:key"]).Returns("SuperSecretKeyForJwtTokenGenerationWithAtLeast32Characters");
            _mockConfiguration.Setup(x => x["JwtSecurityToken:issuer"]).Returns("ContabilidadAPI");
            _mockConfiguration.Setup(x => x["JwtSecurityToken:audience"]).Returns("ContabilidadAPIClient");
            _mockConfiguration.Setup(x => x["JwtSecurityToken:DurationInMinutes"]).Returns("60");
            
            _controller = new AuthController(_mockConfiguration.Object, _mockAccessService.Object);
        }

        [Fact(DisplayName = "POST /api/Auth/login - Debe autenticar usuario válido")]
        public async Task Login_DebeRetornarOk_CuandoCredencialesSonValidas()
        {
            // Arrange
            var loginRequest = new UserLogin
            {
                Username = "12345678",
                Password = "password123"
            };

            var userData = new UsuarioLoginDTO
            {
                idDocumento = "12345678",
                Nombres = "Juan Pérez",
                Correo = "juan@example.com"
            };

            var apiResponse = new ApiResponse<UsuarioLoginDTO>(userData, "Login exitoso");

            _mockAccessService
                .Setup(x => x.ValidarPersonal(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<UsuarioLoginDTO>>().Subject;
            response.Success.Should().BeTrue();
            response.Data.idDocumento.Should().Be("12345678");
            response.Data.token.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "POST /api/Auth/login - Debe retornar Unauthorized con credenciales inválidas")]
        public async Task Login_DebeRetornarUnauthorized_CuandoCredencialesSonInvalidas()
        {
            // Arrange
            var loginRequest = new UserLogin
            {
                Username = "12345678",
                Password = "wrongpassword"
            };

            var apiResponse = new ApiResponse<UsuarioLoginDTO>(null, "Credenciales inválidas")
            {
                Success = false
            };

            _mockAccessService
                .Setup(x => x.ValidarPersonal(loginRequest.Username, loginRequest.Password))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact(DisplayName = "POST /api/Auth/GetPerfilByDni - Debe retornar perfiles por DNI")]
        public async Task GetPerfilByDni_DebeRetornarOk_CuandoExistenPerfiles()
        {
            // Arrange
            var dni = "12345678";
            var perfiles = new List<PerfilDTO>
            {
                new PerfilDTO { perfilId = 1, perfilDescripcion = "Administrador" },
                new PerfilDTO { perfilId = 2, perfilDescripcion = "Usuario" }
            };

            var apiResponse = new ApiResponse<List<PerfilDTO>>(perfiles, "Perfiles encontrados");

            _mockAccessService
                .Setup(x => x.GetPerfilesByUsuario(dni))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetPerfilByDni(dni);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<List<PerfilDTO>>>().Subject;
            response.Data.Should().HaveCount(2);
        }

        [Fact(DisplayName = "POST /api/Auth/GetPerfilByDni - Debe retornar NotFound cuando no hay perfiles")]
        public async Task GetPerfilByDni_DebeRetornarNotFound_CuandoNoExistenPerfiles()
        {
            // Arrange
            var dni = "99999999";
            var apiResponse = new ApiResponse<List<PerfilDTO>>(null, "No se encontraron perfiles");

            _mockAccessService
                .Setup(x => x.GetPerfilesByUsuario(dni))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.GetPerfilByDni(dni);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact(DisplayName = "POST /api/Auth/login - Debe manejar excepciones")]
        public async Task Login_DebeManejarExcepciones()
        {
            // Arrange
            var loginRequest = new UserLogin
            {
                Username = "12345678",
                Password = "password123"
            };

            _mockAccessService
                .Setup(x => x.ValidarPersonal(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Theory(DisplayName = "POST /api/Auth/login - Debe validar campos requeridos")]
        [InlineData("", "password")]
        [InlineData("user", "")]
        public async Task Login_DebeValidarCamposRequeridos(string username, string password)
        {
            // Arrange
            var loginRequest = new UserLogin
            {
                Username = username,
                Password = password
            };

            var apiResponse = new ApiResponse<UsuarioLoginDTO>(null, "Credenciales incompletas")
            {
                Success = false
            };

            _mockAccessService
                .Setup(x => x.ValidarPersonal(username, password))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}
