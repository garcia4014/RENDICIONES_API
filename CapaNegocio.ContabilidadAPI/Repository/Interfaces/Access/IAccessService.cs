using CapaDatos.ContabilidadAPI.Models;
using CapaDatos.ContabilidadAPI.Models.Access;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces.Access
{
    public interface IAccessService
    {
        public Task<ApiResponse<IEnumerable<Perfil_Usuario>>> GetPerfilesByUsuario(string idDocumento);
        public Task<ApiResponse<PersonalDto>> ValidarPersonal(string username, string password);
    }
}
