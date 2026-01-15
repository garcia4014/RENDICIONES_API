using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    public interface ITipoGastoServices
    {
        public Task<ApiResponse<IEnumerable<TipoGasto>>> GetListTipoGasto();
        public Task<ApiResponse<TipoGasto>> GetTipoGastoById(int TgId);
    }
}
