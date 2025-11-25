using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    /// <summary>
    /// Interfaz del servicio para operaciones de ComprobantePago
    /// </summary>
    public interface IComprobantePagoService
    {
        /// <summary>
        /// Obtiene todos los comprobantes de pago con paginación
        /// </summary>
        /// <param name="pagina">Número de página</param>
        /// <param name="tamanoPagina">Tamaño de página</param>
        /// <returns>Respuesta con lista paginada de comprobantes</returns>
        Task<ApiResponse<PagedResult<ComprobantePagoDto>>> GetAllAsync(int pagina = 1, int tamanoPagina = 10);

        /// <summary>
        /// Obtiene un comprobante de pago por su ID
        /// </summary>
        /// <param name="id">ID del comprobante</param>
        /// <returns>Respuesta con el comprobante encontrado</returns>
        Task<ApiResponse<ComprobantePagoDto>> GetByIdAsync(int id);

        /// <summary>
        /// Busca comprobantes con filtros aplicados
        /// </summary>
        /// <param name="filtro">Filtros de búsqueda</param>
        /// <returns>Respuesta con lista filtrada de comprobantes</returns>
        Task<ApiResponse<PagedResult<ComprobantePagoDto>>> BuscarAsync(ComprobantePagoFiltroDto filtro);

        /// <summary>
        /// Obtiene comprobantes por ID de cabecera de viáticos
        /// </summary>
        /// <param name="svIdCabecera">ID de la cabecera</param>
        /// <returns>Respuesta con lista de comprobantes</returns>
        Task<ApiResponse<List<ComprobantePagoDto>>> GetByCabeceraIdAsync(int svIdCabecera);

        /// <summary>
        /// Obtiene comprobantes por ID de detalle de viáticos
        /// </summary>
        /// <param name="svIdDetalle">ID del detalle</param>
        /// <returns>Respuesta con lista de comprobantes</returns>
        Task<ApiResponse<List<ComprobantePagoDto>>> GetByDetalleIdAsync(int svIdDetalle);

        /// <summary>
        /// Crea un nuevo comprobante de pago
        /// </summary>
        /// <param name="createDto">Datos del comprobante a crear</param>
        /// <returns>Respuesta con el comprobante creado</returns>
        Task<ApiResponse<ComprobantePagoDto>> CreateAsync(ComprobantePagoCreateDto createDto);

        /// <summary>
        /// Actualiza un comprobante de pago existente
        /// </summary>
        /// <param name="updateDto">Datos actualizados del comprobante</param>
        /// <returns>Respuesta con el comprobante actualizado</returns>
        Task<ApiResponse<ComprobantePagoDto>> UpdateAsync(ComprobantePagoUpdateDto updateDto);

        /// <summary>
        /// Elimina (borrado lógico) un comprobante de pago
        /// </summary>
        /// <param name="id">ID del comprobante a eliminar</param>
        /// <returns>Respuesta indicando el resultado de la operación</returns>
        Task<ApiResponse<bool>> DeleteAsync(int id);

        /// <summary>
        /// Valida la duplicidad de un comprobante por serie y correlativo
        /// </summary>
        /// <param name="serie">Serie del comprobante</param>
        /// <param name="correlativo">Correlativo del comprobante</param>
        /// <param name="idExcluir">ID a excluir de la validación (para actualizaciones)</param>
        /// <returns>True si existe duplicado</returns>
        Task<bool> ExisteDuplicadoAsync(string serie, string correlativo, int? idExcluir = null);

        /// <summary>
        /// Obtiene estadísticas de comprobantes por período
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Respuesta con estadísticas</returns>
        Task<ApiResponse<ComprobantePagoEstadisticasDto>> GetEstadisticasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene dashboard de rendiciones para un empleado específico
        /// </summary>
        /// <param name="svEmpDni">DNI del empleado</param>
        /// <param name="fechaInicio">Fecha de inicio (opcional)</param>
        /// <param name="fechaFin">Fecha de fin (opcional)</param>
        /// <returns>Respuesta con estadísticas del dashboard</returns>
        Task<ApiResponse<RendicionesDashboardDto>> GetRendicionesDashboardAsync(string[] estados,string svEmpDni, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    }

    /// <summary>
    /// DTO para estadísticas de comprobantes
    /// </summary>
    public class ComprobantePagoEstadisticasDto
    {
        public int TotalComprobantes { get; set; }
        public decimal MontoTotal { get; set; }
        public int ComprobantesPendientes { get; set; }
        public int ComprobantesValidados { get; set; }
        public int ComprobantesSunat { get; set; }
    }

    /// <summary>
    /// Clase para resultados paginados
    /// </summary>
    /// <typeparam name="T">Tipo de datos</typeparam>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}