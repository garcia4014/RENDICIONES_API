using CapaDatos.ContabilidadAPI.Models;

namespace CapaDatos.ContabilidadAPI.DAO.Interfaces
{
    public interface IPoliticaTipoGastoPersona
    {
        public Task<IEnumerable<PoliticaTipoGastoPersona>> GetListPoliticaTipoGastoPersona();

    }
}
