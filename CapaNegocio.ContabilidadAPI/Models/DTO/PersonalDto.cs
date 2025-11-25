using CapaDatos.ContabilidadAPI.Models.Access;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class PersonalDto
    {
        public string idDocumento { get; set; }
        public string nombres { get; set; }
        public string usrSidige { get; set; }
        public string empresa { get; set; }
        public string token { get; set; }
        public int? TP_ID { get; set; } = 0;
        public string? TP_DESCRIPCION { get; set; } = string.Empty;
        public List<Perfil_Usuario>  perfil_Usuarios { get; set; }
    }
}
