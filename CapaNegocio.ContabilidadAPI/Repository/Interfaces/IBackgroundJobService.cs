using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    /// <summary>
    /// Servicio para ejecutar tareas en background
    /// </summary>
    public interface IBackgroundJobService
    {
        /// <summary>
        /// Encola una validación de comprobante SUNAT para ejecutar en background
        /// </summary>
        /// <param name="comprobanteId">ID del comprobante a validar</param>
        void EnqueueValidacionSunat(int comprobanteId);
    }
}
