using CapaDatos.ContabilidadAPI.DAO.Interfaces.General;
using CapaDatos.ContabilidadAPI.Models.General;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.DAO.Implementation.General
{
    public class IEmpplaImpl : IEmppla
    {
        private GeneralDBContext _context;
        public IEmpplaImpl(GeneralDBContext context)
        {
            _context = context;
        }

        public async Task<MVT_EMPPLA> GetEMMPLA(string idDocumento)
        {
            return await _context.Empleados.FirstAsync(x => x.Code.Equals(idDocumento));
        }
    }
}
