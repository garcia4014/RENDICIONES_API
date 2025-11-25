using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using CapaDatos.ContabilidadAPI.Models.General;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace CapaDatos.ContabilidadAPI.DAO.Implementation
{
    public class SviaticoImpl : ISviatico
    {
        private readonly SvrendicionesContext _context;
        private readonly GeneralDBContext _contextGeneral;
        public SviaticoImpl(SvrendicionesContext context, GeneralDBContext generalDBContext)
        {
            _context = context;
            _contextGeneral = generalDBContext;
        }


        public async Task<List<SviaticosCabecera>> GetListSviaticosCabecera()
        {
            return await _context.SviaticosCabeceras
                .Include(c => c.Detalles)
                .Include(e => e.SolicitudEstadoFlujo)
                .ToListAsync();
        }

        public async Task<List<SviaticosCabecera>> GetListSviaticosCabeceraDNI(string idDocumento)
        {
            return await _context.SviaticosCabeceras
                .Include(c => c.Detalles)
                .ThenInclude(x => x.ComprobantesPago)
                .Include(e => e.SolicitudEstadoFlujo)
                .Where(c => c.SvEmpCodigo == idDocumento)
                .ToListAsync();
        }

        public async Task<SviaticosCabecera> GetSviaticosCabecera(int id)
        {
            return await _context.SviaticosCabeceras
                                .Include(c => c.Detalles)
                                .Include(e => e.SolicitudEstadoFlujo)
                                .FirstOrDefaultAsync(c => c.SvId == id);
        }

        public async Task<(SviaticosCabecera Viatico, MVT_EMPPLA Empleado)> GetSviaticosCabeceraV2(int id)
        {
            try
            {
                var viatico = await _context.SviaticosCabeceras
                                .Include(c => c.Detalles)
                                .Include(e => e.SolicitudEstadoFlujo)
                                .FirstOrDefaultAsync(c => c.SvId == id);
                if (viatico == null)
                    return (null, null);

                var empleado = await _contextGeneral.Empleados.FirstOrDefaultAsync(e => e.Code == viatico.SvEmpDni);

                return (viatico, empleado);
            }
            catch (Exception ex)
            {
                return (null, null);
            }

        }

        public async Task<List<SviaticosCabecera>> GetViaticosFiltrados(string? svEmpDni, DateTime? fechaInicio, DateTime? fechaFin, int[]? estados, string? descripcion, int? pagina = null, int? tamanoPagina = null)
        {
            try
            {
                var query = _context.SviaticosCabeceras
                    .Include(c => c.Detalles)
                    .Include(e => e.SolicitudEstadoFlujo)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(svEmpDni))
                {
                    query = query.Where(s => s.SvEmpDni == svEmpDni);
                }

                if (fechaInicio.HasValue)
                {
                    query = query.Where(s => s.SvFechaCreacion >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    var fechaFinConHora = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(s => s.SvFechaCreacion <= fechaFinConHora);
                }

                if (estados != null && estados.Length > 0)
                {
                    query = query.Where(s => estados.Contains(s.SvSefId));
                }

                if (!string.IsNullOrEmpty(descripcion))
                {
                    query = query.Where(s => s.SvDescripcion.Contains(descripcion));
                }

                query = query.OrderByDescending(s => s.SvFechaCreacion);

                if (pagina.HasValue && tamanoPagina.HasValue && pagina > 0 && tamanoPagina > 0)
                {
                    var skip = (pagina.Value - 1) * tamanoPagina.Value;
                    query = query.Skip(skip).Take(tamanoPagina.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log error
                return new List<SviaticosCabecera>();
            }
        }

        // Método para obtener viáticos filtrados con conteo de estados
        public async Task<(List<SviaticosCabecera> viaticos, Dictionary<int, int> conteoEstados)> GetViaticosFiltradosConConteo(string? svEmpDni, DateTime? fechaInicio, DateTime? fechaFin, int[]? estados, string? descripcion, int? pagina = null, int? tamanoPagina = null)
        {
            try
            {
                //Tomar Mes Actual
                fechaFin = DateTime.Now;
                fechaInicio = new DateTime(fechaFin.GetValueOrDefault().Year, fechaFin.GetValueOrDefault().Month, 1);
                

                // Construir query base con filtros (sin paginación para el conteo)
                var queryBase = _context.SviaticosCabeceras.AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(svEmpDni))
                {
                    queryBase = queryBase.Where(s => s.SvEmpDni == svEmpDni);
                }

                queryBase = queryBase.Where(s => s.SvFechaCreacion >= fechaInicio.Value);

                if (fechaInicio.HasValue)
                {
                    queryBase = queryBase.Where(s => s.SvFechaCreacion >= fechaInicio.Value);
                }

                if (fechaFin.HasValue)
                {
                    var fechaFinConHora = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                    queryBase = queryBase.Where(s => s.SvFechaCreacion <= fechaFinConHora);
                }

                if (estados != null && estados.Length > 0)
                {
                    queryBase = queryBase.Where(s => estados.Contains(s.SvSefId));
                }

                if (!string.IsNullOrEmpty(descripcion))
                {
                    queryBase = queryBase.Where(s => s.SvDescripcion.Contains(descripcion));
                }

                // 1. Primero obtener conteo por estados (sin paginación)
                var conteoEstados = await queryBase
                    .GroupBy(s => s.SvSefId)
                    .Select(g => new { EstadoId = g.Key, Cantidad = g.Count() })
                    .ToDictionaryAsync(x => x.EstadoId, x => x.Cantidad);

                // 2. Luego obtener viáticos paginados
                var queryViaticos = queryBase
                    .Include(c => c.Detalles)
                    .Include(d=>d.ComprobantesPago)
                    .Include(e => e.SolicitudEstadoFlujo)
                    .OrderByDescending(s => s.SvFechaCreacion);

                // Aplicar paginación solo a los viáticos
                if (pagina.HasValue && tamanoPagina.HasValue && pagina > 0 && tamanoPagina > 0)
                {
                    var skip = (pagina.Value - 1) * tamanoPagina.Value;
                    queryViaticos = (IOrderedQueryable<SviaticosCabecera>)queryViaticos.Skip(skip).Take(tamanoPagina.Value);
                }

                var viaticos = await queryViaticos.ToListAsync();

                return (viaticos, conteoEstados);
            }
            catch (Exception ex)
            {
                // Log error
                return (new List<SviaticosCabecera>(), new Dictionary<int, int>());
            }
        }

        public async Task<bool> SaveCabecera(SviaticosCabecera cabecera)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.SviaticosCabeceras.Add(cabecera);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;

            }
            catch (DbUpdateException ex)
            {
                Console.Write(ex.InnerException?.Message);
                await transaction.RollbackAsync();
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }

        }

        //Detalle
        public async Task<IEnumerable<SviaticosDetalle>> GetListSviaticosDetalle()
        {
            throw new NotImplementedException();
        }


        public async Task<SviaticosDetalle> GetSviaticosDetalle(int id)
        {
            try
            {
                var d = await _context.SviaticosDetalles.Include(x => x.ComprobantesPago)
                             .FirstOrDefaultAsync(c => c.SvdId == id);
                return d;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> UpdateCabecera(SviaticosCabecera cabecera, List<SviaticosDetalle> detalles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.SviaticosCabeceras.Update(cabecera);
                await _context.SaveChangesAsync();


                var detallesActuales = await _context.SviaticosDetalles
                                                     .Where(d => d.SvdIdCabecera == cabecera.SvId)
                                                     .ToListAsync();

                var isDetallesNuevos = detalles.Select(d => d.SvdId).ToList();
                var detallesAEliminar = detallesActuales
                                        .Where(d => !isDetallesNuevos.Contains(d.SvdId))
                                        .ToList();

                _context.SviaticosDetalles.RemoveRange(detallesAEliminar);

                // AGREGAR o ACTUALIZAR detalles
                foreach (var item in detalles)
                {
                    var detallesExists = detallesActuales.FirstOrDefault(d => d.SvdId == item.SvdId);
                    if (detallesExists != null)
                    {
                        _context.Entry(detallesExists).CurrentValues.SetValues(item);
                    }
                    else
                    {
                        item.SvdId = 0;
                        _context.SviaticosDetalles.Add(item);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // Implementación de métodos para gestión de estados
        public async Task<bool> UpdateEstadoSolicitud(int sviaticoId, int nuevoEstadoId, string comentarios = "")
        {
            try
            {
                var sviatico = await _context.SviaticosCabeceras.FirstOrDefaultAsync(s => s.SvId == sviaticoId);
                if (sviatico == null)
                    return false;

                sviatico.SvSefId = nuevoEstadoId;
                sviatico.Comentarios = comentarios;

                var detalles = sviatico.Detalles;

                foreach ( var detalle in detalles)
                {
                    detalle.Observado = false;
                    detalle.Observacion = string.Empty;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<SolicitudEstadoFlujo>> GetEstadosDisponibles()
        {
            return await _context.SolicitudEstadoFlujos
                .Where(e => e.SefEstado == true && e.SefProceso == "Viaticos")
                .OrderBy(e => e.SefId)
                .ToListAsync();
        }

        public async Task<SolicitudEstadoFlujo> GetEstadoPorId(int estadoId)
        {
            return await _context.SolicitudEstadoFlujos
                .FirstOrDefaultAsync(e => e.SefId == estadoId);
        }

        public async Task<(int pendientes, decimal aprobadas, int enRevision, decimal totalMes)> GetDashboardEstadisticas(string codigoUsuario)
        {
            try
            {
                var fechaActual = DateTime.Now;
                var inicioMes = new DateTime(fechaActual.Year, fechaActual.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                // Viáticos Pendientes (Estado = 1 - Solicitado)
                var pendientes = await _context.SviaticosCabeceras
                    .Where(s => s.SvSefId == 1 && s.SvEmpCodigo == codigoUsuario)
                    .CountAsync();

                // Rendiciones Aprobadas (Estado = 3 - Aprobado) - Suma de totales
                var aprobadas = await _context.SviaticosCabeceras
                    .Where(s => s.SvSefId == 3 && s.SvEmpCodigo == codigoUsuario)
                    .SumAsync(s => s.SvTotalSolicitado ?? 0);

                // En Revisión (Estado = 2 - Abierto)
                var enRevision = await _context.SviaticosCabeceras
                    .Where(s => s.SvSefId == 2 && s.SvEmpCodigo == codigoUsuario)
                    .CountAsync();

                // Total del Mes (suma de todos los viáticos del mes actual)
                var totalMes = await _context.SviaticosCabeceras
                    .Where(s => s.SvEmpCodigo == codigoUsuario &&
                               s.SvFechaCreacion >= inicioMes && s.SvFechaCreacion <= finMes)
                    .SumAsync(s => s.SvTotalSolicitado ?? 0);

                return (pendientes, aprobadas, enRevision, totalMes);
            }
            catch (Exception ex)
            {
                // Log error
                return (0, 0, 0, 0);
            }
        }
         
        public async Task<bool> ActualizarDetalleObservado(int svdId, bool observado,string comentario)
        {
            try
            {
                var detalle = await _context.SviaticosDetalles.FirstOrDefaultAsync(d => d.SvdId == svdId);
                if (detalle == null)
                    return false;

                detalle.Observado = observado;
                detalle.Observacion = comentario;

                var cabecera = await _context.SviaticosCabeceras.FirstOrDefaultAsync(d => d.SvId == detalle.SvdIdCabecera);
                if (cabecera == null)
                    return false;

                cabecera.SvSefId = 8;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ActualizarDetalleAprobado(int svdId, bool aprobado)
        {
            try
            {
                var detalle = await _context.SviaticosDetalles.FirstOrDefaultAsync(d => d.SvdId == svdId);
                if (detalle == null)
                    return false;

                detalle.Aprobado = aprobado;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
