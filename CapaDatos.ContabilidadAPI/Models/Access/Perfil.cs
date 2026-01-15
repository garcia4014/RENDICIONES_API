using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.Models.Access
{
    public class Perfil
    {
        [Key]
        public int idPerfil { get; set; }
        public string perfilDescripcion { get; set; }
        public bool estadoActivo { get; set; }

        public ICollection<Perfil_Usuario> Perfil_Usuarios { get; set; }
    }
}
