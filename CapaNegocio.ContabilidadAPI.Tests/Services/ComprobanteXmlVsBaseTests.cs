using System.Globalization;
using System.Xml.Linq;
using CapaDatos.ContabilidadAPI;
using CapaDatos.ContabilidadAPI.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio.ContabilidadAPI.Tests.Services;

/// <summary>
/// Tests de integración: comparan los valores monetarios declarados en el XML de SUNAT
/// contra los valores almacenados en la BD por el servicio automático de desglose.
/// Estructura: XML (verdad oficial SUNAT) = expected; BD (procesado por el servicio) = actual.
/// Si el servicio ejecutó correctamente todos los tests deben pasar al 100%.
/// </summary>
[Trait("Category", "Integration")]
public class ComprobanteXmlVsBaseTests
{
    // ── Cadena de conexión (igual que appsettings.Development.json) ──────────────────────────
    private const string ConnStr =
        "Data Source=192.168.200.31;Initial Catalog=SVRENDICIONES;" +
        "Persist Security Info=True;User ID=sa;Password=B1Admin;" +
        "Encrypt=True;TrustServerCertificate=True";

    private static readonly string XmlDir =
        Path.Combine(AppContext.BaseDirectory, "TestData");

    // ── Fixtures: (archivo, ruc, serie, correlativo) ─────────────────────────────────────────
    // El nombre del archivo es exactamente {ruc}-{serie}-{correlativo}.xml,
    // que coincide con lo que el servicio guardó en la BD.
    public static IEnumerable<object[]> Fixtures =>
    [
        ["10248876897-F001-00020189.xml",  10248876897L, "F001", "00020189" ],
        ["10248876897-F001-00020218.xml",  10248876897L, "F001", "00020218" ],
        ["10473551794-F001-811.xml",       10473551794L, "F001", "811"      ],
        ["10760160933-FA01-331.xml",       10760160933L, "FA01", "331"      ],
        ["15513033181-FJ02-00000030.xml",  15513033181L, "FJ02", "00000030" ],
        ["15610074712-F002-00001446.xml",  15610074712L, "F002", "00001446" ],
        ["20119407738-F423-00033565.xml",  20119407738L, "F423", "00033565" ],
        ["20127765279-F08R-00059925.xml",  20127765279L, "F08R", "00059925" ],
        ["20127765279-F64X-00004634.xml",  20127765279L, "F64X", "00004634" ],
        ["20381235051-F042-00007646.xml",  20381235051L, "F042", "00007646" ],
        ["20454750773-FEN0-27409.xml",     20454750773L, "FEN0", "27409"    ],
        ["20455494614-F002-00005346.xml",  20455494614L, "F002", "00005346" ],
        ["20511465061-F104-01838739.xml",  20511465061L, "F104", "01838739" ],
        ["20511465061-F204-03172506.xml",  20511465061L, "F204", "03172506" ],
        ["20517252558-F311-02072371.xml",  20517252558L, "F311", "02072371" ],
        ["20538225763-FT03-47244.xml",     20538225763L, "FT03", "47244"    ],
        ["20559331547-FE06-00001494.xml",  20559331547L, "FE06", "00001494" ],
        ["20601892627-FEN0-67642.xml",     20601892627L, "FEN0", "67642"    ],
        ["20602302793-E001-2446.xml",      20602302793L, "E001", "2446"     ],
        ["20602969780-F010-00041909.xml",  20602969780L, "F010", "00041909" ],
        ["20605365346-F001-17471.xml",     20605365346L, "F001", "17471"    ],
        ["20608280333-FE19-502338.xml",    20608280333L, "FE19", "502338"   ],
        ["20608280333-FG62-503931.xml",    20608280333L, "FG62", "503931"   ],
        ["20609866749-F005-00000885.xml",  20609866749L, "F005", "00000885" ],
        ["20610527605-F020-4463.xml",      20610527605L, "F020", "4463"     ],
        ["20612956104-E001-825.xml",       20612956104L, "E001", "825"      ],
        ["20612956104-E001-826.xml",       20612956104L, "E001", "826"      ],
        ["20613881621-F001-4594.xml",      20613881621L, "F001", "4594"     ],
    ];

    // ── helpers ────────────────────────────────────────────────────────────────────────────

    private static SvrendicionesContext CrearContexto()
    {
        var opts = new DbContextOptionsBuilder<SvrendicionesContext>()
            .UseSqlServer(ConnStr)
            .Options;
        return new SvrendicionesContext(opts);
    }

