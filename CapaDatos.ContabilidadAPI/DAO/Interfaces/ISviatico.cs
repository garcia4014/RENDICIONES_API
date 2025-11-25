using CapaDatos.ContabilidadAPI.Models;
using CapaDatos.ContabilidadAPI.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.DAO.Interfaces
{
    public interface ISviatico
    {
        public Task<List<SviaticosCabecera>> GetListSviaticosCabecera();
        public Task<List<SviaticosCabecera>> GetListSviaticosCabeceraDNI(string idDocumento);
        public Task<SviaticosCabecera> GetSviaticosCabecera(int id);
        public Task<(SviaticosCabecera Viatico, MVT_EMPPLA Empleado)> GetSviaticosCabeceraV2(int id);

        // Método para obtener viáticos filtrados
        public Task<List<SviaticosCabecera>> GetViaticosFiltrados(string? svEmpDni, DateTime? fechaInicio, DateTime? fechaFin, int[]? estados, string? descripcion, int? pagina = null, int? tamanoPagina = null);

        // Método para obtener viáticos filtrados con conteo de estados
        public Task<(List<SviaticosCabecera> viaticos, Dictionary<int, int> conteoEstados)> GetViaticosFiltradosConConteo(string? svEmpDni, DateTime? fechaInicio, DateTime? fechaFin, int[]? estados, string? descripcion, int? pagina = null, int? tamanoPagina = null);

        public Task<bool> SaveCabecera(SviaticosCabecera sviaticosCabecera);
        public Task<bool> UpdateCabecera(SviaticosCabecera sviaticosCabecera, List<SviaticosDetalle> detalles);
        public Task<IEnumerable<SviaticosDetalle>> GetListSviaticosDetalle();
        public Task<SviaticosDetalle> GetSviaticosDetalle(int id); 
        public Task<bool> ActualizarDetalleObservado(int svdId, bool observado,string observacion);
        public Task<bool> ActualizarDetalleAprobado(int svdId, bool aprobado); 
        public Task<bool> UpdateEstadoSolicitud(int sviaticoId, int nuevoEstadoId, string comentarios = "");
        public Task<List<SolicitudEstadoFlujo>> GetEstadosDisponibles();
        public Task<SolicitudEstadoFlujo> GetEstadoPorId(int estadoId); 
        public Task<(int pendientes, decimal aprobadas, int enRevision, decimal totalMes)> GetDashboardEstadisticas(string codigoUsuario);
    }
}
