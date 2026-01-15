using Xunit;
using Moq;
using FluentAssertions;
using ContabilidadAPI.Controllers;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ContabilidadAPI.Tests.Controllers
{
    /// <summary>
    /// Pruebas unitarias para OcrController
    /// </summary>
    public class OcrControllerTests
    {
        private readonly Mock<IOcrService> _mockOcrService;
        private readonly Mock<ILogger<OcrController>> _mockLogger;
        private readonly OcrController _controller;

        public OcrControllerTests()
        {
            _mockOcrService = new Mock<IOcrService>();
            _mockLogger = new Mock<ILogger<OcrController>>();
            _controller = new OcrController(_mockOcrService.Object, _mockLogger.Object);
        }

        [Fact(DisplayName = "POST /api/Ocr/extract-text - Debe extraer texto de archivo PDF")]
        public async Task ExtractText_DebeRetornarOk_ConTextExtraido()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Fake PDF content";
            var fileName = "factura.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => ms.CopyTo(stream));

            var ocrResponse = new OcrResponseDto
            {
                ExtractedText = "FACTURA\nRUC: 20123456789\nTotal: S/. 100.00",
                Confidence = 95.5,
                ProcessingTimeMs = 1234
            };

            var apiResponse = new ApiResponse<OcrResponseDto>(ocrResponse, "Texto extraído exitosamente");

            _mockOcrService
                .Setup(x => x.ExtractTextAsync(It.IsAny<OcrRequestDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ExtractText(fileMock.Object);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<ApiResponse<OcrResponseDto>>().Subject;
            response.Data.ExtractedText.Should().Contain("FACTURA");
            response.Data.Confidence.Should().BeGreaterThan(90);
        }

        [Fact(DisplayName = "POST /api/Ocr/extract-text - Debe retornar BadRequest sin archivo")]
        public async Task ExtractText_DebeRetornarBadRequest_CuandoArchivoEsNull()
        {
            // Act
            var result = await _controller.ExtractText(null);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("No se ha proporcionado un archivo válido");
        }

        [Fact(DisplayName = "POST /api/Ocr/extract-text - Debe retornar BadRequest con archivo vacío")]
        public async Task ExtractText_DebeRetornarBadRequest_CuandoArchivoEstaVacio()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _controller.ExtractText(fileMock.Object);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact(DisplayName = "POST /api/Ocr/extract-text - Debe manejar errores del servicio")]
        public async Task ExtractText_DebeManejErroresDelServicio()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.Length).Returns(1000);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var apiResponse = new ApiResponse<OcrResponseDto>(null, "Error al procesar imagen")
            {
                Success = false,
                Errors = new List<string> { "Formato no soportado" }
            };

            _mockOcrService
                .Setup(x => x.ExtractTextAsync(It.IsAny<OcrRequestDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ExtractText(fileMock.Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Theory(DisplayName = "POST /api/Ocr/extract-text - Debe soportar diferentes formatos")]
        [InlineData("factura.pdf")]
        [InlineData("recibo.jpg")]
        [InlineData("comprobante.png")]
        public async Task ExtractText_DebeSoportarDiferentesFormatos(string fileName)
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(1000);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var ocrResponse = new OcrResponseDto { ExtractedText = "Texto de prueba" };
            var apiResponse = new ApiResponse<OcrResponseDto>(ocrResponse, "OK");

            _mockOcrService
                .Setup(x => x.ExtractTextAsync(It.IsAny<OcrRequestDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.ExtractText(fileMock.Object);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockOcrService.Verify(x => x.ExtractTextAsync(It.IsAny<OcrRequestDto>()), Times.Once);
        }
    }
}
