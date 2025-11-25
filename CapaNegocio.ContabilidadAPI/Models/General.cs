using CapaDatos.ContabilidadAPI.Models;
using CapaDatos.ContabilidadAPI.Models.General;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Models
{
    public class General
    {
        public List<TipoGastoDto> Gastos { get; set; }
        public List<PoliticaTipoGastoPersonaDto> PoliticaTipoGastoPersona { get; set; }
        public MVT_EMPPLA Empleado { get; set; }
    }

    
}
