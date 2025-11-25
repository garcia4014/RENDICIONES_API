using CapaDatos.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio.ContabilidadAPI.Repository.Interfaces
{
    public interface INotificacionService
    {
        public Task<bool> SendMail(int id);
    }
}
