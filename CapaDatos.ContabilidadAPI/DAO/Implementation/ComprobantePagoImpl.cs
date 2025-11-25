using Azure;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CapaDatos.ContabilidadAPI.DAO.Implementation
{
    /// <summary>
    /// Implementación de DAO para ComprobantePago
    /// </summary>
    public class ComprobantePagoImpl : IComprobantePago
    {
        private readonly SvrendicionesContext _context;

        public ComprobantePagoImpl(SvrendicionesContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los comprobantes de pago activos
        /// </summary>
        public async Task<List<ComprobantePago>> GetAllAsync()
        {
            try
            {
                return await _context.ComprobantesPago
                    .Where(c => c.Activo == true)
                    .Include(c => c.SviaticosCabecera)
                    .Include(c => c.SviaticosDetalle)
                    .OrderByDescending(c => c.FechaCarga)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener comprobantes de pago: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un comprobante de pago por su ID
        /// </summary>
        public async Task<ComprobantePago?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.ComprobantesPago
                    .Where(c => c.Id == id && c.Activo == true)
                    .Include(c => c.SviaticosCabecera)
                    .Include(c => c.SviaticosDetalle)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener comprobante de pago por ID: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene comprobantes de pago por ID de cabecera de viáticos
        /// </summary>
        public async Task<List<ComprobantePago>> GetByCabeceraIdAsync(int svIdCabecera)
        {
            try
            {
                return await _context.ComprobantesPago
                    .Where(c => c.SvIdCabecera == svIdCabecera && c.Activo == true)
                    .Include(c => c.SviaticosCabecera)
                    .Include(c => c.SviaticosDetalle)
                    .OrderByDescending(c => c.FechaCarga)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener comprobantes por cabecera: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene comprobantes de pago por ID de detalle de viáticos
        /// </summary>
        public async Task<List<ComprobantePago>> GetByDetalleIdAsync(int svIdDetalle)
        {
            try
            {
                return await _context.ComprobantesPago
                    .Where(c => c.SvIdDetalle == svIdDetalle && c.Activo == true)
                    .Include(c => c.SviaticosCabecera)
                    .Include(c => c.SviaticosDetalle)
                    .OrderByDescending(c => c.FechaCarga)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener comprobantes por detalle: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea un nuevo comprobante de pago
        /// </summary>
        public async Task<ComprobantePago> CreateAsync(ComprobantePago comprobante)
        {
            try
            {
                // Establecer valores por defecto
                comprobante.FechaCarga = DateTime.Now;
                comprobante.Activo = true;

                _context.ComprobantesPago.Add(comprobante);
                await _context.SaveChangesAsync();

                // Incluir navegación en el retorno
                return await GetByIdAsync(comprobante.Id) ?? comprobante;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear comprobante de pago: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un comprobante de pago existente
        /// </summary>
        public async Task<ComprobantePago> UpdateAsync(ComprobantePago comprobante)
        {
            try
            {
                var existingComprobante = await _context.ComprobantesPago
                    .FirstOrDefaultAsync(c => c.Id == comprobante.Id && c.Activo == true);

                if (existingComprobante == null)
                {
                    throw new ArgumentException("Comprobante de pago no encontrado o inactivo");
                }

                // Actualizar campos
                existingComprobante.SvIdCabecera = comprobante.SvIdCabecera;
                existingComprobante.SvIdDetalle = comprobante.SvIdDetalle;
                existingComprobante.TipoComprobante = comprobante.TipoComprobante;
                existingComprobante.Descripcion = comprobante.Descripcion;
                existingComprobante.Serie = comprobante.Serie;
                existingComprobante.Correlativo = comprobante.Correlativo;
                existingComprobante.FechaEmision = comprobante.FechaEmision;
                existingComprobante.Monto = comprobante.Monto;
                existingComprobante.Ruc = comprobante.Ruc;
                existingComprobante.RazonSocial = comprobante.RazonSocial;
                existingComprobante.Ruta = comprobante.Ruta;
                existingComprobante.ValidoSunat = comprobante.ValidoSunat;
                existingComprobante.Notas = comprobante.Notas;

                await _context.SaveChangesAsync();

                return await GetByIdAsync(existingComprobante.Id) ?? existingComprobante;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar comprobante de pago: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Realiza borrado lógico de un comprobante de pago
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var comprobante = await _context.ComprobantesPago
                    .FirstOrDefaultAsync(c => c.Id == id && c.Activo == true);

                if (comprobante == null)
                {
                    return false;
                }

                comprobante.Activo = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar comprobante de pago: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca comprobantes por serie y correlativo
        /// </summary>
        public async Task<List<ComprobantePago>> GetBySerieCorrelattivoAsync(string serie, string correlativo)
        {
            try
            {
                return await _context.ComprobantesPago
                    .Where(c => c.Serie == serie && c.Correlativo == correlativo && c.Activo == true)
                    .Include(c => c.SviaticosCabecera)
                    .Include(c => c.SviaticosDetalle)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar comprobantes por serie/correlativo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene comprobantes por RUC emisor
        /// </summary>
        public async Task<List<ComprobantePago>> GetByRucAsync(long ruc)
        {
            try
            {
                return await _context.ComprobantesPago
                    .Where(c => c.Ruc == ruc && c.Activo == true)
                    .Include(c => c.SviaticosCabecera)
                    .Include(c => c.SviaticosDetalle)
                    .OrderByDescending(c => c.FechaCarga)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener comprobantes por RUC: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene comprobantes por fecha de emisión
        /// </summary>
        public async Task<List<ComprobantePago>> GetByFechaEmisionAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                return await _context.ComprobantesPago
                    .Where(c => c.FechaEmision >= fechaInicio &&
                               c.FechaEmision <= fechaFin &&
                               c.Activo == true)
                    .Include(c => c.SviaticosCabecera)
                    .Include(c => c.SviaticosDetalle)
                    .OrderByDescending(c => c.FechaCarga)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener comprobantes por fecha: {ex.Message}", ex);
            }
        }

        public async Task<int> InactiveVoucherPrevius(int idCabecera, int idDetalle)
        {
            try
            {
                var comprobantes = await _context.ComprobantesPago
                    .Where(x => x.SvIdCabecera == idCabecera && x.SvIdDetalle == idDetalle && x.Activo == true)
                    .ToListAsync();

                foreach (var comprobante in comprobantes)
                {
                    comprobante.Activo = false;
                    comprobante.ValidoSunat = false;
                }

                await _context.SaveChangesAsync();
                return comprobantes.Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al inactivar comprobantes previos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene estadísticas del dashboard de rendiciones para un empleado
        /// </summary>
        public async Task<(int rendicionesPendientes, int comprobantesCargados, int validadosSunat, int pendientesValidacion)>  GetRendicionesDashboardAsync(string? svEmpDni, DateTime fechaInicio, DateTime fechaFin, int[] estadosFlujo  )
        {
            try
            {
                // 1. Rendiciones Pendientes - Estado 5 en SVIATICOS_CABECERA
                var rendicionesPendientesQuery = _context.SviaticosCabeceras
                    .Where(sc => estadosFlujo.Contains(sc.SvSefId) &&
                                sc.SvFechaCreacion >= fechaInicio &&
                                sc.SvFechaCreacion <= fechaFin);

                if (!string.IsNullOrEmpty(svEmpDni))
                    rendicionesPendientesQuery = rendicionesPendientesQuery.Where(sc => sc.SvEmpDni == svEmpDni);

                var rendicionesPendientes = await rendicionesPendientesQuery.CountAsync();

                // Obtener comprobantes para este empleado en el rango de fechas
                var comprobantesQuery =
                    from cp in _context.ComprobantesPago
                    join sd in _context.SviaticosDetalles on cp.SvIdDetalle equals sd.SvdId
                    join sc in _context.SviaticosCabeceras on sd.SvdIdCabecera equals sc.SvId
                    where cp.FechaCarga >= fechaInicio && cp.FechaCarga <= fechaFin
                    select new { cp, sc };

                if (!string.IsNullOrEmpty(svEmpDni))
                    comprobantesQuery = comprobantesQuery.Where(x => x.sc.SvEmpDni == svEmpDni);

                // 2. Comprobantes Cargados - Activo = 1
                var comprobantesCargados = await comprobantesQuery
                    .Where(x => x.cp.Activo == true)
                    .CountAsync();

                // 3. Validados SUNAT - ValidoSunat = 1 y Activo = 1
                var validadosSunat = await comprobantesQuery
                    .Where(x => x.cp.ValidoSunat == true && x.cp.Activo == true)
                    .CountAsync();

                // 4. Pendientes Validación - Comprobantes activos no validados por SUNAT
                var pendientesValidacion = await comprobantesQuery
                    .Where(x => x.cp.ValidoSunat == false && x.cp.Activo == true)
                    .CountAsync();

                return (rendicionesPendientes, comprobantesCargados, validadosSunat, pendientesValidacion);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener estadísticas del dashboard: {ex.Message}", ex);
            }
        }



    }
}