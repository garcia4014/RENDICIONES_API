using AutoMapper;
using CapaDatos.ContabilidadAPI.Models;
using CapaDatos.ContabilidadAPI.Models.Access;
using CapaDatos.ContabilidadAPI.Models.General;
using CapaNegocio.ContabilidadAPI.Models.DTO;


namespace CapaNegocio.ContabilidadAPI.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Personal, PersonalDto>();
            CreateMap<Personal, PersonalReadDto>(); // Nuevo mapeo para PersonalReadDto
            CreateMap<PoliticaTipoGastoPersona, PoliticaTipoGastoPersonaDto>();
            CreateMap<TipoGasto, TipoGastoDto>();
            CreateMap<SviaticosCabecera, SviaticosCabeceraDTO>();
            CreateMap<SolicitudEstadoFlujo, SolicitudEstadoFlujoDTO>();

            CreateMap<SviaticosCabecera, SviaticosCabeceraDTOResponse>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.SolicitudEstadoFlujo))
                .ForMember(dest => dest.ComprobantesPago, opt => opt.MapFrom(src => src.ComprobantesPago));

            CreateMap<SviaticosCabecera, SviaticosCabecerav2DTOResponse>()
             .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles))
             .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.SolicitudEstadoFlujo))
             .ForMember(dest => dest.ComprobantesPago, opt => opt.MapFrom(src => src.ComprobantesPago))
             .ForMember(dest => dest.Empleado, opt => opt.Ignore()); // Inicialmente ignoramos Empleado

            CreateMap<SviaticosDetalle, SviaticosDetalleDTO>();
            CreateMap<SviaticosDetalle, SviaticosDetalleDTOResponse>();
            CreateMap<SolicitudEstadoFlujo, SolicitudEstadoFlujoDTO>();
            CreateMap<MVT_EMPPLA, EmpleadoDTO>(); // Crear un mapa para la tabla de empleados

            // Mapeos para ComprobantePago
            CreateMap<ComprobantePago, ComprobantePagoDto>()
                .ForMember(dest => dest.SvTipoGasto, opt => opt.MapFrom(src => src.TipoGasto != null ? src.TipoGasto.TgDescripcion : null));
            CreateMap<ComprobantePagoCreateDto, ComprobantePago>();
            CreateMap<ComprobantePagoUpdateDto, ComprobantePago>();

            // Mapeos para Notificaciones
            CreateMap<Notificacion, NotificacionDto>();
            CreateMap<NotificacionCreateDto, Notificacion>();
            CreateMap<NotificacionUpdateDto, Notificacion>();

            // Mapeos para UsuarioTipoPersona
            CreateMap<UsuarioTipoPersona, UsuarioTipoPersonaDto>()
                .ForMember(dest => dest.TipoPersonaDescripcion, opt => opt.MapFrom(src => src.TipoPersona.TpDescripcion))
                .ForMember(dest => dest.TipoPersonaAbreviada, opt => opt.MapFrom(src => src.TipoPersona.TpDescripcionAbreviada));
            
            CreateMap<UsuarioTipoPersonaCreateDto, UsuarioTipoPersona>();
            
            CreateMap<UsuarioTipoPersonaUpdateDto, UsuarioTipoPersona>()
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.TipoPersona, opt => opt.Ignore());



        }
    }
}
