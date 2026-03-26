namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    public interface IRendicionRepository
    {
        Task<CapaDatos.ContabilidadAPI.RendicionCabecera> CrearCabeceraAsync(CapaDatos.ContabilidadAPI.RendicionCabecera cabecera);
        Task<CapaDatos.ContabilidadAPI.RendicionDetalle> AgregarDetalleAsync(Guid rendId, CapaDatos.ContabilidadAPI.RendicionDetalle detalle);
        Task<bool> EliminarDetalleAsync(Guid detalleId);
        Task<bool> SubirComprobanteAsync(Guid detalleId, string url);
        Task<CapaDatos.ContabilidadAPI.RendicionCabecera> ObtenerPorIdAsync(Guid rendId);
    }
}
