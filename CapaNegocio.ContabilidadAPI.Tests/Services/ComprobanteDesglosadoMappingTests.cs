using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Repository.Implementation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CapaNegocio.ContabilidadAPI.Tests.Services;

/// <summary>
/// Tests de integración para <see cref="ComprobanteDesglosadoBackgroundService.ActualizarComprobanteConDatosXml"/>.
/// Ejercitan el pipeline completo: ExtractFromXml → ActualizarComprobanteConDatosXml → ComprobantePago.
/// </summary>
public class ComprobanteDesglosadoMappingTests
{
    private static readonly string XmlDir = Path.Combine(AppContext.BaseDirectory, "TestData");

    private static string LoadXml(string fileName) =>
        File.ReadAllText(Path.Combine(XmlDir, fileName));

    private static ComprobanteDesglosadoBackgroundService CrearServicio()
    {
        var logger = new Mock<ILogger<ComprobanteDesglosadoBackgroundService>>();
        logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        var sp     = new Mock<IServiceProvider>();
        var config = new Mock<IConfiguration>();
        return new ComprobanteDesglosadoBackgroundService(logger.Object, sp.Object, config.Object);
    }

    // ──────────────────────────────────────────────────────────────────
    // FJ02-00000030 — IGV Especial 10.5%, 2 líneas
    // Igv esperado : 6.22  (cabecera SUNAT)
    // Igv CON bug  : 6.23  (suma 1.57+4.66, redondeo acumulado)
    // Subtotal esp.: 59.27 (suma TaxableAmount de líneas)
    // ──────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Mapping FJ02 - IGV Especial: Igv=6.22 (cabecera), NO 6.23 (suma líneas)")]
    public void Mapping_FJ02_IgvEspecial_UsaTaxAmtCabeceraNoSumaLineas()
    {
        // Arrange
        var xmlResult  = ComprobanteExtractor.ExtractFromXml(LoadXml("15513033181-FJ02-00000030.xml"));
        var comprobante = new ComprobantePago { Id = 1, Ruc = 15513033181L };
        var servicio   = CrearServicio();

        // Act
        servicio.ActualizarComprobanteConDatosXml(comprobante, xmlResult);

        // Assert
        comprobante.Igv.Should().Be(6.22m,
            because: "IGV oficial de SUNAT es 6.22; la suma de líneas 1.57+4.66=6.23 difiere por redondeo");
        comprobante.Subtotal.Should().Be(59.27m);
        comprobante.IgvEspecial.Should().BeTrue();
        comprobante.Gravado.Should().BeFalse();
        comprobante.Desglosado.Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────────────────
    // F001-811 — IGV Normal 18%, 1 línea, cabecera sin Percent ni ExCode
    // Igv esperado  : 19.07
    // Igv CON bug   :  0    (MontoIgvDocumento vacío → igvDocumento=0 → igvFinal=0)
    // Subtotal esp. : 105.93
    // ──────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Mapping F001-811 - IGV Normal 18%: Igv=19.07 (NO 0)")]
    public void Mapping_F001_IgvNormal18_IgvEsCorrecto()
    {
        // Arrange
        var xmlResult  = ComprobanteExtractor.ExtractFromXml(LoadXml("10473551794-F001-811.xml"));
        var comprobante = new ComprobantePago { Id = 2, Ruc = 10473551794L };
        var servicio   = CrearServicio();

        // Act
        servicio.ActualizarComprobanteConDatosXml(comprobante, xmlResult);

        // Assert
        comprobante.Igv.Should().Be(19.07m,
            because: "el IGV de cabecera (19.07) no se capturaba cuando Percent=NULL y ExCode=NULL → igvFinal=0");
        comprobante.Subtotal.Should().Be(105.93m);
        comprobante.IgvEspecial.Should().BeFalse();
        comprobante.Gravado.Should().BeTrue();
        comprobante.Desglosado.Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────────────────
    // FEN0-27409 — IGV 18%, ExCode=10 solo en InvoiceLine (no en cabecera)
    // Igv esperado  : 4.58
    // Igv CON bug   : 0    (mismo bug que F001)
    // Subtotal esp. : 25.42
    // ──────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Mapping FEN0 - IGV 18% ExCode en línea: Igv=4.58 (NO 0)")]
    public void Mapping_FEN0_IgvNormal18_ExCodeEnLinea_IgvEsCorrecto()
    {
        // Arrange
        var xmlResult  = ComprobanteExtractor.ExtractFromXml(LoadXml("20454750773-FEN0-27409.xml"));
        var comprobante = new ComprobantePago { Id = 3, Ruc = 20454750773L };
        var servicio   = CrearServicio();

        // Act
        servicio.ActualizarComprobanteConDatosXml(comprobante, xmlResult);

        // Assert
        comprobante.Igv.Should().Be(4.58m);
        comprobante.Subtotal.Should().Be(25.42m);
        comprobante.Gravado.Should().BeTrue();
        comprobante.Desglosado.Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────────────────
    // F64X-00004634 — 6 líneas IGV 18%, 2 líneas con TaxableAmt=6.10
    // Igv esperado  : 5.28  (cabecera SUNAT); suma líneas = 5.29
    // Igv CON bug   : 0     (MontoIgvDocumento vacío)
    // Subtotal esp. : 29.31 (suma de las 6 líneas: 3.56+6.10+1.86+4.15+7.54+6.10)
    // Subtotal c/bug: 23.21 (solo 5 líneas porque Contains descarta el segundo 6.10)
    // ──────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Mapping F64X - 6 líneas con importe duplicado: Igv=5.28 (NO 0 ni 5.29)")]
    public void Mapping_F64X_MultiLinea_IgvEsCabecera_No5_29()
    {
        // Arrange
        var xmlResult  = ComprobanteExtractor.ExtractFromXml(LoadXml("20127765279-F64X-00004634.xml"));
        var comprobante = new ComprobantePago { Id = 4, Ruc = 20127765279L };
        var servicio   = CrearServicio();

        // Act
        servicio.ActualizarComprobanteConDatosXml(comprobante, xmlResult);

        // Assert
        comprobante.Igv.Should().Be(5.28m,
            because: "SUNAT declara IGV=5.28; suma de TaxAmt por línea es 5.29");
        comprobante.Subtotal.Should().Be(29.31m,
            because: "suma de TaxableAmt de las 6 líneas (incluyendo el 6.10 duplicado): 3.56+6.10+1.86+4.15+7.54+6.10=29.31");
        comprobante.Gravado.Should().BeTrue();
        comprobante.Desglosado.Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────────────────
    // Casos límite
    // ──────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Mapping - resultado vacío del extractor: Igv=0, Subtotal=0, Desglosado=true")]
    public void Mapping_ResultadoExtractorVacio_CamposEnCero()
    {
        // Arrange
        var xmlResult  = new ComprobanteExtractorResult(); // todo vacío
        var comprobante = new ComprobantePago { Id = 5 };
        var servicio   = CrearServicio();

        // Act
        servicio.ActualizarComprobanteConDatosXml(comprobante, xmlResult);

        // Assert
        comprobante.Igv.Should().Be(0m);
        comprobante.Subtotal.Should().Be(0m);
        comprobante.Desglosado.Should().BeTrue();
        comprobante.IgvEspecial.Should().BeFalse();
        comprobante.Gravado.Should().BeFalse();
    }

    [Fact(DisplayName = "Mapping - comprobante inafecto: Subtotal=monto inafecto, Igv=0")]
    public void Mapping_Inafecto_IgvEsCero()
    {
        // Arrange
        var xmlResult = new ComprobanteExtractorResult
        {
            MontosInafectos = ["100.00"],
            AfectacionIgvDetectada = true
        };
        var comprobante = new ComprobantePago { Id = 6 };
        var servicio   = CrearServicio();

        // Act
        servicio.ActualizarComprobanteConDatosXml(comprobante, xmlResult);

        // Assert
        comprobante.Subtotal.Should().Be(100.00m);
        comprobante.Igv.Should().Be(0m);
        comprobante.Inafecto.Should().BeTrue();
        comprobante.Desglosado.Should().BeTrue();
    }
}
