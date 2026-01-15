# Configuración del Servicio OCR con Tesseract

## Instalación y Configuración

### 1. Descargar archivos de idioma de Tesseract

Los archivos `.traineddata` deben descargarse desde:
- **GitHub oficial**: https://github.com/tesseract-ocr/tessdata

### 2. Archivos de idioma recomendados

- **Español**: `spa.traineddata`
- **Inglés**: `eng.traineddata`
- **Múltiples idiomas**: `spa+eng.traineddata`

### 3. Estructura de carpetas

```
tu-proyecto/
├── tessdata/
│   ├── spa.traineddata
│   ├── eng.traineddata
│   └── spa+eng.traineddata
└── processed-images/ (opcional)
```

### 4. Configuración en appsettings.json

```json
{
  "OcrConfiguration": {
    "TesseractDataPath": "./tessdata",
    "DefaultLanguage": "spa",
    "DefaultPageSegMode": 3,
    "EnableImagePreprocessing": true,
    "MaxFileSizeMB": 10,
    "MaxPagesPerPdf": 50,
    "DpiForPdfConversion": 300,
    "SaveProcessedImages": false,
    "ProcessedImagesPath": "./processed-images",
    "TimeoutSeconds": 300
  }
}
```

### 5. Uso en Program.cs o Startup.cs

```csharp
// Opción 1: Configuración por defecto
builder.Services.AddOcrServices();

// Opción 2: Configuración personalizada
builder.Services.AddOcrServices(options =>
{
    options.TesseractDataPath = "./tessdata";
    options.DefaultLanguage = "spa";
    options.MaxFileSizeMB = 20;
});

// Opción 3: Configuración simple
builder.Services.AddOcrServices("./tessdata", "spa");
```

### 6. Uso en Controladores

```csharp
[ApiController]
public class OcrController : ControllerBase
{
    private readonly IOcrService _ocrService;
    
    public OcrController(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }
    
    [HttpPost("extract-text")]
    public async Task<IActionResult> ExtractText([FromForm] IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        
        var request = new OcrRequestDto
        {
            FileData = memoryStream.ToArray(),
            FileType = GetFileType(file.FileName),
            Language = "spa",
            FileName = file.FileName
        };
        
        var result = await _ocrService.ExtractTextAsync(request);
        return Ok(result);
    }
}
```

## Formatos de Archivo Soportados

- **Imágenes**: JPG, JPEG, PNG, BMP, TIFF
- **Documentos**: PDF (convertido a imágenes)

## Parámetros de Configuración

| Parámetro | Descripción | Valor por defecto |
|-----------|-------------|-------------------|
| `TesseractDataPath` | Ruta a archivos .traineddata | `"./tessdata"` |
| `DefaultLanguage` | Idioma por defecto | `"spa"` |
| `DefaultPageSegMode` | Modo de segmentación | `3 (Auto)` |
| `EnableImagePreprocessing` | Preprocesar imágenes | `true` |
| `MaxFileSizeMB` | Tamaño máximo de archivo | `10 MB` |
| `MaxPagesPerPdf` | Páginas máximas por PDF | `50` |
| `DpiForPdfConversion` | DPI para conversión PDF | `300` |
| `TimeoutSeconds` | Timeout de procesamiento | `300 seg` |

## Notas Importantes

1. **Rendimiento**: El servicio OCR puede ser intensivo en CPU
2. **Memoria**: PDFs grandes pueden consumir mucha memoria
3. **Calidad**: Imágenes de alta resolución dan mejores resultados
4. **Idiomas**: Descarga solo los idiomas que necesites
5. **Thread Safety**: El servicio está protegido para uso concurrente