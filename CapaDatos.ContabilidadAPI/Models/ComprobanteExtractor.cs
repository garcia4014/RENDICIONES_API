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
        
        // Montos específicos por tipo de afectación
        public List<string> MontosGravados { get; set; } = new();
        public List<string> MontosInafectos { get; set; } = new();
        public List<string> MontosExonerados { get; set; } = new();
        public List<string> MontosIgvEspecial { get; set; } = new();
        public List<string> MontosImpuestoConsumo { get; set; } = new();
        
        // Indicador de si se pudo extraer la afectación del IGV
        public bool AfectacionIgvDetectada { get; set; } = false;
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

        // Regex para detectar montos de afectación del IGV
        private static readonly Regex MontoGravadoRegex =
            new Regex(@"(?:grava[dt]|base\s*imponible|op\.\s*grava[dt]|operaci[oó]n\s*grava[dt])[\s:\-]*(\d+\.\d{1,2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex MontoInafectoRegex =
            new Regex(@"(?:inafect|no\s*afect|op\.\s*inafect)[\s:\-]*(\d+\.\d{1,2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex MontoExoneradoRegex =
            new Regex(@"(?:exonera[dt]|op\.\s*exonera[dt])[\s:\-]*(\d+\.\d{1,2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex MontoIgvEspecialRegex =
            new Regex(@"(?:igv\s*especial|igv\s*10%|10%\s*igv)[\s:\-]*(\d+\.\d{1,2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex MontoImpuestoConsumoRegex =
            new Regex(@"(?:i\.?s\.?c\.?|impuesto\s*selectivo|impuesto\s*consumo|impuesto\s*al\s*consumo)[\s:\-]*(\d+\.\d{1,2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

            // =======================
            // 7. Montos de Afectación del IGV
            // =======================
            bool afectacionDetectada = false;

            // Monto Gravado
            foreach (Match m in MontoGravadoRegex.Matches(ocrText))
            {
                if (m.Groups.Count > 1)
                {
                    result.MontosGravados.Add(m.Groups[1].Value);
                    afectacionDetectada = true;
                }
            }

            // Monto Inafecto
            foreach (Match m in MontoInafectoRegex.Matches(ocrText))
            {
                if (m.Groups.Count > 1)
                {
                    result.MontosInafectos.Add(m.Groups[1].Value);
                    afectacionDetectada = true;
                }
            }

            // Monto Exonerado
            foreach (Match m in MontoExoneradoRegex.Matches(ocrText))
            {
                if (m.Groups.Count > 1)
                {
                    result.MontosExonerados.Add(m.Groups[1].Value);
                    afectacionDetectada = true;
                }
            }

            // Monto IGV Especial
            foreach (Match m in MontoIgvEspecialRegex.Matches(ocrText))
            {
                if (m.Groups.Count > 1)
                {
                    result.MontosIgvEspecial.Add(m.Groups[1].Value);
                    afectacionDetectada = true;
                }
            }

            // Monto Impuesto al Consumo (ISC)
            foreach (Match m in MontoImpuestoConsumoRegex.Matches(ocrText))
            {
                if (m.Groups.Count > 1)
                {
                    result.MontosImpuestoConsumo.Add(m.Groups[1].Value);
                    afectacionDetectada = true;
                }
            }

            result.AfectacionIgvDetectada = afectacionDetectada;

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
                Console.WriteLine("[XML] Iniciando parseo de XML...");
                XDocument doc = XDocument.Parse(xmlContent);
                XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
                XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";

                // Intentar obtener el namespace raíz (puede ser Invoice, CreditNote, DebitNote, etc.)
                var root = doc.Root;
                if (root == null)
                {
                    Console.WriteLine("[XML] Error: No se encontró nodo raíz");
                    return result;
                }

                var rootNamespace = root.GetDefaultNamespace();
                Console.WriteLine($"[XML] Namespace raíz: {rootNamespace}");
                Console.WriteLine($"[XML] Nombre del elemento raíz: {root.Name.LocalName}");

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
                
                Console.WriteLine($"[XML] Fecha de emisión encontrada: '{issueDate ?? "NULL"}'");
                
                if (!string.IsNullOrEmpty(issueDate))
                {
                    result.FechasEmision.Add(issueDate);
                    Console.WriteLine($"[XML] Fecha agregada a FechasEmision: {issueDate}");
                }
                else
                {
                    Console.WriteLine("[XML] No se encontró fecha de emisión en el XML");
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

                // 5. CARGOS ADICIONALES (AllowanceCharge)
                bool afectacionDetectada = false;
                
                Console.WriteLine("[XML] Buscando AllowanceCharge...");
                var allowanceCharges = root.Elements(rootNamespace + "AllowanceCharge").Concat(root.Elements(cac + "AllowanceCharge"));
                var allowanceChargesList = allowanceCharges.ToList();
                Console.WriteLine($"[XML] AllowanceCharge encontrados: {allowanceChargesList.Count}");
                
                foreach (var allowanceCharge in allowanceChargesList)
                {
                    var chargeIndicator = allowanceCharge.Element(cbc + "ChargeIndicator")?.Value;
                    var reasonCode = allowanceCharge.Element(cbc + "AllowanceChargeReasonCode")?.Value;
                    var amount = allowanceCharge.Element(cbc + "Amount")?.Value;
                    
                    Console.WriteLine($"[XML] AllowanceCharge - ChargeIndicator: {chargeIndicator}, ReasonCode: {reasonCode}, Amount: {amount}");
                    
                    // Si es un cargo (true) y tiene monto
                    if (chargeIndicator == "true" && !string.IsNullOrEmpty(amount))
                    {
                        // Código 50 = Cargos adicionales (puede ser ISC, servicios, etc)
                        if (reasonCode == "50")
                        {
                            Console.WriteLine($"[XML] Cargo adicional detectado (código 50): {amount}");
                            result.MontosImpuestoConsumo.Add(amount);
                            afectacionDetectada = true;
                        }
                    }
                }

                // 6. MONTOS DE AFECTACIÓN DEL IGV (desde TaxSubtotal)
                Console.WriteLine("[XML] Buscando nodos TaxTotal...");
                var taxTotals = root.Elements(rootNamespace + "TaxTotal").Concat(root.Elements(cac + "TaxTotal"));
                var taxTotalsList = taxTotals.ToList();
                Console.WriteLine($"[XML] TaxTotals encontrados: {taxTotalsList.Count}");
                
                foreach (var taxTotal in taxTotalsList)
                {
                    Console.WriteLine($"[XML] Procesando TaxTotal: {taxTotal.Name}");
                    var taxSubtotals = taxTotal.Elements(cac + "TaxSubtotal");
                    var taxSubtotalsList = taxSubtotals.ToList();
                    Console.WriteLine($"[XML] TaxSubtotals encontrados: {taxSubtotalsList.Count}");
                    
                    foreach (var taxSubtotal in taxSubtotalsList)
                    {
                        Console.WriteLine($"[XML] Procesando TaxSubtotal...");
                        var taxCategory = taxSubtotal.Element(cac + "TaxCategory");
                        Console.WriteLine($"[XML] TaxCategory encontrado: {taxCategory != null}");
                        
                        var taxExemptionReasonCode = taxCategory?.Element(cbc + "TaxExemptionReasonCode")?.Value;
                        var taxableAmount = taxSubtotal.Element(cbc + "TaxableAmount")?.Value;
                        var taxPercent = taxCategory?.Element(cbc + "Percent")?.Value;
                        
                        Console.WriteLine($"[XML] TaxExemptionReasonCode: {taxExemptionReasonCode ?? "NULL"}");
                        Console.WriteLine($"[XML] TaxableAmount: {taxableAmount ?? "NULL"}");
                        Console.WriteLine($"[XML] TaxPercent: {taxPercent ?? "NULL"}");
                        
                        // Verificar si es ISC por TaxScheme
                        var taxScheme = taxCategory?.Element(cac + "TaxScheme");
                        var taxSchemeId = taxScheme?.Element(cbc + "ID")?.Value;
                        Console.WriteLine($"[XML] TaxSchemeId: {taxSchemeId ?? "NULL"}");
                        
                        if (!string.IsNullOrEmpty(taxableAmount))
                        {
                            Console.WriteLine($"[XML] Evaluando taxableAmount: {taxableAmount}");
                            
                            // Si es ISC (código 2000)
                            if (taxSchemeId == "2000")
                            {
                                Console.WriteLine($"[XML] ISC detectado: {taxableAmount}");
                                result.MontosImpuestoConsumo.Add(taxableAmount);
                                afectacionDetectada = true;
                                continue;
                            }
                            
                            // Códigos SUNAT para IGV:
                            // 10 = Gravado - Operación Onerosa (puede ser 18% o 10%)
                            // 20 = Exonerado - Operación Onerosa
                            // 30 = Inafecto - Operación Onerosa
                            // 17 = Gravado - IVAP (IGV Especial 10%)
                            // 50 = ISC (Impuesto Selectivo al Consumo)
                            
                            switch (taxExemptionReasonCode)
                            {
                                case "10": // Gravado - VALIDAR PORCENTAJE
                                    // Verificar el porcentaje para clasificar correctamente
                                    if (!string.IsNullOrEmpty(taxPercent))
                                    {
                                        if (decimal.TryParse(taxPercent, System.Globalization.NumberStyles.Any, 
                                            System.Globalization.CultureInfo.InvariantCulture, out decimal percent))
                                        {
                                            if (percent == 10 || percent == 10.0m)
                                            {
                                                Console.WriteLine($"[XML] IGV Especial (10%) detectado: {taxableAmount}");
                                                result.MontosIgvEspecial.Add(taxableAmount);
                                            }
                                            else // 18% u otro porcentaje se considera gravado normal
                                            {
                                                Console.WriteLine($"[XML] Gravado ({percent}%) detectado: {taxableAmount}");
                                                result.MontosGravados.Add(taxableAmount);
                                            }
                                        }
                                        else
                                        {
                                            // Si no se puede parsear, asumir gravado normal
                                            Console.WriteLine($"[XML] Gravado (% no parseable) detectado: {taxableAmount}");
                                            result.MontosGravados.Add(taxableAmount);
                                        }
                                    }
                                    else
                                    {
                                        // Si no hay porcentaje, asumir gravado normal (18%)
                                        Console.WriteLine($"[XML] Gravado (sin % especificado, asumiendo 18%) detectado: {taxableAmount}");
                                        result.MontosGravados.Add(taxableAmount);
                                    }
                                    afectacionDetectada = true;
                                    break;
                                case "20": // Exonerado
                                    Console.WriteLine($"[XML] Exonerado detectado: {taxableAmount}");
                                    result.MontosExonerados.Add(taxableAmount);
                                    afectacionDetectada = true;
                                    break;
                                case "30": // Inafecto
                                    Console.WriteLine($"[XML] Inafecto detectado: {taxableAmount}");
                                    result.MontosInafectos.Add(taxableAmount);
                                    afectacionDetectada = true;
                                    break;
                                case "17": // IGV Especial (IVAP) - siempre 10%
                                    Console.WriteLine($"[XML] IGV Especial (código 17) detectado: {taxableAmount}");
                                    result.MontosIgvEspecial.Add(taxableAmount);
                                    afectacionDetectada = true;
                                    break;
                                case "50": // ISC
                                    Console.WriteLine($"[XML] ISC (código 50) detectado: {taxableAmount}");
                                    result.MontosImpuestoConsumo.Add(taxableAmount);
                                    afectacionDetectada = true;
                                    break;
                                default:
                                    Console.WriteLine($"[XML] Código desconocido: {taxExemptionReasonCode}");
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("[XML] TaxableAmount es NULL o vacío");
                        }
                    }
                }
                
                result.AfectacionIgvDetectada = afectacionDetectada;
                
                // Siempre buscar en InvoiceLine para capturar montos detallados por línea
                Console.WriteLine("[XML] Buscando en InvoiceLines...");
                var invoiceLines = root.Elements(rootNamespace + "InvoiceLine").Concat(root.Elements(cac + "InvoiceLine"));
                var invoiceLinesList = invoiceLines.ToList();
                Console.WriteLine($"[XML] InvoiceLines encontrados: {invoiceLinesList.Count}");
                
                foreach (var invoiceLine in invoiceLinesList)
                {
                    var lineTaxTotals = invoiceLine.Elements(cac + "TaxTotal");
                    foreach (var lineTaxTotal in lineTaxTotals)
                    {
                        var lineTaxSubtotals = lineTaxTotal.Elements(cac + "TaxSubtotal");
                        foreach (var lineTaxSubtotal in lineTaxSubtotals)
                        {
                            var lineTaxCategory = lineTaxSubtotal.Element(cac + "TaxCategory");
                            var lineTaxExemptionReasonCode = lineTaxCategory?.Element(cbc + "TaxExemptionReasonCode")?.Value;
                            var lineTaxableAmount = lineTaxSubtotal.Element(cbc + "TaxableAmount")?.Value;
                            var lineTaxPercent = lineTaxCategory?.Element(cbc + "Percent")?.Value;
                            
                            Console.WriteLine($"[XML-LINE] TaxExemptionReasonCode: {lineTaxExemptionReasonCode ?? "NULL"}");
                            Console.WriteLine($"[XML-LINE] TaxableAmount: {lineTaxableAmount ?? "NULL"}");
                            Console.WriteLine($"[XML-LINE] TaxPercent: {lineTaxPercent ?? "NULL"}");
                            
                            if (!string.IsNullOrEmpty(lineTaxableAmount) && !string.IsNullOrEmpty(lineTaxExemptionReasonCode))
                            {
                                // Sumar el monto al tipo correspondiente (evitar duplicados)
                                switch (lineTaxExemptionReasonCode)
                                {
                                    case "10": // Gravado - VALIDAR PORCENTAJE
                                        if (!result.MontosGravados.Contains(lineTaxableAmount) && !result.MontosIgvEspecial.Contains(lineTaxableAmount))
                                        {
                                            // Verificar el porcentaje para clasificar correctamente
                                            if (!string.IsNullOrEmpty(lineTaxPercent))
                                            {
                                                if (decimal.TryParse(lineTaxPercent, System.Globalization.NumberStyles.Any, 
                                                    System.Globalization.CultureInfo.InvariantCulture, out decimal linePercent))
                                                {
                                                    if (linePercent == 10 || linePercent == 10.0m)
                                                    {
                                                        Console.WriteLine($"[XML-LINE] IGV Especial (10%) detectado: {lineTaxableAmount}");
                                                        result.MontosIgvEspecial.Add(lineTaxableAmount);
                                                    }
                                                    else // 18% u otro porcentaje
                                                    {
                                                        Console.WriteLine($"[XML-LINE] Gravado ({linePercent}%) detectado: {lineTaxableAmount}");
                                                        result.MontosGravados.Add(lineTaxableAmount);
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine($"[XML-LINE] Gravado (% no parseable) detectado: {lineTaxableAmount}");
                                                    result.MontosGravados.Add(lineTaxableAmount);
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"[XML-LINE] Gravado (sin %, asumiendo 18%) detectado: {lineTaxableAmount}");
                                                result.MontosGravados.Add(lineTaxableAmount);
                                            }
                                            afectacionDetectada = true;
                                        }
                                        break;
                                    case "20": // Exonerado
                                        if (!result.MontosExonerados.Contains(lineTaxableAmount))
                                        {
                                            Console.WriteLine($"[XML-LINE] Exonerado detectado: {lineTaxableAmount}");
                                            result.MontosExonerados.Add(lineTaxableAmount);
                                            afectacionDetectada = true;
                                        }
                                        break;
                                    case "30": // Inafecto
                                        if (!result.MontosInafectos.Contains(lineTaxableAmount))
                                        {
                                            Console.WriteLine($"[XML-LINE] Inafecto detectado: {lineTaxableAmount}");
                                            result.MontosInafectos.Add(lineTaxableAmount);
                                            afectacionDetectada = true;
                                        }
                                        break;
                                    case "17": // IGV Especial (IVAP) - siempre 10%
                                        if (!result.MontosIgvEspecial.Contains(lineTaxableAmount))
                                        {
                                            Console.WriteLine($"[XML-LINE] IGV Especial (código 17) detectado: {lineTaxableAmount}");
                                            result.MontosIgvEspecial.Add(lineTaxableAmount);
                                            afectacionDetectada = true;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                
                result.AfectacionIgvDetectada = afectacionDetectada;
                
                Console.WriteLine($"[XML] Resultado final - Gravados: {result.MontosGravados.Count}, Inafectos: {result.MontosInafectos.Count}, Exonerados: {result.MontosExonerados.Count}, IgvEspecial: {result.MontosIgvEspecial.Count}, ImpuestoConsumo: {result.MontosImpuestoConsumo.Count}");
                Console.WriteLine($"[XML] AfectacionDetectada: {afectacionDetectada}");

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
