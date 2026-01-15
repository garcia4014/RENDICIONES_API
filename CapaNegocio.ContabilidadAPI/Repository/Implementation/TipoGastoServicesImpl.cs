using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    public class TipoGastoServicesImpl : ITipoGastoServices
    {
        private readonly ITipoGasto _dao;
        public TipoGastoServicesImpl(ITipoGasto dao)
        {
            _dao = dao;
        }
        public async Task<ApiResponse<IEnumerable<TipoGasto>>> GetListTipoGasto()
        {
            try
            {
                var list = await _dao.GetListTipoGasto();

                if (list == null)
                    return new ApiResponse<IEnumerable<TipoGasto>>("Lista Tipo de Gasto no ha sido encontrada.");

                return new ApiResponse<IEnumerable<TipoGasto>>(list);
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<TipoGasto>>("Error al obtener el tipo de gasto: " + ex.Message);
            }   
        }

        public async Task<ApiResponse<TipoGasto>> GetTipoGastoById(int TgId)
        {
            try
            {
                var item = await _dao.GetTipoGastoById(TgId);

                if (item == null)
                    return new ApiResponse<TipoGasto>("Tipo de Gasto no encontrado.");

                return new ApiResponse<TipoGasto>(item);
            }
            catch
            (Exception ex)
            {
                return new ApiResponse<TipoGasto>("Error al obtener el tipo de gasto: " + ex.Message);
            }
        }
    }
}
