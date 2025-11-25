using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos.ContabilidadAPI;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    public interface IRendicionRepository
    {
        Task<RendicionCabecera> CrearCabeceraAsync(RendicionCabecera cabecera);
        Task<RendicionDetalle> AgregarDetalleAsync(Guid rendId, RendicionDetalle detalle);
        Task<bool> EliminarDetalleAsync(Guid detalleId);
        Task<bool> SubirComprobanteAsync(Guid detalleId, string url);
        Task<RendicionCabecera> ObtenerPorIdAsync(Guid rendId);
    }
}
