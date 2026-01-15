using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    public interface ISviaticoService
    {
        public Task<ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>> GetListSviaticosCabecera();
        public Task<ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>> GetListSviaticosCabeceraDNI(string idDocumento);
        public Task<ApiResponse<SviaticosCabeceraDTOResponse>> GetSviaticoCabecera(int id);
        public Task<ApiResponse<SviaticosCabecerav2DTOResponse>> GetSviaticoCabeceraV2(int id);
        public Task<ApiResponse<SviaticosCabeceraDTOResponse>> SaveCabecera(SviaticosCabeceraDTO sviaticosCabecera);
        public Task<ApiResponse<SviaticosCabeceraDTOResponse>> UpdateCabecera(SviaticosCabeceraUpdateDTO dto);

        public Task<IEnumerable<SviaticosDetalle>> GetListSviaticosDetalle();
        public Task<ApiResponse<SviaticosDetalleDTOResponse>> GetSviaticosDetalle(int id);

        // Método para obtener viáticos filtrados
        public Task<ApiResponse<IEnumerable<SviaticosCabeceraDTOResponse>>> GetViaticosFiltrados(SviaticoFiltroDto filtro);

        // Método para obtener viáticos filtrados con conteo de estados
        public Task<ApiResponse<ViaticosFiltradosResponseDto>> GetViaticosFiltradosConConteo(SviaticoFiltroDto filtro);

        // Métodos para actualizar campos de detalle
        public Task<ApiResponse<bool>> ActualizarDetalleObservado(int svdId, bool observado, string? comentario); 
        public Task<ApiResponse<bool>> ActualizarDetalleAprobado(int svdId, bool aprobado);

        // Métodos para gestión de estados
        public Task<ApiResponse<SviaticosCabeceraDTOResponse>> ActualizarEstadoSolicitud(int sviaticoId, int nuevoEstadoId, string? comentario = null);
        public Task<ApiResponse<IEnumerable<SolicitudEstadoFlujo>>> GetEstadosDisponibles();
        public Task<ApiResponse<SolicitudEstadoFlujo>> GetEstadoActual(int sviaticoId);

        // Método para dashboard de estadísticas
        public Task<ApiResponse<ViaticoDashboardDto>> GetDashboardEstadisticas(string codigoUsuario);
    }
}
