using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Models.DTO
{
    public class EmailTemplateHelper
    {
        public static async Task<string> GetEmailTemplateAsync(string filePath, ViaticoViewModel model)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"No se encontró el archivo: {filePath}");
            }

            string template = await File.ReadAllTextAsync(filePath);

            // Reemplazar los placeholders con valores reales
            template = template.Replace("@Model.Empleado.Name", model.Empleado.Name);
            template = template.Replace("@Model.SviaticoCabecera.SvNumero", model.SviaticoCabecera.SvNumero);
            template = template.Replace("@Model.Empleado.U_MVT_NOM1", model.Empleado.U_MVT_NOM1);
            template = template.Replace("@Model.Empleado.U_MVT_APPAT", model.Empleado.U_MVT_APPAT);


            return template;
        }
    }


}


public class ViaticoViewModel
{
    public EmpleadoDTO Empleado { get; set; }
    public SviaticosCabeceraDTOResponse SviaticoCabecera { get; set; }
    public List<TipoGastoDto> LstTipoGasto { get; set; }
    public List<PoliticaTipoGastoPersonaDto> LstPoliticTipoPersona { get; set; }
}