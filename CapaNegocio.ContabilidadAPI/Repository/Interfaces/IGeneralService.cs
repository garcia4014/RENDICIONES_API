using CapaDatos.ContabilidadAPI.Models.General;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    public interface IGeneralService
    {
        public Task<ApiResponse<General>> GetGeneralData(string idDocumento);
        public Task<EmpleadoDTO> GetEmpleado(string idDocumento);
    }
}
