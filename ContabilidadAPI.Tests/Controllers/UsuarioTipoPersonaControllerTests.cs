using ContabilidadAPI.Controllers;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace ContabilidadAPI.Tests.Controllers
{
    public class UsuarioTipoPersonaControllerTests
    {
        private readonly Mock<IUsuarioTipoPersonaService> _mockService;
        private readonly UsuarioTipoPersonaController _controller;

        public UsuarioTipoPersonaControllerTests()
        {
            _mockService = new Mock<IUsuarioTipoPersonaService>();
            _controller = new UsuarioTipoPersonaController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var usuarios = new List<UsuarioTipoPersonaDto>
            {
                new UsuarioTipoPersonaDto { Code = "USR001", Nombre = "Usuario 1" },
                new UsuarioTipoPersonaDto { Code = "USR002", Nombre = "Usuario 2" }
            };
            var response = new ApiResponse<List<UsuarioTipoPersonaDto>>(usuarios, "Success");
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(response);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(response);
            _mockService.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ShouldReturnBadRequest_WhenServiceFails()
        {
            // Arrange
            var response = new ApiResponse<List<UsuarioTipoPersonaDto>>(null, "Error");
            response.Success = false;
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(response);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetByCode_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            var usuario = new UsuarioTipoPersonaDto { Code = "USR001", Nombre = "Usuario 1" };
            var response = new ApiResponse<UsuarioTipoPersonaDto>(usuario, "Success");
            _mockService.Setup(s => s.GetByCodeAsync("USR001")).ReturnsAsync(response);

            // Act
            var result = await _controller.GetByCode("USR001");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.GetByCodeAsync("USR001"), Times.Once);
        }

        [Fact]
        public async Task GetByCode_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var response = new ApiResponse<UsuarioTipoPersonaDto>(null, "Not found");
            response.Success = true; // Success is true but Data is null
            _mockService.Setup(s => s.GetByCodeAsync("NOEXISTE")).ReturnsAsync(response);

            // Act
            var result = await _controller.GetByCode("NOEXISTE");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetByCode_ShouldReturnBadRequest_WhenServiceFailsWithData()
        {
            // Arrange
            var usuario = new UsuarioTipoPersonaDto { Code = "USR001", Nombre = "Usuario 1" };
            var response = new ApiResponse<UsuarioTipoPersonaDto>(usuario, "Error");
            response.Success = false;
            _mockService.Setup(s => s.GetByCodeAsync("USR001")).ReturnsAsync(response);

            // Act
            var result = await _controller.GetByCode("USR001");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetFiltered_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var filtro = new UsuarioTipoPersonaFiltroDto { PageNumber = 1, PageSize = 10 };
            var usuarios = new List<UsuarioTipoPersonaDto>
            {
                new UsuarioTipoPersonaDto { Code = "USR001", Nombre = "Usuario 1" }
            };
            var response = new ApiResponse<List<UsuarioTipoPersonaDto>>(usuarios, "Success");
            _mockService.Setup(s => s.GetFilteredAsync(filtro)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetFiltered(filtro);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.GetFilteredAsync(filtro), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var createDto = new UsuarioTipoPersonaCreateDto { CodUsu = "USR001", TpId = 1 };
            var usuario = new UsuarioTipoPersonaDto { Code = "USR001", Nombre = "Usuario 1" };
            var response = new ApiResponse<UsuarioTipoPersonaDto>(usuario, "Success");
            _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.ActionName.Should().Be(nameof(_controller.GetByCode));
            _mockService.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenServiceFails()
        {
            // Arrange
            var createDto = new UsuarioTipoPersonaCreateDto { CodUsu = "USR001", TpId = 1 };
            var response = new ApiResponse<UsuarioTipoPersonaDto>(null, "Error");
            response.Success = false;
            _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var updateDto = new UsuarioTipoPersonaUpdateDto { TpId = 2 };
            var usuario = new UsuarioTipoPersonaDto { Code = "USR001", Nombre = "Usuario Updated" };
            var response = new ApiResponse<UsuarioTipoPersonaDto>(usuario, "Success");
            _mockService.Setup(s => s.UpdateAsync("USR001", updateDto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Update("USR001", updateDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.UpdateAsync("USR001", updateDto), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var updateDto = new UsuarioTipoPersonaUpdateDto { TpId = 2 };
            var response = new ApiResponse<UsuarioTipoPersonaDto>(null, "Not found");
            response.Success = true; // Success true but Data is null
            _mockService.Setup(s => s.UpdateAsync("NOEXISTE", updateDto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Update("NOEXISTE", updateDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var response = new ApiResponse<bool>(true, "Deleted successfully");
            _mockService.Setup(s => s.DeleteAsync("USR001")).ReturnsAsync(response);

            // Act
            var result = await _controller.Delete("USR001");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.DeleteAsync("USR001"), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var response = new ApiResponse<bool>(false, "Usuario no encontrado");
            response.Success = false;
            _mockService.Setup(s => s.DeleteAsync("NOEXISTE")).ReturnsAsync(response);

            // Act
            var result = await _controller.Delete("NOEXISTE");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Exists_ShouldReturnOk_WithTrue_WhenUserExists()
        {
            // Arrange
            var response = new ApiResponse<bool>(true, "Exists");
            _mockService.Setup(s => s.ExistsAsync("USR001")).ReturnsAsync(response);

            // Act
            var result = await _controller.Exists("USR001");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(response);
            _mockService.Verify(s => s.ExistsAsync("USR001"), Times.Once);
        }

        [Fact]
        public async Task GetByTipoPersona_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var usuarios = new List<UsuarioTipoPersonaDto>
            {
                new UsuarioTipoPersonaDto { Code = "USR001", Nombre = "Usuario 1" }
            };
            var response = new ApiResponse<List<UsuarioTipoPersonaDto>>(usuarios, "Success");
            _mockService.Setup(s => s.GetByTipoPersonaAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetByTipoPersona(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.GetByTipoPersonaAsync(1), Times.Once);
        }

        [Theory]
        [InlineData("USR001")]
        [InlineData("USR002")]
        [InlineData("USR999")]
        public async Task GetByCode_ShouldWorkWithDifferentCodes(string code)
        {
            // Arrange
            var usuario = new UsuarioTipoPersonaDto { Code = code, Nombre = $"Usuario {code}" };
            var response = new ApiResponse<UsuarioTipoPersonaDto>(usuario, "Success");
            _mockService.Setup(s => s.GetByCodeAsync(code)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetByCode(code);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.GetByCodeAsync(code), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetByTipoPersona_ShouldWorkWithDifferentTipoPersonaIds(int tpId)
        {
            // Arrange
            var usuarios = new List<UsuarioTipoPersonaDto>
            {
                new UsuarioTipoPersonaDto { Code = "USR001", Nombre = "Usuario 1" }
            };
            var response = new ApiResponse<List<UsuarioTipoPersonaDto>>(usuarios, "Success");
            _mockService.Setup(s => s.GetByTipoPersonaAsync(tpId)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetByTipoPersona(tpId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.GetByTipoPersonaAsync(tpId), Times.Once);
        }
    }
}
