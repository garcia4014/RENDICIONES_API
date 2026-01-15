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
    public class PoliticaTipoGastoPersonaImpl : IPoliticaTipoGastoPersona
    {
        private SvrendicionesContext _context;

        public PoliticaTipoGastoPersonaImpl(SvrendicionesContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PoliticaTipoGastoPersona>> GetListPoliticaTipoGastoPersona()
        {
            return await _context.PoliticaTipoGastoPersonas.ToListAsync();
        }
    }
}
