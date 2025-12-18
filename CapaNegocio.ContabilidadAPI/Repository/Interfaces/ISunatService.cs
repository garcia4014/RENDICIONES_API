using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    /// <summary>
    /// Interface para el servicio coordinador de SUNAT
    /// </summary>
    public interface ISunatService
    {
        /// <summary>
        /// Valida un comprobante de pago obteniendo automáticamente el token
        /// </summary>
        /// <param name="clientId">Client ID de SUNAT</param>
        /// <param name="clientSecret">Client Secret de SUNAT</param>
        /// <param name="rucConsultante">RUC de quien realiza la consulta</param>
        /// <param name="request">Datos del comprobante a validar</param>
        /// <returns>Resultado de la validación con descripciones</returns>
        Task<ApiResponse<SunatComprobanteValidationResultDto>> ValidarComprobanteCompletoAsync(
            string clientId, 
            string clientSecret, 
            string rucConsultante, 
            SunatComprobanteRequestDto request);
        
    }
}