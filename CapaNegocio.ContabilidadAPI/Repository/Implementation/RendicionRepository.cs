using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CapaDatos.ContabilidadAPI; 
using CapaNegocio.ContabilidadAPI.Repository.Interfaces; 

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    public class RendicionRepository : IRendicionRepository
    {
        private readonly CapaDatos.ContabilidadAPI.SvrendicionesContext _context;

        public RendicionRepository(CapaDatos.ContabilidadAPI.SvrendicionesContext context)
        {
            _context = context;
        }

        public async Task<RendicionCabecera> CrearCabeceraAsync(RendicionCabecera cabecera)
        {
            cabecera.RendId = Guid.NewGuid();
            cabecera.FechaCreacion = DateTime.Now;

            _context.RendicionCabeceras.Add(cabecera);
            await _context.SaveChangesAsync();

            return cabecera;
        }

        public async Task<RendicionDetalle> AgregarDetalleAsync(Guid rendId, RendicionDetalle detalle)
        {
            detalle.DetId = Guid.NewGuid();
            detalle.RendId = rendId;

            _context.RendicionDetalles.Add(detalle);
            await _context.SaveChangesAsync();

            return detalle;
        }

        public async Task<bool> EliminarDetalleAsync(Guid detalleId)
        {
            var detalle = await _context.RendicionDetalles.FindAsync(detalleId);
            if (detalle == null) return false;

            _context.RendicionDetalles.Remove(detalle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubirComprobanteAsync(Guid detalleId, string url)
        {
            var detalle = await _context.RendicionDetalles.FindAsync(detalleId);
            if (detalle == null) return false;

            detalle.ComprobanteUrl = url;
            _context.RendicionDetalles.Update(detalle);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<RendicionCabecera> ObtenerPorIdAsync(Guid rendId)
        {
            return await _context.RendicionCabeceras
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.RendId == rendId);
        }
         
    }
}


