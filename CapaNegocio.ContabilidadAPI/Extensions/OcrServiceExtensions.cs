using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Implementation;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CapaNegocio.ContabilidadAPI.Extensions
{
    /// <summary>
    /// Extensiones para la configuración de servicios OCR
    /// </summary>
    public static class OcrServiceExtensions
    {
        /// <summary>
        /// Agrega los servicios de OCR al contenedor de dependencias
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configureOptions">Configuración opcional para OCR</param>
        /// <returns>Colección de servicios configurada</returns>
        public static IServiceCollection AddOcrServices(
            this IServiceCollection services,
            Action<OcrConfigurationDto>? configureOptions = null)
        {
            // Configurar opciones de OCR
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            else
            {
                // Configuración por defecto
                services.Configure<OcrConfigurationDto>(options =>
                {
                    options.TesseractDataPath = "./tessdata";
                    options.DefaultLanguage = "spa";
                    options.DefaultPageSegMode = OcrPageSegMode.Auto;
                    options.EnableImagePreprocessing = true;
                    options.MaxFileSizeMB = 10;
                    options.MaxPagesPerPdf = 50;
                    options.DpiForPdfConversion = 300;
                    options.TimeoutSeconds = 300;
                });
            }

            // Registrar servicio OCR como Singleton debido a la inicialización de Tesseract
            services.AddSingleton<IOcrService, TesseractOcrService>();

            return services;
        }

        /// <summary>
        /// Agrega servicios OCR con configuración específica desde appsettings
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="tesseractDataPath">Ruta a los datos de Tesseract</param>
        /// <param name="defaultLanguage">Idioma por defecto</param>
        /// <returns>Colección de servicios configurada</returns>
        public static IServiceCollection AddOcrServices(
            this IServiceCollection services,
            string tesseractDataPath,
            string defaultLanguage = "spa")
        {
            return services.AddOcrServices(options =>
            {
                options.TesseractDataPath = tesseractDataPath;
                options.DefaultLanguage = defaultLanguage;
            });
        }
    }
}