using CapaDatos.ContabilidadAPI.DAO.Interfaces.Access;
using CapaDatos.ContabilidadAPI.Models.Access;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.DAO.Implementation.Access
{
    public class IAccessImpl : IAccess
    {
        private AccessDBContext _context;
        private SvrendicionesContext _svrContext;

        public IAccessImpl(AccessDBContext context, SvrendicionesContext svrContext)
        {
            this._context = context;
            _svrContext = svrContext;   
        }

        public async Task<List<Perfil_Usuario>> GetPerfilesByUsuario(string idDocumento)
        {
            return await _context.Perfil_Usuario
                  .Include(pu => pu.Perfil) 
                  .Where(x => x.idDocumento.ToUpper().Equals(idDocumento.ToUpper())
              && x.estadoActivo == true)
                  .ToListAsync();
        }

        public async Task<Personal> ValidarPersonal(string username, string password)
        {
            try
            {
                var personal = await _context.Personal.FirstOrDefaultAsync(
                                x => x.UsrSidige.ToUpper().Equals(username.ToUpper())
                                && x.PswSidige.Equals(password));

                var perfilWeb = await _svrContext.UsuarioTipoPersonas.Include(x => x.TipoPersona).Where(x => x.Code == personal.IdDocumento).FirstOrDefaultAsync();
                if (perfilWeb != null)
                {
                    personal.TP_DESCRIPCION = perfilWeb.TipoPersona.TpDescripcion;
                    personal.TP_ID = perfilWeb.TipoPersona.TpId;
                }

                return personal;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
