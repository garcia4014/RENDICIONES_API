using AutoMapper;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    public class UsuarioTipoPersonaServiceImpl : IUsuarioTipoPersonaService
    {
        private readonly IUsuarioTipoPersonaDao _usuarioTipoPersonaDao;
        private readonly IMapper _mapper;

        public UsuarioTipoPersonaServiceImpl(IUsuarioTipoPersonaDao usuarioTipoPersonaDao, IMapper mapper)
        {
            _usuarioTipoPersonaDao = usuarioTipoPersonaDao;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<UsuarioTipoPersonaDto>>> GetAllAsync()
        {
            try
            {
                var usuarios = await _usuarioTipoPersonaDao.GetAllAsync();
                var usuariosDto = _mapper.Map<IEnumerable<UsuarioTipoPersonaDto>>(usuarios);

                return new ApiResponse<IEnumerable<UsuarioTipoPersonaDto>>(usuariosDto, "Usuarios obtenidos exitosamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<UsuarioTipoPersonaDto>>(null, $"Error al obtener usuarios: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UsuarioTipoPersonaDto>> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return new ApiResponse<UsuarioTipoPersonaDto>(null, "El código es requerido");
                }

                var usuario = await _usuarioTipoPersonaDao.GetByCodeAsync(code);
                if (usuario == null)
                {
                    return new ApiResponse<UsuarioTipoPersonaDto>(null, "Usuario no encontrado");
                }

                var usuarioDto = _mapper.Map<UsuarioTipoPersonaDto>(usuario);
                return new ApiResponse<UsuarioTipoPersonaDto>(usuarioDto, "Usuario obtenido exitosamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<UsuarioTipoPersonaDto>(null, $"Error al obtener usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaginatedResult<UsuarioTipoPersonaDto>>> GetFilteredAsync(UsuarioTipoPersonaFiltroDto filtro)
        {
            try
            {
                if (filtro.Pagina <= 0) filtro.Pagina = 1;
                if (filtro.TamañoPagina <= 0) filtro.TamañoPagina = 10;
                if (filtro.TamañoPagina > 100) filtro.TamañoPagina = 100;

                var (usuarios, totalCount) = await _usuarioTipoPersonaDao.GetFilteredAsync(
                    filtro.Code, 
                    filtro.TpId, 
                    filtro.UserSAP, 
                    filtro.Activo,
                    filtro.Pagina, 
                    filtro.TamañoPagina);

                var usuariosDto = _mapper.Map<IEnumerable<UsuarioTipoPersonaDto>>(usuarios);

                var result = new PaginatedResult<UsuarioTipoPersonaDto>
                {
                    Data = usuariosDto.ToList(),
                    TotalRecords = totalCount,
                    Page = filtro.Pagina,
                    PageSize = filtro.TamañoPagina,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filtro.TamañoPagina),
                    HasNextPage = filtro.Pagina < (int)Math.Ceiling((double)totalCount / filtro.TamañoPagina),
                    HasPreviousPage = filtro.Pagina > 1
                };

                return new ApiResponse<PaginatedResult<UsuarioTipoPersonaDto>>(result, "Usuarios filtrados exitosamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<PaginatedResult<UsuarioTipoPersonaDto>>(null, $"Error al filtrar usuarios: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UsuarioTipoPersonaDto>> CreateAsync(UsuarioTipoPersonaCreateDto createDto)
        {
            try
            {
                // Validar si el código ya existe
                var existeUsuario = await _usuarioTipoPersonaDao.ExistsAsync(createDto.Code);
                if (existeUsuario)
                {
                    return new ApiResponse<UsuarioTipoPersonaDto>(null, "Ya existe un usuario con este código");
                }

                var usuario = _mapper.Map<UsuarioTipoPersona>(createDto);
                var usuarioCreado = await _usuarioTipoPersonaDao.CreateAsync(usuario);

                var usuarioDto = _mapper.Map<UsuarioTipoPersonaDto>(usuarioCreado);
                return new ApiResponse<UsuarioTipoPersonaDto>(usuarioDto, "Usuario creado exitosamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<UsuarioTipoPersonaDto>(null, $"Error al crear usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UsuarioTipoPersonaDto>> UpdateAsync(string code, UsuarioTipoPersonaUpdateDto updateDto)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return new ApiResponse<UsuarioTipoPersonaDto>(null, "El código es requerido");
                }

                var usuarioExistente = await _usuarioTipoPersonaDao.GetByCodeAsync(code);
                if (usuarioExistente == null)
                {
                    return new ApiResponse<UsuarioTipoPersonaDto>(null, "Usuario no encontrado");
                }

                // Mapear los cambios
                _mapper.Map(updateDto, usuarioExistente);
                usuarioExistente.Code = code; // Asegurar que el código no cambie

                var usuarioActualizado = await _usuarioTipoPersonaDao.UpdateAsync(usuarioExistente);
                if (usuarioActualizado == null)
                {
                    return new ApiResponse<UsuarioTipoPersonaDto>(null, "Error al actualizar usuario");
                }

                var usuarioDto = _mapper.Map<UsuarioTipoPersonaDto>(usuarioActualizado);
                return new ApiResponse<UsuarioTipoPersonaDto>(usuarioDto, "Usuario actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<UsuarioTipoPersonaDto>(null, $"Error al actualizar usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return new ApiResponse<bool>(false, "El código es requerido");
                }

                var usuario = await _usuarioTipoPersonaDao.GetByCodeAsync(code);
                if (usuario == null)
                {
                    return new ApiResponse<bool>(false, "Usuario no encontrado");
                }

                var resultado = await _usuarioTipoPersonaDao.DeleteAsync(code);
                if (resultado)
                {
                    return new ApiResponse<bool>(true, "Usuario eliminado exitosamente");
                }

                return new ApiResponse<bool>(false, "Error al eliminar usuario");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, $"Error al eliminar usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ExistsAsync(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return new ApiResponse<bool>(false, "El código es requerido");
                }

                var existe = await _usuarioTipoPersonaDao.ExistsAsync(code);
                return new ApiResponse<bool>(existe, "Consulta realizada exitosamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, $"Error al verificar existencia: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<UsuarioTipoPersonaDto>>> GetByTipoPersonaAsync(int tpId)
        {
            try
            {
                if (tpId <= 0)
                {
                    return new ApiResponse<IEnumerable<UsuarioTipoPersonaDto>>(null, "El ID del tipo de persona es requerido");
                }

                var usuarios = await _usuarioTipoPersonaDao.GetByTipoPersonaAsync(tpId);
                var usuariosDto = _mapper.Map<IEnumerable<UsuarioTipoPersonaDto>>(usuarios);

                return new ApiResponse<IEnumerable<UsuarioTipoPersonaDto>>(usuariosDto, "Usuarios obtenidos exitosamente");
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<UsuarioTipoPersonaDto>>(null, $"Error al obtener usuarios por tipo de persona: {ex.Message}");
            }
        }
    }
}