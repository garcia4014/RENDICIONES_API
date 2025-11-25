using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using SkiaSharp;
using System.Diagnostics;
using Tesseract;
using PdfPig = UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    /// <summary>
    /// Servicio de OCR usando Tesseract con soporte para PDF e imágenes
    /// </summary>
    public class TesseractOcrService : IOcrService, IDisposable
    {
        private readonly ILogger<TesseractOcrService> _logger;
        private readonly OcrConfigurationDto _config;
        private TesseractEngine? _tesseractEngine;
        private readonly object _engineLock = new object();

        public TesseractOcrService(ILogger<TesseractOcrService> logger, IOptions<OcrConfigurationDto> config)
        {
            _logger = logger;
            _config = config.Value;
            InitializeTesseract();
        }

        public async Task<ApiResponse<OcrResponseDto>> ExtractTextAsync(OcrRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando extracción de texto para archivo de tipo: {FileType}", request.FileType);
                
                var stopwatch = Stopwatch.StartNew();

                // Validar tamaño del archivo
                if (request.FileData.Length > _config.MaxFileSizeMB * 1024 * 1024)
                {
                    return new ApiResponse<OcrResponseDto>($"El archivo excede el tamaño máximo permitido de {_config.MaxFileSizeMB}MB");
                }

                ApiResponse<OcrResponseDto> result = request.FileType switch
                {
                    OcrFileType.PDF => await ExtractTextFromPdfAsync(request.FileData, request.Language, request.PageSegMode),
                    OcrFileType.JPG or OcrFileType.JPEG or OcrFileType.PNG or OcrFileType.BMP or OcrFileType.TIFF => 
                        await ExtractTextFromImageAsync(request.FileData, request.Language, request.PageSegMode),
                    _ => new ApiResponse<OcrResponseDto>("Tipo de archivo no soportado")
                };

                if (result.Success && result.Data != null)
                {
                    result.Data.ProcessingTime = stopwatch.Elapsed;
                    result.Data.ProcessedAt = DateTime.Now;
                    result.Data.FileName = request.FileName;
                    result.Data.FileType = request.FileType;
                }

                stopwatch.Stop();
                _logger.LogInformation("Extracción completada en {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la extracción de texto");
                return new ApiResponse<OcrResponseDto>("Error inesperado durante la extracción de texto");
            }
        }

        public async Task<ApiResponse<OcrResponseDto>> ExtractTextFromImageAsync(
            byte[] imageBytes, 
            string language = "spa", 
            OcrPageSegMode pageSegMode = OcrPageSegMode.Auto)
        {
            try
            {
                _logger.LogInformation("Iniciando extracción de texto de imagen. Tamaño: {Size} bytes", imageBytes.Length);
                var stopwatch = Stopwatch.StartNew();

                // Validar que tengamos datos de imagen
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    _logger.LogError("Datos de imagen vacíos o nulos");
                    return new ApiResponse<OcrResponseDto>("Datos de imagen vacíos");
                }

                // Preprocesar imagen si está habilitado
                if (_config.EnableImagePreprocessing)
                {
                    _logger.LogInformation("Preprocesando imagen...");
                    imageBytes = await PreprocessImageAsync(imageBytes);
                    _logger.LogInformation("Imagen preprocesada. Nuevo tamaño: {Size} bytes", imageBytes.Length);
                }

                var result = await Task.Run(() =>
                {
                    lock (_engineLock)
                    {
                        if (_tesseractEngine == null)
                        {
                            _logger.LogError("Motor de Tesseract no inicializado");
                            return new ApiResponse<OcrResponseDto>("Motor de Tesseract no inicializado");
                        }

                        try
                        {
                            _logger.LogInformation("Cargando imagen en Tesseract...");
                            using var img = Pix.LoadFromMemory(imageBytes);
                            
                            if (img == null)
                            {
                                _logger.LogError("No se pudo cargar la imagen en Tesseract");
                                return new ApiResponse<OcrResponseDto>("No se pudo cargar la imagen");
                            }

                            _logger.LogInformation("Imagen cargada. Dimensiones: {Width}x{Height}", img.Width, img.Height);
                            
                            using var page = _tesseractEngine.Process(img, Tesseract.PageSegMode.Auto);
                            
                            var text = page.GetText();
                            var confidence = page.GetMeanConfidence();

                            _logger.LogInformation("OCR completado. Texto extraído: {TextLength} caracteres, Confianza: {Confidence}%", 
                                text?.Length ?? 0, confidence * 100);

                            var response = new OcrResponseDto
                            {
                                ExtractedText = text ?? string.Empty,
                                Confidence = confidence,
                                ProcessedPages = 1,
                                Language = language,
                                PageResults = new List<OcrPageResultDto>
                                {
                                    new OcrPageResultDto
                                    {
                                        PageNumber = 1,
                                        Text = text ?? string.Empty,
                                        Confidence = confidence,
                                        WordCount = text?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0,
                                        PageProcessingTime = stopwatch.Elapsed
                                    }
                                }
                            };

                            var message = string.IsNullOrWhiteSpace(text) 
                                ? "Imagen procesada pero no se detectó texto" 
                                : "Texto extraído exitosamente";

                            return new ApiResponse<OcrResponseDto>(response, message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error procesando imagen con Tesseract");
                            return new ApiResponse<OcrResponseDto>("Error procesando la imagen: " + ex.Message);
                        }
                    }
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en extracción de texto de imagen");
                return new ApiResponse<OcrResponseDto>("Error procesando la imagen: " + ex.Message);
            }
        }

        public async Task<ApiResponse<OcrResponseDto>> ExtractTextFromPdfAsync(
            byte[] pdfBytes, 
            string language = "spa", 
            OcrPageSegMode pageSegMode = OcrPageSegMode.Auto,
            int maxPages = 50)
        {
            try
            {
                _logger.LogInformation("Iniciando extracción de texto de PDF");
                var stopwatch = Stopwatch.StartNew();
                var allText = new List<string>();
                var pageResults = new List<OcrPageResultDto>();
                float totalConfidence = 0f;

                // Primero intentar extraer texto nativo del PDF
                var nativeTextResult = await ExtractNativeTextFromPdfAsync(pdfBytes);
                
                if (!string.IsNullOrWhiteSpace(nativeTextResult.extractedText))
                {
                    _logger.LogInformation("Texto nativo extraído del PDF: {Length} caracteres", nativeTextResult.extractedText.Length);
                    
                    // Si encontramos texto nativo, crear respuesta
                    var response = new OcrResponseDto
                    {
                        ExtractedText = nativeTextResult.extractedText,
                        Confidence = 1.0f, // Texto nativo tiene máxima confianza
                        ProcessedPages = nativeTextResult.pageCount,
                        Language = language,
                        PageResults = nativeTextResult.pageTexts.Select((text, index) => new OcrPageResultDto
                        {
                            PageNumber = index + 1,
                            Text = text,
                            Confidence = 1.0f,
                            WordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                            PageProcessingTime = TimeSpan.Zero
                        }).ToList(),
                        ProcessingTime = stopwatch.Elapsed
                    };

                    stopwatch.Stop();
                    return new ApiResponse<OcrResponseDto>(response, "Texto extraído del PDF usando extracción nativa");
                }

                _logger.LogInformation("No se encontró texto nativo en el PDF. Intentando OCR...");

                // Si no hay texto nativo, intentar OCR sobre imágenes generadas del PDF
                return await ProcessPdfWithOcrAsync(pdfBytes, language, pageSegMode, maxPages, stopwatch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando PDF");
                return new ApiResponse<OcrResponseDto>("Error procesando el PDF: " + ex.Message);
            }
        }

        private async Task<(string extractedText, int pageCount, List<string> pageTexts)> ExtractNativeTextFromPdfAsync(byte[] pdfBytes)
        {
            try
            {
                using var stream = new MemoryStream(pdfBytes);
                using var document = PdfPig.PdfDocument.Open(stream);

                var allPages = new List<string>();
                var pageTexts = new List<string>();

                foreach (var page in document.GetPages())
                {
                    var lines = page.Text?.Split('\n') ?? Array.Empty<string>();

                    var processedLines = lines
                        .Select(l =>
                        {
                            var cols = System.Text.RegularExpressions.Regex.Split(l.Trim(), @"\s{3,}");
                            return string.Join(" | ", cols);
                        })
                        .ToList();

                    string pageFormatted =
                        $"=== PÁGINA {page.Number} ===\n" + string.Join("\n", processedLines);

                    allPages.Add(pageFormatted);
                    pageTexts.Add(pageFormatted);
                }

                return (string.Join("\n\n", allPages), document.NumberOfPages, pageTexts);
            }
            catch
            {
                return ("", 0, new List<string>());
            }
        }

          
        private async Task<ApiResponse<OcrResponseDto>> ProcessPdfWithOcrAsync(
            byte[] pdfBytes, 
            string language, 
            OcrPageSegMode pageSegMode, 
            int maxPages, 
            Stopwatch stopwatch)
        {
            try
            {                
                using var stream = new MemoryStream(pdfBytes);
                using var document = PdfPig.PdfDocument.Open(stream);
                
                var pageResults = new List<OcrPageResultDto>();
                
                int pagesToProcess = Math.Min(document.NumberOfPages, maxPages);
                
                for (int i = 1; i <= pagesToProcess; i++)
                {
                    var page = document.GetPage(i);
                    pageResults.Add(new OcrPageResultDto
                    {
                        PageNumber = i,
                        Text = $"Página {i}: Sin texto nativo detectado. Requiere conversión a imagen para OCR.",
                        Confidence = 0f,
                        WordCount = 0,
                        PageProcessingTime = TimeSpan.Zero
                    });
                }

                var response = new OcrResponseDto
                {
                    ExtractedText = $"PDF ESCANEADO DETECTADO: {document.NumberOfPages} página(s) sin texto nativo.\n\n" +
                                   "Para extraer texto de PDFs escaneados:\n" +
                                   "1. Convierta el PDF a imágenes (JPG/PNG) usando herramientas externas\n" +
                                   "2. Use los endpoints de imagen para cada página\n" +
                                   "3. O use un servicio de conversión PDF→Imagen→OCR dedicado\n\n" +
                                   "Páginas procesadas: " + pagesToProcess,
                    Confidence = 0f,
                    ProcessedPages = pagesToProcess,
                    Language = language,
                    PageResults = pageResults,
                    ProcessingTime = stopwatch.Elapsed
                };

                return new ApiResponse<OcrResponseDto>(response, 
                    "PDF analizado - Se requiere conversión a imágenes para OCR de documentos escaneados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando PDF con OCR");
                return new ApiResponse<OcrResponseDto>("Error procesando PDF para OCR: " + ex.Message);
            }
        }

        public async Task<bool> IsConfiguredAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    lock (_engineLock)
                    {
                        return _tesseractEngine != null && Directory.Exists(_config.TesseractDataPath);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando configuración de Tesseract");
                return false;
            }
        }

        public async Task<List<string>> GetAvailableLanguagesAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    var languages = new List<string>();
                    var tessDataPath = _config.TesseractDataPath;
                    
                    // Convertir a ruta absoluta si es relativa
                    if (!Path.IsPathRooted(tessDataPath))
                    {
                        tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), tessDataPath);
                    }
                    
                    if (Directory.Exists(tessDataPath))
                    {
                        var trainedDataFiles = Directory.GetFiles(tessDataPath, "*.traineddata");
                        languages.AddRange(trainedDataFiles.Select(file => 
                            Path.GetFileNameWithoutExtension(file)));
                    }
                    
                    return languages;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo idiomas disponibles");
                return new List<string>();
            }
        }

        public async Task<byte[]> PreprocessImageAsync(byte[] imageBytes)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var inputStream = new MemoryStream(imageBytes);
                    using var bitmap = SKBitmap.Decode(inputStream);
                    
                    if (bitmap == null)
                        return imageBytes;

                    // Crear una nueva imagen procesada
                    var info = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Gray8);
                    using var surface = SKSurface.Create(info);
                    var canvas = surface.Canvas;

                    // Aplicar filtros para mejorar OCR
                    using var paint = new SKPaint
                    {
                        IsAntialias = false,
                        FilterQuality = SKFilterQuality.High
                    };

                    // Convertir a escala de grises y aumentar contraste
                    var colorMatrix = new float[]
                    {
                        0.299f, 0.587f, 0.114f, 0, 0,
                        0.299f, 0.587f, 0.114f, 0, 0,
                        0.299f, 0.587f, 0.114f, 0, 0,
                        0,      0,      0,      1, 0
                    };

                    using var colorFilter = SKColorFilter.CreateColorMatrix(colorMatrix);
                    paint.ColorFilter = colorFilter;

                    canvas.DrawBitmap(bitmap, 0, 0, paint);

                    using var image = surface.Snapshot();
                    using var data = image.Encode(SKEncodedImageFormat.Png, 90);
                    
                    return data.ToArray();
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error preprocesando imagen, usando imagen original");
                return imageBytes;
            }
        }

        private void InitializeTesseract()
        {
            try
            {
                lock (_engineLock)
                {
                    _logger.LogInformation("Inicializando Tesseract con ruta: {Path}", _config.TesseractDataPath);
                    
                    // Convertir a ruta absoluta si es relativa
                    var tessDataPath = _config.TesseractDataPath;
                    if (!Path.IsPathRooted(tessDataPath))
                    {
                        tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), tessDataPath);
                    }
                    
                    _logger.LogInformation("Ruta absoluta de Tesseract: {AbsolutePath}", tessDataPath);
                    _logger.LogInformation("Directorio actual: {CurrentDirectory}", Directory.GetCurrentDirectory());
                    
                    if (!Directory.Exists(tessDataPath))
                    {
                        _logger.LogError("Directorio de datos de Tesseract no encontrado: {Path}", tessDataPath);
                        return;
                    }

                    // Verificar archivos .traineddata
                    var trainedDataFiles = Directory.GetFiles(tessDataPath, "*.traineddata");
                    _logger.LogInformation("Archivos .traineddata encontrados: {Files}", string.Join(", ", trainedDataFiles.Select(Path.GetFileName)));
                    
                    if (trainedDataFiles.Length == 0)
                    {
                        _logger.LogError("No se encontraron archivos .traineddata en: {Path}", tessDataPath);
                        return;
                    }

                    _tesseractEngine = new TesseractEngine(tessDataPath, _config.DefaultLanguage, EngineMode.Default);
                    _logger.LogInformation("Tesseract inicializado correctamente con idioma: {Language}", _config.DefaultLanguage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inicializando Tesseract");
            }
        }

        private async Task<byte[]> ConvertPdfPageToImageAsync(byte[] pdfBytes, int pageIndex)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Intentando convertir página {PageIndex} de PDF a imagen", pageIndex);
                    
                    // Por ahora, como las librerías de PDF a imagen tienen problemas de compatibilidad,
                    // vamos a intentar extraer texto directamente del PDF usando PdfSharp
                    using var stream = new MemoryStream(pdfBytes);
                    var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                    
                    if (pageIndex >= document.PageCount)
                    {
                        _logger.LogError("Índice de página {PageIndex} fuera de rango. Total páginas: {PageCount}", 
                            pageIndex, document.PageCount);
                        throw new ArgumentOutOfRangeException(nameof(pageIndex));
                    }

                    // Como no podemos convertir fácilmente PDF a imagen con las librerías actuales,
                    // vamos a crear una imagen simple con texto blanco sobre fondo negro
                    // para que Tesseract pueda procesarla
                    var pageText = ExtractTextFromPdfPage(document, pageIndex);
                    
                    if (string.IsNullOrWhiteSpace(pageText))
                    {
                        _logger.LogWarning("No se pudo extraer texto de la página {PageIndex}", pageIndex);
                        // Crear una imagen de 1x1 píxel para indicar página vacía
                        return CreateEmptyImage();
                    }

                    // Crear una imagen simple con el texto extraído
                    return CreateTextImage(pageText);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error convirtiendo página PDF a imagen");
                    return CreateEmptyImage();
                }
            });
        }

        private string ExtractTextFromPdfPage(PdfDocument document, int pageIndex)
        {
            try
            {
                var page = document.Pages[pageIndex];
                
                // PdfSharp no tiene extracción de texto nativa, pero podemos intentar
                // obtener información básica de la página
                var pageInfo = $"Página {pageIndex + 1} - Tamaño: {page.Width}x{page.Height}";
                
                _logger.LogInformation("Información de página: {PageInfo}", pageInfo);
                
                // Para esta implementación básica, retornamos información de la página
                return pageInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extrayendo texto de página PDF");
                return string.Empty;
            }
        }

        private byte[] CreateEmptyImage()
        {
            try
            {
                // Crear una imagen de 100x100 píxeles blanca
                using var bitmap = new SKBitmap(100, 100);
                using var canvas = new SKCanvas(bitmap);
                
                canvas.Clear(SKColors.White);
                
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 90);
                
                return data.ToArray();
            }
            catch
            {
                // Si falla, retornar un array vacío
                return new byte[0];
            }
        }

        private byte[] CreateTextImage(string text)
        {
            try
            {
                // Crear una imagen con texto para pruebas
                var width = 800;
                var height = 600;
                
                using var bitmap = new SKBitmap(width, height);
                using var canvas = new SKCanvas(bitmap);
                
                canvas.Clear(SKColors.White);
                
                using var paint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 16,
                    IsAntialias = true,
                    Typeface = SKTypeface.Default
                };

                var lines = text.Split('\n');
                var y = 50f;
                
                foreach (var line in lines.Take(20)) // Máximo 20 líneas
                {
                    canvas.DrawText(line, 50, y, paint);
                    y += 25;
                }
                
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 90);
                
                return data.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando imagen con texto");
                return CreateEmptyImage();
            }
        }

        public void Dispose()
        {
            lock (_engineLock)
            {
                _tesseractEngine?.Dispose();
                _tesseractEngine = null;
            }
        }
    }
}