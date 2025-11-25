using CapaDatos.ContabilidadAPI.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.DAO.Interfaces.General
{
    public interface IEmppla
    {
        public Task<MVT_EMPPLA> GetEMMPLA(string idDocumento);
    }
}
