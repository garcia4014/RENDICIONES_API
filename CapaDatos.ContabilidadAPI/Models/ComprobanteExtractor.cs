using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CapaDatos.ContabilidadAPI.Models
{
    public class ComprobanteExtractorResult
    {
        public List<string> Rucs { get; set; } = new();
        public List<string> RazonesSociales { get; set; } = new();
        public List<string> FechasEmision { get; set; } = new();
        public List<string> MontosTotales { get; set; } = new();
        public List<string> Series { get; set; } = new();
        public List<string> Correlativos { get; set; } = new();
    }

    public static class ComprobanteExtractor
    {
        private static readonly Regex RucRegex =
            new Regex(@"\b(10|15|16|17|18|20)\d{9}", RegexOptions.Compiled);

        private static readonly Regex Serie =
            new Regex(@"([BF][A-Z0-9]{0,3})[^\dA-Z]{0,4}?(\d{3,10})", RegexOptions.Compiled);

        private static readonly Regex Correlativo =
            new Regex(@"(?<!\d)(?:B|F)[A-Z0-9]{0,3}\s*(?:[-:º°]*\s*)?(\d{1,8})(?!\d)", RegexOptions.Compiled);


        private static readonly Regex[] RazonSocialRegexes =
        {
            new Regex(@"([A-ZÁÉÍÓÚÑ0-9 \.\-]+?(?:S\.A\.C|S\.A|S\.R\.L|E\.I\.R\.L|S\.A\.A))",
                RegexOptions.Compiled),

            new Regex(@"\b([A-ZÁÉÍÓÚÑ][A-ZÁÉÍÓÚÑ0-9 \-\.\&]{10,})\b",
                RegexOptions.Compiled),

            new Regex(@"([A-ZÁÉÍÓÚÑ][A-ZÁÉÍÓÚÑ0-9 \-\.&\/]{10,200})",
                RegexOptions.Compiled)
        };

        private static readonly Regex FechaEmisionRegex =
            new Regex(@"(?<!\d)(\d{2}[\/\-]\d{2}[\/\-]\d{4})(?!\d)", RegexOptions.Compiled);

        private static readonly Regex MontoTotalRegex =
            new Regex(@"(?<![0-9])(\d+\.\d{1,2})(?![0-9])", RegexOptions.Compiled);

        public static ComprobanteExtractorResult Extract(string ocrText)
        {
            var result = new ComprobanteExtractorResult();

            if (string.IsNullOrWhiteSpace(ocrText))
                return result;

            // =======================
            // 1. RUC (TODOS)
            // =======================
            foreach (Match m in RucRegex.Matches(ocrText))
            {
                result.Rucs.Add(m.Value);
            }

            // =======================
            // 2. Razón Social (TODAS)
            // =======================
            foreach (var regex in RazonSocialRegexes)
            {
                foreach (Match m in regex.Matches(ocrText))
                {
                    var cleaned = CleanRazonSocial(m.Value);
                    if (!string.IsNullOrWhiteSpace(cleaned))
                        result.RazonesSociales.Add(cleaned);
                }
            }

            result.RazonesSociales = result.RazonesSociales.Distinct().ToList();

            // =======================
            // 3. Fecha de Emisión
            // =======================
            foreach (Match m in FechaEmisionRegex.Matches(ocrText))
            {
                result.FechasEmision.Add(m.Groups[1].Value);
            }

            // =======================
            // 4. Monto Total
            // =======================
            foreach (Match m in MontoTotalRegex.Matches(ocrText))
            {
                result.MontosTotales.Add(m.Groups[1].Value);
            }

            // =======================
            // 5. Serie
            // =======================
            foreach (Match m in Serie.Matches(ocrText))
            {
                result.Series.Add(m.Groups[1].Value);
            }

            // =======================
            // 6. Correlativo 
            // =======================
            foreach (Match m in Correlativo.Matches(ocrText))
            {
                result.Correlativos.Add(m.Groups[1].Value);
            }

            return result;
        }

        /// <summary>
        /// Extrae información de un archivo XML de comprobante electrónico SUNAT
        /// </summary>
        /// <param name="xmlContent">Contenido del XML como string</param>
        /// <returns>Datos extraídos del comprobante</returns>
        public static ComprobanteExtractorResult ExtractFromXml(string xmlContent)
        {
            var result = new ComprobanteExtractorResult();

            if (string.IsNullOrWhiteSpace(xmlContent))
                return result;

            try
            {
                XDocument doc = XDocument.Parse(xmlContent);
                XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
                XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";

                // Intentar obtener el namespace raíz (puede ser Invoice, CreditNote, DebitNote, etc.)
                var root = doc.Root;
                if (root == null)
                    return result;

                var rootNamespace = root.GetDefaultNamespace();

                // 1. SERIE Y CORRELATIVO (ID del documento)
                var documentId = root.Element(rootNamespace + "ID")?.Value ?? 
                                root.Element(cbc + "ID")?.Value;
                if (!string.IsNullOrEmpty(documentId))
                {
                    var parts = documentId.Split('-');
                    if (parts.Length == 2)
                    {
                        result.Series.Add(parts[0].Trim());
                        result.Correlativos.Add(parts[1].Trim());
                    }
                }

                // 2. FECHA DE EMISIÓN
                var issueDate = root.Element(rootNamespace + "IssueDate")?.Value ??
                               root.Element(cbc + "IssueDate")?.Value;
                if (!string.IsNullOrEmpty(issueDate))
                {
                    result.FechasEmision.Add(issueDate);
                }

                // 3. RUC Y RAZÓN SOCIAL DEL EMISOR
                var accountingSupplierParty = root.Element(rootNamespace + "AccountingSupplierParty") ??
                                             root.Element(cac + "AccountingSupplierParty");
                
                if (accountingSupplierParty != null)
                {
                    var party = accountingSupplierParty.Element(cac + "Party");
                    if (party != null)
                    {
                        // RUC
                        var partyIdentification = party.Element(cac + "PartyIdentification");
                        var ruc = partyIdentification?.Element(cbc + "ID")?.Value;
                        if (!string.IsNullOrEmpty(ruc))
                        {
                            result.Rucs.Add(ruc);
                        }

                        // Razón Social
                        var partyLegalEntity = party.Element(cac + "PartyLegalEntity");
                        var razonSocial = partyLegalEntity?.Element(cbc + "RegistrationName")?.Value;
                        if (!string.IsNullOrEmpty(razonSocial))
                        {
                            result.RazonesSociales.Add(razonSocial.Trim());
                        }
                    }
                }

                // 4. MONTO TOTAL (varios intentos)
                // Intentar LegalMonetaryTotal/PayableAmount
                var legalMonetaryTotal = root.Element(rootNamespace + "LegalMonetaryTotal") ??
                                        root.Element(cac + "LegalMonetaryTotal");
                
                var montoTotal = legalMonetaryTotal?.Element(cbc + "PayableAmount")?.Value;
                
                // Si no existe, intentar TaxTotal/TaxAmount + otros
                if (string.IsNullOrEmpty(montoTotal))
                {
                    var taxInclusiveAmount = legalMonetaryTotal?.Element(cbc + "TaxInclusiveAmount")?.Value;
                    if (!string.IsNullOrEmpty(taxInclusiveAmount))
                    {
                        montoTotal = taxInclusiveAmount;
                    }
                }

                if (!string.IsNullOrEmpty(montoTotal))
                {
                    result.MontosTotales.Add(montoTotal);
                }

                return result;
            }
            catch (Exception)
            {
                // Si falla el parsing XML, intentar extraer con regex del texto plano
                return Extract(xmlContent);
            }
        }

        private static string CleanRazonSocial(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Trim();

            if (text.Contains("===")) return "";
            if (text.Contains("PÁGINA")) return "";

            return text;
        }
    }
}
