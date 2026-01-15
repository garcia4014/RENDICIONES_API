using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class PoliticaTipoGastoPersonaDto
    {
        public int PtgpId { get; set; }

        public int? PtgpIdTg { get; set; }

        public int? PtgpIdTp { get; set; }
        public decimal PtgpMonto { get; set; }

        public bool? PtgpEstado { get; set; }

    }
}
