using AutoMapper;
using CapaDatos.ContabilidadAPI.DAO.Interfaces.Access;
using CapaDatos.ContabilidadAPI.Models.Access;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces.Access;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation.Access
{
    public class AccessServiceImpl : IAccessService
    {
        private readonly IAccess _dao;
        private readonly IMapper _mapper;
        public AccessServiceImpl(IAccess dao, IMapper mapper)
        {
            _dao = dao;
            _mapper = mapper;
        }
        public async Task<ApiResponse<IEnumerable<Perfil_Usuario>>> GetPerfilesByUsuario(string idDocumento)
        {
            try
            {
                var list = await _dao.GetPerfilesByUsuario(idDocumento);

                if(list == null)   
                    return new ApiResponse<IEnumerable<Perfil_Usuario>>("Perfil Usuario no ha sido encontrado");

                return new ApiResponse<IEnumerable<Perfil_Usuario>>(list);
            }
            catch
            (Exception ex)
            {
                return new ApiResponse<IEnumerable<Perfil_Usuario>>("Error al obtener el perfil_usuario: " + ex.Message);
            }
        }

        public async  Task<ApiResponse<PersonalDto>> ValidarPersonal(string username, string password)
        {
            try
            {
                var item = await _dao.ValidarPersonal(username, password);

                if (item == null) { 
                    return new ApiResponse<PersonalDto>("Usuario no ha sido encontrado. Revise sus credenciales.");
                }

                var list = await _dao.GetPerfilesByUsuario(item.IdDocumento);

                PersonalDto personal = _mapper.Map<PersonalDto>(item);
                personal.perfil_Usuarios = list;
                return new ApiResponse<PersonalDto>(personal);
            
            }
            catch
            (Exception ex)
            {
                return new ApiResponse<PersonalDto>("Error al obtener el usuario: " + ex.Message);
            }
        }
    }
}
