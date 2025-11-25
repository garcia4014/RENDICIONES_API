using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    /// <summary>
    /// Interface para el servicio de autenticación con SUNAT
    /// </summary>
    public interface ISunatTokenService
    {
        /// <summary>
        /// Obtiene un token de acceso de SUNAT
        /// </summary>
        /// <param name="clientId">Client ID obtenido desde SOL</param>
        /// <param name="clientSecret">Client Secret obtenido desde SOL</param>
        /// <returns>Token de acceso</returns>
        Task<ApiResponse<SunatTokenResponseDto>> ObtenerTokenAsync(string clientId, string clientSecret);

        /// <summary>
        /// Verifica si un token es válido y no ha expirado
        /// </summary>
        /// <param name="token">Token a verificar</param>
        /// <returns>True si el token es válido</returns>
        Task<bool> ValidarTokenAsync(string token);
    }
}