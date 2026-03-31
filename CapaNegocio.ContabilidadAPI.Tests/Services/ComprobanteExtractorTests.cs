using CapaDatos.ContabilidadAPI.Models;
using FluentAssertions;

namespace CapaNegocio.ContabilidadAPI.Tests.Services;

/// <summary>
/// Tests de parseo XML para <see cref="ComprobanteExtractor.ExtractFromXml"/>.
/// Usan XMLs reales descargados de SUNAT como fixtures en TestData/.
/// </summary>
public class ComprobanteExtractorTests
{
    private static readonly string XmlDir = Path.Combine(AppContext.BaseDirectory, "TestData");

    private static string LoadXml(string fileName) =>
        File.ReadAllText(Path.Combine(XmlDir, fileName));

    // ──────────────────────────────────────────────
    // FJ02-00000030 — RUC 15513033181
    // IGV Especial 10.5%, 2 líneas
    // Header TaxSubtotal: schemeId=1000, Percent=NULL, ExCode=NULL
    // InvoiceLine 1: TaxableAmt=14.93, TaxAmt=1.57, Code=10, Pct=10.5
    // InvoiceLine 2: TaxableAmt=44.34, TaxAmt=4.66, Code=10, Pct=10.5
    // Header TaxAmount = 6.22  (suma líneas = 1.57+4.66 = 6.23 ← el redondeo que causó el bug reportado)
    // ──────────────────────────────────────────────

    [Fact(DisplayName = "FJ02 - IGV Especial 10.5% - detecta 2 líneas IGV especial")]
    public void ExtractFromXml_FJ02_DetectaDosLineasIgvEspecial()
    {
        var result = ComprobanteExtractor.ExtractFromXml(LoadXml("15513033181-FJ02-00000030.xml"));

        result.AfectacionIgvDetectada.Should().BeTrue();
        result.MontosIgvEspecial.Should().HaveCount(2);
        result.MontosIgvEspecial.Should().Contain("14.93");
        result.MontosIgvEspecial.Should().Contain("44.34");
        result.MontosGravados.Should().BeEmpty();
    }

    [Fact(DisplayName = "FJ02 - IGV Especial 10.5% - MontoIgvDocumento es 6.22 (cabecera), NO la suma de líneas 6.23")]
    public void ExtractFromXml_FJ02_MontoIgvDocumentoEsCabecera_No6_23()
    {
        var result = ComprobanteExtractor.ExtractFromXml(LoadXml("15513033181-FJ02-00000030.xml"));

        // Bug conocido: la suma de TaxAmount por línea es 1.57 + 4.66 = 6.23
        // SUNAT declara en la cabecera TaxSubtotal un TaxAmount = 6.22 (valor oficial)
        result.MontoIgvDocumento.Should().Be("6.22",
            because: "SUNAT declara IGV=6.22 en la cabecera; la suma de líneas 1.57+4.66=6.23 difiere por redondeo");
    }

    [Fact(DisplayName = "FJ02 - IGV Especial 10.5% - MontosBaseIgvEspecial contiene los TaxAmount por línea")]
    public void ExtractFromXml_FJ02_MontosBaseIgvEspecialSonLosTaxAmountPorLinea()
    {
        var result = ComprobanteExtractor.ExtractFromXml(LoadXml("15513033181-FJ02-00000030.xml"));

        result.MontosBaseIgvEspecial.Should().HaveCount(2);
        result.MontosBaseIgvEspecial.Should().Contain("1.57");
        result.MontosBaseIgvEspecial.Should().Contain("4.66");
    }

    // ──────────────────────────────────────────────
    // F001-811 — RUC 10473551794
    // IGV Normal 18%, 1 línea
    // Header TaxSubtotal: schemeId=1000, Percent=NULL, ExCode=NULL   ← no captura MontoIgvDocumento (BUG)
    // InvoiceLine 1: TaxableAmt=105.93, TaxAmt=19.07, Code=10, Pct=18
    // Header TaxAmount = 19.07
    // ──────────────────────────────────────────────

    [Fact(DisplayName = "F001-811 - IGV Normal 18% - detecta como gravado")]
    public void ExtractFromXml_F001_DetectaGravado()
    {
        var result = ComprobanteExtractor.ExtractFromXml(LoadXml("10473551794-F001-811.xml"));

        result.AfectacionIgvDetectada.Should().BeTrue();
        result.MontosGravados.Should().Contain("105.93");
        result.MontosIgvEspecial.Should().BeEmpty();
    }

    [Fact(DisplayName = "F001-811 - IGV Normal 18% - MontoIgvDocumento es 19.07 aunque cabecera no tenga Percent ni ExCode")]
    public void ExtractFromXml_F001_MontoIgvDocumentoCapturadoDesdeCabecera()
    {
        var result = ComprobanteExtractor.ExtractFromXml(LoadXml("10473551794-F001-811.xml"));

        // BUG: el path schemeId=1000 / Percent=NULL / ExCode=NULL no captura MontoIgvDocumento
        // → igvDocumento=0 → igvFinal=0 → comprobante.Igv=0 (en vez de 19.07)
        result.MontoIgvDocumento.Should().Be("19.07",
            because: "el TaxAmount=19.07 está en el TaxSubtotal de cabecera aunque no tenga Percent ni TaxExemptionReasonCode");
    }

