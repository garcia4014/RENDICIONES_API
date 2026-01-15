namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// Configuración para el servicio OCR
    /// </summary>
    public class OcrConfigurationDto
    {
        public string TesseractDataPath { get; set; } = "./tessdata";
        public string DefaultLanguage { get; set; } = "spa";
        public OcrPageSegMode DefaultPageSegMode { get; set; } = OcrPageSegMode.Auto;
        public bool EnableImagePreprocessing { get; set; } = true;
        public int MaxFileSizeMB { get; set; } = 10;
        public int MaxPagesPerPdf { get; set; } = 50;
        public int DpiForPdfConversion { get; set; } = 300;
        public bool SaveProcessedImages { get; set; } = false;
        public string? ProcessedImagesPath { get; set; }
        public int TimeoutSeconds { get; set; } = 300; // 5 minutos
    }
}