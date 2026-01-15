using CapaNegocio.ContabilidadAPI.Repository.Implementation;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CapaNegocio.ContabilidadAPI.Extensions
{
    /// <summary>
    /// Extensiones para la configuración de servicios SUNAT
    /// </summary>
    public static class SunatServiceExtensions
    {
        /// <summary>
        /// Agrega los servicios de SUNAT al contenedor de dependencias
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>Colección de servicios configurada</returns>
        public static IServiceCollection AddSunatServices(this IServiceCollection services)
        {
            // Configurar HttpClient para SUNAT
            services.AddHttpClient<ISunatTokenService, SunatTokenService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "ContabilidadAPI/1.0");
            });

            services.AddHttpClient<ISunatComprobanteService, SunatComprobanteService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "ContabilidadAPI/1.0");
            });

            // Registrar servicios
            services.AddScoped<ISunatTokenService, SunatTokenService>();
            services.AddScoped<ISunatComprobanteService, SunatComprobanteService>();
            services.AddScoped<ISunatService, SunatService>();

            return services;
        }
    }
}