    // ──────────────────────────────────────────────
    // FEN0-27409 — RUC 20454750773
    // IGV 18%, 1 línea, ExCode=10 solo en InvoiceLine (no en cabecera)
    // Header TaxSubtotal: schemeId=1000, Percent=NULL, ExCode=NULL   ← mismo bug que F001
    // InvoiceLine 1: TaxableAmt=25.42, TaxAmt=4.58, Code=10, Pct=18
    // Header TaxAmount = 4.58
    // ──────────────────────────────────────────────

    [Fact(DisplayName = "FEN0 - IGV 18% ExCode en línea - detecta como gravado")]
    public void ExtractFromXml_FEN0_DetectaGravado()
    {
        var result = ComprobanteExtractor.ExtractFromXml(LoadXml("20454750773-FEN0-27409.xml"));

        result.AfectacionIgvDetectada.Should().BeTrue();
        result.MontosGravados.Should().Contain("25.42");
        result.MontosIgvEspecial.Should().BeEmpty();
    }

    [Fact(DisplayName = "FEN0 - IGV 18% ExCode en línea - MontoIgvDocumento es 4.58 desde cabecera")]
    public void ExtractFromXml_FEN0_MontoIgvDocumentoEsDeCabecera()
    {
        var result = ComprobanteExtractor.ExtractFromXml(LoadXml("20454750773-FEN0-27409.xml"));

        result.MontoIgvDocumento.Should().Be("4.58");
    }

    // ──────────────────────────────────────────────
    // F64X-00004634 — RUC 20127765279
    // IGV 18%, 6 líneas — 2 de ellas tienen TaxableAmt=6.10 (DUPLICADO)
    // Header TaxSubtotal: schemeId=1000, Percent=NULL, ExCode=NULL
    // Líneas: 3.56, 6.10, 1.86, 4.15, 7.54, 6.10 (suma=29.31; header TaxableAmt=29.32)
    // Header TaxAmount = 5.28   (suma líneas = 0.64+1.10+0.34+0.75+1.36+1.10 = 5.29 por redondeo)
    // ──────────────────────────────────────────────

    [Fact(DisplayName = "F64X - 6 líneas IGV 18% - detecta las 6 líneas (incluyendo el importe duplicado 6.10)")]
    public void ExtractFromXml_F64X_Detecta6Lineas_IncluyendoDuplicado()
    {
        var result = ComprobanteExtractor.ExtractFromXml(LoadXml("20127765279-F64X-00004634.xml"));

        result.AfectacionIgvDetectada.Should().BeTrue();
        // BUG: el Contains check en InvoiceLine descarta la segunda línea con TaxableAmt=6.10
        // → MontosGravados.Count=5 en vez de 6; suma=23.21 en vez de 29.31
        result.MontosGravados.Should().HaveCount(6,
            because: "hay 6 líneas válidas; aunque dos tienen el mismo importe (6.10), ambas son líneas distintas");
        result.MontosIgvEspecial.Should().BeEmpty();
    }

    [Fact(DisplayName = "F64X - 6 líneas IGV 18% - MontoIgvDocumento es 5.28 (cabecera), NO 5.29 por suma de líneas")]
    public void ExtractFromXml_F64X_MontoIgvDocumentoEsCabecera_No5_29()
    {
        var result = ComprobanteExtractor.ExtractFromXml(LoadXml("20127765279-F64X-00004634.xml"));

        result.MontoIgvDocumento.Should().Be("5.28",
            because: "SUNAT declara IGV=5.28 en la cabecera; la suma de TaxAmount por línea da 5.29");
    }

    // ──────────────────────────────────────────────
    // Casos límite
    // ──────────────────────────────────────────────

    [Fact(DisplayName = "XML vacío - AfectacionIgvDetectada=false, sin montos")]
    public void ExtractFromXml_XmlVacio_NoDetectaAfectacion()
    {
        var result = ComprobanteExtractor.ExtractFromXml("");

        result.AfectacionIgvDetectada.Should().BeFalse();
        result.MontosGravados.Should().BeEmpty();
        result.MontosIgvEspecial.Should().BeEmpty();
        result.MontoIgvDocumento.Should().BeEmpty();
    }

    [Fact(DisplayName = "XML inválido - no lanza excepción")]
    public void ExtractFromXml_XmlInvalido_NoLanzaExcepcion()
    {
        var act = () => ComprobanteExtractor.ExtractFromXml("ESTO_NO_ES_XML<<<");
        act.Should().NotThrow();
    }
}
