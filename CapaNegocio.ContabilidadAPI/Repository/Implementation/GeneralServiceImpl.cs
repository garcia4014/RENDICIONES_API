using AutoMapper;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.DAO.Interfaces.General;
using CapaDatos.ContabilidadAPI.Models;
using CapaDatos.ContabilidadAPI.Models.Access;
using CapaDatos.ContabilidadAPI.Models.General;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Repository.Implementation
{
    public class GeneralServiceImpl : IGeneralService
    {
        private readonly ITipoGasto _tipoGastoDao;
        private readonly IPoliticaTipoGastoPersona _politicaTipoGastoPersona;
        private readonly IEmppla _emppla;
        private readonly IMapper _mapper;
        public GeneralServiceImpl(ITipoGasto tipoGasto, IPoliticaTipoGastoPersona politicaTipoGastoPersona, IEmppla emppla,IMapper mapper)
        {
            _tipoGastoDao = tipoGasto;
            _politicaTipoGastoPersona = politicaTipoGastoPersona;
            _emppla = emppla;
            _mapper = mapper;
        }

        public async Task<ApiResponse<General>> GetGeneralData(string idDocumento)
        {
            try
            {
                var tipoGastos = await _tipoGastoDao.GetListTipoGasto();
                var tipoGastoMapper = _mapper.Map<List<TipoGastoDto>>(tipoGastos);

                var politicasTGP = await _politicaTipoGastoPersona.GetListPoliticaTipoGastoPersona();
                var politicaMapper = _mapper.Map<List<PoliticaTipoGastoPersonaDto>>(politicasTGP);

                var empleado = await _emppla.GetEMMPLA(idDocumento);

                var general = new General()
                {
                    Gastos = tipoGastoMapper,
                    PoliticaTipoGastoPersona = politicaMapper,
                    Empleado = empleado
                };

                if (general == null)
                    return new ApiResponse<General>("La lista General no ha sido encontrada.");

                return new ApiResponse<General>(general);
            }
            catch (Exception ex)
            {
                return new ApiResponse<General>("Error al obtener la lista general: " + ex.Message);
            }
        }

        public async Task<EmpleadoDTO> GetEmpleado(string idDocumento)
        {
            var empleado = await _emppla.GetEMMPLA(idDocumento);
            var empleadoDto = _mapper.Map<EmpleadoDTO>(empleado);
            return empleadoDto;
        }
    }
}
