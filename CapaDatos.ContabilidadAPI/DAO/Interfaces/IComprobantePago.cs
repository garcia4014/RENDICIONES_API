using CapaDatos.ContabilidadAPI.Models;

namespace CapaDatos.ContabilidadAPI.DAO.Interfaces
{
    /// <summary>
    /// Interfaz para operaciones de datos de ComprobantePago
    /// </summary>
    public interface IComprobantePago
    {
        /// <summary>
        /// Obtiene todos los comprobantes de pago activos
        /// </summary>
        /// <returns>Lista de comprobantes de pago activos</returns>
        Task<List<ComprobantePago>> GetAllAsync();

        /// <summary>
        /// Obtiene un comprobante de pago por su ID
        /// </summary>
        /// <param name="id">ID del comprobante</param>
        /// <returns>Comprobante de pago o null si no existe</returns>
        Task<ComprobantePago?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene comprobantes de pago por ID de cabecera de viáticos
        /// </summary>
        /// <param name="svIdCabecera">ID de la cabecera de viáticos</param>
        /// <returns>Lista de comprobantes relacionados</returns>
        Task<List<ComprobantePago>> GetByCabeceraIdAsync(int svIdCabecera);

        /// <summary>
        /// Obtiene comprobantes de pago por ID de detalle de viáticos
        /// </summary>
        /// <param name="svIdDetalle">ID del detalle de viáticos</param>
        /// <returns>Lista de comprobantes relacionados</returns>
        Task<List<ComprobantePago>> GetByDetalleIdAsync(int svIdDetalle);

        /// <summary>
        /// Crea un nuevo comprobante de pago
        /// </summary>
        /// <param name="comprobante">Datos del comprobante a crear</param>
        /// <returns>Comprobante creado con ID asignado</returns>
        Task<ComprobantePago> CreateAsync(ComprobantePago comprobante);

        /// <summary>
        /// Actualiza un comprobante de pago existente
        /// </summary>
        /// <param name="comprobante">Datos actualizados del comprobante</param>
        /// <returns>Comprobante actualizado</returns>
        Task<ComprobantePago> UpdateAsync(ComprobantePago comprobante);

        /// <summary>
        /// Realiza borrado lógico de un comprobante de pago
        /// </summary>
        /// <param name="id">ID del comprobante a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Busca comprobantes por serie y correlativo
        /// </summary>
        /// <param name="serie">Serie del comprobante</param>
        /// <param name="correlativo">Correlativo del comprobante</param>
        /// <returns>Lista de comprobantes que coinciden</returns>
        Task<List<ComprobantePago>> GetBySerieCorrelattivoAsync(string serie, string correlativo);

        /// <summary>
        /// Obtiene comprobantes por RUC emisor
        /// </summary>
        /// <param name="ruc">RUC del emisor</param>
        /// <returns>Lista de comprobantes del RUC</returns>
        Task<List<ComprobantePago>> GetByRucAsync(long ruc);

        /// <summary>
        /// Obtiene comprobantes por fecha de emisión
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Lista de comprobantes en el rango de fechas</returns>
        Task<List<ComprobantePago>> GetByFechaEmisionAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<int> InactiveVoucherPrevius(int idCabecera, int idDetalle);

        /// <summary>
        /// Obtiene estadísticas del dashboard de rendiciones para un empleado
        /// </summary>
        /// <param name="svEmpDni">DNI del empleado</param>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Tupla con las estadísticas (rendicionesPendientes, comprobantesCargados, validadosSunat, pendientesValidacion)</returns>
        Task<(int rendicionesPendientes, int comprobantesCargados, int validadosSunat, int pendientesValidacion)> GetRendicionesDashboardAsync(string? svEmpDni, DateTime fechaInicio, DateTime fechaFin, int[] estadosFlujo);

        /// <summary>
        /// Actualiza el estado de observado de un comprobante
        /// </summary>
        /// <param name="comprobanteId">ID del comprobante</param>
        /// <param name="observado">Estado observado</param>
        /// <param name="observacion">Comentario de observación</param>
        /// <returns>True si se actualizó correctamente</returns>
        Task<bool> ActualizarComprobanteObservado(int comprobanteId, bool observado, string observacion);

        /// <summary>
        /// Actualiza el estado de aprobado de un comprobante
        /// </summary>
        /// <param name="comprobanteId">ID del comprobante</param>
        /// <param name="aprobado">Estado aprobado</param>
        /// <returns>True si se actualizó correctamente</returns>
        Task<bool> ActualizarComprobanteAprobado(int comprobanteId, bool aprobado);

    }
}