using CapaDatos.ContabilidadAPI;
using CapaDatos.ContabilidadAPI.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio.ContabilidadAPI.Tests.Services;

/// <summary>
/// Tests de integración: comparan los flags/categorías derivados del XML de SUNAT
/// contra los valores almacenados en la BD por el servicio automático de desglose.
/// Estructura: XML (verdad oficial SUNAT) = expected; BD (procesado por el servicio) = actual.
/// </summary>
[Trait("Category", "Integration")]
public class ComprobanteDesglosadoFlagsTests
{
    private const string ConnStr =
        "Data Source=192.168.200.31;Initial Catalog=SVRENDICIONES;" +
        "Persist Security Info=True;User ID=sa;Password=B1Admin;" +
        "Encrypt=True;TrustServerCertificate=True";

    private static SvrendicionesContext CrearContexto()
    {
        var opts = new DbContextOptionsBuilder<SvrendicionesContext>()
            .UseSqlServer(ConnStr)
            .Options;
        return new SvrendicionesContext(opts);
    }

    private static async Task<ComprobantePago> ObtenerComprobante(
        SvrendicionesContext db, long ruc, string serie, string correlativo)
    {
        var cp = await db.ComprobantesPago.FirstOrDefaultAsync(c =>
            c.Ruc == ruc && c.Serie == serie && c.Correlativo == correlativo);

        cp.Should().NotBeNull(
            $"El comprobante RUC {ruc} {serie}-{correlativo} debe existir en la BD");

        return cp!;
    }

    // ════════════════════════════════════════════════════════════════════════════════════════
    // TEST 1 ─ Desglosado=true para todos: el servicio debió marcarlos al procesar
    // ════════════════════════════════════════════════════════════════════════════════════════

    public static IEnumerable<object[]> TodosLosFixtures =>
    [
        [10248876897L, "F001", "00020189" ],
        [10248876897L, "F001", "00020218" ],
        [10473551794L, "F001", "811"      ],
        [10760160933L, "FA01", "331"      ],
        [15513033181L, "FJ02", "00000030" ],
        [15610074712L, "F002", "00001446" ],
        [20119407738L, "F423", "00033565" ],
        [20127765279L, "F08R", "00059925" ],
        [20127765279L, "F64X", "00004634" ],
        [20381235051L, "F042", "00007646" ],
        [20454750773L, "FEN0", "27409"    ],
        [20455494614L, "F002", "00005346" ],
        [20511465061L, "F104", "01838739" ],
        [20511465061L, "F204", "03172506" ],
        [20517252558L, "F311", "02072371" ],
        [20538225763L, "FT03", "47244"    ],
        [20559331547L, "FE06", "00001494" ],
        [20601892627L, "FEN0", "67642"    ],
        [20602302793L, "E001", "2446"     ],
        [20602969780L, "F010", "00041909" ],
        [20605365346L, "F001", "17471"    ],
        [20608280333L, "FE19", "502338"   ],
        [20608280333L, "FG62", "503931"   ],
        [20609866749L, "F005", "00000885" ],
        [20610527605L, "F020", "4463"     ],
        [20612956104L, "E001", "825"      ],
        [20612956104L, "E001", "826"      ],
        [20613881621L, "F001", "4594"     ],
    ];

    [Theory(DisplayName = "Comprobante debe estar marcado como Desglosado=true en la BD")]
    [MemberData(nameof(TodosLosFixtures))]
    public async Task Comprobante_EnBd_EstaDesglosado(long ruc, string serie, string correlativo)
    {
        await using var db = CrearContexto();
        var cp = await ObtenerComprobante(db, ruc, serie, correlativo);

        cp.Desglosado.Should().BeTrue(
            $"El comprobante {serie}-{correlativo} debe tener Desglosado=true " +
            $"porque el servicio ya procesó su XML");
    }

    // ════════════════════════════════════════════════════════════════════════════════════════
    // TEST 2 ─ FJ02: IGV especial 10.5% → IgvEspecial=true, Gravado=false
    // XML: InvoiceLine ExCode=10, Percent=10.5
    // ════════════════════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "FJ02 (IGV Especial 10.5%) — BD debe tener IgvEspecial=true y Gravado=false")]
    public async Task FJ02_IgvEspecial_True_Gravado_False()
    {
        await using var db = CrearContexto();
        var cp = await ObtenerComprobante(db, 15513033181L, "FJ02", "00000030");

        using var scope = new FluentAssertions.Execution.AssertionScope();
        cp.IgvEspecial.Should().BeTrue(
            "el XML declara InvoiceLine con Percent=10.5 (tasa reducida < 18%)");
        cp.Gravado.Should().BeFalse(
            "cuando hay IGV especial, Gravado debe ser false");
    }

    // ════════════════════════════════════════════════════════════════════════════════════════
    // TEST 3 ─ F001-811: IGV normal 18% → Gravado=true, IgvEspecial=false
    // XML: InvoiceLine ExCode=10, Percent=18
    // ════════════════════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "F001-811 (IGV 18%) — BD debe tener Gravado=true e IgvEspecial=false")]
    public async Task F001_Gravado_True_IgvEspecial_False()
    {
        await using var db = CrearContexto();
        var cp = await ObtenerComprobante(db, 10473551794L, "F001", "811");

        using var scope = new FluentAssertions.Execution.AssertionScope();
        cp.Gravado.Should().BeTrue(
            "el XML declara InvoiceLine con Percent=18 (IGV estándar)");
        cp.IgvEspecial.Should().BeFalse(
            "la tasa es 18%, no reducida");
    }

    // ════════════════════════════════════════════════════════════════════════════════════════
    // TEST 4 ─ FEN0-27409: IGV normal 18% → Gravado=true, IgvEspecial=false
    // ════════════════════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "FEN0-27409 (IGV 18%, ExCode en línea) — BD debe tener Gravado=true e IgvEspecial=false")]
    public async Task FEN0_Gravado_True_IgvEspecial_False()
    {
        await using var db = CrearContexto();
        var cp = await ObtenerComprobante(db, 20454750773L, "FEN0", "27409");

        using var scope = new FluentAssertions.Execution.AssertionScope();
        cp.Gravado.Should().BeTrue(
            "el XML declara InvoiceLine con Percent=18");
        cp.IgvEspecial.Should().BeFalse();
    }

    // ════════════════════════════════════════════════════════════════════════════════════════
    // TEST 5 ─ F64X-00004634: 6 líneas IGV 18% → Gravado=true, MontoGravado > 0
    // Bug original: el Contains check eliminaba la 2da línea con TaxableAmt=6.10
    // → MontoGravado = 23.21 (5 líneas) en vez de 29.31/29.32 (6 líneas)
    // ════════════════════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "F64X (6 líneas IGV 18%, hay importes duplicados) — BD debe tener Gravado=true y MontoGravado > 0")]
    public async Task F64X_Gravado_True_MontoGravadoPositivo()
    {
        await using var db = CrearContexto();
        var cp = await ObtenerComprobante(db, 20127765279L, "F64X", "00004634");

        using var scope = new FluentAssertions.Execution.AssertionScope();
        cp.Gravado.Should().BeTrue(
            "el XML declara 6 líneas con Percent=18");
        cp.IgvEspecial.Should().BeFalse();
        cp.MontoGravado.Should().BeGreaterThan(0m,
            "debe haber capturado las 6 líneas gravadas");
        // El TaxableAmount oficial de cabecera es 29.32
        cp.MontoGravado.Should().BeGreaterThanOrEqualTo(29.00m,
            "suma de 6 líneas debe superar 29 (no 23.21 que sería el bug del Contains)");
    }
}
