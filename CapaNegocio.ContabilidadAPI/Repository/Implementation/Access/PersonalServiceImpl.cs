using AutoMapper;
using CapaDatos.ContabilidadAPI.DAO.Interfaces.Access;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces.Access;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation.Access
{
    /// <summary>
    /// Implementación del servicio para Personal
    /// </summary>
    public class PersonalServiceImpl : IPersonalService
    {
        private readonly IPersonalDao _personalDao;
        private readonly IMapper _mapper;

        public PersonalServiceImpl(IPersonalDao personalDao, IMapper mapper)
        {
            _personalDao = personalDao;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene personal por documento de identidad
        /// </summary>
        public async Task<ApiResponse<PersonalReadDto>> GetByIdDocumentoAsync(string idDocumento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idDocumento))
                {
                    return new ApiResponse<PersonalReadDto>(null, "El documento de identidad es requerido");
                }

                var personal = await _personalDao.GetByIdDocumentoAsync(idDocumento);
                
                if (personal == null)
                {
                    return new ApiResponse<PersonalReadDto>(null, "Personal no encontrado");
                }

                var personalDto = _mapper.Map<PersonalReadDto>(personal);
                return new ApiResponse<PersonalReadDto>(personalDto, "Personal encontrado exitosamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<PersonalReadDto>(null, $"Error al obtener personal: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene personal filtrado con paginación
        /// </summary>
        public async Task<ApiResponse<PaginatedResult<PersonalReadDto>>> GetPersonalFiltradoAsync(PersonalFiltroDto filtro)
        {
            try
            {
                if (filtro == null)
                {
                    filtro = new PersonalFiltroDto();
                }

                // Validar paginación
                if (filtro.Pagina <= 0) filtro.Pagina = 1;
                if (filtro.TamanoPagina <= 0) filtro.TamanoPagina = 10;
                //if (filtro.TamanoPagina > 100) filtro.TamanoPagina = 100; // Límite máximo

                var (personal, totalRegistros) = await _personalDao.GetPersonalFiltradoAsync(
                    filtro.Nombres,
                    filtro.IdDocumento,
                    filtro.Empresa,
                    filtro.UsrSidige,
                    filtro.Pagina,
                    filtro.TamanoPagina);

                var personalDtos = _mapper.Map<List<PersonalReadDto>>(personal);

                var resultado = new PaginatedResult<PersonalReadDto>
                {
                    Data = personalDtos,
                    TotalRecords = totalRegistros,
                    Page = filtro.Pagina,
                    PageSize = filtro.TamanoPagina,
                    TotalPages = (int)Math.Ceiling(totalRegistros / (double)filtro.TamanoPagina),
                    HasNextPage = filtro.Pagina * filtro.TamanoPagina < totalRegistros,
                    HasPreviousPage = filtro.Pagina > 1
                };

                var mensaje = $"Se encontraron {totalRegistros} registros. Página {filtro.Pagina} de {resultado.TotalPages}";
                return new ApiResponse<PaginatedResult<PersonalReadDto>>(resultado, mensaje);
            }
            catch (Exception ex)
            {
                return new ApiResponse<PaginatedResult<PersonalReadDto>>(null, $"Error al obtener personal filtrado: {ex.Message}");
            }
        }

        /// <summary>
        /// Busca personal por nombres (búsqueda parcial)
        /// </summary>
        public async Task<ApiResponse<List<PersonalReadDto>>> BuscarPorNombresAsync(string nombres)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombres))
                {
                    return new ApiResponse<List<PersonalReadDto>>(new List<PersonalReadDto>(), "El nombre es requerido para la búsqueda");
                }

                if (nombres.Length < 3)
                {
                    return new ApiResponse<List<PersonalReadDto>>(new List<PersonalReadDto>(), "El nombre debe tener al menos 3 caracteres");
                }

                var personal = await _personalDao.GetByNombresAsync(nombres);
                var personalDtos = _mapper.Map<List<PersonalReadDto>>(personal);

                var mensaje = $"Se encontraron {personalDtos.Count} registros que coinciden con '{nombres}'";
                return new ApiResponse<List<PersonalReadDto>>(personalDtos, mensaje);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<PersonalReadDto>>(null, $"Error al buscar personal por nombres: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si existe personal con el documento especificado
        /// </summary>
        public async Task<ApiResponse<bool>> ExistePersonalAsync(string idDocumento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idDocumento))
                {
                    return new ApiResponse<bool>(false, "El documento de identidad es requerido");
                }

                var existe = await _personalDao.ExistePersonalAsync(idDocumento);
                var mensaje = existe ? "El personal existe en el sistema" : "El personal no existe en el sistema";
                
                return new ApiResponse<bool>(existe, mensaje);
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, $"Error al verificar existencia del personal: {ex.Message}");
            }
        }
    }
}