using System.ComponentModel.DataAnnotations;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    /// <summary>
    /// DTO para la solicitud de OCR
    /// </summary>
    public class OcrRequestDto
    {
        [Required]
        public byte[] FileData { get; set; }

        [Required]
        public OcrFileType FileType { get; set; }

        public string Language { get; set; } = "spa"; // Español por defecto

        public OcrPageSegMode PageSegMode { get; set; } = OcrPageSegMode.Auto;

        public bool PreprocessImage { get; set; } = true;

        public string? FileName { get; set; }
    }

    /// <summary>
    /// Tipos de archivo soportados para OCR
    /// </summary>
    public enum OcrFileType
    {
        PDF,
        JPG,
        JPEG,
        PNG,
        BMP,
        TIFF
    }

    /// <summary>
    /// Modos de segmentación de página de Tesseract
    /// </summary>
    public enum OcrPageSegMode
    {
        Auto = 3,
        SingleColumn = 4,
        SingleBlock = 6,
        SingleLine = 7,
        SingleWord = 8,
        SingleChar = 10
    }
}