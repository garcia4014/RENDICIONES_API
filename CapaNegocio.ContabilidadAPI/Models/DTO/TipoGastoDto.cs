using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class TipoGastoDto
    {
        public int TgId { get; set; }

        public string? TgDescripcion { get; set; }

        public decimal? TgPrecioUMax { get; set; }

        public bool? TgEstado { get; set; }
    }
}
