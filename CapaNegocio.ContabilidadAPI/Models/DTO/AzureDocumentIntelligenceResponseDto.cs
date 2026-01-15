using System.Text.Json.Serialization;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// Respuesta del análisis de Azure Document Intelligence
    /// </summary>
    public class AzureDocumentIntelligenceResponseDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("createdDateTime")]
        public DateTime? CreatedDateTime { get; set; }

        [JsonPropertyName("lastUpdatedDateTime")]
        public DateTime? LastUpdatedDateTime { get; set; }

        [JsonPropertyName("analyzeResult")]
        public AzureAnalyzeResult? AnalyzeResult { get; set; }
    }

    public class AzureAnalyzeResult
    {
        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; } = string.Empty;

        [JsonPropertyName("modelId")]
        public string ModelId { get; set; } = string.Empty;

        [JsonPropertyName("stringIndexType")]
        public string StringIndexType { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("pages")]
        public List<AzurePage>? Pages { get; set; }

        [JsonPropertyName("documents")]
        public List<AzureDocument>? Documents { get; set; }
    }

    public class AzureDocument
    {
        [JsonPropertyName("docType")]
        public string DocType { get; set; } = string.Empty;

        [JsonPropertyName("boundingRegions")]
        public List<AzureBoundingRegion>? BoundingRegions { get; set; }

        [JsonPropertyName("fields")]
        public Dictionary<string, AzureField>? Fields { get; set; }

        [JsonPropertyName("confidence")]
        public float? Confidence { get; set; }

        [JsonPropertyName("spans")]
        public List<AzureSpan>? Spans { get; set; }
    }

    public class AzureField
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("valueString")]
        public string? ValueString { get; set; }

        [JsonPropertyName("valueDate")]
        public string? ValueDate { get; set; }

        [JsonPropertyName("valueNumber")]
        public double? ValueNumber { get; set; }

        [JsonPropertyName("valueCurrency")]
        public AzureCurrency? ValueCurrency { get; set; }

        [JsonPropertyName("valueAddress")]
        public AzureAddress? ValueAddress { get; set; }

        [JsonPropertyName("valueArray")]
        public List<AzureField>? ValueArray { get; set; }

        [JsonPropertyName("valueObject")]
        public Dictionary<string, AzureField>? ValueObject { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("boundingRegions")]
        public List<AzureBoundingRegion>? BoundingRegions { get; set; }

        [JsonPropertyName("confidence")]
        public float? Confidence { get; set; }

        [JsonPropertyName("spans")]
        public List<AzureSpan>? Spans { get; set; }
    }

    public class AzureCurrency
    {
        [JsonPropertyName("currencySymbol")]
        public string? CurrencySymbol { get; set; }

        [JsonPropertyName("amount")]
        public double? Amount { get; set; }

        [JsonPropertyName("currencyCode")]
        public string? CurrencyCode { get; set; }
    }

    public class AzureAddress
    {
        [JsonPropertyName("road")]
        public string? Road { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("streetAddress")]
        public string? StreetAddress { get; set; }

        [JsonPropertyName("stateDistrict")]
        public string? StateDistrict { get; set; }

        [JsonPropertyName("house")]
        public string? House { get; set; }
    }

    public class AzurePage
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("angle")]
        public float? Angle { get; set; }

        [JsonPropertyName("width")]
        public float? Width { get; set; }

        [JsonPropertyName("height")]
        public float? Height { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("words")]
        public List<AzureWord>? Words { get; set; }

        [JsonPropertyName("lines")]
        public List<AzureLine>? Lines { get; set; }

        [JsonPropertyName("spans")]
        public List<AzureSpan>? Spans { get; set; }
    }

    public class AzureWord
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("polygon")]
        public List<float>? Polygon { get; set; }

        [JsonPropertyName("confidence")]
        public float? Confidence { get; set; }

        [JsonPropertyName("span")]
        public AzureSpan? Span { get; set; }
    }

    public class AzureLine
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("polygon")]
        public List<float>? Polygon { get; set; }

        [JsonPropertyName("spans")]
        public List<AzureSpan>? Spans { get; set; }
    }

    public class AzureBoundingRegion
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("polygon")]
        public List<float>? Polygon { get; set; }
    }

    public class AzureSpan
    {
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }
    }
}
