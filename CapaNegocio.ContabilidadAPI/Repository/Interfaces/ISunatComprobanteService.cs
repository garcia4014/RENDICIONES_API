using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    /// <summary>
    /// Interface para el servicio de consulta de comprobantes de SUNAT
    /// </summary>
    public interface ISunatComprobanteService
    {
        /// <summary>
        /// Valida un comprobante de pago en SUNAT
        /// </summary>
        /// <param name="rucConsultante">RUC de quien realiza la consulta</param>
        /// <param name="token">Token de acceso válido</param>
        /// <param name="request">Datos del comprobante a validar</param>
        /// <returns>Resultado de la validación</returns>
        Task<ApiResponse<SunatComprobanteResponseDto>> ValidarComprobanteAsync(
            string rucConsultante, 
            string token, 
            SunatComprobanteRequestDto request);

        /// <summary>
        /// Obtiene la descripción del estado del comprobante
        /// </summary>
        /// <param name="estadoCp">Código del estado</param>
        /// <returns>Descripción del estado</returns>
        string ObtenerDescripcionEstadoComprobante(int estadoCp);

        /// <summary>
        /// Obtiene la descripción del estado del RUC
        /// </summary>
        /// <param name="estadoRuc">Código del estado del RUC</param>
        /// <returns>Descripción del estado</returns>
        string ObtenerDescripcionEstadoRuc(string estadoRuc);

        /// <summary>
        /// Obtiene la descripción de la condición domiciliaria
        /// </summary>
        /// <param name="condDomiRuc">Código de condición domiciliaria</param>
        /// <returns>Descripción de la condición</returns>
        string ObtenerDescripcionCondicionDomiciliaria(string condDomiRuc);

    }
}