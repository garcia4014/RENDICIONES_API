namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO para la respuesta de OCR
    /// </summary>
    public class OcrResponseDto
    {
        public string ExtractedText { get; set; }
        public float Confidence { get; set; }
        public int ProcessedPages { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public string Language { get; set; }
        public OcrFileType FileType { get; set; }
        public List<OcrPageResultDto> PageResults { get; set; } = new List<OcrPageResultDto>();
        public string? FileName { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Resultado de OCR por página
    /// </summary>
    public class OcrPageResultDto
    {
        public int PageNumber { get; set; }
        public string Text { get; set; }
        public float Confidence { get; set; }
        public int WordCount { get; set; }
        public TimeSpan PageProcessingTime { get; set; }
    }
}