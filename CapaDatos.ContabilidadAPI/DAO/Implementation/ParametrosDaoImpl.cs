using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CapaDatos.ContabilidadAPI.DAO.Implementation
{
    public class ParametrosDaoImpl : IParametrosDao
    {
        private readonly SvrendicionesContext _context;

        public ParametrosDaoImpl(SvrendicionesContext context)
        {
            _context = context;
        }

        public async Task<bool> ActualizarTokenAsync(string nuevoToken)
        {
            try
            {
                var parametro = await _context.Parametros.FindAsync(1);
                
                if (parametro == null)
                {
                    return false;
                }

                parametro.Valor = nuevoToken;
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Parametros?> ObtenerParametroPorIdAsync(int id)
        {
            return await _context.Parametros.FindAsync(id);
        }
    }
}