    /// <summary>
    /// Lee el TaxSubtotal de CABECERA del XML (hijo directo del Invoice/TaxTotal, no de InvoiceLine)
    /// y devuelve (TaxAmount=IGV oficial SUNAT, TaxableAmount=Base imponible oficial SUNAT).
    /// </summary>
    private static (decimal Igv, decimal Subtotal) ExtraerTaxHeader(string archivo)
    {
        var doc     = XDocument.Load(Path.Combine(XmlDir, archivo));
        var root    = doc.Root!;
        var rootNs  = root.GetDefaultNamespace();
        XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

        // TaxTotal de cabecera = hijo directo del elemento raíz (no descendiente de InvoiceLine)
        var taxTotal    = root.Element(rootNs + "TaxTotal") ?? root.Element(cac + "TaxTotal");
        var taxSubtotal = taxTotal!.Element(cac + "TaxSubtotal");

        var igv      = decimal.Parse(taxSubtotal!.Element(cbc + "TaxAmount")!.Value,     CultureInfo.InvariantCulture);
        var subtotal = decimal.Parse(taxSubtotal!.Element(cbc + "TaxableAmount")!.Value, CultureInfo.InvariantCulture);
        return (igv, subtotal);
    }

    private static decimal ExtraerPayable(string archivo)
    {
        var doc    = XDocument.Load(Path.Combine(XmlDir, archivo));
        var root   = doc.Root!;
        var rootNs = root.GetDefaultNamespace();
        XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

        var lmt = root.Element(rootNs + "LegalMonetaryTotal") ?? root.Element(cac + "LegalMonetaryTotal");
        return decimal.Parse(lmt!.Element(cbc + "PayableAmount")!.Value, CultureInfo.InvariantCulture);
    }

    // ════════════════════════════════════════════════════════════════════════════════════════
    // TEST 1 ─ IGV
    // El bug original: para FJ02 la BD guardaba 6.23 (suma de líneas) en lugar de 6.22 (cabecera).
    // Para F001, FEN0, F64X la BD guardaba 0 (no se capturaba MontoIgvDocumento).
    // ════════════════════════════════════════════════════════════════════════════════════════

    [Theory(DisplayName = "IGV en BD debe coincidir exactamente con TaxAmount de cabecera del XML (valor oficial SUNAT)")]
    [MemberData(nameof(Fixtures))]
    public async Task Igv_EnBd_DebeCoincidir_ConTaxAmountCabeceraXml(
        string archivo, long ruc, string serie, string correlativo)
    {
        var (expectedIgv, _) = ExtraerTaxHeader(archivo);

        await using var db = CrearContexto();
        var cp = await db.ComprobantesPago.FirstOrDefaultAsync(c =>
            c.Ruc == ruc && c.Serie == serie && c.Correlativo == correlativo);

        cp.Should().NotBeNull(
            $"El comprobante RUC {ruc} {serie}-{correlativo} debe existir en la BD");

        cp!.Igv.Should().Be(expectedIgv,
            $"IGV en BD ({cp.Igv}) debe coincidir con TaxAmount de cabecera del XML ({expectedIgv}) " +
            $"— archivo: {archivo}");
    }

    // ════════════════════════════════════════════════════════════════════════════════════════
    // TEST 2 ─ Subtotal (base imponible)
    // ════════════════════════════════════════════════════════════════════════════════════════

    [Theory(DisplayName = "Subtotal en BD debe coincidir exactamente con TaxableAmount de cabecera del XML (valor oficial SUNAT)")]
    [MemberData(nameof(Fixtures))]
    public async Task Subtotal_EnBd_DebeCoincidir_ConTaxableAmountCabeceraXml(
        string archivo, long ruc, string serie, string correlativo)
    {
        var (_, expectedSubtotal) = ExtraerTaxHeader(archivo);

        await using var db = CrearContexto();
        var cp = await db.ComprobantesPago.FirstOrDefaultAsync(c =>
            c.Ruc == ruc && c.Serie == serie && c.Correlativo == correlativo);

        cp.Should().NotBeNull(
            $"El comprobante RUC {ruc} {serie}-{correlativo} debe existir en la BD");

        cp!.Subtotal.Should().Be(expectedSubtotal,
            $"Subtotal en BD ({cp.Subtotal}) debe coincidir con TaxableAmount de cabecera del XML ({expectedSubtotal}) " +
            $"— archivo: {archivo}");
    }

    // ════════════════════════════════════════════════════════════════════════════════════════
    // TEST 3 ─ Monto total (importe a pagar)
    // ════════════════════════════════════════════════════════════════════════════════════════

    [Theory(DisplayName = "Monto total en BD debe coincidir con PayableAmount del XML (SUNAT)")]
    [MemberData(nameof(Fixtures))]
    public async Task MontoTotal_EnBd_DebeCoincidir_ConPayableAmountXml(
        string archivo, long ruc, string serie, string correlativo)
    {
        var expectedMonto = ExtraerPayable(archivo);

        await using var db = CrearContexto();
        var cp = await db.ComprobantesPago.FirstOrDefaultAsync(c =>
            c.Ruc == ruc && c.Serie == serie && c.Correlativo == correlativo);

        cp.Should().NotBeNull(
            $"El comprobante RUC {ruc} {serie}-{correlativo} debe existir en la BD");

        cp!.Monto.Should().Be(expectedMonto,
            $"Monto en BD ({cp.Monto}) debe coincidir con PayableAmount del XML ({expectedMonto}) " +
            $"— archivo: {archivo}");
    }
}
