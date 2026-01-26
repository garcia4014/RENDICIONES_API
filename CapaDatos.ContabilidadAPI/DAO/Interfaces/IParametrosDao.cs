using CapaDatos.ContabilidadAPI.Models;

namespace CapaDatos.ContabilidadAPI.DAO.Interfaces
{
    public interface IParametrosDao
    {
        Task<bool> ActualizarTokenAsync(string nuevoToken);
        Task<Parametros?> ObtenerParametroPorIdAsync(int id);
    }
}
