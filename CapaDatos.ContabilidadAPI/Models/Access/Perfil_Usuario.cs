using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos.ContabilidadAPI.Models.Access
{
    public class Perfil_Usuario
    {
        [Key]
        public int idPerfil_Usuario { get; set; }
        public string idDocumento { get; set; }
        public int idPerfil { get; set; }
        public bool estadoActivo { get; set; }
        public Perfil Perfil { get; set; }

    }
}
