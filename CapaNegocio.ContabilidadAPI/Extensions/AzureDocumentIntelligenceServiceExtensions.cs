using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Implementation;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CapaNegocio.ContabilidadAPI.Extensions
{
    /// <summary>
    /// Extensiones para configurar Azure Document Intelligence Service
    /// </summary>
    public static class AzureDocumentIntelligenceServiceExtensions
    {
        /// <summary>
        /// Agrega los servicios de Azure Document Intelligence
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>Colección de servicios modificada</returns>
        public static IServiceCollection AddAzureDocumentIntelligenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurar el DTO desde appsettings
            services.Configure<AzureDocumentIntelligenceConfigurationDto>(
                configuration.GetSection("AzureDocumentIntelligence"));

            // Registrar HttpClient para el servicio
            services.AddHttpClient<IAzureDocumentIntelligenceService, AzureDocumentIntelligenceService>();

            return services;
        }
    }
}
