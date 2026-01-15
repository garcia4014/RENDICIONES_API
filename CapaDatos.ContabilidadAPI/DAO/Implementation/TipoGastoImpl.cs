using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.DAO.Implementation
{
    public class TipoGastoImpl : ITipoGasto
    {
        private SvrendicionesContext _context;

        public TipoGastoImpl(SvrendicionesContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<TipoGasto>> GetListTipoGasto()
        {        
            return await _context.TipoGastos.Where(x=>x.TgEstado == true).ToListAsync();
        }

        public async Task<TipoGasto> GetTipoGastoById(int TgId)
        {
            return await _context.TipoGastos.FirstOrDefaultAsync(x => x.TgId == TgId);
        }
    }
}
