using CapaDatos.ContabilidadAPI.Models.Access;

namespace CapaDatos.ContabilidadAPI.DAO.Interfaces.Access
{
    public interface IAccess
    {
        Task<Personal> ValidarPersonal(string username, string password); 
        Task<List<Perfil_Usuario>> GetPerfilesByUsuario(string idDocumento);
    }
}
