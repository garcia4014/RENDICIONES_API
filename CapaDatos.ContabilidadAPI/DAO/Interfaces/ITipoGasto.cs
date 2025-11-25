using CapaDatos.ContabilidadAPI.Models;

namespace CapaDatos.ContabilidadAPI.DAO.Interfaces
{
    public interface ITipoGasto
    {
        public Task<IEnumerable<TipoGasto>> GetListTipoGasto();
        public Task<TipoGasto> GetTipoGastoById(int TgId);

    }
}
